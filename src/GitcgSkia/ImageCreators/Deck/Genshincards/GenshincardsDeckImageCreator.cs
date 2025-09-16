using GitcgSharp.Shared.ImageCreators.Deck.Abstractions;
using GitcgSkia.Extensions;
using HoyolabHttpClient.Extensions;
using HoyolabHttpClient.Models;
using HoyolabHttpClient.Models.Interfaces;
using SkiaSharp;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Action = HoyolabHttpClient.Models.Action;
using SkillElements = HoyolabHttpClient.Extensions.SkillElements;

namespace GitcgSkia.ImageCreators.Deck.Genshincards;

public class GenshincardsDeckImageCreator(
    ImageCacheService imageCacheService
) : IDeckImageCreationService
{
    private static readonly SKSizeI RoleCardSize = new(128, 128);
    private static readonly SKSizeI ActionCardCroppedSize = new(420, 720 / 10);

    private static readonly int DeckWidth = ActionCardCroppedSize.Width;

    private const int ImagePaddingX = 15;
    private const int ImagePaddingY = 10;

    private const int ActionCardsY = 150;

    private const int RoleCardSpacing = 10;
    private const int ActionCardSpacing = 0;
    private const int DeckSpacing = 100;

    private static readonly int SkillValueOffsetX = 15;
    private const int ActionCardNameOffsetX = 45;
    private static readonly int QuantityOffsetX = ActionCardCroppedSize.Width - 45;

    private static readonly int ActionCardCenterOffsetY = ActionCardCroppedSize.Height / 2;

    private const int ActionCardCornerRadius = 15;

    private const int ActionCardNameWrappingLength = 250;

    private const int QuantityFontSize = 40;
    private const int CardNameFontSize = 22;
    private const int SkillValueFontSize = 28;
    private static readonly SKPointI ActionCardsPosition = new(ImagePaddingX, 150);

    private static ValueTask<SKTypeface> LoadQuantityTypefaceAsync()
        => SkAssetsUtils.LoadRobotoBoldFontAsync();
    private static ValueTask<SKTypeface> LoadActionCardTypefaceAsync()
        => SkAssetsUtils.LoadRobotoRegularFontAsync();
    private static ValueTask<SKTypeface> LoadSkillValueTypefaceAsync()
        => SkAssetsUtils.LoadRobotoBoldFontAsync();

    public GenshincardsDeckImageOptions Options { get; set; } = new();

    Task<Stream> IDeckImageCreationService.CreateImageAsync(IDeckData deckData)
        => CreateImageAsync(deckData);

    public async Task<Stream> CreateImageAsync(
        params IEnumerable<IDeckData> inputDecks
    )
    {
        var decks = inputDecks.ToArray();

        var decksCount = decks.Length;

        if (decksCount == 0)
        {
            throw new InvalidOperationException(
                "No decks to render."
            );
        }

        var imageWidth =
            DeckWidth * decksCount +
            DeckSpacing * (decksCount - 1) +
            ImagePaddingX * 2;
        var imageHeight =
            ActionCardsPosition.Y +
            ActionCardCroppedSize.Height * Utils.NumberActionCards +
            ActionCardSpacing * (Utils.NumberActionCards - 1) +
            ImagePaddingY;

        //using var image = new Image<Rgba32>(imageWidth, imageHeight);
        using var image = new SKBitmap(imageWidth, imageHeight);
        using var canvas = new SKCanvas(image);

        BackgroundColor(
            canvas: canvas,
            backgroundColor: Options.BackgroundColor
        );

        using var paint = new SKPaint
        {
            IsAntialias = true,
        };

        ICollection<Task> drawTasks = [];

        var currentPosition = SKPointI.Empty;

        // Draw role cards
        var roleCardsWidth = Utils.CalcRoleCardsWidth(
            roleCardWidth: RoleCardSize.Width,
            roleCardSpacing: RoleCardSpacing
        );
        var roleCardsOffset = DeckWidth / 2 - roleCardsWidth / 2;

        currentPosition.Y = ImagePaddingY;

        for (var i = 0; i < decksCount; i++)
        {
            var deck = decks[i];

            var indexOffset = (DeckWidth + DeckSpacing) * i;

            currentPosition.X = ImagePaddingX + indexOffset + roleCardsOffset;

            foreach (var roleCard in deck.RoleCards)
            {
                drawTasks.Add(
                    LoadAndDrawRoleCardAsync(
                        canvas: canvas,
                        roleCard: roleCard,
                        position: currentPosition,
                        paint: paint
                    ).AsTask()
                );

                currentPosition.Offset(
                    dx: RoleCardSize.Width + RoleCardSpacing,
                    dy: 0
                );
            }
        }

        var maxY = 0;
        for (var i = 0; i < decksCount; i++)
        {
            var deck = decks[i];

            var indexOffset = (DeckWidth + DeckSpacing) * i;

            currentPosition = new SKPointI(
                x: ImagePaddingX + indexOffset,
                y: ActionCardsY
            );

            var actionCards = deck.ActionCards.ToArray();
            var actionCardsCount = actionCards.Length;

            var actionCardIndex = 0;
            while (actionCardIndex < actionCardsCount)
            {
                var quantity = CalculateActionCardQuantity(
                    actionCards: actionCards,
                    actionCardIndex: actionCardIndex,
                    actionCard: out var actionCard
                );

                drawTasks.Add(
                    LoadAndDrawActionCardAsync(
                        canvas: canvas,
                        actionCard: actionCard,
                        position: currentPosition,
                        quantity: quantity,
                        paint: paint
                    ).AsTask()
                );

                actionCardIndex += quantity;

                currentPosition.Offset(
                    dx: 0,
                    dy: ActionCardCroppedSize.Height + ActionCardSpacing
                );
            }

            if (currentPosition.Y > maxY)
            {
                maxY = currentPosition.Y;
            }
        }

        await Task.WhenAll(drawTasks);

        imageHeight = maxY + ImagePaddingY;

        using var croppedImage = new SKBitmap(imageWidth, imageHeight);

        image.ExtractSubset(
            destination: croppedImage,
            subset: new SKRectI(0, 0, imageWidth, imageHeight)
        );

        var data = croppedImage.Encode(
            format: SKEncodedImageFormat.Png,
            quality: 100
        );

        return data.AsStream();
    }

    private async ValueTask LoadAndDrawRoleCardAsync(
        SKCanvas canvas,
        Role roleCard,
        SKPointI position,
        SKPaint paint
    )
    {
        using var cardImage = await imageCacheService
            .LoadIconSmallAsync(roleCard);

        using var cardResized = cardImage.Resize(
            size: RoleCardSize,
            sampling: Utils.DefaultSamplingOptions
        );

        canvas.DrawBitmap(
            bitmap: cardResized,
            p: position,
            paint: paint
        );
    }

    private async ValueTask LoadAndDrawActionCardAsync(
        SKCanvas canvas,
        Action actionCard,
        SKPointI position,
        int quantity,
        SKPaint paint
    )
    {
        using var loadedCard = await LoadActionCardAsync(actionCard);

        // Draw card image
        canvas.DrawBitmap(
            bitmap: loadedCard,
            p: position,
            paint: paint
        );

        var actionCardCenterY = position.Y + ActionCardCenterOffsetY;

        SKSizeI cardNameOffset = new(
            width: ActionCardNameOffsetX,
            height: ActionCardCenterOffsetY
        );

        //var cardNameTextOptions = new RichTextOptions(ActionCardNameFont)
        //{
        //    Origin = cardNamePosition,
        //    WrappingLength = ActionCardNameWrappingLength,
        //    VerticalAlignment = VerticalAlignment.Center,
        //    LineSpacing = 1.2f
        //};
        var cardNamePosition = position + cardNameOffset;

        using var cardNameBitmap = new SKBitmap(
            width: ActionCardNameWrappingLength,
            height: 100
        );
        using var cardNameCanvas = new SKCanvas(cardNameBitmap);

        using var cardNameFont = new SKFont
        {
            Typeface = await SkAssetsUtils.LoadRobotoRegularFontAsync(),
            Size = CardNameFontSize,
        };

        using var cardNamePaint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.White,
        };

        List<string> cardNameRuns = [];
        var cardNameWords = actionCard.Basic.Name
            .Split(' ').ToArray();

        var left = 0;
        while (left < cardNameWords.Length)
        {
            var width = 0f;
            var right = left;

            while (width < ActionCardNameWrappingLength && right < cardNameWords.Length)
            {
                right++;
                var run = string.Join(" ", cardNameWords[left..right]);

                cardNameFont.MeasureText(
                    text: run,
                    paint: cardNamePaint,
                    bounds: out var b
                );

                if (b.Width > ActionCardNameWrappingLength)
                {
                    cardNameRuns.Add(string.Join<string>(
                        separator: ' ',
                        values: cardNameWords[left..(right - 1)]
                    ));
                    break;
                }

                if (right >= cardNameWords.Length)
                {
                    right++;
                    cardNameRuns.Add(string.Join<string>(
                        separator: ' ',
                        values: cardNameWords[left..(right - 1)]
                    ));
                    break;
                }
            }
            left = right - 1;
        }

        var cardNameRunOffsetY = -2f;
        foreach (var run in cardNameRuns)
        {
            cardNameRunOffsetY += CardNameFontSize;
            cardNameCanvas.DrawText(
                text: run,
                p: new SKPoint(0, cardNameRunOffsetY),
                font: cardNameFont,
                paint: cardNamePaint
            );
        }

        var cardNameBitmapHeight = (int)cardNameRunOffsetY + 6;

        canvas.DrawBitmap(
            bitmap: cardNameBitmap,
            source: new SKRectI(0, 0, cardNameBitmap.Width, cardNameBitmapHeight),
            dest: SKRectI.Create(
                cardNamePosition.X,
                actionCardCenterY - cardNameBitmapHeight / 2,
                ActionCardNameWrappingLength,
                cardNameBitmapHeight
            ),
            paint: paint
        );

        // Draw skill values
        const int SkillElementSize = 20;

        const int SkillValueTotalHeight =
            SkillValueFontSize + SkillElementSize;

        var (skillValueOffsetX, skillValueOffsetY) = (
            SkillValueOffsetX,
            ActionCardCenterOffsetY - SkillValueTotalHeight / 2
        );

        var skillValueOffset = new SKSizeI(
            width: skillValueOffsetX,
            height: skillValueOffsetY
        );

        var skillValuePosition = position + skillValueOffset;

        var skillElement = actionCard.GetSkillElement();

        using var SkillValueFont = new SKFont
        {
            Typeface = await LoadSkillValueTypefaceAsync(),
            Size = SkillValueFontSize,
        };

        using var SkillValueFontPaint = new SKPaint
        {
            IsAntialias = true,
            Color = skillElement.GetColor(),
        };

        if (ShouldRenderSecondarySkillValue(actionCard, out var skillElement2))
        {
            using var skillValue2Paint = new SKPaint
            {
                IsAntialias = true,
                Color = skillElement2.GetColor(),
            };

            canvas.DrawText(
                text: $"{actionCard.SkillValue}",
                p: skillValuePosition - new SKSizeI(11, 24),
                font: SkillValueFont,
                paint: SkillValueFontPaint
            );
            canvas.DrawText(
                text: $"{actionCard.SkillValue2}",
                p: skillValuePosition + new SKSizeI(9, 24),
                font: SkillValueFont,
                paint: skillValue2Paint
            );
        }
        else
        {
            canvas.DrawText(
                text: $"{actionCard.SkillValue}",
                p: skillValuePosition + new SKSizeI(0, 24),
                font: SkillValueFont,
                paint: SkillValueFontPaint
            );
        }

        // draw skill elements
        using var elementIcon = await skillElement.LoadElementIconAsync();
        using var resizedElementIcon = elementIcon.Resize(
            size: new SKSizeI(
                width: SkillElementSize,
                height: SkillElementSize),
            sampling: Utils.DefaultSamplingOptions
        );

        if (ShouldRenderSecondarySkillElement(actionCard, out skillElement2))
        {
            const int IconsWidth = SkillElementSize * 2 + 3;

            var SkillElementIconsOffset = new SKSizeI(
                width: 8 - (IconsWidth / 2),
                height: SkillValueFontSize
            );
            var skillElementPosition = skillValuePosition + SkillElementIconsOffset;

            var skillElementIconOffset2 = new SKSizeI(
                width: SkillElementSize,
                height: 0
            );
            var skillElementPosition2 = skillElementPosition + skillElementIconOffset2;

            using var elementIcon2 = await skillElement2.LoadElementIconAsync();

            using var resizedElementIcon2 = elementIcon2.Resize(
                size: new SKSizeI(
                    width: SkillElementSize,
                    height: SkillElementSize),
                sampling: Utils.DefaultSamplingOptions
            );

            canvas.DrawBitmap(
                bitmap: resizedElementIcon,
                p: skillElementPosition,
                paint: paint
            );
            canvas.DrawBitmap(
                bitmap: resizedElementIcon2,
                p: skillElementPosition2,
                paint: paint
            );
        }
        else
        {
            var skillValueElementIconOffset = new SKSizeI(
                width: -3,
                height: SkillValueFontSize
            );

            var skillValueElementIconPosition = skillValuePosition + skillValueElementIconOffset;

            canvas.DrawBitmap(
                bitmap: resizedElementIcon,
                p: skillValueElementIconPosition,
                paint: paint
            );
        }

        // Draw quantity
        var quantityPosition = position + new SKSizeI(
            width: QuantityOffsetX,
            height: ActionCardCenterOffsetY
        );

        using var quantityFont = new SKFont
        {
            Typeface = await LoadQuantityTypefaceAsync(),
            Size = QuantityFontSize,
        };


        using var quantityPaint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.Black,
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = 3f
        };

        canvas.DrawText(
            text: $"×{quantity}",
            p: quantityPosition + new SKPointI(0, 16),
            font: quantityFont,
            paint: quantityPaint
        );

        quantityPaint.Color = SKColors.White;
        quantityPaint.Style = SKPaintStyle.Fill;

        canvas.DrawText(
            text: $"×{quantity}",
            p: quantityPosition + new SKPointI(0, 16),
            font: quantityFont,
            paint: quantityPaint
        );
    }

    private async ValueTask<SKBitmap> LoadActionCardAsync(
        Action actionCard
    )
    {
        var cardImage = await imageCacheService.LoadIconAsync(actionCard);

        var cardWidth = ActionCardCroppedSize.Width;
        var cardHeight = cardImage.Height * cardWidth / cardImage.Width;

        var cropPosition = new SKPointI(
            x: 0,
            y: cardHeight / 3
        );

        var brushColor = actionCard.IsArcaneLegend() ?
            SKColors.Purple : SKColors.Black;

        using var shader = SKShader.CreateLinearGradient(
            start: new SKPoint(x: 0, y: 0),
            end: new SKPoint(x: cardWidth * 1.5f, y: 0),
            colors: [brushColor, SKColor.Empty],
            colorPos: [0, 1],
            mode: SKShaderTileMode.Clamp
        );

        var bitmap = new SKBitmap(
            width: ActionCardCroppedSize.Width,
            height: ActionCardCroppedSize.Height
        );

        using var canvas = new SKCanvas(bitmap);

        using var resized = cardImage.Resize(
            size: new SKSizeI(cardWidth, cardHeight),
            sampling: Utils.DefaultSamplingOptions
        );

        using var paint = new SKPaint
        {
            IsAntialias = true,
        };

        var destRect = SKRect.Create(
            x: 0,
            y: 0,
            width: ActionCardCroppedSize.Width,
            height: ActionCardCroppedSize.Height
        );

        canvas.ClipRoundRect(
            rect: new(destRect, ActionCardCornerRadius),
            antialias: true
        );

        canvas.DrawBitmap(
            bitmap: resized,
            source: SKRect.Create(cropPosition, ActionCardCroppedSize),
            dest: destRect,
            paint: paint
        );

        using var gradientPaint = new SKPaint
        {
            IsAntialias = true,
            Shader = shader
        };

        canvas.DrawPaint(gradientPaint);

        return bitmap;
    }

    private static int CalculateActionCardQuantity(
        Action[] actionCards, int actionCardIndex,
        out Action actionCard)
    {
        actionCard = actionCards[actionCardIndex];

        var quantity = 0;

        var actionCardsCount = actionCards.Length;
        var actionCardId = actionCard.Basic.ItemId;

        Action nextActionCard;
        int nextActionCardIndex;

        do
        {
            quantity++;

            nextActionCardIndex = actionCardIndex + quantity;
            if (nextActionCardIndex >= actionCardsCount)
            {
                break;
            }

            nextActionCard = actionCards[actionCardIndex + quantity];
        } while (nextActionCard.Basic.ItemId == actionCardId);

        return quantity;
    }

    private static void BackgroundColor(
        SKCanvas canvas,
        string backgroundColor
    )
    {
        if (!SKColor.TryParse(backgroundColor, out var color))
        {
            throw new InvalidOperationException("Invalid background color.");
        }

        canvas.Clear(color);
    }

    private static bool ShouldRenderSecondarySkillElement(
        Action actionCard,
        [NotNullWhen(true)]
        out SkillElement? skillElement2
    )
    {
        skillElement2 = actionCard.GetSkillElement2();
        if (skillElement2 == null)
        {
            return false;
        }

        return true;
    }

    private static bool ShouldRenderSecondarySkillValue(
        Action actionCard,
        [NotNullWhen(true)]
        out SkillElement? skillElement2
    )
    {
        skillElement2 = actionCard.GetSkillElement2();
        if (skillElement2 == null)
        {
            return false;
        }

        var arcaneLegend = SkillElements.ArcaneLegend;
        if (skillElement2 == arcaneLegend)
        {
            return false;
        }

        return true;
    }
}