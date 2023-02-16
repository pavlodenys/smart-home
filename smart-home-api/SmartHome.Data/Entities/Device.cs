namespace SmartHome.Data.Entities
{
    public class Device
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public Device() { }
        public Device(string name, string description)
        {
            Name = name;
            Description = description;
            Id = Id;
        }

        public bool IsActive { get; set; }

    }
}
