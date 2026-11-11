namespace GitcgNetCord.MainApp.Entities;

public class GameVersion
{
    public int Id { get; set; }
    public string VersionNumber { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTimeOffset ReleaseDate { get; set; }
    public bool IsProcessed { get; set; }
}
