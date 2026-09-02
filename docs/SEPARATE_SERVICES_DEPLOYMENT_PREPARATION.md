# Separate-Services Cloud Deployment Preparation

## Purpose

This document prepares Smart Home for a prototype cloud deployment without
running Docker Compose online. Each infrastructure concern is provided by a
separate managed service, while the existing ASP.NET API remains the application
backend.

This is a preparation guide, not a record of a completed deployment. The
event-driven ingestion changes described below are not present in the current
application yet.

For a deployment that preserves the current RabbitMQ, Connector, polling worker,
and PostgreSQL topology, use the Oracle VM instructions in
[`DEPLOYMENT.md`](../DEPLOYMENT.md) instead.

## Recommended Prototype Architecture

| Concern | Current local component | Proposed cloud service |
| --- | --- | --- |
| Web UI | `smart-home-ui` container | Cloudflare Pages |
| API, authentication, SignalR, and scenario actions | `smart-home-api` container | Render Free web service |
| Database | PostgreSQL container | Supabase Free PostgreSQL |
| MQTT broker | RabbitMQ MQTT plugin | EMQX Serverless |
| MQTT consumer and point persistence | `smart-home-connector` | Replace with an EMQX HTTP rule and an API ingestion endpoint |
| iPhone push notifications | ntfy sender in the API | Hosted `ntfy.sh` and the ntfy iOS app |

Target data flow:

```text
ESP8266
   |
   | MQTT over TLS
   v
EMQX Serverless
   |
   | authenticated HTTPS webhook
   v
ASP.NET API on Render
   |-- validate and deduplicate reading
   |-- write Point to Supabase PostgreSQL
   |-- evaluate affected scenarios immediately
   |-- publish the reading to connected SignalR clients
   `-- send a transition notification to ntfy

Cloudflare Pages UI -- HTTPS/WebSocket --> ASP.NET API
ntfy.sh --------------------------------> ntfy iOS app
```

No Docker Compose process runs in this design. Render may still build the API
from its individual Dockerfile; Docker is then only a build and packaging
format, not the production orchestrator.

## Why the Current Services Should Not Simply Be Split

The present topology contains two continuously running processes:

- `SmartHome.Connector` maintains a RabbitMQ connection, stores points, and
  forwards them to SignalR.
- `ScenarioWorker` wakes every 20 seconds and polls PostgreSQL for scenario
  transitions.

Free web-service plans are designed around incoming HTTP traffic and may suspend
background processes. Render's free web service suspends after 15 minutes with
no incoming HTTP request or WebSocket message and takes about one minute to wake.
It also does not offer a free background-worker service. Deploying the Connector
unchanged would therefore require a paid worker or an always-on VM.

The proposed HTTP ingestion endpoint makes every sensor measurement an incoming
request. It eliminates the dedicated Connector and allows scenario evaluation to
happen as part of the same event flow.

## Expected Free-Tier Fit

The soil firmware currently publishes once every 20 seconds:

```text
3 readings/minute
4,320 readings/day
approximately 129,600 readings per 30-day month
```

This is within the relevant prototype quotas:

- EMQX Serverless includes 1 million rule-action executions, 1 million session
  minutes, and 1 GB of traffic per month. One continuously connected sensor uses
  about 43,200 session minutes per 30-day month, and 129,600 webhook actions are
  below the action quota.
- Supabase Free includes a 500 MB PostgreSQL database and 5 GB egress. An active
  sensor prevents the project from being considered inactive, but reading
  retention is required before the database approaches 500 MB.
- Render includes 750 free instance-hours per workspace per calendar month.
  Incoming sensor webhooks should keep the single API service awake while the
  sensor is online.
- Cloudflare Pages Free is sufficient for this static Svelte application.

These plans provide no production SLA. Limits and plan terms can change, so
re-check them immediately before deployment:

- [EMQX Cloud pricing](https://docs.emqx.com/en/cloud/latest/price/pricing.html)
- [EMQX Serverless plan and integrations](https://docs.emqx.com/en/cloud/latest/price/plans.html)
- [Supabase pricing](https://supabase.com/pricing)
- [Render free services](https://render.com/docs/free)
- [Cloudflare Pages limits](https://developers.cloudflare.com/pages/platform/limits/)

### Free-tier behavior to accept for the prototype

- If the sensor is offline for more than 15 minutes, Render can suspend the API.
  The first webhook after recovery can encounter a cold start of about one
  minute. Configure EMQX webhook retries and verify this behavior end to end.
- Render can restart a free service. No required state may live on its local
  filesystem.
- Supabase Free has no automatic backups. Export the database periodically.
- EMQX Serverless can pause after an extended period without connected clients.
- ntfy delivery and iOS push delivery are external best-effort dependencies.

## Required Application Preparation

### 1. Upgrade unsupported runtimes

The API currently targets .NET 7 and its Dockerfile uses .NET 7 images. The UI
Dockerfile uses Node 14. Both versions are end-of-life.

Before public deployment:

- move API, Data, Logic, Core, Connector (while retained), and test projects to a
  currently supported .NET LTS version;
- update the SDK and ASP.NET runtime images in
  `smart-home-api/SmartHome.Api/Dockerfile`;
- update the frontend build runtime to a supported Node.js LTS version;
- rebuild and run focused backend and frontend tests after the upgrade.

References:

- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [Node.js supported releases](https://nodejs.org/en/about/previous-releases)

### 2. Remove secrets from source and rotate them

Do this before making the repository public or connecting it to a hosted build:

- rotate the Wi-Fi password currently present in
  `sensors/soil-moisture-v2/include/config.h`;
- rotate local PostgreSQL, RabbitMQ, and JWT secrets that appear in tracked
  development configuration;
- keep the real sensor configuration in an ignored local header derived from a
  committed example file;
- use provider environment variables for database, JWT, webhook, and ntfy
  secrets;
- never expose the ntfy topic to the frontend;
- never log the ntfy topic, database password, MQTT password, or ingestion key.

The random ntfy topic is a capability secret for this prototype: anyone who
knows it can publish or subscribe. The current iPhone subscription can remain,
but the topic must be stored only as a Render secret.

### 3. Add an authenticated ingestion endpoint

Add an API endpoint such as:

```http
POST /api/ingest/mqtt
Authorization: Bearer <INGEST_API_KEY>
Content-Type: application/json
```

Keep the firmware's existing payload contract:

```json
{
  "Id": 4,
  "Name": "%",
  "Value": 28.4,
  "Time": 1788345600
}
```

The endpoint must:

1. Compare the bearer value with a server-side secret using a constant-time
   comparison.
2. Reject missing or invalid sensor/chart-data IDs.
3. Reject non-finite or unreasonable values and malformed timestamps.
4. Convert a valid Unix timestamp to UTC. For cloud ingestion, reject a missing
   or implausible timestamp so retries can be deduplicated reliably; update the
   firmware to wait for NTP before its first publish.
5. Deduplicate retries before inserting a Point.
6. Store the Point using the scoped `SmartHomeDbContext`.
7. Evaluate only scenarios associated with the affected sensor.
8. Broadcast the saved Point with `IHubContext<SensorsHub>`.
9. Return `2xx` only after the reading is durably accepted.

Do not reuse the user JWT fallback value for device ingestion. Use a separate,
long random `INGEST_API_KEY` and support key rotation.

### 4. Make webhook delivery idempotent

EMQX must retry temporary API failures and Render cold starts. A retry must not
create a second database point or a second notification.

The current sensor payload has no explicit event identifier. For the prototype,
use `(DataId, Time)` as the idempotency key and add a unique database constraint.
For a stronger design, add a boot identifier and monotonically increasing
sequence number to the firmware payload.

Expected responses:

| Situation | Response |
| --- | --- |
| New valid reading stored | `202 Accepted` or `200 OK` |
| Valid duplicate already stored | `200 OK` |
| Invalid payload or unknown data ID | `400 Bad Request` |
| Invalid ingestion secret | `401 Unauthorized` |
| Temporary database or application failure | `5xx` so EMQX can retry |

### 5. Move scenario evaluation into the ingestion flow

Extract the scenario transition logic from `ScenarioWorker` into a scoped
service that can evaluate scenarios for one sensor and one newly stored Point.

Preserve the implemented behavior:

- evaluate only the latest fresh reading;
- trigger only on the safe-to-matching transition;
- keep `IsConditionActive` persisted across restarts;
- use hysteresis before re-arming;
- set `LastTriggeredAt` only after successful action delivery;
- do not repeat notifications while the moisture remains low.

During migration, do not run both event-driven evaluation and the polling worker
without coordination. That creates duplicate action races. Add an explicit
`Scenarios:EvaluationMode` setting with `Polling` and `Ingestion` values. Register
`ScenarioWorker` only in Polling mode, and run ingestion evaluation only in
Ingestion mode. Use Ingestion mode in Render; retain Polling mode locally only as
a temporary rollback path.

The existing prototype still accepts one possible duplicate when ntfy accepts a
request but its response is lost. Full exactly-once external delivery would need
an outbox and delivery identifier; that is optional for this prototype.

### 6. Replace Connector-to-SignalR forwarding

The Connector currently calls the SignalR hub as a client after saving a Point.
After the API owns ingestion, inject `IHubContext<SensorsHub>` and broadcast the
saved DTO directly from the API process.

After this path is verified:

- do not deploy `SmartHome.Connector`;
- do not deploy RabbitMQ;
- keep the Connector source temporarily for rollback until the cloud path is
  proven;
- remove obsolete RabbitMQ cloud configuration from the deployment secrets.

### 7. Add API health and readiness endpoints

Add endpoints suitable for Render monitoring:

```text
GET /health/live   process is running
GET /health/ready  process can reach PostgreSQL and startup migrations succeeded
```

Health responses must not reveal configuration, credentials, or provider URLs.

### 8. Configure frontend URLs for production

The current frontend contains two localhost assumptions:

- `VITE_BASE_URL=http://localhost:5200/` in the UI Dockerfile;
- `http://localhost:5200/hub` in `Chart.svelte`.

Replace them with build-time environment variables, for example:

```text
VITE_API_BASE_URL=https://<render-service>.onrender.com/
VITE_SIGNALR_URL=https://<render-service>.onrender.com/hub
```

Configure these values in Cloudflare Pages. Do not put server secrets in any
`VITE_*` variable; Vite embeds them into the public JavaScript bundle.

Add Svelte SPA fallback routing so refreshing a client route serves
`index.html` rather than returning 404.

### 9. Restrict CORS

`Startup.cs` currently allows every origin, method, and header. Replace that
policy with the exact Cloudflare Pages production origin and any explicitly
required preview origin.

Keep local development origins in development-only configuration. Verify both
normal API calls and the SignalR WebSocket handshake after tightening CORS.

### 10. Add database retention and export

At the current 20-second interval, raw history grows by about 129,600 rows per
month. The exact storage consumption depends on row and index sizes, so measure
it after deployment.

For the prototype:

- retain detailed readings for a defined period, initially 90 days;
- optionally aggregate older data to hourly or daily minimum, maximum, and
  average values;
- export schema and data periodically because Supabase Free does not include
  automatic backups;
- alert or review usage before reaching the 500 MB database limit.

Do not add retention deletion until the retention period and backup procedure
have been explicitly approved.

## Provider Configuration

### Supabase PostgreSQL

1. Create a Free project in a region reasonably close to the Render service.
2. Generate and store a strong database password.
3. Copy the Npgsql-compatible connection string from the Supabase dashboard.
   Prefer the provider-recommended pooler connection for a hosted web service.
4. Require TLS in the connection string.
5. Set the Render secret:

   ```text
   ConnectionStrings__DefaultConnection=<supabase connection string>
   ```

6. Allow the API startup migration to run once, then inspect the migration logs
   and schema.
7. Do not expose the database connection string to Cloudflare Pages or EMQX.

### Render API

Create one Web Service from the repository:

```text
Root/build context: smart-home-api
Dockerfile: SmartHome.Api/Dockerfile
Health check: /health/ready
```

Set secrets in Render, not in tracked files:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<supabase connection string>
Jwt__Key=<long random JWT signing key>
Notifications__Ntfy__Enabled=true
Notifications__Ntfy__BaseUrl=https://ntfy.sh
Notifications__Ntfy__Topic=<existing random topic subscribed on iPhone>
Ingestion__ApiKey=<long random webhook key>
Scenarios__EvaluationMode=Ingestion
AllowedOrigins__0=https://<cloudflare-pages-hostname>
```

Confirm the API listens on `0.0.0.0` and the port expected by Render. Do not
depend on local disk persistence.

### EMQX Serverless

1. Create a Serverless deployment near the API region.
2. Create a dedicated MQTT username and strong password for the soil sensor.
3. Use the TLS MQTT endpoint and port supplied by EMQX.
4. Create an HTTP Server connector targeting:

   ```text
   https://<render-service>.onrender.com/api/ingest/mqtt
   ```

5. Add the ingestion authorization header without exposing it in logs:

   ```text
   Authorization: Bearer <INGEST_API_KEY>
   ```

6. Add a rule for the exact sensor topic and map the incoming JSON fields
   without changing their names or numeric types.
7. Configure bounded retries for timeouts and `5xx` responses. Do not retry
   permanent `400` or `401` failures indefinitely.
8. Test with a manually published payload before changing the ESP8266.

EMQX Serverless supports HTTP data integration but not a direct PostgreSQL
connector. The HTTP API path is intentional because it keeps validation,
authorization, scenario transitions, SignalR, and notification behavior in one
application boundary.

### Cloudflare Pages

Create a Pages project for `smart-home-ui`:

```text
Root directory: smart-home-ui
Build command: npm ci && npm run build
Output directory: dist
```

Set only public build variables:

```text
VITE_API_BASE_URL=https://<render-service>.onrender.com/
VITE_SIGNALR_URL=https://<render-service>.onrender.com/hub
```

After the first deployment, copy the final Pages origin into the Render CORS
configuration and redeploy the API if necessary.

### ESP8266 firmware

Change the soil sensor to the EMQX values supplied by its dashboard:

```text
MQTT_HOST=<emqx hostname>
MQTT_PORT=<emqx TLS port>
MQTT_USERNAME=<sensor username>
MQTT_PASSWORD=<sensor password>
MQTT_TOPIC=<configured EMQX rule topic>
```

The current firmware uses `WiFiClient` and plaintext MQTT. Replace it with
`WiFiClientSecure`, validate the broker certificate with an appropriate CA
certificate, and confirm NTP time before the TLS handshake.

Keep `SOIL_DATA_ID=4` only if production database data confirms that chart-data
ID `4` belongs to the intended soil-moisture sensor. Do not assume local IDs are
identical after creating a fresh database.

## Deployment Order

Use this order so every new component has a testable downstream target:

1. Rotate and externalize all existing secrets.
2. Upgrade supported .NET and Node runtimes.
3. Implement and test authenticated, idempotent API ingestion.
4. Extract event-driven scenario evaluation and direct SignalR broadcasting.
5. Add health checks, production URL configuration, and restricted CORS.
6. Create Supabase and deploy the API to Render with ntfy initially disabled.
7. Verify schema migration, API health, authentication, and database CRUD.
8. Enable ntfy and send a direct API-side test to the subscribed iPhone topic.
9. Create EMQX, configure its HTTPS connector/rule, and publish a manual test
   reading.
10. Confirm one Point is stored and one SignalR update appears for that event.
11. Deploy the Svelte UI to Cloudflare Pages and verify API and SignalR access.
12. Update the ESP8266 to MQTT over TLS and flash it manually.
13. Verify continuous sensor ingestion with Render in Ingestion mode; confirm
    the polling worker is not registered and the Connector/RabbitMQ services are
    not deployed.
14. Create a moisture automation and verify trigger, no-repeat, recovery, and a
    second trigger.
15. Document database export and rollback procedures before treating the
    prototype as unattended.

## Verification Checklist

### API and database

- [ ] `/health/live` returns success.
- [ ] `/health/ready` confirms PostgreSQL connectivity.
- [ ] EF migrations completed against Supabase.
- [ ] An invalid ingestion secret returns `401`.
- [ ] An invalid payload returns `400` without inserting a Point.
- [ ] Reposting the same `(DataId, Time)` returns success but inserts only one
      Point.
- [ ] No secret appears in Render logs.

### MQTT and ingestion

- [ ] ESP8266 connects to EMQX using TLS and certificate validation.
- [ ] EMQX receives the expected topic and JSON payload.
- [ ] The HTTP rule delivers a reading to Render.
- [ ] A Render cold-start test eventually stores one reading without duplicates.
- [ ] Retry behavior is visible in provider metrics but does not duplicate data.

### UI and SignalR

- [ ] Cloudflare Pages loads over HTTPS.
- [ ] Login and authenticated API operations use the Render URL.
- [ ] Browser developer tools show no CORS errors.
- [ ] SignalR connects with `wss://` and receives a new reading.
- [ ] Refreshing a nested UI route does not return 404.

### Moisture notification

- [ ] The iPhone remains subscribed to the configured random ntfy topic.
- [ ] A fresh value below the threshold triggers one notification.
- [ ] Additional low values do not trigger repeated notifications.
- [ ] The UI status `Triggered, waiting for recovery` appears while the
      condition is active; this is the expected latched state.
- [ ] A value at or above `threshold + hysteresis` re-arms the scenario.
- [ ] A second downward crossing sends a second notification.
- [ ] An ntfy failure does not mark the scenario as successfully triggered.

### Free-tier monitoring

- [ ] Render instance-hours and cold starts are reviewed.
- [ ] EMQX session minutes, traffic, rule actions, and failed deliveries are
      reviewed.
- [ ] Supabase database size and egress are reviewed.
- [ ] A database export has been downloaded and restore-tested.

## Rollback

Do not delete the existing local Docker Compose path during prototype rollout.
If the separated deployment fails:

1. Point the ESP8266 back to the local RabbitMQ MQTT broker.
2. Start the existing PostgreSQL, RabbitMQ, API, Connector, and UI Compose
   services locally.
3. Disable or pause the EMQX HTTP rule to prevent duplicate ingestion.
4. Keep the cloud database intact for investigation and export.
5. Do not run cloud and local notification workers against the same active
   scenario data unless duplicate-action protection has been verified.

## Alternatives

### Oracle Always Free VM

Use [`DEPLOYMENT.md`](../DEPLOYMENT.md) when the priority is minimum code change.
It can host the current Compose stack, but it requires VM patching, firewalling,
backups, Docker maintenance, and MQTT TLS configuration.

### Cloudflare-native serverless rewrite

A later version could replace the ASP.NET API and PostgreSQL with Cloudflare
Workers and D1. EMQX would invoke a Worker, which stores the reading, evaluates
the scenario, and calls ntfy. This removes all continuously running application
processes, but it is a significant rewrite of authentication, Entity Framework,
SignalR, and the current API contracts. It is not the recommended first cloud
prototype.

## Definition of Ready to Deploy

The separate-services prototype is ready for deployment only when:

- no production credential remains in tracked source;
- supported runtimes build and the focused test suites pass;
- the ingestion endpoint is authenticated and idempotent;
- scenario evaluation is event-driven and cannot race the polling worker;
- the API broadcasts points without `SmartHome.Connector`;
- production API and SignalR URLs are configurable;
- CORS permits only intended origins;
- MQTT uses TLS with broker certificate validation;
- Supabase export and retention procedures are documented;
- cold-start, retry, notification latch, recovery, and duplicate-event behavior
  have been verified end to end.
