using SmartHome.Data.Entities;

namespace SmartHome.Data.DTO
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class ScenarioDeviceDto
    {
        public int ScenarioId { get; set; }
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public DeviceDto? Device { get; set; }
    }
}