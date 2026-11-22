using GitcgNetCord.MainApp.Infrastructure.HealthChecks;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace GitcgNetCord.MainApp.GatewayHandlers;

/// <summary>
/// Gateway event handler that updates the bot health tracker when READY, RESUME,
/// or DISCONNECT events occur on the GatewayClient.
/// </summary>
public class DiscordBotReadyGatewayHandler(
    GatewayClient client,
    DiscordBotHealthTracker tracker
) : IReadyGatewayHandler, IResumeGatewayHandler, IDisconnectGatewayHandler
{
    public ValueTask HandleAsync(ReadyEventArgs args)
    {
        tracker.SetReady(args.User.Username, client.Latency);
        return default;
    }

    public ValueTask HandleAsync()
    {
        tracker.SetResumed(client.Latency);
        return default;
    }

    public ValueTask HandleAsync(DisconnectEventArgs args)
    {
        tracker.SetDisconnected();
        return default;
    }
}
