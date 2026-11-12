namespace GitcgNetCord.MainApp.Entities;

public class DiscordDailyReportChannel
{
    public ulong Id { get; set; }
    public ulong GuildId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
