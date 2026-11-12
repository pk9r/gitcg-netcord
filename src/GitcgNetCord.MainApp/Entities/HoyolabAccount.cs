namespace GitcgNetCord.MainApp.Entities;

public class HoyolabAccount
{
    public Guid Id { get; set; }
    public ulong DiscordUserId { get; set; }
    
    public string HoyolabUserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string GameRoleId { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public bool IsDailySnapshotEnabled { get; set; } = false;

    public DiscordUser DiscordUser { get; set; } = null!;
    public ICollection<CardSnapshot> CardSnapshots { get; set; } = [];
    public ICollection<DiscordDailyReportThread> DailyReportThreads { get; set; } = [];
}
