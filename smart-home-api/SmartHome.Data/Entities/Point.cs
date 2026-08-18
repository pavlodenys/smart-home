using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHome.Data.Entities
{
    public class Point : IValue
    {
        private DateTime _dateTime;

        public int Id { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime DateTime
        {
            get => System.DateTime.SpecifyKind(_dateTime, DateTimeKind.Utc);
            set => _dateTime = value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => System.DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }

        public int DataId { get; set; }
        [ForeignKey(nameof(DataId))]
        public virtual Data Data { get; set; }
    }
}
