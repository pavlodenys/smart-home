using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SmartHome.Api.Ingestion;

namespace SmartHome.Tests.Api;

public sealed class IngestionApiKeyHandlerTests
{
    private const string ApiKey = "test-ingestion-key-at-least-32-bytes";

    [Test]
    public async Task AuthenticateAsync_accepts_a_valid_bearer_key()
    {
        var (handler, _) = await CreateHandler($"Bearer {ApiKey}");

        var result = await handler.AuthenticateAsync();

        Assert.That(result.Succeeded, Is.True);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("Bearer wrong-ingestion-key-at-least-32-bytes")]
    [TestCase("Basic test-ingestion-key-at-least-32-bytes")]
    public async Task AuthenticateAsync_rejects_missing_or_invalid_credentials(string? authorization)
    {
        var (handler, _) = await CreateHandler(authorization);

        var result = await handler.AuthenticateAsync();

        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public async Task ChallengeAsync_returns_401_with_a_bearer_challenge()
    {
        var (handler, context) = await CreateHandler(null);

        await handler.ChallengeAsync(new AuthenticationProperties());

        Assert.Multiple(() =>
        {
            Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
            Assert.That(context.Response.Headers.WWWAuthenticate.ToString(), Is.EqualTo("Bearer"));
        });
    }

    private static async Task<(IngestionApiKeyHandler Handler, DefaultHttpContext Context)> CreateHandler(
        string? authorization)
    {
        var schemeOptions = new AuthenticationSchemeOptions();
        var optionsMonitor = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();
        optionsMonitor
            .Setup(options => options.Get(IngestionAuthenticationDefaults.AuthenticationScheme))
            .Returns(schemeOptions);
        optionsMonitor
            .Setup(options => options.CurrentValue)
            .Returns(schemeOptions);

        var validator = new IngestionApiKeyValidator(Options.Create(new IngestionOptions
        {
            ApiKey = ApiKey
        }));
        var handler = new IngestionApiKeyHandler(
            optionsMonitor.Object,
            NullLoggerFactory.Instance,
            UrlEncoder.Default,
            validator);
        var context = new DefaultHttpContext();
        if (authorization is not null)
        {
            context.Request.Headers.Authorization = authorization;
        }

        var scheme = new AuthenticationScheme(
            IngestionAuthenticationDefaults.AuthenticationScheme,
            null,
            typeof(IngestionApiKeyHandler));
        await handler.InitializeAsync(scheme, context);

        return (handler, context);
    }
}
