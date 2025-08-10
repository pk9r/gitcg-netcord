namespace GitcgNetCord.MainApp.Entities;

public class HoyolabAccount
{
    public Guid Id { get; set; }
    public ulong DiscordUserId { get; set; }
    
    public string HoyolabUserId { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string GameRoleId { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;

    public DiscordUser DiscordUser { get; set; } = null!;
}
