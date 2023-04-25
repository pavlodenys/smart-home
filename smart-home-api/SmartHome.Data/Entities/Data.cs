using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHome.Data.Entities
{
    public class Data
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public int SensorId { get; set; }
        [ForeignKey(nameof(SensorId))]
        public virtual Sensor Sensor { get; set; } = new Sensor();
        public virtual ICollection<Point> Points { get; set; } = new List<Point>();
    }
}
