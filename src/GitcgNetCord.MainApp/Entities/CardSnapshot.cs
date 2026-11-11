namespace GitcgNetCord.MainApp.Entities;

public class CardSnapshot
{
    public Guid Id { get; set; }
    public Guid HoyolabAccountId { get; set; }
    public ulong DiscordUserId { get; set; }

    public string Name { get; set; } = string.Empty;
    public SnapshotType SnapshotType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public int TotalGames { get; set; }
    public int TotalWins { get; set; }

    public Dictionary<int, CardStatSummary> CardData { get; set; } = [];

    public HoyolabAccount HoyolabAccount { get; set; } = null!;
}
