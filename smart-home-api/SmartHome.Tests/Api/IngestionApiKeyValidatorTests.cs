using Microsoft.Extensions.Options;
using NUnit.Framework;
using SmartHome.Api.Ingestion;

namespace SmartHome.Tests.Api;

public sealed class IngestionApiKeyValidatorTests
{
    private const string PrimaryKey = "primary-ingestion-key-32-bytes-long";
    private const string RotationKey = "rotation-ingestion-key-32-bytes-long";

    [TestCase(PrimaryKey)]
    [TestCase(RotationKey)]
    public void IsValid_accepts_primary_and_rotation_keys(string apiKey)
    {
        var validator = CreateValidator();

        Assert.That(validator.IsValid(apiKey), Is.True);
    }

    [Test]
    public void IsValid_rejects_an_unknown_key()
    {
        var validator = CreateValidator();

        Assert.That(validator.IsValid("wrong-ingestion-key-32-bytes-long"), Is.False);
    }

    private static IngestionApiKeyValidator CreateValidator() =>
        new(Options.Create(new IngestionOptions
        {
            ApiKey = PrimaryKey,
            ApiKeys = [RotationKey]
        }));
}
