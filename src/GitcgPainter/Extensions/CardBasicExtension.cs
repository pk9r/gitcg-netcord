using HoyolabHttpClient.Models.Interfaces;
using SixLabors.ImageSharp;

namespace GitcgPainter.Extensions;

public static class CardBasicExtension
{
    public static string GetCacheKeyBorderedIcon(this ICardBasic card)
    {
        return $"basic_{card.Basic.ItemId}_icon_bordered";
    }

    public static string GetCacheKeyIconBordered(this ICardBasic card, Size size)
    {
        return $"basic_{card.Basic.ItemId}_icon_bordered_{size.Width}x{size.Height}";
    }
}
