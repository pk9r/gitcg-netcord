using SkiaSharp;

using ExtensionSkillElements = HoyolabHttpClient.Extensions.SkillElements;

namespace GitcgSkia.Extensions;

public static class SkillElements
{
    public static IDictionary<string, SKColor> Colors { get; }

    static SkillElements()
    {
        Colors = new Dictionary<string, SKColor>
        {
            [ExtensionSkillElements.Cyro.Value] = ElementColors.Cyro,
            [ExtensionSkillElements.Hydro.Value] = ElementColors.Hydro,
            [ExtensionSkillElements.Pyro.Value] = ElementColors.Pyro,
            [ExtensionSkillElements.Electro.Value] = ElementColors.Electro,
            [ExtensionSkillElements.Anemo.Value] = ElementColors.Anemo,
            [ExtensionSkillElements.Geo.Value] = ElementColors.Geo,
            [ExtensionSkillElements.Dendro.Value] = ElementColors.Dendro,
            [ExtensionSkillElements.Unaligned.Value] = ElementColors.Unaligned,
            [ExtensionSkillElements.Aligned.Value] = ElementColors.Aligned,
            [ExtensionSkillElements.Energy.Value] = ElementColors.Energy,
            [ExtensionSkillElements.ArcaneLegend.Value] = ElementColors.ArcaneLegend
        };
    }

    public static ValueTask<SKBitmap> LoadElementIconAsync(this string value)
    {
        if (value == ExtensionSkillElements.Cyro.Value)
            return SkAssetsUtils.LoadImageFromManifestResourceAsync(SkAssetsUtils.CryoPath);
        if (value == ExtensionSkillElements.Hydro.Value)
            return SkAssetsUtils.LoadImageFromManifestResourceAsync(SkAssetsUtils.HydroPath);
        if (value == ExtensionSkillElements.Pyro.Value)
            return SkAssetsUtils.LoadImageFromManifestResourceAsync(SkAssetsUtils.PyroPath);
        if (value == ExtensionSkillElements.Electro.Value)
            return SkAssetsUtils.LoadImageFromManifestResourceAsync(SkAssetsUtils.ElectroPath);
        if (value == ExtensionSkillElements.Anemo.Value)
            return SkAssetsUtils.LoadImageFromManifestResourceAsync(SkAssetsUtils.AnemoPath);
        if (value == ExtensionSkillElements.Geo.Value)
            return SkAssetsUtils.LoadImageFromManifestResourceAsync(SkAssetsUtils.GeoPath);
        if (value == ExtensionSkillElements.Dendro.Value)
            return SkAssetsUtils.LoadImageFromManifestResourceAsync(SkAssetsUtils.DendroPath);

        if (value == ExtensionSkillElements.Unaligned.Value)
            return SkAssetsUtils.LoadImageFromManifestResourceAsync(SkAssetsUtils.TcgActionPointCommonBlackPath);
        if (value == ExtensionSkillElements.Aligned.Value)
            return SkAssetsUtils.LoadImageFromManifestResourceAsync(SkAssetsUtils.TcgActionPointCommonWhitePath);

        if (value == ExtensionSkillElements.Energy.Value)
            return SkAssetsUtils.LoadImageFromManifestResourceAsync(SkAssetsUtils.TcgCharacterRechargePointPath);

        if (value == ExtensionSkillElements.ArcaneLegend.Value)
            return SkAssetsUtils.LoadImageFromManifestResourceAsync(SkAssetsUtils.CostSecretPath);

        throw new ArgumentException("Invalid value", nameof(value));
    }
}
