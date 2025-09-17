using HoyolabHttpClient.Models;
using SkiaSharp;
using ExtensionSkillElements = HoyolabHttpClient.Extensions.SkillElements;

namespace GitcgSkia.Extensions;

public static class SkillElementExtension
{
    public static SKColor GetColor(this SkillElement? skillElement)
    {
        skillElement ??= ExtensionSkillElements.Aligned;

        return SkillElements.Colors[skillElement.Value];
    }

    public static ValueTask<SKBitmap> LoadElementIconAsync(
        this SkillElement? skillElement
    )
    {
        skillElement ??= ExtensionSkillElements.Aligned;

        return skillElement.Value.LoadElementIconAsync();
    }
}