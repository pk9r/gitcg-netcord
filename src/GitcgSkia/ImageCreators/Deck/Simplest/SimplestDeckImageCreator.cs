using GitcgSharp.Shared.ImageCreators.Deck.Abstractions;
using GitcgSkia.Extensions;
using HoyolabHttpClient.Models.Interfaces;
using SkiaSharp;

namespace GitcgSkia.ImageCreators.Deck.Simplest;

public class SimplestDeckImageCreator(
    ImageCacheService imageCacheService
) : IDeckImageCreator<SimplestDeckImageOptions>
{
    #region Constants

    private const int ImagePadding = 10;
    private const int RoleCardHorizontalGap = 40;
    private const int ActionCardGap = 30;

    private static readonly SKSizeI RoleCardSize = new(150, 257);
    private static readonly SKSizeI ActionCardSize = new(105, 180);
    private static readonly SKPointI ActionCardsPosition = new(10, 300);

    #endregion

    public SimplestDeckImageOptions Options { get; set; } = new();

    public Task<Stream> CreateImageAsync(IDeckData deckData)
        => CreateImageAsync(deckData, Options);

    public async Task<Stream> CreateImageAsync(
        IDeckData deckData,
        SimplestDeckImageOptions? options = null
    )
    {
        options ??= Options;
        var actionCardColumns = options.ActionCardColumns;
        if (actionCardColumns <= 0)
        {
            actionCardColumns = 10;
        }

        var actionCardRows = (int)Math.Ceiling((double)deckData.ActionCards.Count / actionCardColumns);

        var roleCardsWidth = RoleCardSize.Width * Utils.NumberRoleCards +
                             RoleCardHorizontalGap * (Utils.NumberRoleCards - 1);

        var actionCardsWidth = ActionCardSize.Width * actionCardColumns + ActionCardGap * (actionCardColumns - 1);
        var actionCardsHeight = ActionCardSize.Height * actionCardRows + ActionCardGap * (actionCardRows - 1);

        var minWidth = roleCardsWidth + ImagePadding * 2;
        var widthImage = actionCardsWidth + ImagePadding * 2;
        if (widthImage < minWidth)
        {
            widthImage = minWidth;
        }

        var heightImage = ActionCardsPosition.Y + actionCardsHeight + ImagePadding;

        using var image = new SKBitmap(widthImage, heightImage);
        using var canvas = new SKCanvas(image);

        // Fill background
        if (SKColor.TryParse(options.BackgroundColor, out var backgroundColorResult))
        {
            canvas.Clear(backgroundColorResult);
        }
        else
        {
            canvas.Clear(SKColors.Transparent);
        }

        // Preload all role and action card bitmaps concurrently
        var roleCardTasks = deckData.RoleCards
            .Select(roleCard => imageCacheService.LoadBorderedIconAsync(roleCard, RoleCardSize).AsTask())
            .ToArray();

        var actionCardTasks = deckData.ActionCards
            .Select(actionCard => imageCacheService.LoadBorderedIconAsync(actionCard, ActionCardSize).AsTask())
            .ToArray();

        var roleBitmaps = await Task.WhenAll(roleCardTasks);
        var actionBitmaps = await Task.WhenAll(actionCardTasks);

        using var paint = new SKPaint
        {
            IsAntialias = true
        };

        // Draw role cards
        var roleCardsX = widthImage / 2 - roleCardsWidth / 2;
        var roleCardsY = ImagePadding;

        var currentX = (float)roleCardsX;
        var currentY = (float)roleCardsY;

        foreach (var cardBitmap in roleBitmaps)
        {
            canvas.DrawBitmap(
                bitmap: cardBitmap,
                x: currentX,
                y: currentY,
                sampling: Utils.DefaultSamplingOptions,
                paint: paint
            );

            currentX += RoleCardSize.Width + RoleCardHorizontalGap;
        }

        // Draw action cards
        currentX = ActionCardsPosition.X;
        currentY = ActionCardsPosition.Y;
        var index = 0;

        foreach (var cardBitmap in actionBitmaps)
        {
            canvas.DrawBitmap(
                bitmap: cardBitmap,
                x: currentX,
                y: currentY,
                sampling: Utils.DefaultSamplingOptions,
                paint: paint
            );

            index++;
            if (index % actionCardColumns != 0)
            {
                currentX += ActionCardSize.Width + ActionCardGap;
            }
            else
            {
                currentX = ActionCardsPosition.X;
                currentY += ActionCardSize.Height + ActionCardGap;
            }
        }

        // Encode image to PNG
        var data = image.Encode(
            format: SKEncodedImageFormat.Png,
            quality: 100
        );

        return data.AsStream();
    }
}
