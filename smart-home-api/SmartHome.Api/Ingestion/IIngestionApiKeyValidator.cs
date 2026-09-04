namespace SmartHome.Api.Ingestion;

public interface IIngestionApiKeyValidator
{
    bool IsValid(string apiKey);
}
