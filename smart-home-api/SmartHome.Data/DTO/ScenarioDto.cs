using SmartHome.Core;
using SmartHome.Core.Enums;

namespace SmartHome.Data.DTO
{
    public class ScenarioDto : IDeleted
    {
        public int Id { get; set; }
        public int SensorValue { get; set; }
        public int Value { get; set; }
        public ComparisonOperator Operator { get; set; }
        public string? Command { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<ScenarioSensorDto>? Sensors { get; set; }
        public ICollection<ScenarioDeviceDto>? Devices { get; set; }
    }
}