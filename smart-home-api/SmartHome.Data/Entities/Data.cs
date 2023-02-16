namespace SmartHome.Data.Entities
{
    public class Data
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public virtual ICollection<Point> Points { get; set; }
    }
}
