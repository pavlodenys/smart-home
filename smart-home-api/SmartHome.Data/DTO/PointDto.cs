namespace SmartHome.Data.DTO
{
    public class PointDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTime DateTime { get; set; }
        public long Time { get; set; }
    }
}