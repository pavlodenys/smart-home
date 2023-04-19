using SmartHome.Api.Utilities;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using SmartHome.Logic;

namespace SmartHome.Api.Worker
{
    public class ScenarioProducer : BackgroundService
    {
        private readonly ScenariosQueue _queue;
        private readonly ScenarioService _scenarioService;

        public ScenarioProducer(ScenariosQueue queue, ScenarioService scenarioService)
        {
            _queue = queue;
            _scenarioService = scenarioService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                
                var scenarios = GetScenarios();

                foreach(var scenario in scenarios)
                {
                    _queue.Enqueue(scenario);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Delay for 1 minute
            }
        }

        private IEnumerable<ScenarioDto> GetScenarios()
        {
            return _scenarioService.GetScenarios();
        }
    }
}
