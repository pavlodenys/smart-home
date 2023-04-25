using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using SmartHome.Logic;
using System.Security.Claims;

namespace SmartHome.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HomeController : ControllerBase
    {
        private IRepository<Sensor, SensorDto> _repo { get; set; }
        //private IRepository<SmartHome.Data.Entities.Data, ChartDataDto> _dataRepo { get; set; }
        private IRepository<Point, PointDto> _pointsRepo { get; set; }
        private IRepository<Device, DeviceDto> _devicesRepo { get; set; }


        private readonly ILogger<HomeController> _logger;


        private IService _service { get; set; }

        public HomeController(IRepository<Sensor, SensorDto> repo, IService service, IRepository<Device, DeviceDto> devicesRepo, ILogger<HomeController> logger, IRepository<Point, PointDto> pointsRepo)
        {
            _repo = repo;
            _service = service;
            _devicesRepo = devicesRepo;
            _logger = logger;
            _pointsRepo = pointsRepo;
        }

        [HttpGet]
        [Route("sensors")]
        //[Authorize]
        public IActionResult GetSensors()
        {
            var user = HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;

            var sensors = _repo.GetAll();

            _logger.Log(LogLevel.Information, "Get Sensors Data");

            return Ok(sensors);
        }
        [HttpGet]
        [Route("sensors/{id}/{date}")]
        //[Authorize]
        public async Task<IActionResult> GetSensorDetails(int id, string date)
        {
            var filterDate = DateTime.Parse(date);
            var sensor = await _repo.GetById(b => b.Id == id, x => x.Include(y => y.Data).ThenInclude(z => z.Points));
            
            foreach(var data in sensor.ChartData)
            {
                data.Data = data.Data?.Where(x => x.DateTime >=filterDate).ToArray();
            }
            _logger.Log(LogLevel.Information, "Get Sensors Details");

            return Ok(sensor);
        }

        [HttpGet]
        [Route("sensors/{id}/data/{page}/{count}")]
        //[Authorize]
        public IActionResult GetSensorData(int id, int page, int count)
        {
            var sensorData = _pointsRepo.GetAll(b => b.DataId == id, page, count);

            _logger.Log(LogLevel.Information, "Get Sensors Data");

            return Ok(sensorData);
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
        public IActionResult DeleteSensor(int id)
        {
            var entity = _repo.GetById(id); // TODO: delete by id
            if (entity != null)
            {
                var deleteResult = _repo.Delete(entity);
                return Ok(deleteResult);
            }

            return Ok(false);
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
    }
}
