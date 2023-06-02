// DHT11 sensor connected to D1 input of ESP8266
// main challanges:
// - connect ESP to Windows 10(need to install proper driver)
// - baund - 9600

#include <SPI.h>
#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>
#include <Adafruit_Sensor.h>
#include <DHT.h>
#include <DHT_U.h>
#include <PubSubClient.h>
#include <ArduinoJson.h>
#include "RTClib.h"
#include <TimeLib.h>

#define DHTTYPE DHT11

struct Point
{
  int Id;
  float Value;
  time_t Time;
};
//const DateTime &dt;

// const char *ssid = "Nexty"; // Enter SSID here
const char *ssid = "Potter"; // Enter SSID here
const char *password = "04041992";

// const char *broker = "192.168.0.151";
const char *broker = "192.168.3.21";
const int port = 1883;

const char *user = "rmuser";
const char *pass = "rmpassword";

WiFiClient espClient;
PubSubClient client(espClient);

void callback(char *topic, byte *payload, unsigned int length);
void reconnect();

#define DHT_PIN 5
DHT dht(DHT_PIN, DHTTYPE);

float Temperature;
float Humidity;
char tempId[] = "2 ";
char humId[] = "3 ";

void setup()
{
  Serial.begin(9600);
  delay(100);

  pinMode(5, INPUT);

  dht.begin();

  Serial.println("Connecting to ");
  Serial.println(ssid);

  // connect to your local wi-fi network
  WiFi.begin(ssid, password);

  // check wi-fi is connected to wi-fi network
  while (WiFi.status() != WL_CONNECTED)
  {
    delay(1000);
    Serial.print(".");
  }

  Serial.println("");
  Serial.println("WiFi connected..!");
  Serial.print("Got IP: ");
  Serial.println(WiFi.localIP());

  configTime(0, 0, "pool.ntp.org"); // Set NTP time server

  Serial.println("Waiting for NTP time sync...");
  while (!time(nullptr))
  {
    delay(1000);
    Serial.println("Waiting for NTP time sync...");
  }

  client.setServer(broker, port);
  client.setCallback(callback);
  // TODO: get sensor id
}

void loop()
{
  if (!client.connected())
  {
    reconnect();
  }
  else
  {
    delay(20000);
    float h = dht.readHumidity();
    float t = dht.readTemperature();
   // int time_now = dt.unixtime();

    if (isnan(h) || isnan(t))
    {
      Serial.println("Failed to get data");
      return;
    }

    time_t now = time(nullptr); // Get current time
    char timeVal[17];
    strcpy(timeVal, ctime(&now));
    Serial.println(ctime(&now)); // Print current date and time

    char tempVal[17];
    char humVal[17];
  

    sprintf_P(tempVal, "%f", t);
    sprintf_P(humVal, "%f", h);
    //sprintf_P(timeVal, "%i", now);

    Point temperature = {
        2,
        t,
        now};
    Point humidity = {3, h, now};

    StaticJsonDocument<128> docT;
    StaticJsonDocument<128> docH;
    docT["Id"] = temperature.Id;
    docH["Id"] = humidity.Id;
    docH["Name"] = "%";
    docT["Name"] = "C";
    docT["Value"] = temperature.Value;
    docH["Value"] = humidity.Value;

    docT["Time"] = temperature.Time;
    docH["Time"] = humidity.Time;
    char jsonT[128];
    char jsonH[128];
    serializeJson(docT, jsonT);
    serializeJson(docH, jsonH);
    Serial.println(jsonT);
    Serial.println(jsonH);
    client.publish("sensors_data", jsonT); // TODO: add ID !!!
    client.publish("sensors_data", jsonH);
  }
}

void callback(char *topic, byte *payload, unsigned int length)
{
  Serial.println("Message arrived");
}

void reconnect()
{
  Serial.println("Trying to connect to RabbitMQ...");

  while (!client.connected())
  {
    if (client.connect("DHT11", user, pass))
    {
      Serial.println("Connected to RabbitMQ.");
    }
    else
    {
      Serial.println("Failed to connect. Trying in 5 seconds...");
      delay(5000);
    }
  }
}
