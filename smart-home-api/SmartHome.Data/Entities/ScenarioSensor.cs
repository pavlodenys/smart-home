namespace SmartHome.Data.Entities
{
    public class ScenarioSensor
    {
        public int ScenarioId { get; set; }
        public virtual Scenario Scenario { get; set; }
        public int Id { get; set; }
        public int SensorId { get; set; }
        public virtual Sensor Sensor { get; set; }
    }
}
