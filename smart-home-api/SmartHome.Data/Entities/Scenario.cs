using Newtonsoft.Json.Linq;
using SmartHome.Core;
using SmartHome.Core.Enums;

namespace SmartHome.Data.Entities
{
    public class Scenario : IDeleted
    {
        public int Id { get; set; }
        public int SensorValue { get; set; }
        public int Value { get; set; }
        public ComparisonOperator Operator { get; set; }
        public string? Command { get; set; }
        public bool IsDeleted { get; set; }
        public virtual ICollection<ScenarioSensor>? Sensors { get; set; }
        public virtual ICollection<ScenarioDevice>? Devices { get; set; }
    }
}
