using GitcgNetCord.MainApp.Infrastructure.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace GitcgNetCord.MainApp.Tests.HealthChecks;

public class DiscordBotHealthCheckTests
{
    private readonly DiscordBotHealthTracker _tracker = new();
    private readonly DiscordBotHealthCheck _healthCheck;

    public DiscordBotHealthCheckTests()
    {
        _healthCheck = new DiscordBotHealthCheck(_tracker);
    }

    [Fact]
    public async Task CheckHealthAsync_InitialState_ReturnsUnhealthy()
    {
        var context = new HealthCheckContext();

        var result = await _healthCheck.CheckHealthAsync(context);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("not ready", result.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenReady_ReturnsHealthyWithMetadata()
    {
        var context = new HealthCheckContext();
        _tracker.SetReady("TestBot#1234", TimeSpan.FromMilliseconds(42));

        var result = await _healthCheck.CheckHealthAsync(context);

        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("TestBot#1234", result.Description);
        Assert.Equal("TestBot#1234", result.Data["botUsername"]);
        Assert.Equal(42.0, (double)result.Data["latencyMs"]);
        Assert.NotNull(result.Data["readyAt"]);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenDisconnectedAfterReady_ReturnsUnhealthy()
    {
        var context = new HealthCheckContext();
        _tracker.SetReady("TestBot#1234", TimeSpan.FromMilliseconds(42));
        _tracker.SetDisconnected();

        var result = await _healthCheck.CheckHealthAsync(context);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.NotNull(result.Data["lastDisconnectAt"]);
    }
}
