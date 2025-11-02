using GitcgNetCord.MainApp.Enums;

namespace GitcgNetCord.MainApp.Entities;

public class DiscordCardCodeChannel
{
    public ulong Id { get; set; }

    public SharingDecodeType SharingDecodeType { get; set; } = 
        SharingDecodeType.ImageGameBackground;
}