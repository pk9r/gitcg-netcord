using System.Threading.Tasks;
using HoyolabHttpClient.Extensions;
using HoyolabHttpClient.Models;
using HoyolabHttpClient.Models.Interfaces;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace GitcgPainter.Extensions;

public static class ImageCacheServiceExtension
{
    public static Size BorderSize { get; } = new(300, 514);

    public static Task<Image> LoadIconSmallAsync(
        this ImageCacheService imageCacheService, Role role)
            => imageCacheService.LoadHttpImageAsync(role.Basic.IconSmall);

    public static Task<Image> LoadIconAsync(
        this ImageCacheService imageCacheService, Basic basic)
            => imageCacheService.LoadHttpImageAsync(basic.Icon);

    public static Task<Image> LoadIconAsync(
        this ImageCacheService imageCacheService, ICardBasic card)
            => imageCacheService.LoadIconAsync(card.Basic);

    public static async Task<Image> LoadBorderedIconAsync(
        this ImageCacheService imageCacheService, ICardBasic card)
    {
        var cacheKey = card.GetCacheKeyBorderedIcon();

        var cachedBorderedIcon =
            await imageCacheService.LoadCachedImageAsync(cacheKey);

        if (cachedBorderedIcon != null) return cachedBorderedIcon;

        Image? cachedBorder;
        if (IsArcaneCard(card))
            cachedBorder = await imageCacheService.LoadArcaneBorderAsync();
        else
            cachedBorder = await Utils.LoadCardBorderAsync();

        using (cachedBorder)
        {
            var cachedIcon = await imageCacheService.LoadIconAsync(card);

            var borderedIcon = BorderIcon(cachedIcon, cachedBorder);

            cachedBorderedIcon = await imageCacheService
                .CacheImageAsync(cacheKey, borderedIcon);

            return cachedBorderedIcon;
        }
    }

    public static async Task<Image> LoadArcaneBorderAsync(this ImageCacheService imageCacheService)
    {
        var cachedArcaneBorder = await imageCacheService
            .LoadCachedImageAsync(Utils.ArcaneBorderCacheKey);

        if (cachedArcaneBorder != null) return cachedArcaneBorder;

        var arcaneBorder = await Utils.LoadArcaneBorderAsync();

        // Resize border
        arcaneBorder.Mutate(ctx => ctx.Resize(BorderSize));

        await imageCacheService.CacheImageAsync(
            Utils.ArcaneBorderCacheKey, arcaneBorder);
        cachedArcaneBorder = arcaneBorder;

        return cachedArcaneBorder;
    }

    private static bool IsArcaneCard(ICardBasic card)
    {
        return 
            card is Action actionCard && 
            actionCard.IsArcaneLegend();
    }

    private static Image BorderIcon(Image icon, Image border)
    {
        icon.Mutate(ctx =>
        {
            ctx.Resize(border.Size)
               .DrawImage(border, opacity: 1.0f);
        });

        return icon;
    }

}
