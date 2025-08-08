using System;
using System.IO;
using System.Threading.Tasks;
using GitcgPainter.Extensions;
using GitcgPainter.ImageCreators.Deck.Abstractions;
using HoyolabHttpClient.Models.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GitcgPainter.ImageCreators.Deck.Simplest;

public class SimplestDeckImageCreator(
    ImageCacheService imageCacheService)
    : IDeckImageCreationService
{
    #region Constants
    private const int ImagePadding = 10;
    private const int RoleCardHorizontalGap = 40;
    private const int ActionCardGap = 30;
    #endregion

    private Size RoleCardSize { get; } = new(150, 257);

    private Size ActionCardSize { get; } = new(105, 180);

    private Point ActionCardsPosition { get; } = new(10, 300);

    private readonly ImageCacheService _imageCacheService = imageCacheService;

    public SimplestDeckImageOptions Options { get; set; } = new();

    public async Task<byte[]> CreateImageAsync(IDeckData deck)
    {
        var actionCardColumns = Options.ActionCardColumns;
        var actionCardRows = (int)Math.Ceiling((double)deck.ActionCards.Count / actionCardColumns);

        var roleCardsWidth = RoleCardSize.Width * Utils.NumberRoleCards + RoleCardHorizontalGap * (Utils.NumberRoleCards - 1);

        var actionCardsWidth = ActionCardSize.Width * actionCardColumns + ActionCardGap * (actionCardColumns - 1);
        var actionCardsHeight = ActionCardSize.Height * actionCardRows + ActionCardGap * (actionCardRows - 1);

        var minWidth = roleCardsWidth + ImagePadding * 2;
        var widthImage = actionCardsWidth + ImagePadding * 2;
        if (widthImage < minWidth)
        {
            widthImage = minWidth;
        }

        var heightImage = ActionCardsPosition.Y + actionCardsHeight + ImagePadding;

        using var image = new Image<Rgba32>(widthImage, heightImage);

        // Fill background
        if (!Color.TryParse(Options.BackgroundColor, out var backgroundColorResult))
        {
            backgroundColorResult = Color.Transparent;
        }

        image.Mutate(ctx => ctx.BackgroundColor(backgroundColorResult));

        // Draw role cards
        var roleCardsX = widthImage / 2 - roleCardsWidth / 2;
        var roleCardsY = ImagePadding;

        Point currentPosition;

        currentPosition = new Point(roleCardsX, roleCardsY);
        foreach (var roleCard in deck.RoleCards)
        {
            using var cardImage = await _imageCacheService.LoadBorderedIconAsync(roleCard);

            if (cardImage is not null)
            {
                cardImage.Mutate(context => context.Resize(RoleCardSize));

                image.Mutate(ctx => ctx
                    .DrawImage(cardImage, currentPosition, opacity: 1.0f));
            }

            currentPosition.Offset(RoleCardSize.Width + RoleCardHorizontalGap, 0);
        }

        // Draw action cards
        var index = 0;

        currentPosition = ActionCardsPosition;
        foreach (var actionCard in deck.ActionCards)
        {
            using var cardImage = await _imageCacheService.LoadBorderedIconAsync(actionCard);
            if (cardImage is not null)
            {
                cardImage.Mutate(context => context.Resize(ActionCardSize));

                image.Mutate(ctx => ctx
                    .DrawImage(cardImage, currentPosition, opacity: 1.0f));
            }

            index++;
            if (index % actionCardColumns == 0)
            {
                currentPosition.Offset(0, ActionCardSize.Height + ActionCardGap);
                currentPosition.X = ActionCardsPosition.X;
            }
            else
            {
                currentPosition.Offset(ActionCardSize.Width + ActionCardGap, 0);
            }
        }

        // Save image to PNG
        using var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream);
        memoryStream.Position = 0;
        var pngBytes = memoryStream.ToArray();

        return pngBytes;
    }
}
