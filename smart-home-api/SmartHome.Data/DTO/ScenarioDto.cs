using SmartHome.Core;
using SmartHome.Core.Enums;

namespace SmartHome.Data.DTO
{
    public class ScenarioDto : IDeleted
    {
        public int Id { get; set; }
        public double Threshold { get; set; }
        public double Hysteresis { get; set; } = 2;
        public ComparisonOperator Operator { get; set; }
        public ScenarioActionType ActionType { get; set; }
        public string? Command { get; set; }
        public bool IsConditionActive { get; set; }
        public DateTime? LastTriggeredAt { get; set; }
        public bool IsDeleted { get; set; }
        public ICollection<ScenarioSensorDto>? Sensors { get; set; }
        public ICollection<ScenarioDeviceDto>? Devices { get; set; }
    }
}
