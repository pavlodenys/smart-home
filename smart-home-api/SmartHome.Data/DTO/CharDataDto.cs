namespace SmartHome.Data.DTO
{
    public class ChartDataDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        
        public IEnumerable<PointDto>? Data { get; set; }
    }
}