using SmartHome.Api.Utilities;
using SmartHome.Core.Extensions;
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

                var scenarios = (await GetScenarios()).NotDeleted();

                foreach (var scenario in scenarios.ToList())
                {
                    _queue.Enqueue(scenario as ScenarioDto);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Delay for 1 minute
            }
        }

        private async Task<IEnumerable<ScenarioDto>> GetScenarios()
        {
            return await _scenarioService.GetScenarios();
        }
    }
}
