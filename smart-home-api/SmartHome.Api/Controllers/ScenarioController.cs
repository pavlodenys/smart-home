using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

            var existing = _scenarioRepo.GetById(dto.Id);
            if (existing == null)
            {
                return NotFound();
            }

            dto.IsConditionActive = existing.IsConditionActive;
            dto.LastTriggeredAt = existing.LastTriggeredAt;
            var saveResult = await _scenarioRepo.Update(dto.Id, dto);

            return Ok(saveResult);
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
