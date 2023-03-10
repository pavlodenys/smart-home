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
    public class HomeController : ControllerBase
    {
        private IRepository<Sensor, SensorDto> _repo { get; set; }
        private IRepository<Device, DeviceDto> _devicesRepo { get; set; }
        private IRepository<Scenario, ScenarioDto> _scenarioRepo { get; set; }

        private readonly ILogger<HomeController> _logger;


        private IService _service { get; set; }

        public HomeController(IRepository<Sensor, SensorDto> repo, IService service, IRepository<Device, DeviceDto> devicesRepo, IRepository<Scenario, ScenarioDto> scenarioRepo, ILogger<HomeController> logger)
        {
            _repo = repo;
            _service = service;
            _devicesRepo = devicesRepo;
            _scenarioRepo = scenarioRepo;
            _logger = logger;
        }

        [HttpGet]
        [Route("sensors")]
        //[Authorize]
        public IActionResult GetSensors()
        {
            var sensors = _repo.GetAll();

            _logger.Log(LogLevel.Information, "Get Sensors Data");
            
            return Ok(sensors);
        }
        [HttpGet]
        [Route("sensors/{id}")]
        //[Authorize]
        public IActionResult GetSensorDetails(int id)
        {
            var sensor = _repo.GetById(b => b.Id == id, x => x.Include(y => y.Data).ThenInclude(z => z.Points));

            _logger.Log(LogLevel.Information, "Get Sensors Data");

            return Ok(sensor);
        }

        [HttpPost]
        [Route("sensors")]
        public async Task<IActionResult> AddSensor([FromBody] SensorDto sensor)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.ToList());
            }

            var newSensor = await _repo.Create(sensor);

            return Ok(newSensor);
        }

        [HttpPut]
        [Route("sensor/{id}")]
        public IActionResult UpdateSensor(int id, [FromBody] SensorDto sensorDto)
        {
            try
            {
                var sensor = _repo.Update(id, sensorDto);
                //_unitOfWork.SaveChanges();

                return Ok(sensor);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("sensors/{id}")]
        public async Task<IActionResult> DeleteSensor(int id)
        {
            var entity = _repo.GetById(id); // TODO: delete by id
            var deleteResult = _repo.Delete(entity);

            return Ok(deleteResult);
        }

        [HttpGet]
        [Route("device")]
        public IActionResult GetAllDevices()
        {
            var devices = _devicesRepo.GetAll();

            return Ok(devices);
        }

        [HttpPatch]
        [Route("device/{id}")]
        public IActionResult ChangeDeviceStatus(int id)
        {
            var changeResult = _service.ChangeStatus(id);

            return Ok(changeResult);
        }

        [HttpPost]
        [Route("scenario")]
        public async Task<IActionResult> SaveScenario([FromBody]ScenarioDto dto)
        {
            var saveResult = await _scenarioRepo.Create(dto);

            return Ok(saveResult);
        }
    }
}
