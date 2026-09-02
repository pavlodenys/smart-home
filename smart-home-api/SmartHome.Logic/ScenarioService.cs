using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using Microsoft.EntityFrameworkCore;

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
            return await _scenarioRepo._dbContext.Scenarios
                .AsNoTracking()
                .Where(scenario => !scenario.IsDeleted)
                .Select(scenario => new ScenarioDto
                {
                    Id = scenario.Id,
                    Threshold = scenario.Threshold,
                    Hysteresis = scenario.Hysteresis,
                    Operator = scenario.Operator,
                    ActionType = scenario.ActionType,
                    Command = scenario.Command,
                    IsConditionActive = scenario.IsConditionActive,
                    LastTriggeredAt = scenario.LastTriggeredAt,
                    IsDeleted = scenario.IsDeleted,
                    Sensors = scenario.Sensors!.Select(link => new ScenarioSensorDto
                        {
                            Id = link.Id,
                            ScenarioId = link.ScenarioId,
                            SensorId = link.SensorId,
                            Sensor = new SensorDto
                            {
                                Id = link.Sensor.Id,
                                Name = link.Sensor.Name,
                                Description = link.Sensor.Description,
                                Type = link.Sensor.Type
                            }
                        }).ToList(),
                    Devices = scenario.Devices!.Select(link => new ScenarioDeviceDto
                        {
                            Id = link.Id,
                            ScenarioId = link.ScenarioId,
                            DeviceId = link.DeviceId,
                            Device = new DeviceDto
                            {
                                Id = link.Device.Id,
                                Name = link.Device.Name,
                                Description = link.Device.Description,
                                IsActive = link.Device.IsActive
                            }
                        }).ToList()
                })
                .ToListAsync();
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
