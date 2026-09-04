using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace SmartHome.Api.Ingestion;

public sealed class IngestionApiKeyValidator : IIngestionApiKeyValidator
{
    private readonly byte[][] _apiKeyHashes;

    public IngestionApiKeyValidator(IOptions<IngestionOptions> options)
    {
        _apiKeyHashes = options.Value.GetApiKeys()
            .Select(Hash)
            .ToArray();
    }

    public bool IsValid(string apiKey)
    {
        var presentedHash = Hash(apiKey);
        var isValid = false;

        foreach (var configuredHash in _apiKeyHashes)
        {
            isValid |= CryptographicOperations.FixedTimeEquals(presentedHash, configuredHash);
        }

        return isValid;
    }

    private static byte[] Hash(string apiKey) =>
        SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
}
