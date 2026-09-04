using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartHome.Api.Ingestion;
using SmartHome.Data;
using SmartHome.Data.Entities;

namespace SmartHome.Api.Controllers;

[ApiController]
[Route("api/ingest")]
public sealed class IngestController : ControllerBase
{
    private readonly SmartHomeDbContext _dbContext;
    private readonly IngestionOptions _options;
    private readonly TimeProvider _timeProvider;

    public IngestController(
        SmartHomeDbContext dbContext,
        IOptions<IngestionOptions> options,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    [HttpPost("mqtt")]
    [Authorize(AuthenticationSchemes = IngestionAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> IngestMqtt(
        [FromBody] MqttIngestionRequest request,
        CancellationToken cancellationToken)
    {
        if (!double.IsFinite(request.Value)
            || Math.Abs(request.Value) > _options.MaximumAbsoluteValue)
        {
            return BadRequest("The reading value is outside the accepted range.");
        }

        DateTimeOffset readingTime;
        try
        {
            readingTime = DateTimeOffset.FromUnixTimeSeconds(request.Time);
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest("The reading timestamp is invalid.");
        }

        var now = _timeProvider.GetUtcNow();
        if (readingTime < now - _options.MaximumReadingAge
            || readingTime > now + _options.MaximumFutureSkew)
        {
            return BadRequest("The reading timestamp is outside the accepted range.");
        }

        var dataExists = await _dbContext.Set<SmartHome.Data.Entities.Data>()
            .AnyAsync(data => data.Id == request.Id, cancellationToken);
        if (!dataExists)
        {
            return BadRequest("The sensor data ID is unknown.");
        }

        var point = new Point
        {
            Name = request.Name.Trim(),
            Value = request.Value,
            DateTime = readingTime.UtcDateTime,
            DataId = request.Id
        };

        _dbContext.Set<Point>().Add(point);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { point.Id });
    }
}
