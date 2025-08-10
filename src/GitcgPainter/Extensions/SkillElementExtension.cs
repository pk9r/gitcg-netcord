using System.Threading.Tasks;
using HoyolabHttpClient.Models;
using SixLabors.ImageSharp;
using ExtensionSkillElements = HoyolabHttpClient.Extensions.SkillElements;

namespace GitcgPainter.Extensions;

public static class SkillElementExtension
{
    public static Color GetColor(this SkillElement? skillElement)
    {
        skillElement ??= ExtensionSkillElements.Aligned;

        return SkillElements.Colors[skillElement.Value];
    }

    public static Task<Image> LoadElementIconAsync(
        this SkillElement? skillElement
    )
    {
        skillElement ??= ExtensionSkillElements.Aligned;

        return skillElement.Value.LoadElementIconAsync();
    }
}