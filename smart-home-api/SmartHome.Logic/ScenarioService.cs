using SmartHome.Data.DTO;
using SmartHome.Data.Entities;

namespace SmartHome.Logic
{
    public class ScenarioService
    {
        private readonly IRepository<Scenario, ScenarioDto> _scenarioRepo;

        public ScenarioService(IRepository<Scenario, ScenarioDto> scenarioRepo)
        {
            _scenarioRepo = scenarioRepo;
        }

        public IEnumerable<ScenarioDto> GetScenarios()
        {
            return _scenarioRepo.GetAll();
        }
    }
}
