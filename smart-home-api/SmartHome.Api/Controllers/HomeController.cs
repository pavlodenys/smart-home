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
    }
}
