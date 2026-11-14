namespace GitcgNetCord.MainApp.Entities;

public class DiscordGuildConfig
{
    public ulong GuildId { get; set; }
    public string? InviteUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
