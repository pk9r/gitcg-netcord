using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

using ExtensionSkillElements = HoyolabHttpClient.Extensions.SkillElements;

namespace GitcgPainter.Extensions;

public static class SkillElements
{
    public static IDictionary<string, Color> Colors { get; }

    static SkillElements()
    {
        Colors = new Dictionary<string, Color>
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

    public static Task<Image> LoadElementIconAsync(this string value)
    {
        if (value == ExtensionSkillElements.Cyro.Value)
            return Utils.LoadImageFromManifestResourceAsync(Utils.CryoPath);
        if (value == ExtensionSkillElements.Hydro.Value)
            return Utils.LoadImageFromManifestResourceAsync(Utils.HydroPath);
        if (value == ExtensionSkillElements.Pyro.Value)
            return Utils.LoadImageFromManifestResourceAsync(Utils.PyroPath);
        if (value == ExtensionSkillElements.Electro.Value)
            return Utils.LoadImageFromManifestResourceAsync(Utils.ElectroPath);
        if (value == ExtensionSkillElements.Anemo.Value)
            return Utils.LoadImageFromManifestResourceAsync(Utils.AnemoPath);
        if (value == ExtensionSkillElements.Geo.Value)
            return Utils.LoadImageFromManifestResourceAsync(Utils.GeoPath);
        if (value == ExtensionSkillElements.Dendro.Value)
            return Utils.LoadImageFromManifestResourceAsync(Utils.DendroPath);
        
        if (value == ExtensionSkillElements.Unaligned.Value)
            return Utils.LoadImageFromManifestResourceAsync(Utils.TcgActionPointCommonBlackPath);
        if (value == ExtensionSkillElements.Aligned.Value)
            return Utils.LoadImageFromManifestResourceAsync(Utils.TcgActionPointCommonWhitePath);
        
        if (value == ExtensionSkillElements.Energy.Value)
            return Utils.LoadImageFromManifestResourceAsync(Utils.TcgCharacterRechargePointPath);
        
        if (value == ExtensionSkillElements.ArcaneLegend.Value)
            return Utils.LoadImageFromManifestResourceAsync(Utils.CostSecretPath);
        
        throw new ArgumentException("Invalid value", nameof(value));
    }
}
