using SmartHome.Logic;

namespace SmartHome.Connector.Services
{
    public class ScenarioService
    {
        private IService _service { get; set; }
        public ScenarioService(IService service) {
            _service= service;
        }

        public async Task<bool> ChangeDeviceStatus(int id)
        {
            return await _service.ChangeStatus(id);
        }
    }
}
