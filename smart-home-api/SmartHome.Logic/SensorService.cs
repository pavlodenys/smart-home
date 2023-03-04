using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Logic
{
    public class SensorService
    {
        private readonly IRepository<Sensor, SensorDto> _sensorRepo;
        private readonly IRepository<SmartHome.Data.Entities.Data, ChartDataDto> _chartRepo;

        public SensorService(IRepository<Sensor, SensorDto> sensorRepo, IRepository<Data.Entities.Data, ChartDataDto> chartRepo)
        {
            _sensorRepo = sensorRepo;
            _chartRepo = chartRepo;
        }

        public async Task<SensorDto> CreateNewSensor(SensorDto dto)
        {
            foreach(var data in dto.ChartData)
            {
                var chartData = await _chartRepo.Create(data);

            }
            
           var sensor = await _sensorRepo.Create(dto);

            return sensor;
        }
    }
}
