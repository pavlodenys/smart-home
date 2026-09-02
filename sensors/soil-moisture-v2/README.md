# ESP8266 capacitive soil-moisture sensor v2.0

PlatformIO firmware for a NodeMCU v2 (ESP8266) connected to a capacitive
soil-moisture sensor v2.0. It publishes moisture readings to the same RabbitMQ
MQTT bridge used by the DHT11 firmware.

## Wiring

| Sensor | NodeMCU v2 |
| --- | --- |
| VCC | `3V3` |
| GND | `GND` |
| AOUT | `A0` |

Power the probe from `3.3V`. The NodeMCU development board has an input divider
on `A0`; a bare ESP8266 ADC accepts only about 1.0 V and needs an external divider.

## Configuration

Copy the committed example to the ignored local configuration, then edit it
before flashing:

```powershell
Copy-Item include/config.example.h include/config.h
```

- Wi-Fi SSID and password
- a unique `OTA_PASSWORD` for local wireless firmware updates
- RabbitMQ/MQTT host and credentials
- `SOIL_DATA_ID` if ID `4` is already used
- dry and wet ADC calibration values

To calibrate, read the serial output with the probe completely dry and record the
raw `ADC` value as `SOIL_DRY_ADC`. Repeat with the sensing area in water (keep the
electronics above water) and record it as `SOIL_WET_ADC`.

## Build and flash

```powershell
pio run
pio run --target upload
pio device monitor
```

## Wireless firmware updates

Flash the firmware once over USB using the default `nodemcuv2` environment.
After it connects to Wi-Fi, it advertises as `soil-moisture.local` and accepts
password-protected updates from the same local network. Add these local-only
values to `include/config.h`:

```cpp
constexpr char OTA_HOSTNAME[] = "soil-moisture";
constexpr char OTA_PASSWORD[] = "a-long-random-ota-password";
```

Build and upload over Wi-Fi:

```powershell
$env:OTA_PASSWORD = "your-OTA-password"
platformio run -e nodemcuv2-ota --target upload --upload-port soil-moisture.local
```

Use the device IP address in place of `soil-moisture.local` when local DNS does
not resolve it. Keep OTA on the private Wi-Fi network; it must not be exposed
through router port forwarding.

If PlatformIO is not installed on the host, build with Docker from the repository
root. The named volume preserves downloaded ESP8266 toolchains between builds:

```powershell
docker build -f sensors/Dockerfile.platformio -t smart-home-platformio sensors
docker run --rm -v "${PWD}/sensors/soil-moisture-v2:/workspace" -v platformio-core-cache:/root/.platformio smart-home-platformio run
```

The firmware publishes every 20 seconds to `sensors_data`:

```json
{"Id":4,"Name":"%","Value":52.4,"Time":1786800000}
```
