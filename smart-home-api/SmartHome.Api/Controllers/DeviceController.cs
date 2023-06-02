using Microsoft.AspNetCore.Http;
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
    public class DeviceController : ControllerBase
    {
        private IRepository<Device, DeviceDto> _devicesRepo { get; set; }


        private readonly ILogger<HomeController> _logger;


        private IService _service { get; set; }

        public DeviceController(IService service, IRepository<Device, DeviceDto> devicesRepo, ILogger<HomeController> logger)
        {
            _service = service;
            _devicesRepo = devicesRepo;
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAllDevices()
        {
            var devices = await _devicesRepo.GetAll();

            return Ok(devices);
        }

        [HttpPatch]
        [Route("{id}")]
        public IActionResult ChangeDeviceStatus(int id)
        {
            var changeResult = _service.ChangeStatus(id);

            return Ok(changeResult);
        }
    }
}
