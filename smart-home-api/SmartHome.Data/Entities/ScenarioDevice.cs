namespace SmartHome.Data.Entities
{
    public class ScenarioDevice
    {
        public int ScenarioId { get; set; }
        public virtual Scenario Scenario { get; set; }
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public virtual Device Device { get; set; }
    }
}
