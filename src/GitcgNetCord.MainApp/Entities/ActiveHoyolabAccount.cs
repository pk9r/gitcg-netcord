namespace GitcgNetCord.MainApp.Entities;

public class ActiveHoyolabAccount
{
    public ulong DiscordUserId { get; set; }

    public Guid? HoyolabAccountId { get; set; }

    public DiscordUser DiscordUser { get; set; } = null!;
    
    public HoyolabAccount? HoyolabAccount { get; set; }
}
