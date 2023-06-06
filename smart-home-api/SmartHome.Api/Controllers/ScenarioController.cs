using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using SmartHome.Logic;

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
            var scenarious = await _scenarioRepo.GetAll();
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
            var saveResult = await _scenarioRepo.Create(dto);

            return Ok(saveResult);
        }

        [HttpPut]
        [Route("")]
        public async Task<IActionResult> UpdateScenario([FromBody] ScenarioDto dto)
        {
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
    }
}
