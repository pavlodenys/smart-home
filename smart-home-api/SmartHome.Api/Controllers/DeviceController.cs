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

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> AddDevice([FromBody] DeviceDto device)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newDevice = await _devicesRepo.Create(device);
            return Ok(newDevice);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateDevice(int id, [FromBody] DeviceDto device)
        {
            var updatedDevice = await _devicesRepo.Update(id, device);
            return updatedDevice == null ? NotFound() : Ok(updatedDevice);
        }

        [HttpPatch]
        [Route("{id}")]
        public async Task<IActionResult> ChangeDeviceStatus(int id)
        {
            var changeResult = await _service.ChangeStatus(id);

            return Ok(changeResult);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var entity = _devicesRepo.GetById(id);
            if (entity == null)
            {
                return NotFound();
            }

            var deleteResult = await _devicesRepo.Delete(entity);
            return Ok(deleteResult);
        }
    }
}
