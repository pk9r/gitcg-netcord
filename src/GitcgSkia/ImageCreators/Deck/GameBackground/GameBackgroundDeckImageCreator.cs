using System.Drawing;
using GitcgSharp.Shared.ImageCreators.Deck.Abstractions;
using GitcgSkia.Extensions;
using HoyolabHttpClient.Models.Interfaces;
using SkiaSharp;

namespace GitcgSkia.ImageCreators.Deck.GameBackground;

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

    private static readonly SKSizeI RoleCardSize = new(150, 257);
    private static readonly SKSizeI ActionCardSize = new(100, 171);

    private static readonly SKPoint AuthorLocation = new(160, 1510);

    public GameBackgroundDeckImageOptions Options { get; set; } = new();

    public async Task<Stream> CreateImageAsync(IDeckData deckData)
    {
        var background = await SkAssetsUtils
            .LoadBackgroundDeckGameAsync();

        using var image = new SKBitmap(background.Info);
        using var canvas = new SKCanvas(image);

        canvas.DrawBitmap(background, 0, 0);
        background.Dispose();

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
        var currentPosition = SKPoint.Empty;

        // Draw role cards
        currentPosition.Offset(
            dx: xRolesBase,
            dy: yRolesBase
        );
        foreach (var roleCard in deckData.RoleCards)
        {
            drawTasks.Add(
                item: LoadAndDrawCardAsync(
                    image: canvas,
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

        currentPosition = new SKPoint(
            x: xActionsBase,
            y: yActionsBase
        );
        foreach (var actionCard in deckData.ActionCards)
        {
            drawTasks.Add(
                item: LoadAndDrawCardAsync(
                    image: canvas,
                    position: currentPosition,
                    card: actionCard,
                    size: ActionCardSize
                )
            );

            // update position
            actionCardIndex++;
            var (dx, dy) = (0, 0);
            if (actionCardIndex % NColumns != 0)
            {
                dx = ActionCardSize.Width + ActionCardSpacing;
            }
            else
            {
                dy = ActionCardSize.Height + ActionCardSpacing;
                currentPosition.X = xActionsBase;

            }
            currentPosition.Offset(dx, dy);
        }

        if (!string.IsNullOrWhiteSpace(Options.Author))
        {
            using var authorPaint = new SKPaint();
            authorPaint.Color = Color.White.ToSkColor();
            authorPaint.IsAntialias = true;

            using var authorFont = new SKFont
            {
                Typeface = await SkAssetsUtils.LoadAntonRegularFontAsync(),
                Size = AuthorFontSize
            };

            canvas.DrawText(
                text: Options.Author,
                p: AuthorLocation,
                font: authorFont,
                paint: authorPaint
            );
        }

        await Task.WhenAll(tasks: drawTasks);

        // Encode image to PNG
        var data = image.Encode(
            format: SKEncodedImageFormat.Png,
            quality: 100
        );

        return data.AsStream();
    }

    private async Task LoadAndDrawCardAsync(
        SKCanvas image, SKPoint position,
        ICardBasic card, SKSizeI size
    )
    {
        using var cachedCardImage =
            await LoadBorderedIconAsync(
                card: card,
                size: size
            );

        image.DrawBitmap(
            bitmap: cachedCardImage,
            p: position,
            paint: new SKPaint
            {
                IsAntialias = true
            }
        );
    }

    private async Task<SKBitmap> LoadBorderedIconAsync(
        ICardBasic card, SKSizeI size
    )
    {
        var cacheKey = card.GetCacheKeyIconBordered(size);

        var cachedBorderedResizedIcon =
            await imageCacheService.LoadCachedImageAsync(cacheKey);

        if (cachedBorderedResizedIcon != null)
            return cachedBorderedResizedIcon;

        var cachedBorderedIcon = await imageCacheService
            .LoadBorderedIconAsync(card);

        var borderedResizedIcon = ResizeIcon(cachedBorderedIcon, size);

        cachedBorderedResizedIcon = await imageCacheService
            .CacheImageAsync(cacheKey, borderedResizedIcon);

        return cachedBorderedResizedIcon;
    }

    private static SKBitmap ResizeIcon(SKBitmap icon, SKSizeI size)
    {
        var samplingOptions = new SKSamplingOptions(
            filter: SKFilterMode.Linear,
            mipmap: SKMipmapMode.Linear
        );

        var iconResized = icon.Resize(
            size: size,
            sampling: samplingOptions
        );

        return iconResized;
    }
}