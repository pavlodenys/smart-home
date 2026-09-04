using System.Text;

namespace SmartHome.Api.Ingestion;

public sealed class IngestionOptions
{
    public const string SectionName = "Ingestion";

    public string ApiKey { get; set; } = string.Empty;

    public string[] ApiKeys { get; set; } = [];

    public double MaximumAbsoluteValue { get; set; } = 1_000_000;

    public TimeSpan MaximumReadingAge { get; set; } = TimeSpan.FromDays(1);

    public TimeSpan MaximumFutureSkew { get; set; } = TimeSpan.FromMinutes(5);

    public IEnumerable<string> GetApiKeys()
    {
        if (!string.IsNullOrWhiteSpace(ApiKey))
        {
            yield return ApiKey;
        }

        foreach (var apiKey in ApiKeys.Where(key => !string.IsNullOrWhiteSpace(key)))
        {
            yield return apiKey;
        }
    }

    public static bool IsValid(IngestionOptions options)
    {
        var apiKeys = options.GetApiKeys().ToArray();

        return apiKeys.Length > 0
            && apiKeys.All(key => Encoding.UTF8.GetByteCount(key) >= 32)
            && double.IsFinite(options.MaximumAbsoluteValue)
            && options.MaximumAbsoluteValue > 0
            && options.MaximumReadingAge > TimeSpan.Zero
            && options.MaximumFutureSkew >= TimeSpan.Zero;
    }
}
