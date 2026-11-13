using GitcgNetCord.MainApp.Infrastructure;
using GitcgNetCord.MainApp.Models;
using Xunit;

namespace GitcgNetCord.MainApp.Tests;

public class DailySnapshotProcessorTests
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
        var offset = DailySnapshotProcessor.GetServerUtcOffset(region);
        Assert.Equal(TimeSpan.FromHours(expectedHours), offset);
    }

    [Fact]
    public void DailyReportTriggerRequest_Defaults_ForceUpdateIsTrue()
    {
        var request = new DailyReportTriggerRequest();
        Assert.Null(request.Uid);
        Assert.True(request.ForceUpdate);
    }

    [Fact]
    public void DailyReportTriggerRequest_WithUid_SetsPropertiesCorrectly()
    {
        var request = new DailyReportTriggerRequest(Uid: "123456789", ForceUpdate: false);
        Assert.Equal("123456789", request.Uid);
        Assert.False(request.ForceUpdate);
    }
}
