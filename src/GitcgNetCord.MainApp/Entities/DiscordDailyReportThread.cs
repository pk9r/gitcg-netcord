namespace GitcgNetCord.MainApp.Entities;

public class DiscordDailyReportThread
{
    public int Id { get; set; }
    public ulong ThreadId { get; set; }
    public ulong GuildId { get; set; }
    public ulong DiscordUserId { get; set; }
    public Guid HoyolabAccountId { get; set; }
    public HoyolabAccount? HoyolabAccount { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
