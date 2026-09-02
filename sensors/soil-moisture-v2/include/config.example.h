#pragma once

#include <Arduino.h>

constexpr char WIFI_SSID[] = "replace-with-wifi-ssid";
constexpr char WIFI_PASSWORD[] = "replace-with-wifi-password";

// Used for password-protected wireless firmware updates on the local network.
constexpr char OTA_HOSTNAME[] = "soil-moisture";
constexpr char OTA_PASSWORD[] = "replace-with-a-long-random-ota-password";

constexpr char MQTT_HOST[] = "replace-with-mqtt-host";
constexpr uint16_t MQTT_PORT = 1883;
constexpr char MQTT_USERNAME[] = "replace-with-mqtt-username";
constexpr char MQTT_PASSWORD[] = "replace-with-mqtt-password";
constexpr char MQTT_TOPIC[] = "sensors_data";

constexpr int SOIL_DATA_ID = 4;

// Replace these values with measurements from your own dry and wet calibration.
constexpr int SOIL_DRY_ADC = 850;
constexpr int SOIL_WET_ADC = 350;

constexpr unsigned long PUBLISH_INTERVAL_MS = 20000;
constexpr uint8_t ADC_SAMPLE_COUNT = 10;

static_assert(SOIL_DRY_ADC != SOIL_WET_ADC,
              "Dry and wet calibration values must differ");
