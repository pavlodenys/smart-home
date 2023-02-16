using SmartHome.Core.Enums;
using SmartHome.Data.Entities;

namespace SmartHome.Data.DTO
{
    public class ScenarioDto
    {
        public int Id { get; set; }
        public int SensorValue { get; set; }
        public int Value { get; set; }
        public ComparisonOperator Operator { get; set; }
        public string? Command { get; set; }
        public ICollection<ScenarioSensorDto>? Sensors { get; set; }
        public ICollection<ScenarioDeviceDto>? Devices { get; set; }
    }
}