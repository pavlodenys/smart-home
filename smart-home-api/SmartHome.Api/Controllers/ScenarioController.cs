using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using SmartHome.Logic;
using SmartHome.Core.Enums;

namespace SmartHome.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ScenarioController : ControllerBase
    {
        private IRepository<Scenario, ScenarioDto> _scenarioRepo { get; set; }
        private ScenarioService _service { get; set; }

        public ScenarioController(IRepository<Scenario, ScenarioDto> scenarioRepo, ScenarioService service)
        {
            _scenarioRepo = scenarioRepo;
            _service = service;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetScenarious()
        {
            var scenarious = await _service.GetScenarios();
            return base.Ok(scenarious);
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetScenario(int id)
        {
            return Ok(_scenarioRepo.GetById(id));
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SaveScenario([FromBody] ScenarioDto dto)
        {
            var validationError = ValidateScenario(dto);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            dto.Id = 0;
            dto.IsDeleted = false;
            dto.IsConditionActive = false;
            dto.LastTriggeredAt = null;
            var saveResult = await _scenarioRepo.Create(dto);

            return Ok(saveResult);
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> UpdateScenario([FromBody] ScenarioDto dto)
        {
            var validationError = ValidateScenario(dto);
            if (validationError != null)
            {
                return BadRequest(validationError);
            }

            var database = _scenarioRepo._dbContext;
            var existing = await database.Scenarios
                .Include(scenario => scenario.Sensors)
                .Include(scenario => scenario.Devices)
                .SingleOrDefaultAsync(scenario => scenario.Id == dto.Id && !scenario.IsDeleted);
            if (existing == null)
            {
                return NotFound();
            }

            var sensorId = dto.Sensors!.Single().SensorId;
            if (!await database.Sensors.AnyAsync(sensor => sensor.Id == sensorId))
            {
                return BadRequest("The selected sensor does not exist.");
            }

            existing.Threshold = dto.Threshold;
            existing.Hysteresis = dto.Hysteresis;
            existing.Operator = dto.Operator;
            existing.ActionType = dto.ActionType;
            existing.Command = dto.Command?.Trim();

            var sensorLinks = existing.Sensors?.ToList() ?? new List<ScenarioSensor>();
            if (sensorLinks.Count == 0)
            {
                database.ScenarioSensors.Add(new ScenarioSensor
                {
                    ScenarioId = existing.Id,
                    SensorId = sensorId
                });
            }
            else
            {
                sensorLinks[0].SensorId = sensorId;
                database.ScenarioSensors.RemoveRange(sensorLinks.Skip(1));
            }

            var deviceLinks = existing.Devices?.ToList() ?? new List<ScenarioDevice>();
            if (dto.ActionType == ScenarioActionType.Notification)
            {
                database.ScenarioDevices.RemoveRange(deviceLinks);
            }
            else
            {
                var deviceId = dto.Devices!.Single().DeviceId;
                if (!await database.Devices.AnyAsync(device => device.Id == deviceId))
                {
                    return BadRequest("The selected device does not exist.");
                }

                if (deviceLinks.Count == 0)
                {
                    database.ScenarioDevices.Add(new ScenarioDevice
                    {
                        ScenarioId = existing.Id,
                        DeviceId = deviceId
                    });
                }
                else
                {
                    deviceLinks[0].DeviceId = deviceId;
                    database.ScenarioDevices.RemoveRange(deviceLinks.Skip(1));
                }
            }

            await database.SaveChangesAsync();

            return Ok(dto);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteScenario(int id)
        {
            var deleteResult = await _service.DeleteScenario(id);
            return Ok(deleteResult);
        }

        private static string? ValidateScenario(ScenarioDto dto)
        {
            if (!Enum.IsDefined(typeof(ComparisonOperator), dto.Operator)
                || !Enum.IsDefined(typeof(ScenarioActionType), dto.ActionType))
            {
                return "The scenario operator or action type is invalid.";
            }

            if (!double.IsFinite(dto.Threshold) || !double.IsFinite(dto.Hysteresis) || dto.Hysteresis < 0)
            {
                return "Threshold must be finite and hysteresis must be a finite non-negative number.";
            }

            if (dto.Sensors == null || dto.Sensors.Count != 1 || dto.Sensors.Any(sensor => sensor.SensorId <= 0))
            {
                return "A scenario must contain exactly one valid sensor.";
            }

            if (dto.ActionType == ScenarioActionType.Notification)
            {
                if (string.IsNullOrWhiteSpace(dto.Command))
                {
                    return "A notification scenario requires a message.";
                }

                if (dto.Command.Length > 250)
                {
                    return "The notification message cannot exceed 250 characters.";
                }

                if (dto.Devices?.Count > 0)
                {
                    return "A notification scenario cannot contain a device.";
                }

                return null;
            }

            if (dto.Devices == null || dto.Devices.Count != 1 || dto.Devices.Any(device => device.DeviceId <= 0))
            {
                return "A device scenario must contain exactly one valid device.";
            }

            return null;
        }
    }
}
