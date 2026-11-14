using GitcgNetCord.MainApp.Infrastructure.HealthChecks;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace GitcgNetCord.MainApp.GatewayHandlers;

/// <summary>
/// Gateway event handler that updates the bot health tracker when the READY event is received
/// and watches for disconnect events on the GatewayClient.
/// </summary>
public class DiscordBotReadyGatewayHandler : IReadyGatewayHandler
{
    private readonly GatewayClient _client;
    private readonly DiscordBotHealthTracker _tracker;

    public DiscordBotReadyGatewayHandler(
        GatewayClient client,
        DiscordBotHealthTracker tracker
    )
    {
        _client = client;
        _tracker = tracker;

        _client.Disconnect += OnDisconnectAsync;
    }

    public ValueTask HandleAsync(ReadyEventArgs args)
    {
        _tracker.SetReady(args.User.Username, _client.Latency);
        return default;
    }

    private ValueTask OnDisconnectAsync(DisconnectEventArgs args)
    {
        _tracker.SetDisconnected();
        return default;
    }
}
