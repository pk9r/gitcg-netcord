using System.Diagnostics.CodeAnalysis;
using HoyolabHttpClient.Extensions;
using HoyolabHttpClient.Models;
using HoyolabHttpClient.Models.Interfaces;
using SkiaSharp;
using Action = HoyolabHttpClient.Models.Action;

namespace GitcgSkia.Extensions;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class ImageCacheServiceExtension
{
    public static SKSizeI BorderSize { get; } = new(300, 514);

    public static ValueTask<SKBitmap>
        LoadIconSmallAsync(
            this ImageCacheService imageCacheService,
            Role role
        )
    {
        return imageCacheService.LoadHttpImageAsync(
            uri: role.Basic.IconSmall
        );
    }

    public static ValueTask<SKBitmap> LoadIconAsync(
        this ImageCacheService imageCacheService,
        Basic basic
    )
    {
        return imageCacheService.LoadHttpImageAsync(
            uri: basic.Icon
        );
    }

    public static ValueTask<SKBitmap>
        LoadIconAsync(
            this ImageCacheService imageCacheService,
            ICardBasic card
        )
    {
        return imageCacheService.LoadIconAsync(card.Basic);
    }

    public static async ValueTask<SKBitmap>
        LoadBorderedIconAsync(
            this ImageCacheService imageCacheService,
            ICardBasic card
        )
    {
        var cacheKey = card.GetCacheKeyBorderedIcon();

        var cachedBorderedIcon = await imageCacheService
            .LoadCachedImageAsync(key: cacheKey);

        if (cachedBorderedIcon != null) return cachedBorderedIcon;

        SKBitmap cachedBorder;
        if (IsArcaneCard(card: card))
            cachedBorder = await imageCacheService.LoadArcaneBorderAsync();
        else
            cachedBorder = await SkAssetsUtils.LoadCardBorderAsync();

        using (cachedBorder)
        {
            var cachedIcon = await imageCacheService.LoadIconAsync(card: card);

            var borderedIcon = BorderIcon(icon: cachedIcon, border: cachedBorder);

            cachedBorderedIcon = await imageCacheService
                .CacheImageAsync(key: cacheKey, image: borderedIcon);

            return cachedBorderedIcon;
        }
    }

    public static async ValueTask<SKBitmap> LoadArcaneBorderAsync(
        this ImageCacheService imageCacheService
    )
    {
        var cachedArcaneBorder = await imageCacheService
            .LoadCachedImageAsync(key: Utils.ArcaneBorderCacheKey);

        if (cachedArcaneBorder != null) return cachedArcaneBorder;

        using var arcaneBorder = await SkAssetsUtils.LoadArcaneBorderAsync();

        // Resize border
        using var arcaneBorderResized = arcaneBorder.Resize(
            size: BorderSize,
            sampling: new SKSamplingOptions(
                filter: SKFilterMode.Linear,
                mipmap: SKMipmapMode.Linear
            )
        );

        await imageCacheService.CacheImageAsync(
            key: Utils.ArcaneBorderCacheKey,
            image: arcaneBorderResized
        );
        cachedArcaneBorder = arcaneBorder;

        return cachedArcaneBorder;
    }

    private static bool IsArcaneCard(ICardBasic card)
    {
        return
            card is Action actionCard &&
            actionCard.IsArcaneLegend();
    }

    private static SKBitmap BorderIcon(
        SKBitmap icon,
        SKBitmap border
    )
    {
        var bitmap = new SKBitmap(border.Info);
        using var canvas = new SKCanvas(bitmap);

        using var iconResized = icon.Resize(
            size: border.Info.Size,
            sampling: new SKSamplingOptions(
                filter: SKFilterMode.Linear,
                mipmap: SKMipmapMode.Linear
            )
        );

        canvas.DrawBitmap(
            bitmap: iconResized,
            x: 0f, y: 0f
        );

        canvas.DrawBitmap(
            bitmap: border,
            x: 0f, y: 0f
        );

        return bitmap;
    }
}