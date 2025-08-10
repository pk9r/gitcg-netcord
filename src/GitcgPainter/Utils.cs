using System;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using System.Reflection;
using System.Threading.Tasks;

namespace GitcgPainter;

internal static class Utils
{
    public const int NumberRoleCards = 3;
    public const int NumberActionCards = 30;

    public const string ArcaneBorderCacheKey = "arcane_border";

    public const string CardBorderPath =
        "GitcgPainter.assets.images.CardBorder.png";
    public const string ArcaneBorderPath =
        "GitcgPainter.assets.images.ArcaneBorder.png";

    public const string BackgroundDeckGamePath =
        "GitcgPainter.assets.images.bg_deck_game.png";

    public const string AnemoPath =
        "GitcgPainter.assets.images.anemo.png";
    public const string CryoPath =
        "GitcgPainter.assets.images.cryo.png";
    public const string DendroPath =
        "GitcgPainter.assets.images.dendro.png";
    public const string ElectroPath =
        "GitcgPainter.assets.images.electro.png";
    public const string GeoPath =
        "GitcgPainter.assets.images.geo.png";
    public const string HydroPath =
        "GitcgPainter.assets.images.hydro.png";
    public const string PyroPath =
        "GitcgPainter.assets.images.pyro.png";

    public const string CostSecretPath =
        "GitcgPainter.assets.images.cost_secret.png";
    public const string TcgActionPointCommonBlackPath =
        "GitcgPainter.assets.images.tcg_action_point_common_black.png";
    public const string TcgActionPointCommonWhitePath =
        "GitcgPainter.assets.images.tcg_action_point_common_white.png";
    public const string TcgCharacterRechargePointPath =
        "GitcgPainter.assets.images.tcg_character_recharge_point.png";

    private const string AntonRegularFontPath =
        "GitcgPainter.assets.fonts.Anton.Anton-Regular.ttf";

    private const string RobotoRegularFontPath =
        "GitcgPainter.assets.fonts.Roboto.Roboto-Regular.ttf";
    private const string RobotoBoldFontPath =
        "GitcgPainter.assets.fonts.Roboto.Roboto-Bold.ttf";

    public static readonly Assembly GitcgPainterAssembly = typeof(Utils).Assembly;

    public static readonly FontFamily AntonRegularFontFamily =
        LoadFontFamilyFromManifestResource(AntonRegularFontPath);

    public static readonly FontFamily RobotoRegularFontFamily =
        LoadFontFamilyFromManifestResource(RobotoRegularFontPath);

    public static readonly FontFamily RobotoBoldFontFamily =
        LoadFontFamilyFromManifestResource(RobotoBoldFontPath);

    public static Task<Image> LoadCardBorderAsync()
        => LoadImageFromManifestResourceAsync(CardBorderPath);

    public static Task<Image> LoadArcaneBorderAsync()
        => LoadImageFromManifestResourceAsync(ArcaneBorderPath);

    public static Task<Image> LoadBackgroundDeckGameAsync()
        => LoadImageFromManifestResourceAsync(BackgroundDeckGamePath);

    public static int CalcRoleCardsWidth(int roleCardWidth, int roleCardSpacing)
        => (roleCardWidth * NumberRoleCards) + (roleCardSpacing * (NumberRoleCards - 1));

    public static async Task<Image> LoadImageFromManifestResourceAsync(string name)
    {
        using var stream = GitcgPainterAssembly.GetManifestResourceStream(name) ??
            throw new InvalidOperationException($"Resource {name} not found.");

        var image = await Image.LoadAsync(stream);

        return image;
    }

    private static FontFamily LoadFontFamilyFromManifestResource(string path)
    {
        using var stream = GitcgPainterAssembly.GetManifestResourceStream(path) ??
            throw new InvalidOperationException($"Resource {path} not found.");

        var fontFamily = new FontCollection().Add(stream);

        return fontFamily;
    }
}
