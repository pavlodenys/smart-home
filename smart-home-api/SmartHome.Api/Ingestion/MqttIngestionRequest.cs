using System.ComponentModel.DataAnnotations;

namespace SmartHome.Api.Ingestion;

public sealed class MqttIngestionRequest
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public double Value { get; set; }

    public long Time { get; set; }
}
