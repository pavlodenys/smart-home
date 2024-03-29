﻿using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHome.Data.Entities
{
    public class Sensor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        //public int DataId { get; set; }
        //[ForeignKey(nameof(DataId))]
        //public virtual Data Data { get; set; }
        public virtual ICollection<Data> Data { get; set; }
    }
}
