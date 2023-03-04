using Microsoft.EntityFrameworkCore.ChangeTracking;
using SmartHome.Data.Entities;

namespace SmartHome.Data.DTO
{
    public class ScenarioSensorDto {
        public int ScenarioId { get; set; }
       public int Id { get; set; }
        public int SensorId { get; set; }
        public SensorDto? Sensor { get; set; }
   }
    public class SensorDto
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public int DataId { get; set; }

        public IEnumerable<ChartDataDto>? ChartData { get; set; }

     
    }
}