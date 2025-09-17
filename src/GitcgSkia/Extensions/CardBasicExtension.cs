using HoyolabHttpClient.Models.Interfaces;
using SkiaSharp;

namespace GitcgSkia.Extensions;

public static class CardBasicExtension
{
    public static string GetCacheKeyBorderedIcon(this ICardBasic card)
    {
        return $"basic_{card.Basic.ItemId}" +
               $"_icon_bordered";
    }

    public static string GetCacheKeyIconBordered(
        this ICardBasic card, SKSizeI size
    )
    {
        return $"basic_{card.Basic.ItemId}" +
               $"_icon_bordered" +
               $"_{size.Width}x{size.Height}";
    }
}
