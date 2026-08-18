using System.Text.Json;
using NUnit.Framework;
using SmartHome.Data.Entities;

namespace SmartHome.Tests.Data;

public class PointTests
{
    [Test]
    public void DateTime_RestoresUtcKindForSqlValues()
    {
        var sqlValue = new DateTime(2026, 8, 15, 15, 38, 2, DateTimeKind.Unspecified);

        var point = new Point { DateTime = sqlValue };

        Assert.That(point.DateTime.Kind, Is.EqualTo(DateTimeKind.Utc));
        Assert.That(point.DateTime.Ticks, Is.EqualTo(sqlValue.Ticks));
        StringAssert.Contains("2026-08-15T15:38:02Z", JsonSerializer.Serialize(point));
    }

    [Test]
    public void DateTime_ConvertsLocalValuesToUtcBeforeStorage()
    {
        var localValue = new DateTime(2026, 8, 15, 18, 38, 2, DateTimeKind.Local);

        var point = new Point { DateTime = localValue };

        Assert.That(point.DateTime, Is.EqualTo(localValue.ToUniversalTime()));
        Assert.That(point.DateTime.Kind, Is.EqualTo(DateTimeKind.Utc));
    }
}
