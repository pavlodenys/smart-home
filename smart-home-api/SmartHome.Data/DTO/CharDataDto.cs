namespace SmartHome.Data.DTO
{
    public class ChartDataDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public IEnumerable<PointDto> Data { get; set; }
    }
}