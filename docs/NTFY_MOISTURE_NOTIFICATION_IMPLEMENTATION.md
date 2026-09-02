# ntfy Moisture Notification Automation

## Goal

Reuse the existing Scenario/Automation feature to send one notification to an
iPhone when the newest soil-moisture reading drops below a configured threshold.

The prototype uses the hosted `ntfy.sh` service and the ntfy iOS application. It
does not implement Apple Push Notification service (APNs), Web Push, multiple
notification recipients, or a self-hosted ntfy server.

Example automation:

> When Soil Moisture is below 30%, send "Plant moisture is low: 28%".

## Definition of done

- A user can create a Notification automation from the existing Scenario UI.
- The automation stores one sensor, one comparison operator, one numeric
  threshold, an action type, and a notification message.
- Only the newest non-stale sensor reading is evaluated.
- A notification is sent once when the condition changes from safe to low.
- No additional notifications are sent while moisture remains low.
- The automation re-arms after moisture rises above the threshold plus a small
  hysteresis margin.
- Trigger state survives API/container restarts.
- ntfy configuration is supplied through environment variables and is never
  committed.
- Unit tests cover condition evaluation, transition behavior, stale readings,
  and ntfy delivery failures.

## Why the current automation cannot be used unchanged

The existing model and worker provide a useful starting point, but four issues
must be corrected first:

1. `Scenario.svelte` posts the threshold as `value`, while
   `ScenarioConsumer.cs` evaluates `scenario.SensorValue`. A value entered in
   the UI is therefore not the value used by the worker.
2. A scenario currently requires a Device and its only action is toggling that
   device. Notification actions must not require a device.
3. Every scenario is queued every minute. If a low-moisture condition remains
   true, a naive notification action would notify every minute.
4. The background workers use singleton services that retain transient
   repositories and a `SmartHomeDbContext`. The database work must instead run
   inside a fresh dependency-injection scope for each evaluation cycle.

There are currently no Scenario rows in the local live database, so the
prototype does not need to migrate existing automation records.

## Chosen notification path

### iPhone setup

1. Install **ntfy** from the iOS App Store.
2. Generate a long, random topic name. Do not use a topic such as `plant` or
   `smart-home`, because unreserved `ntfy.sh` topics are public.

   ```powershell
   $topic = "smart-home-$([guid]::NewGuid().ToString('N'))"
   $topic
   ```

3. In the ntfy app, subscribe to:

   ```text
   https://ntfy.sh/<generated-topic>
   ```

4. Verify delivery directly before changing the application:

   ```powershell
   $topic = '<generated-topic>'
   Invoke-RestMethod `
     -Method Post `
     -Uri "https://ntfy.sh/$topic" `
     -Headers @{ Title = 'Smart Home'; Tags = 'droplet' } `
     -ContentType 'text/plain' `
     -Body 'ntfy test notification'
   ```

The random topic name acts as the prototype secret. Anyone who learns it can
publish to or subscribe to the topic. Do not put personal information or access
tokens in notification messages. Authenticated ntfy or another provider should
replace this before supporting multiple users.

## Configuration

Add these values to the API configuration model:

```text
Notifications:Ntfy:BaseUrl
Notifications:Ntfy:Topic
Notifications:Ntfy:Enabled
```

Use environment variables in Docker:

```yaml
smart-home-api:
  environment:
    Notifications__Ntfy__Enabled: "true"
    Notifications__Ntfy__BaseUrl: https://ntfy.sh
    Notifications__Ntfy__Topic: ${NTFY_TOPIC}
```

Store `NTFY_TOPIC` in the root `.env` file:

```dotenv
NTFY_ENABLED=true
NTFY_TOPIC=smart-home-<generated-random-value>
```

As part of implementation, add `.env` to `.gitignore` before creating the file.
Do not add a default topic to `appsettings.json`, Dockerfiles, source code, or
tests.

The API should start when notification delivery is disabled. When it is enabled,
startup should fail with a clear configuration error if `BaseUrl` or `Topic` is
missing.

## Data contract

### New action type

Add an enum shared by the entity and DTO:

```csharp
public enum ScenarioActionType
{
    Device = 0,
    Notification = 1
}
```

### Scenario fields

Replace the ambiguous `SensorValue` and `Value` pair with a single `double`
threshold and add persisted trigger state:

```csharp
public double Threshold { get; set; }
public double Hysteresis { get; set; } = 2;
public ScenarioActionType ActionType { get; set; }
public string? Command { get; set; }
public bool IsConditionActive { get; set; }
public DateTime? LastTriggeredAt { get; set; }
```

For the prototype, `Command` is the notification message template. A Device
scenario continues using its existing Device relationship. A Notification
scenario has no Device relationship.

Create a new EF Core migration rather than editing
`20260818103649_InitialPostgreSql`:

```powershell
dotnet ef migrations add AddNotificationScenarioAction `
  --project smart-home-api/SmartHome.Data `
  --startup-project smart-home-api/SmartHome.Api
```

The generated migration should:

- drop `SensorValue`;
- rename `Value` to `Threshold` and change it to `double precision`;
- add `Hysteresis`, `ActionType`, `IsConditionActive`, and `LastTriggeredAt`.

Review the generated migration before applying it. API startup already applies
pending migrations.

### API request

The Scenario POST/PUT contract for an ntfy notification becomes:

```json
{
  "sensors": [{ "sensorId": 4 }],
  "devices": [],
  "threshold": 30,
  "hysteresis": 2,
  "operator": 1,
  "actionType": 1,
  "command": "Plant moisture is low"
}
```

`operator: 1` is the existing `LessThan` operator. Server validation must reject:

- a Notification action with no sensor;
- a Device action with no device;
- a blank notification command;
- a negative hysteresis value;
- multiple sensors or multiple devices in this prototype.

## Runtime design

### Keep the Scenario feature, simplify its executor

Keep the Scenario entity, DTO, controller, UI, comparison enum, and sensor/device
relationships. Replace the producer/queue/consumer chain with one
`ScenarioWorker : BackgroundService`.

The current queue does not provide durability or parallelism. A single worker is
shorter, awaits every operation, and can create a fresh service scope on every
cycle.

Register it as:

```csharp
services.AddHostedService<ScenarioWorker>();
```

`ScenarioWorker` should depend only on `IServiceScopeFactory`, an
`ILogger<ScenarioWorker>`, and `TimeProvider` (or a small clock abstraction for
.NET 7 tests). Repositories, evaluator, database context, and notification sender
are resolved from a scope created inside each cycle.

Recommended initial interval: 20 seconds. This matches the current approximate
soil-sensor publishing interval while remaining simple. Each reading must be no
older than two minutes. Use UTC for both stored points and comparisons.

### Evaluation algorithm

For each non-deleted scenario:

1. Load the associated sensor and only its newest Point, ordered by
   `DateTime DESC`, then `Id DESC`.
2. Skip the scenario when there is no reading or the newest reading is more than
   two minutes old.
3. Evaluate the latest value against `Threshold`.
4. If the condition is true and `IsConditionActive` is false:
   - execute the configured action;
   - after a successful action, set `IsConditionActive = true` and
     `LastTriggeredAt = UtcNow`;
   - save the state in the same worker cycle.
5. If the condition is active, do not execute the action again.
6. Re-arm a `LessThan` automation only when:

   ```text
   latest value >= Threshold + Hysteresis
   ```

   For a `GreaterThan` automation, re-arm when:

   ```text
   latest value <= Threshold - Hysteresis
   ```

   Other operators can re-arm when their comparison becomes false.

For a threshold of 30 and hysteresis of 2, a value of 29 triggers once. Values
29, 28, and 30 do not trigger again. A value of 32 re-arms the automation, and a
later value below 30 can produce a new notification.

Do not mark the condition active when ntfy returns a non-success response. Log
the status without logging the topic and retry on a later worker cycle. A request
that reached ntfy but whose response was lost can produce a duplicate; that is an
accepted prototype limitation.

## ntfy sender

Add:

```text
smart-home-api/SmartHome.Api/Notifications/INotificationSender.cs
smart-home-api/SmartHome.Api/Notifications/NtfyNotificationSender.cs
smart-home-api/SmartHome.Api/Notifications/NtfyOptions.cs
```

Interface:

```csharp
public interface INotificationSender
{
    Task SendAsync(string title, string message, CancellationToken cancellationToken);
}
```

Register the implementation with `AddHttpClient` and a short timeout:

```csharp
services.AddOptions<NtfyOptions>()
    .Bind(Configuration.GetSection("Notifications:Ntfy"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddHttpClient<INotificationSender, NtfyNotificationSender>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
```

`NtfyNotificationSender` sends:

```http
POST https://ntfy.sh/<topic>
Content-Type: text/plain; charset=utf-8
Title: Smart Home
Tags: droplet

Plant moisture is low: 28%
```

Build the destination URI from the validated base URL and escaped topic. Reject
topics containing `/`, `?`, or `#`. Call `EnsureSuccessStatusCode()` and let the
worker log provider failures.

The worker can expand two safe placeholders in `Command`:

```text
{value}      newest sensor value, rounded to one decimal place
{threshold}  configured threshold
```

Example:

```text
Plant moisture is {value}%, below {threshold}%
```

Do not introduce a general template engine for this prototype.

## UI changes

Update `Scenario.svelte`:

1. Add an Action selector with `Device` and `Notification`.
2. Show the Device selector only for Device actions.
3. For Notification actions, show:
   - Threshold, numeric input;
   - Hysteresis, numeric input defaulting to 2;
   - Message, text input;
   - a short note: "You will be notified once, then the automation re-arms after
     moisture recovers."
4. Send numeric values, not strings.
5. Post `threshold`, never the old `value` or `sensorValue` names.
6. On success, append the returned scenario to `scenarios` and close the modal.
7. Show API validation and network errors instead of silently assigning the
   response to `sensors`.

Update `ScenarioItem.svelte` so a notification reads like:

```text
If Soil Moisture is less than 30%, notify: Plant moisture is low
```

Do not display device state for Notification actions.

## Tests

### Backend unit tests

Add focused tests for:

- each comparison operator uses `Threshold`;
- only the newest Point is evaluated;
- stale readings do not trigger;
- safe to low sends exactly one notification;
- repeated low readings do not send again;
- a value below the re-arm margin keeps the scenario active;
- recovery to `Threshold + Hysteresis` re-arms a LessThan scenario;
- a second crossing after recovery sends a second notification;
- a failed ntfy response does not mark the scenario active;
- Notification actions do not require a Device;
- Device actions still require and toggle a Device;
- a cancelled worker exits promptly.

Use a fake `HttpMessageHandler` for ntfy tests. Tests must not call `ntfy.sh`.

### UI checks

Add tests verifying:

- selecting Notification hides the Device selector;
- the request uses `threshold` and numeric values;
- blank message and invalid hysteresis are rejected;
- an API error is shown;
- a saved Notification scenario renders correctly.

## Implementation order

1. Add `.env` protection and ntfy options/sender.
2. Add the Scenario action/threshold/state migration and DTO validation.
3. Extract pure condition and re-arm evaluation with unit tests.
4. Replace the producer/queue/consumer with the scoped Scenario worker.
5. Add Notification action execution and backend tests.
6. Update Scenario creation/display UI and UI tests.
7. Build and test with Docker, reusing dependency caches.
8. Configure the real random topic and send a direct ntfy test.
9. Create a test automation above the current moisture value, confirm one push,
   confirm no repeat, then lower the test threshold or delete the automation.

## Docker verification

Do not use `--no-cache` for code-only changes. Reuse the existing dependency
layers:

```powershell
docker compose build smart-home-api smart-home-ui
docker compose up -d --no-deps --force-recreate smart-home-api smart-home-ui
docker compose ps
docker compose logs --tail 200 smart-home-api smart-home-connector
```

Run the backend test project in an SDK container so the host does not need the
.NET SDK:

```powershell
docker run --rm `
  -v "${PWD}/smart-home-api:/src" `
  -w /src `
  mcr.microsoft.com/dotnet/sdk:7.0 `
  dotnet test SmartHome.Tests/SmartHome.Tests.csproj
```

Run UI checks in a Node container with the package-lock cacheable through the
project directory:

```powershell
docker run --rm `
  -v "${PWD}/smart-home-ui:/app" `
  -w /app `
  node:14 `
  sh -c "npm ci && npm run check && npm run build"
```

Final live proof requires all of the following:

- the soil sensor publishes a fresh reading;
- PostgreSQL contains that reading;
- the worker logs one successful transition without printing the topic;
- the iPhone receives one notification;
- no second notification arrives while the value remains below the threshold;
- after a recovery value and a second downward crossing, a second notification
  arrives.

## Prototype limitations and follow-up

- The topic is an unguessable shared secret, not user authentication.
- There is one notification destination for the whole deployment.
- A network timeout after ntfy accepts a request can cause one duplicate.
- Notification delivery depends on the hosted `ntfy.sh` service and iOS push
  delivery.
- The worker is polling PostgreSQL. A later version can evaluate immediately
  after the connector stores a Point, but that is unnecessary for the prototype.
- Before supporting multiple users, store per-user notification destinations,
  authorize scenario ownership, and use authenticated ntfy topics or another
  provider.
