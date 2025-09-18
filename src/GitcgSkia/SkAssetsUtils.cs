using System.Reflection;
using SkiaSharp;

namespace GitcgSkia;

public static class SkAssetsUtils
{
    public static readonly Assembly Assembly
        = typeof(Utils).Assembly;

    public static readonly string CardBorderPath
        = $"{typeof(Utils).Namespace}.assets.images.card_border.png";

    public static readonly string ArcaneBorderPath
        = $"{typeof(Utils).Namespace}.assets.images.arcane_border.png";

    public static readonly string BackgroundDeckGamePath
        = $"{typeof(Utils).Namespace}.assets.images.bg_deck_game.png";

    public static readonly string AnemoPath
        = $"{typeof(Utils).Namespace}.assets.images.anemo.png";

    public static readonly string CryoPath
        = $"{typeof(Utils).Namespace}.assets.images.cryo.png";

    public static readonly string DendroPath
        = $"{typeof(Utils).Namespace}.assets.images.dendro.png";

    public static readonly string ElectroPath
        = $"{typeof(Utils).Namespace}.assets.images.electro.png";

    public static readonly string GeoPath
        = $"{typeof(Utils).Namespace}.assets.images.geo.png";

    public static readonly string HydroPath
        = $"{typeof(Utils).Namespace}.assets.images.hydro.png";

    public static readonly string PyroPath
        = $"{typeof(Utils).Namespace}.assets.images.pyro.png";

    public static readonly string CostSecretPath
        = $"{typeof(Utils).Namespace}.assets.images.cost_secret.png";

    public static readonly string TcgActionPointCommonBlackPath
        = $"{typeof(Utils).Namespace}.assets.images.tcg_action_point_common_black.png";

    public static readonly string TcgActionPointCommonWhitePath
        = $"{typeof(Utils).Namespace}.assets.images.tcg_action_point_common_white.png";

    public static readonly string TcgCharacterRechargePointPath
        = $"{typeof(Utils).Namespace}.assets.images.tcg_character_recharge_point.png";

    public static readonly string AntonRegularFontPath
        = $"{typeof(Utils).Namespace}.assets.fonts.Anton.Anton-Regular.ttf";

    public static readonly string RobotoRegularFontPath
        = $"{typeof(Utils).Namespace}.assets.fonts.Roboto.Roboto-Regular.ttf";

    public static readonly string RobotoBoldFontPath
        = $"{typeof(Utils).Namespace}.assets.fonts.Roboto.Roboto-Bold.ttf";

    public static ValueTask<SKTypeface> LoadAntonRegularFontAsync()
        => LoadTypefaceFromManifestResourceAsync(AntonRegularFontPath);

    public static ValueTask<SKTypeface> LoadRobotoRegularFontAsync()
        => LoadTypefaceFromManifestResourceAsync(RobotoRegularFontPath);

    public static ValueTask<SKTypeface> LoadRobotoBoldFontAsync()
        => LoadTypefaceFromManifestResourceAsync(RobotoBoldFontPath);

    public static ValueTask<SKBitmap> LoadCardBorderAsync()
        => LoadImageFromManifestResourceAsync(CardBorderPath);

    public static ValueTask<SKBitmap> LoadArcaneBorderAsync()
        => LoadImageFromManifestResourceAsync(ArcaneBorderPath);

    public static ValueTask<SKBitmap> LoadBackgroundDeckGameAsync()
        => LoadImageFromManifestResourceAsync(BackgroundDeckGamePath);
    
    public static async ValueTask<SKBitmap>
        LoadImageFromManifestResourceAsync(string name)
    {
        await using var stream =
            Assembly.GetManifestResourceStream(name) ??
            throw new InvalidOperationException(
                $"Resource {name} not found."
            );

        var image = SKBitmap.Decode(stream);

        return image;
    }

    public static async ValueTask<SKTypeface>
        LoadTypefaceFromManifestResourceAsync(string name)
    {
        await using var stream =
            Assembly.GetManifestResourceStream(name) ??
            throw new InvalidOperationException(
                $"Resource {name} not found."
            );

        var typeface = SKTypeface.FromStream(stream);

        return typeface;
    }
}