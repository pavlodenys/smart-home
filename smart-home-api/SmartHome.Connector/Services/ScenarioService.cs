using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using SmartHome.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Connector.Services
{
    public class ScenarioService
    {
        private IService _service { get; set; }
        public ScenarioService(IService service) {
            _service= service;
        }

        public bool ChangeDeviceStatus(int id)
        {
            return _service.ChangeStatus(id);
        }
    }
}
