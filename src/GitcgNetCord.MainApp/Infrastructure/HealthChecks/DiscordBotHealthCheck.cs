using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GitcgNetCord.MainApp.Infrastructure.HealthChecks;

/// <summary>
/// Health check that evaluates the readiness of the Discord bot Gateway connection.
/// Returns Unhealthy while the bot is connecting or disconnected, and Healthy once READY is processed.
/// </summary>
public class DiscordBotHealthCheck(DiscordBotHealthTracker tracker) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!tracker.IsReady)
        {
            var data = new Dictionary<string, object>();
            if (tracker.LastDisconnectAt.HasValue)
            {
                data["lastDisconnectAt"] = tracker.LastDisconnectAt.Value;
            }

            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    description: "Discord bot is not ready or has disconnected from Gateway.",
                    data: data
                )
            );
        }

        var healthyData = new Dictionary<string, object>
        {
            ["botUsername"] = tracker.BotUsername ?? "Unknown",
        };

        if (tracker.Latency.HasValue)
        {
            healthyData["latencyMs"] = tracker.Latency.Value.TotalMilliseconds;
        }

        if (tracker.ReadyAt.HasValue)
        {
            healthyData["readyAt"] = tracker.ReadyAt.Value;
        }

        return Task.FromResult(
            HealthCheckResult.Healthy(
                description: $"Discord bot is ready and online as '{tracker.BotUsername}'.",
                data: healthyData
            )
        );
    }
}
