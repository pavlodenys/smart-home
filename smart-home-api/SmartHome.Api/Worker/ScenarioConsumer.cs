using SmartHome.Api.Utilities;
using SmartHome.Data.DTO;
using SmartHome.Logic;

namespace SmartHome.Api.Worker
{
    public class ScenarioConsumer : BackgroundService
    {
        private readonly ScenariosQueue _queue;
        private readonly Services _service;

        public ScenarioConsumer(ScenariosQueue queue, Services service)
        {
            _queue = queue;
            _service = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var scenario))
                {
                    // Process the scenario
                    ProcessScenario(scenario);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Delay for 10 second
            }
        }
        private void ProcessScenario(ScenarioDto? scenario)
        {
            if (scenario != null && scenario.Devices != null)
            {
                var scenarioDevice = scenario.Devices.FirstOrDefault();
                if (scenarioDevice != null && scenario.Sensors != null)
                {
                    foreach (var sensor in scenario.Sensors)
                    {
                        var charData = sensor.Sensor?.ChartData.ElementAt(0);
                        if (charData != null)
                        {
                            var freshSensorData = charData.Data?.Where(x => x.DateTime > DateTime.Now.AddMinutes(-1));
                            var result = _service.CheckScenario(scenarioDevice.Device, scenario.Operator, freshSensorData, scenario.SensorValue);
                        }
                    }
                }
            }
        }
    }
}

