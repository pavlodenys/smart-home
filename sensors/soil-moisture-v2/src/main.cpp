#include <Arduino.h>
#include <ArduinoJson.h>
#include <ArduinoOTA.h>
#include <ESP8266WiFi.h>
#include <PubSubClient.h>
#include <time.h>

#include "config.h"
#include "moisture_calibration.h"

namespace
{
WiFiClient wifiClient;
PubSubClient mqttClient(wifiClient);
unsigned long lastPublishMs = 0;
constexpr time_t MIN_VALID_UNIX_TIME = 1577836800; // 2020-01-01T00:00:00Z

void configureOta()
{
    ArduinoOTA.setHostname(OTA_HOSTNAME);
    ArduinoOTA.setPassword(OTA_PASSWORD);
    ArduinoOTA.onStart([]()
                       { Serial.println("OTA update starting"); });
    ArduinoOTA.onEnd([]()
                     { Serial.println("OTA update complete; restarting"); });
    ArduinoOTA.onError([](ota_error_t error)
                       { Serial.printf("OTA update failed, error=%u\n", error); });
    ArduinoOTA.begin();

    Serial.printf("OTA ready at %s.local\n", OTA_HOSTNAME);
}

void connectWifi()
{
    if (WiFi.status() == WL_CONNECTED)
    {
        return;
    }

    Serial.printf("Connecting to Wi-Fi '%s'", WIFI_SSID);
    WiFi.mode(WIFI_STA);
    WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

    while (WiFi.status() != WL_CONNECTED)
    {
        delay(500);
        Serial.print('.');
    }

    Serial.printf("\nWi-Fi connected, IP: %s\n",
                  WiFi.localIP().toString().c_str());
}

void waitForTimeSync()
{
    Serial.print("Waiting for NTP time");
    while (time(nullptr) < MIN_VALID_UNIX_TIME)
    {
        connectWifi();
        ArduinoOTA.handle();
        delay(250);
        Serial.print('.');
    }

    Serial.println(" synchronized");
}

void connectMqtt()
{
    while (!mqttClient.connected())
    {
        const String clientId =
            "soil-moisture-" + String(ESP.getChipId(), HEX);

        Serial.printf("Connecting to MQTT as %s...\n", clientId.c_str());
        if (mqttClient.connect(clientId.c_str(), MQTT_USERNAME, MQTT_PASSWORD))
        {
            Serial.println("MQTT connected");
            return;
        }

        Serial.printf("MQTT connection failed, state=%d; retrying in 5s\n",
                      mqttClient.state());

        // keep servicing OTA requests while retries are blocked on a failing broker login
        for (uint8_t i = 0; i < 20; ++i)
        {
            ArduinoOTA.handle();
            delay(250);
        }
    }
}

int readAverageAdc()
{
    unsigned long total = 0;
    for (uint8_t sample = 0; sample < ADC_SAMPLE_COUNT; ++sample)
    {
        total += analogRead(A0);
        delay(10);
    }

    return static_cast<int>(total / ADC_SAMPLE_COUNT);
}

void publishMoisture()
{
    const int rawAdc = readAverageAdc();
    const float moisturePercent =
        moisturePercentFromAdc(rawAdc, SOIL_DRY_ADC, SOIL_WET_ADC);
    const time_t now = time(nullptr);

    StaticJsonDocument<128> document;
    document["Id"] = SOIL_DATA_ID;
    document["Name"] = "%";
    document["Value"] = moisturePercent;
    document["Time"] = now > 0 ? now : 0;

    char payload[128];
    const size_t payloadLength = serializeJson(document, payload);
    const bool published =
        mqttClient.publish(MQTT_TOPIC,
                           reinterpret_cast<const uint8_t *>(payload),
                           payloadLength,
                           false);

    Serial.printf("ADC=%d, moisture=%.1f%%, publish=%s, payload=%s\n",
                  rawAdc,
                  moisturePercent,
                  published ? "ok" : "failed",
                  payload);
}
} // namespace

void setup()
{
    Serial.begin(115200);
    delay(100);

    pinMode(LED_BUILTIN, OUTPUT);
    digitalWrite(LED_BUILTIN, HIGH); // ESP8266 built-in LED is active-low.

    connectWifi();
    configTime(0, 0, "pool.ntp.org", "time.nist.gov");
    configureOta();
    waitForTimeSync();

    mqttClient.setServer(MQTT_HOST, MQTT_PORT);
    mqttClient.setKeepAlive(30);
    mqttClient.setBufferSize(256);

    connectMqtt();
    publishMoisture();
    lastPublishMs = millis();
}

void loop()
{
    connectWifi();
    ArduinoOTA.handle();
    if (!mqttClient.connected())
    {
        connectMqtt();
    }
    mqttClient.loop();

    const unsigned long nowMs = millis();
    if (nowMs - lastPublishMs >= PUBLISH_INTERVAL_MS)
    {
        lastPublishMs = nowMs;
        publishMoisture();
    }

    delay(10);
}
