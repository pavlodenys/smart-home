using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHome.Data.Entities
{
    public class Point : IValue
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTime DateTime { get; set; }

        public int DataId { get; set; }
        [ForeignKey(nameof(DataId))]
        public virtual Data Data { get; set; } = new Data();
    }
}
