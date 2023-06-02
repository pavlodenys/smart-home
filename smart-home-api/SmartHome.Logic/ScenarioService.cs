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

        public async Task<IEnumerable<ScenarioDto>> GetScenarios()
        {
            return await _scenarioRepo.GetAll();
        }

        //todo: encapsulate delete logic
        public async Task<int> DeleteScenario(int id)
        {
            var deleteResult = _scenarioRepo.GetById(id);
            if (deleteResult == null) return 0;

            deleteResult.IsDeleted = true;

            await _scenarioRepo._dbContext.SaveChangesAsync();

            return deleteResult.Id;
        }
    }
}
