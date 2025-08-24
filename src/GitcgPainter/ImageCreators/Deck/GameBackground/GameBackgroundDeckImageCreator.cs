using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GitcgPainter.Extensions;
using GitcgPainter.ImageCreators.Deck.Abstractions;
using HoyolabHttpClient.Models.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace GitcgPainter.ImageCreators.Deck.GameBackground;

public class GameBackgroundDeckImageCreator(
    ImageCacheService imageCacheService
) : IDeckImageCreationService
{
    private const int RoleCardPaddingY = 170;
    private const int RoleCardSpacing = 40;
    private const int ActionCardPaddingY = 500;
    private const int ActionCardSpacing = 20;
    private const int NColumns = 6;

    private const float AuthorFontSize = 24;

    private static readonly Size RoleCardSize = new(150, 257);
    private static readonly Size ActionCardSize = new(100, 171);

    private static readonly Point AuthorLocation = new(160, 1510);

    private static readonly Font? AuthorFont =
        Utils.AntonRegularFontFamily.CreateFont(AuthorFontSize);

    public GameBackgroundDeckImageOptions Options { get; set; } = new();

    public async Task<Stream> CreateImageAsync(IDeckData deckData)
    {
        using var image = await Utils.LoadBackgroundDeckGameAsync();

        var xCenter = image.Width / 2;

        var roleCardsWidth = Utils.CalcRoleCardsWidth(
            roleCardWidth: RoleCardSize.Width,
            roleCardSpacing: RoleCardSpacing
        );

        var (xRolesBase, yRolesBase) = (
            xCenter - roleCardsWidth / 2,
            RoleCardPaddingY
        );

        var actionCardsWidth =
            ActionCardSize.Width * NColumns +
            ActionCardSpacing * (NColumns - 1);

        var (xActionsBase, yActionsBase) = (
            xCenter - actionCardsWidth / 2,
            ActionCardPaddingY
        );

        List<Task> drawTasks = [];
        var currentPosition = Point.Empty;

        // Draw role cards
        currentPosition.Offset(xRolesBase, yRolesBase);
        foreach (var roleCard in deckData.RoleCards)
        {
            drawTasks.Add(
                LoadAndDrawCardAsync(
                    image: image,
                    position: currentPosition,
                    card: roleCard,
                    size: RoleCardSize
                )
            );

            // update position
            currentPosition.Offset(
                dx: RoleCardSize.Width + RoleCardSpacing,
                dy: 0
            );
        }

        // Draw action cards
        var actionCardIndex = 0;

        currentPosition = new Point(xActionsBase, yActionsBase);
        foreach (var actionCard in deckData.ActionCards)
        {
            drawTasks.Add(
                LoadAndDrawCardAsync(
                    image: image,
                    position: currentPosition,
                    card: actionCard,
                    size: ActionCardSize
                )
            );

            // update position
            actionCardIndex++;
            if (actionCardIndex % NColumns != 0)
            {
                currentPosition.Offset(ActionCardSize.Width + ActionCardSpacing, 0);
            }
            else
            {
                currentPosition.X = xActionsBase;
                currentPosition.Offset(0, ActionCardSize.Height + ActionCardSpacing);
            }
        }

        if (AuthorFont != null && !string.IsNullOrWhiteSpace(Options.Author))
        {
            image.Mutate(context =>
            {
                context.DrawText(
                    text: Options.Author,
                    font: AuthorFont,
                    color: Color.White,
                    location: AuthorLocation
                );
            });
        }

        await Task.WhenAll(drawTasks);

        // Save image to PNG
        var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }

    private async Task LoadAndDrawCardAsync(
        Image image, Point position,
        ICardBasic card, Size size
    )
    {
        using var cachedCardImage = await LoadBorderedIconAsync(card, size);

        //cardImage.Mutate(ctx => ctx.Resize(size));

        image.Mutate(ctx =>
        {
            ctx.DrawImage(
                foreground: cachedCardImage,
                backgroundLocation: position,
                opacity: 1.0f
            );
        });
    }

    private async Task<Image> LoadBorderedIconAsync(ICardBasic card, Size size)
    {
        var cacheKey = card.GetCacheKeyIconBordered(size);

        var cachedBorderedResizedIcon =
            await imageCacheService.LoadCachedImageAsync(cacheKey);

        if (cachedBorderedResizedIcon != null)
            return cachedBorderedResizedIcon;

        var cachedBorderedIcon = await imageCacheService.LoadBorderedIconAsync(card);

        var borderedResizedIcon = ResizeIcon(cachedBorderedIcon, size);

        cachedBorderedResizedIcon = await imageCacheService
            .CacheImageAsync(cacheKey, borderedResizedIcon);

        return cachedBorderedResizedIcon;
    }

    private static Image ResizeIcon(Image icon, Size size)
    {
        icon.Mutate(ctx => ctx.Resize(size));
        return icon;
    }
}