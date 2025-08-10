using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Enums;

public enum SharingDecodeType
{
    [SlashCommandChoice(Name = "text")]
    Text,

    [SlashCommandChoice(Name = "img-simplest")]
    ImageSimplest,

    [SlashCommandChoice(Name = "img-game-background")]
    ImageGameBackground,

    [SlashCommandChoice(Name = "img-genshincards")]
    ImageGenshincards
}

public static class SharingDecodeTypeExtensions
{
    private static readonly IEnumerable<SharingDecodeType> _createImageTypes =
    [
        SharingDecodeType.ImageSimplest,
        SharingDecodeType.ImageGameBackground,
        SharingDecodeType.ImageGenshincards
    ];

    public static bool IsCreateImageType(this SharingDecodeType type)
    {
        return _createImageTypes.Contains(type);
    }
}

