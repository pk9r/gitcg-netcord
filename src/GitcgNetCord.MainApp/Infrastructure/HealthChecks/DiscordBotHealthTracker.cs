namespace GitcgNetCord.MainApp.Infrastructure.HealthChecks;

/// <summary>
/// Thread-safe tracker that maintains real-time Discord Gateway connection and readiness state.
/// </summary>
public class DiscordBotHealthTracker
{
    private volatile bool _isReady;
    private string? _botUsername;
    private TimeSpan? _latency;
    private DateTimeOffset? _readyAt;
    private DateTimeOffset? _lastDisconnectAt;

    /// <summary>
    /// Indicates whether the bot has successfully completed the Discord Gateway READY handshake.
    /// </summary>
    public bool IsReady => _isReady;

    /// <summary>
    /// The authenticated Discord bot username and discriminator or tag.
    /// </summary>
    public string? BotUsername => _botUsername;

    /// <summary>
    /// The current Discord Gateway heartbeat latency.
    /// </summary>
    public TimeSpan? Latency => _latency;

    /// <summary>
    /// The UTC timestamp when the bot became ready.
    /// </summary>
    public DateTimeOffset? ReadyAt => _readyAt;

    /// <summary>
    /// The UTC timestamp of the most recent Gateway disconnection, if any.
    /// </summary>
    public DateTimeOffset? LastDisconnectAt => _lastDisconnectAt;

    /// <summary>
    /// Marks the bot as ready upon receiving the Discord Gateway READY event.
    /// </summary>
    public void SetReady(string botUsername, TimeSpan? latency = null)
    {
        _botUsername = botUsername;
        _latency = latency;
        _readyAt = DateTimeOffset.UtcNow;
        _isReady = true;
    }

    /// <summary>
    /// Marks the bot as unready upon Gateway disconnect or session termination.
    /// </summary>
    public void SetDisconnected()
    {
        _isReady = false;
        _lastDisconnectAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Updates the measured gateway latency.
    /// </summary>
    public void UpdateLatency(TimeSpan latency)
    {
        _latency = latency;
    }
}
