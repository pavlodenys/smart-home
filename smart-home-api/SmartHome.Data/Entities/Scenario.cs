using Newtonsoft.Json.Linq;
using SmartHome.Core;
using SmartHome.Core.Enums;

namespace SmartHome.Data.Entities
{
    public class Scenario : IDeleted
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
        public virtual ICollection<ScenarioSensor>? Sensors { get; set; }
        public virtual ICollection<ScenarioDevice>? Devices { get; set; }
    }
}
