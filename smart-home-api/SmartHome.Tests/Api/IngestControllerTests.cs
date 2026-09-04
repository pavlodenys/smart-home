using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SmartHome.Api.Controllers;
using SmartHome.Api.Ingestion;
using SmartHome.Data;
using SmartHome.Data.Entities;

namespace SmartHome.Tests.Api;

public sealed class IngestControllerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 3, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public async Task IngestMqtt_stores_a_valid_reading_as_utc()
    {
        await using var dbContext = CreateContext();
        dbContext.Set<SmartHome.Data.Entities.Data>().Add(new SmartHome.Data.Entities.Data
        {
            Id = 4,
            Name = "Soil moisture",
            Description = string.Empty,
            Type = "double"
        });
        await dbContext.SaveChangesAsync();
        var controller = CreateController(dbContext);

        var result = await controller.IngestMqtt(new MqttIngestionRequest
        {
            Id = 4,
            Name = "%",
            Value = 28.4,
            Time = Now.ToUnixTimeSeconds()
        }, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var point = await dbContext.Set<Point>().SingleAsync();
        Assert.Multiple(() =>
        {
            Assert.That(point.DataId, Is.EqualTo(4));
            Assert.That(point.Name, Is.EqualTo("%"));
            Assert.That(point.Value, Is.EqualTo(28.4));
            Assert.That(point.DateTime, Is.EqualTo(Now.UtcDateTime));
            Assert.That(point.DateTime.Kind, Is.EqualTo(DateTimeKind.Utc));
        });
    }

    [Test]
    public async Task IngestMqtt_rejects_an_unknown_data_id_without_storing_a_point()
    {
        await using var dbContext = CreateContext();
        var controller = CreateController(dbContext);

        var result = await controller.IngestMqtt(ValidRequest(), CancellationToken.None);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        Assert.That(await dbContext.Set<Point>().CountAsync(), Is.Zero);
    }

    [TestCase(double.NaN)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(1_000_001d)]
    public async Task IngestMqtt_rejects_invalid_values(double value)
    {
        await using var dbContext = CreateContext();
        var controller = CreateController(dbContext);
        var request = ValidRequest();
        request.Value = value;

        var result = await controller.IngestMqtt(request, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [TestCase(-86_401)]
    [TestCase(301)]
    [TestCase(0)]
    public async Task IngestMqtt_rejects_implausible_timestamps(long offsetSeconds)
    {
        await using var dbContext = CreateContext();
        var controller = CreateController(dbContext);
        var request = ValidRequest();
        request.Time = offsetSeconds == 0 ? 0 : Now.AddSeconds(offsetSeconds).ToUnixTimeSeconds();

        var result = await controller.IngestMqtt(request, CancellationToken.None);

        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    private static SmartHomeDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SmartHomeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new SmartHomeDbContext(options);
    }

    private static IngestController CreateController(SmartHomeDbContext dbContext) =>
        new(
            dbContext,
            Options.Create(new IngestionOptions()),
            new FixedTimeProvider(Now));

    private static MqttIngestionRequest ValidRequest() => new()
    {
        Id = 4,
        Name = "%",
        Value = 28.4,
        Time = Now.ToUnixTimeSeconds()
    };

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
