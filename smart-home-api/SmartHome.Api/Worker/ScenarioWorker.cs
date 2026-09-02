using Microsoft.EntityFrameworkCore;
using SmartHome.Api.Notifications;
using SmartHome.Core.Enums;
using SmartHome.Data;
using SmartHome.Data.Entities;
using SmartHome.Logic;
using System.Globalization;

namespace SmartHome.Api.Worker
{
    public sealed class ScenarioWorker : BackgroundService
    {
        private static readonly TimeSpan EvaluationInterval = TimeSpan.FromSeconds(20);
        private static readonly TimeSpan MaximumReadingAge = TimeSpan.FromMinutes(2);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ScenarioWorker> _logger;

        public ScenarioWorker(IServiceScopeFactory scopeFactory, ILogger<ScenarioWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await EvaluateScenarios(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Scenario evaluation cycle failed.");
                }

                await Task.Delay(EvaluationInterval, stoppingToken);
            }
        }

        private async Task EvaluateScenarios(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartHomeDbContext>();
            var notificationSender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

            var scenarios = await dbContext.Scenarios
                .Where(scenario => !scenario.IsDeleted)
                .Include(scenario => scenario.Sensors!)
                    .ThenInclude(link => link.Sensor)
                .Include(scenario => scenario.Devices!)
                    .ThenInclude(link => link.Device)
                .ToListAsync(cancellationToken);

            foreach (var scenario in scenarios)
            {
                try
                {
                    await EvaluateScenario(dbContext, notificationSender, scenario, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Scenario {ScenarioId} evaluation failed.", scenario.Id);
                }
            }
        }

        private async Task EvaluateScenario(
            SmartHomeDbContext dbContext,
            INotificationSender notificationSender,
            Scenario scenario,
            CancellationToken cancellationToken)
        {
            if (scenario.Sensors == null || scenario.Sensors.Count != 1)
            {
                _logger.LogWarning("Scenario {ScenarioId} does not have exactly one sensor.", scenario.Id);
                return;
            }

            var sensorLink = scenario.Sensors.First();

            var latestPoint = await dbContext.Set<Point>()
                .AsNoTracking()
                .Where(point => point.Data.SensorId == sensorLink.SensorId)
                .OrderByDescending(point => point.DateTime)
                .ThenByDescending(point => point.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestPoint == null || DateTime.UtcNow - latestPoint.DateTime > MaximumReadingAge)
            {
                return;
            }

            var matches = ScenarioConditionEvaluator.IsMatch(
                scenario.Operator,
                latestPoint.Value,
                scenario.Threshold);

            if (scenario.IsConditionActive)
            {
                if (ScenarioConditionEvaluator.ShouldRearm(
                    scenario.Operator,
                    latestPoint.Value,
                    scenario.Threshold,
                    scenario.Hysteresis))
                {
                    scenario.IsConditionActive = false;
                    await dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Scenario {ScenarioId} re-armed.", scenario.Id);
                }

                return;
            }

            if (!matches)
            {
                return;
            }

            try
            {
                await ExecuteAction(notificationSender, scenario, sensorLink.Sensor, latestPoint, cancellationToken);
            }
            catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(
                    "Scenario {ScenarioId} notification delivery failed ({FailureType}).",
                    scenario.Id,
                    exception.GetType().Name);
                return;
            }

            scenario.IsConditionActive = true;
            scenario.LastTriggeredAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Scenario {ScenarioId} triggered.", scenario.Id);
        }

        private static async Task ExecuteAction(
            INotificationSender notificationSender,
            Scenario scenario,
            Sensor sensor,
            Point latestPoint,
            CancellationToken cancellationToken)
        {
            if (scenario.ActionType == ScenarioActionType.Notification)
            {
                var value = latestPoint.Value.ToString("0.0", CultureInfo.InvariantCulture);
                var threshold = scenario.Threshold.ToString("0.0", CultureInfo.InvariantCulture);
                var message = (scenario.Command ?? "Sensor threshold reached")
                    .Replace("{value}", value, StringComparison.Ordinal)
                    .Replace("{threshold}", threshold, StringComparison.Ordinal);

                await notificationSender.SendAsync($"Smart Home: {sensor.Name}", message, cancellationToken);
                return;
            }

            if (scenario.Devices == null || scenario.Devices.Count != 1)
            {
                throw new InvalidOperationException($"Device scenario {scenario.Id} does not have exactly one device.");
            }

            var device = scenario.Devices.First().Device;
            device.IsActive = !device.IsActive;
        }
    }
}
