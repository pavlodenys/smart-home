using SmartHome.Api.Utilities;
using SmartHome.Core.Enums;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using SmartHome.Logic;
using System.Diagnostics;
using System.Drawing;

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

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); // Delay for 1 second
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

