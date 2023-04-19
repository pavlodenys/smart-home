using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public ScenarioController(IRepository<Scenario, ScenarioDto> scenarioRepo)
        {
            _scenarioRepo = scenarioRepo;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SaveScenario([FromBody] ScenarioDto dto)
        {
            var saveResult = await _scenarioRepo.Create(dto);

            return Ok(saveResult);
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetScenarious()
        {
            var scenarious = _scenarioRepo.GetAll();
            return base.Ok(await scenarious.ToListAsync());
        }

        [HttpGet]
        [Route("{id}")]
        public IActionResult GetScenario(int id)
        {
            return Ok(_scenarioRepo.GetById(id));
        }
    }
}
