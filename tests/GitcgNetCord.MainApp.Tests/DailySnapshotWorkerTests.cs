using GitcgNetCord.MainApp.Infrastructure;
using Xunit;

namespace GitcgNetCord.MainApp.Tests;

public class DailySnapshotWorkerTests
{
    [Theory]
    [InlineData("os_asia", 8)]
    [InlineData("os_cht", 8)]
    [InlineData("OS_ASIA", 8)]
    [InlineData("os_euro", 1)]
    [InlineData("os_europe", 1)]
    [InlineData("os_usa", -5)]
    [InlineData("os_america", -5)]
    [InlineData("unknown_region", 8)]
    public void GetServerUtcOffset_ReturnsExpectedOffset(string region, int expectedHours)
    {
        var offset = DailySnapshotWorker.GetServerUtcOffset(region);
        Assert.Equal(TimeSpan.FromHours(expectedHours), offset);
    }

    [Fact]
    public void LocalGameDate_BeforeReset_YieldsPreviousDay()
    {
        // Asia is UTC+8. Reset is at 04:00 AM local.
        // At 03:59:59 AM local on Nov 12, game day is still Nov 11.
        var localTime = new DateTimeOffset(2026, 11, 12, 3, 59, 59, TimeSpan.FromHours(8));
        var gameDate = (localTime - TimeSpan.FromHours(4)).Date;
        Assert.Equal(new DateTime(2026, 11, 11), gameDate);
    }

    [Fact]
    public void LocalGameDate_AfterReset_YieldsCurrentDay()
    {
        // At 04:00:00 AM local on Nov 12, game day is Nov 12.
        var localTime = new DateTimeOffset(2026, 11, 12, 4, 0, 0, TimeSpan.FromHours(8));
        var gameDate = (localTime - TimeSpan.FromHours(4)).Date;
        Assert.Equal(new DateTime(2026, 11, 12), gameDate);
    }
}
