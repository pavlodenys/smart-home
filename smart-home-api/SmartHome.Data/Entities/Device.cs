﻿namespace SmartHome.Data.Entities
{
    public class Device
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Id { get; set; }
        public bool IsActive { get; set; }

    }
}
