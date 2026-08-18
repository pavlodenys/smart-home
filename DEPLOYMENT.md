# Full Cloud Deployment Guide

This guide deploys the entire Smart Home stack to an Oracle Cloud Always Free Ubuntu VM:

- Svelte UI
- ASP.NET API and SignalR hub
- PostgreSQL
- RabbitMQ with MQTT
- SmartHome connector
- ESP8266 soil-moisture sensor

The final public endpoints are:

| Endpoint | Purpose |
| --- | --- |
| `https://home.example.com` | UI, API, SignalR, and Swagger |
| `mqtt.home.example.com:1883` | MQTT sensor connection, temporary plain-TCP option |

Use a domain you control in place of `home.example.com`. A subdomain is fine.

## 1. Important Security Notes

Do not deploy the development configuration as-is. It contains localhost URLs and committed development credentials.

Before exposing the application:

- Generate new PostgreSQL, RabbitMQ, and JWT secrets.
- Do not expose PostgreSQL (`5432`), RabbitMQ management (`15672`), or AMQP (`5672`) publicly.
- Do not expose MQTT port `1883` to the entire internet permanently. MQTT on `1883` is plaintext, including its password. Restrict it to your home public IP while using the current firmware, then move to MQTT over TLS before widening access.
- Keep the server and Docker images patched.

## 2. Create an Oracle Cloud Free VM

1. Create an Oracle Cloud account. Oracle requires a payment card for account verification, but the Always Free VM does not have a monthly charge while it stays within the free allocation.
2. Create an Ubuntu 22.04 or Ubuntu 24.04 instance.
3. Prefer an Always Free ARM shape with at least 2 OCPUs and 12 GB RAM if capacity is available. PostgreSQL works on ARM.
4. Reserve a public IPv4 address for the VM.
5. Create these DNS records at your DNS provider:

   ```text
   home.example.com  A  <VM_PUBLIC_IP>
   mqtt.home.example.com  A  <VM_PUBLIC_IP>
   ```

6. In the Oracle VCN security list or network security group, allow inbound TCP:

   - `22` from your own public IP only
   - `80` from anywhere, required for initial HTTPS certificate issuance
   - `443` from anywhere
   - `1883` from your home public IP only, while MQTT uses plain TCP

## 3. Prepare the VM

Connect using SSH:

```bash
ssh ubuntu@<VM_PUBLIC_IP>
```

Install Git and Docker:

```bash
sudo apt update
sudo apt upgrade -y
sudo apt install -y ca-certificates curl git
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER
exit
```

Connect again, then verify Docker:

```bash
ssh ubuntu@<VM_PUBLIC_IP>
docker version
docker compose version
```

Clone the repository:

```bash
git clone <YOUR_REPOSITORY_URL> smart-home
cd smart-home
```

## 4. Make the UI Use Same-Origin URLs

The production UI must not use `localhost`. Caddy will serve the UI, API, and SignalR hub on one HTTPS origin.

Update `smart-home-ui/src/api/httpServise.ts`:

```ts
const baseUrl = import.meta.env.VITE_BASE_URL ?? "/";
```

Update `smart-home-ui/src/components/chart/Chart.svelte`. Replace the hard-coded SignalR URL with:

```ts
const hubUrl = new URL("hub", window.location.origin + "/").toString();

const connection = new signalR.HubConnectionBuilder()
  .withUrl(hubUrl, {
    skipNegotiation: true,
    transport: signalR.HttpTransportType.WebSockets,
  })
  .withAutomaticReconnect()
  .build();
```

Update `smart-home-ui/Dockerfile` so the API base URL is configurable at build time:

```dockerfile
ARG VITE_BASE_URL=/
ENV VITE_BASE_URL=$VITE_BASE_URL
RUN npm run build
```

Remove the old fixed line:

```dockerfile
ENV VITE_BASE_URL=http://localhost:5200/
```

Build and test this locally before deployment:

```powershell
docker compose up -d --build
docker compose logs --tail=100 smart-home-ui smart-home-api
```

## 5. Create Production Secrets

On the VM, create a private `.env` file:

```bash
umask 077
cat > .env <<'EOF'
POSTGRES_DB=SmartHouse
POSTGRES_USER=smarthome
POSTGRES_PASSWORD=REPLACE_WITH_A_LONG_RANDOM_POSTGRES_PASSWORD
RABBITMQ_USERNAME=smarthome_mqtt
RABBITMQ_PASSWORD=REPLACE_WITH_A_LONG_RANDOM_RABBITMQ_PASSWORD
JWT_KEY=REPLACE_WITH_A_LONG_RANDOM_JWT_KEY_AT_LEAST_32_CHARACTERS
PUBLIC_DOMAIN=home.example.com
EOF
```

Generate random values rather than using the example placeholders:

```bash
openssl rand -base64 36
```

Never commit `.env`. Add it to `.gitignore` if it is not already excluded.

## 6. Create the Production Compose File

Create `docker-compose.production.yml` at the repository root:

```yaml
services:
  caddy:
    image: caddy:2.8-alpine
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    environment:
      PUBLIC_DOMAIN: ${PUBLIC_DOMAIN}
    volumes:
      - ./Caddyfile:/etc/caddy/Caddyfile:ro
      - caddy-data:/data
      - caddy-config:/config
    depends_on:
      smart-home-ui:
        condition: service_started
      smart-home-api:
        condition: service_healthy

  smart-home-ui:
    image: smarthomefrontend:production
    build:
      context: smart-home-ui
      dockerfile: Dockerfile
      args:
        VITE_BASE_URL: /
    restart: unless-stopped
    depends_on:
      smart-home-api:
        condition: service_healthy

  smart-home-api:
    image: smarthomebackend:production
    build:
      context: smart-home-api
      dockerfile: ./SmartHome.Api/Dockerfile
    restart: unless-stopped
    environment:
      ASPNETCORE_URLS: http://+:80
      ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      Jwt__Key: ${JWT_KEY}
    depends_on:
      postgres:
        condition: service_healthy
    healthcheck:
      test: ["CMD-SHELL", "bash -c 'exec 3<>/dev/tcp/localhost/80'"]
      interval: 10s
      timeout: 5s
      retries: 12
      start_period: 20s

  smart-home-connector:
    image: smarthomeconnector:production
    build:
      context: smart-home-api
      dockerfile: ./SmartHome.Connector/Dockerfile
    restart: unless-stopped
    environment:
      ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      RabbitMQ__Host: rabbitmq
      RabbitMQ__Username: ${RABBITMQ_USERNAME}
      RabbitMQ__Password: ${RABBITMQ_PASSWORD}
      RabbitMQ__ExchangeName: amq.topic
      RabbitMQ__QueueName: sensors_data
      SignalR__HubUrl: http://smart-home-api/hub
    depends_on:
      smart-home-api:
        condition: service_healthy
      rabbitmq:
        condition: service_healthy

  postgres:
    image: postgres:16-alpine
    restart: unless-stopped
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d ${POSTGRES_DB}"]
      interval: 10s
      timeout: 5s
      retries: 12
      start_period: 20s

  rabbitmq:
    image: rabbitmq:3.11.9-management
    restart: unless-stopped
    hostname: rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USERNAME}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
    command: /bin/sh -c "rabbitmq-plugins enable --offline rabbitmq_mqtt && rabbitmq-server"
    ports:
      - "1883:1883"
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "-q", "ping"]
      interval: 10s
      timeout: 5s
      retries: 12
      start_period: 20s

volumes:
  caddy-data:
  caddy-config:
  postgres-data:
  rabbitmq-data:
```

This Compose file intentionally publishes only HTTPS and MQTT. PostgreSQL, RabbitMQ management, and AMQP are private to the Docker network.

## 7. Create the Caddy Reverse Proxy

Create `Caddyfile` at the repository root:

```caddyfile
{$PUBLIC_DOMAIN} {
    encode zstd gzip

    @api path /api/*
    reverse_proxy @api smart-home-api:80

    @hub path /hub /hub/*
    reverse_proxy @hub smart-home-api:80

    reverse_proxy smart-home-ui:5173
}
```

Caddy obtains and renews the HTTPS certificate automatically once the `A` record resolves to the VM and ports `80` and `443` are reachable.

## 8. Deploy the Stack

Validate the rendered configuration before starting containers:

```bash
docker compose --env-file .env -f docker-compose.production.yml config -q
```

Build and start the production stack:

```bash
docker compose --env-file .env -f docker-compose.production.yml up -d --build
```

Watch startup:

```bash
docker compose --env-file .env -f docker-compose.production.yml ps
docker compose --env-file .env -f docker-compose.production.yml logs -f
```

Expected checks:

```bash
curl -I https://home.example.com
curl -I https://home.example.com/swagger
```

Open `https://home.example.com` and create a new user. The API creates the PostgreSQL schema automatically through EF Core migrations at startup.

## 9. Configure and Flash the ESP8266

Edit `sensors/soil-moisture-v2/include/config.h` before building firmware:

```cpp
constexpr char WIFI_SSID[] = "YOUR_WIFI_NAME";
constexpr char WIFI_PASSWORD[] = "YOUR_WIFI_PASSWORD";

constexpr char MQTT_HOST[] = "mqtt.home.example.com";
constexpr uint16_t MQTT_PORT = 1883;
constexpr char MQTT_USERNAME[] = "smarthome_mqtt";
constexpr char MQTT_PASSWORD[] = "THE_RABBITMQ_PASSWORD_FROM_THE_VM_ENV_FILE";
constexpr char MQTT_TOPIC[] = "sensors_data";

// Must match the chart-data ID created in the application.
constexpr int SOIL_DATA_ID = 4;
```

Build and flash from the sensor directory using PlatformIO:

```powershell
cd sensors/soil-moisture-v2
pio run --target upload
pio device monitor
```

The serial monitor should show a successful MQTT connection and messages like:

```text
ADC=..., moisture=..., publish=ok, payload={"Id":4,...}
```

After the device publishes, open the sensor page in the UI and select the current date. The chart updates through SignalR and the reading remains in PostgreSQL after refresh.

## 10. Confirm the Full Data Flow

On the VM, verify that the connector is connected:

```bash
docker compose --env-file .env -f docker-compose.production.yml logs --tail=50 smart-home-connector
```

Expected output includes:

```text
[*] Signalr state = Connected
```

Verify RabbitMQ has a consumer:

```bash
docker compose --env-file .env -f docker-compose.production.yml exec rabbitmq rabbitmqctl list_queues name messages consumers
```

Expected queue:

```text
sensors_data  0  1
```

Inspect newly stored readings:

```bash
docker compose --env-file .env -f docker-compose.production.yml exec postgres \
  psql -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
  -c 'SELECT "Id", "Value", "DataId", "DateTime" FROM "Point" ORDER BY "Id" DESC LIMIT 10;'
```

## 11. Backup PostgreSQL

Create a directory for backups:

```bash
mkdir -p ~/smart-home-backups
chmod 700 ~/smart-home-backups
```

Create a manual backup:

```bash
docker compose --env-file .env -f docker-compose.production.yml exec -T postgres \
  pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB" \
  > ~/smart-home-backups/smarthouse-$(date +%F).sql
```

Copy backups off the VM. A backup stored only on the VM does not protect against VM loss:

```bash
scp ubuntu@<VM_PUBLIC_IP>:smart-home-backups/smarthouse-YYYY-MM-DD.sql .
```

To restore into a stopped or empty database:

```bash
cat smarthouse-YYYY-MM-DD.sql | docker compose --env-file .env -f docker-compose.production.yml exec -T postgres \
  psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"
```

## 12. Updating the Application

On the VM:

```bash
cd ~/smart-home
git pull
docker compose --env-file .env -f docker-compose.production.yml up -d --build
docker image prune -f
```

Check services after every update:

```bash
docker compose --env-file .env -f docker-compose.production.yml ps
docker compose --env-file .env -f docker-compose.production.yml logs --tail=100 smart-home-api smart-home-connector
```

## 13. MQTT TLS Upgrade

The current ESP8266 firmware uses `WiFiClient`, which sends MQTT credentials in plaintext on port `1883`. It is acceptable only as a temporary setup when Oracle firewall rules limit `1883` to your home public IP.

For a public production deployment, upgrade to MQTT over TLS:

1. Configure RabbitMQ's `rabbitmq_mqtt` listener with a server certificate on `8883`.
2. Use `WiFiClientSecure` instead of `WiFiClient` in the firmware.
3. Configure the ESP8266 with the CA certificate or a validated certificate fingerprint.
4. Change the firmware MQTT port to `8883`.
5. Open `8883` in Oracle networking and UFW, then remove the public `1883` rule.

Do not remove the `1883` firewall allowance until the updated ESP8266 firmware has successfully connected to TLS MQTT.

## 14. Troubleshooting

| Symptom | Check |
| --- | --- |
| HTTPS certificate is not issued | Confirm both DNS records resolve to the VM and TCP `80`/`443` are open in Oracle and UFW. |
| UI loads but API requests fail | Verify the UI was built with `VITE_BASE_URL=/` and Caddy proxies `/api/*`. |
| Charts do not get live updates | Verify Caddy proxies `/hub`, connector log says `Signalr state = Connected`, and the browser page is refreshed after deployment. |
| Sensor publishes but no chart point appears | Check the firmware `SOIL_DATA_ID` matches the connected chart-data ID, normally `4` in this deployment. |
| RabbitMQ rejects the ESP8266 | Verify DNS, port `1883`, credentials, and Oracle/UFW source-IP restrictions. |
| Database is unavailable | Run `docker compose --env-file .env -f docker-compose.production.yml ps` and inspect PostgreSQL logs. |

## 15. Production Checklist

- [ ] DNS for `home.example.com` resolves to the VM.
- [ ] HTTPS opens at `https://home.example.com`.
- [ ] `.env` contains unique, secret production values and is not committed.
- [ ] Only `22`, `80`, `443`, and restricted MQTT access are open.
- [ ] PostgreSQL, RabbitMQ management, and AMQP ports are not public.
- [ ] The ESP8266 publishes to the cloud MQTT hostname.
- [ ] `SOIL_DATA_ID` matches the chart data source.
- [ ] A new reading appears in PostgreSQL and the UI chart.
- [ ] Backups are copied off the VM.
- [ ] MQTT TLS is planned before allowing MQTT access from arbitrary networks.
