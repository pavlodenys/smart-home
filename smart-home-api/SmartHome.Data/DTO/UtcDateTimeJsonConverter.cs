using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartHome.Data.DTO
{
    // Point timestamps are stored/transmitted as UTC; EF Core returns them with Kind=Unspecified,
    // so without this the JSON would lack the "Z" suffix and browsers would misread them as local time.
    public class UtcDateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}
