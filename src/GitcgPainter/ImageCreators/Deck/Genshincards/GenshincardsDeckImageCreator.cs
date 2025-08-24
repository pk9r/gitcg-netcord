using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitcgPainter.Extensions;
using GitcgPainter.ImageCreators.Deck.Abstractions;
using HoyolabHttpClient.Extensions;
using HoyolabHttpClient.Models;
using HoyolabHttpClient.Models.Interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GitcgPainter.ImageCreators.Deck.Genshincards;

public class GenshincardsDeckImageCreator(
    ImageCacheService imageCacheService
) : IDeckImageCreationService
{
    private static readonly Size RoleCardSize = new(128, 128);
    private static readonly Size ActionCardCroppedSize = new(420, 720 / 10);

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
    private static readonly Point ActionCardsPosition = new(ImagePaddingX, 150);

    private static readonly Font QuantityFont =
        Utils.RobotoBoldFontFamily.CreateFont(QuantityFontSize);

    private static readonly Font ActionCardNameFont =
        Utils.RobotoRegularFontFamily.CreateFont(CardNameFontSize);

    private static readonly Font SkillValueFont =
        Utils.RobotoBoldFontFamily.CreateFont(SkillValueFontSize);

    public GenshincardsDeckImageOptions Options { get; set; } = new();

    Task<Stream> IDeckImageCreationService.CreateImageAsync(IDeckData deckData)
        => CreateImageAsync(deckData);

    public async Task<Stream> CreateImageAsync(
        params IEnumerable<IDeckData> inputDecks)
    {
        var decks = inputDecks.ToArray();

        var decksCount = decks.Length;

        if (decksCount == 0)
        {
            throw new InvalidOperationException("No decks to render.");
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

        using var image = new Image<Rgba32>(imageWidth, imageHeight);

        BackgroundColor(image, Options.BackgroundColor);

        ICollection<Task> drawTasks = [];

        var currentPosition = Point.Empty;

        // Draw role cards
        var roleCardsWidth = Utils.CalcRoleCardsWidth(RoleCardSize.Width, RoleCardSpacing);
        var roleCardsOffset = DeckWidth / 2 - roleCardsWidth / 2;

        //currentPosition = new Point(ImagePaddingX, ImagePaddingY);
        currentPosition.Y = ImagePaddingY;

        for (var i = 0; i < decksCount; i++)
        {
            var deck = decks[i];

            var indexOffset = (DeckWidth + DeckSpacing) * i;

            currentPosition.X = ImagePaddingX + indexOffset + roleCardsOffset;

            foreach (var roleCard in deck.RoleCards)
            {
                drawTasks.Add(LoadAndDrawRoleCardAsync(
                    image, roleCard, currentPosition));

                currentPosition.Offset(RoleCardSize.Width + RoleCardSpacing, 0);
            }
        }

        var maxY = 0;
        for (var i = 0; i < decksCount; i++)
        {
            var deck = decks[i];

            var indexOffset = (DeckWidth + DeckSpacing) * i;

            currentPosition = new Point(
                ImagePaddingX + indexOffset, ActionCardsY);

            var actionCards = deck.ActionCards.ToArray();
            var actionCardsCount = actionCards.Length;

            var actionCardIndex = 0;
            while (actionCardIndex < actionCardsCount)
            {
                var quantity = CalculateActionCardQuantity(
                    actionCards, actionCardIndex, out var actionCard);

                drawTasks.Add(LoadAndDrawActionCardAsync(
                    image, actionCard, currentPosition, quantity));

                actionCardIndex += quantity;

                currentPosition.Offset(0, ActionCardCroppedSize.Height + ActionCardSpacing);
            }

            ;

            if (currentPosition.Y > maxY)
            {
                maxY = currentPosition.Y;
            }
        }

        await Task.WhenAll(drawTasks);

        imageHeight = maxY + ImagePaddingY;

        image.Mutate(ctx => ctx.Crop(imageWidth, imageHeight));

        // Save image to PNG
        var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }

    private async Task LoadAndDrawRoleCardAsync(
        Image image, Role roleCard, Point position)
    {
        using var cardImage = await imageCacheService
            .LoadIconSmallAsync(roleCard);

        cardImage.Mutate(ctx => ctx.Resize(RoleCardSize));

        image.Mutate(ctx => { ctx.DrawImage(cardImage, position, opacity: 1.0f); });
    }

    private async Task LoadAndDrawActionCardAsync(
        Image image, HoyolabHttpClient.Models.Action actionCard, Point position, int quantity)
    {
        using var cardImage = await LoadActionCardAsync(actionCard);

        var actionCardCenterY = position.Y + ActionCardCenterOffsetY;

        // Draw card image
        var cardNamePosition = position +
                               new Size(ActionCardNameOffsetX, ActionCardCenterOffsetY);

        var cardNameTextOptions = new RichTextOptions(ActionCardNameFont)
        {
            Origin = cardNamePosition,
            WrappingLength = ActionCardNameWrappingLength,
            VerticalAlignment = VerticalAlignment.Center,
            LineSpacing = 1.2f
        };

        image.Mutate(ctx =>
        {
            ctx.DrawImage(cardImage, position, opacity: 1.0f)
                .DrawText(cardNameTextOptions,
                    text: actionCard.Basic.Name,
                    color: Color.White);
        });

        // Draw skill values
        const int SkillElementSize = 20;

        const int SkillValueTotalHeight =
            SkillValueFontSize + SkillElementSize;

        var skillValueOffsetX = SkillValueOffsetX;
        var skillValueOffsetY = ActionCardCenterOffsetY - SkillValueTotalHeight / 2;
        var skillValueOffset = new Size(skillValueOffsetX, skillValueOffsetY);

        var skillValuePosition = position + skillValueOffset;

        var skillElement = actionCard.GetSkillElement();

        if (ShouldRenderSecondarySkillValue(actionCard, out var skillElement2))
        {
            image.Mutate(ctx => ctx
                .DrawText($"{actionCard.SkillValue}",
                    font: SkillValueFont,
                    color: skillElement.GetColor(),
                    location: skillValuePosition - new Size(11, 0))
                .DrawText($"{actionCard.SkillValue2}",
                    font: SkillValueFont,
                    color: skillElement2.GetColor(),
                    location: skillValuePosition + new Size(9, 0)));
        }
        else
        {
            image.Mutate(ctx => ctx
                .DrawText($"{actionCard.SkillValue}",
                    font: SkillValueFont,
                    color: skillElement.GetColor(),
                    location: skillValuePosition - new Size(1, 0)));
        }

        // draw skill elements
        using var elementIcon = await skillElement.LoadElementIconAsync();
        elementIcon.Mutate(x => x.Resize(SkillElementSize, SkillElementSize));

        if (ShouldRenderSecondarySkillElement(actionCard, out skillElement2))
        {
            const int IconsWidth = SkillElementSize * 2 + 3;

            var skillElementPosition = skillValuePosition +
                                       new Size(8 - (IconsWidth / 2), SkillValueFontSize);
            var skillElementPosition2 = skillElementPosition +
                                        new Size(SkillElementSize, 0);

            using var elementIcon2 = await skillElement2.LoadElementIconAsync();
            elementIcon2.Mutate(x => x.Resize(SkillElementSize, SkillElementSize));

            image.Mutate(ctx => ctx
                .DrawImage(
                    foreground: elementIcon,
                    backgroundLocation: skillElementPosition,
                    opacity: 1.0f)
                .DrawImage(
                    foreground: elementIcon2,
                    backgroundLocation: skillElementPosition2,
                    opacity: 1.0f));
        }
        else
        {
            var skillValueElementIconPosition = skillValuePosition +
                                                new Size(-3, SkillValueFontSize);

            image.Mutate(ctx => ctx
                .DrawImage(
                    foreground: elementIcon,
                    backgroundLocation: skillValueElementIconPosition,
                    opacity: 1.0f));
        }

        // Draw quantity
        var quantityPosition = position +
                               new Size(QuantityOffsetX, ActionCardCenterOffsetY);

        var quantityTextOptions = new RichTextOptions(QuantityFont)
        {
            Origin = quantityPosition,
            VerticalAlignment = VerticalAlignment.Center,
        };

        image.Mutate(ctx =>
        {
            ctx.DrawText(quantityTextOptions,
                text: $"×{quantity}",
                brush: Brushes.Solid(Color.White),
                pen: Pens.Solid(Color.Black, width: 1.0f));
        });
    }

    private async Task<Image> LoadActionCardAsync(
        HoyolabHttpClient.Models.Action actionCard)
    {
        var cardImage = await imageCacheService.LoadIconAsync(actionCard);

        var cardWidth = ActionCardCroppedSize.Width;
        var cardHeight = cardImage.Height * cardWidth / cardImage.Width;

        var cropPosition = new Point(0, cardHeight / 3);

        var brushColor = actionCard.IsArcaneLegend() ? Color.Purple : Color.Black;
        var linearGradientBrush = new LinearGradientBrush(
            p1: new PointF(0.0f, 0.0f),
            p2: new PointF(cardWidth * 1.5f, 0.0f),
            repetitionMode: GradientRepetitionMode.None,
            new ColorStop(0.0f, brushColor), new ColorStop(1.0f, Color.Transparent));

        cardImage.Mutate(ctx => ctx
            .Resize(cardWidth, cardHeight)
            .Crop(new Rectangle(cropPosition, ActionCardCroppedSize))
            .Fill(linearGradientBrush)
            .ApplyRoundedCorners(ActionCardCornerRadius));

        return cardImage;
    }

    private static int CalculateActionCardQuantity(
        HoyolabHttpClient.Models.Action[] actionCards, int actionCardIndex,
        out HoyolabHttpClient.Models.Action actionCard)
    {
        actionCard = actionCards[actionCardIndex];

        var quantity = 0;

        var actionCardsCount = actionCards.Length;
        var actionCardId = actionCard.Basic.ItemId;

        HoyolabHttpClient.Models.Action nextActionCard;
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

    private static void BackgroundColor(Image<Rgba32> image, string backgroundColor)
    {
        if (!Color.TryParse(backgroundColor, out var color))
        {
            throw new InvalidOperationException("Invalid background color.");
        }

        image.Mutate(ctx => ctx.BackgroundColor(color));
    }

    private static bool ShouldRenderSecondarySkillElement(
        HoyolabHttpClient.Models.Action actionCard,
        [NotNullWhen(true)] out SkillElement? skillElement2)
    {
        skillElement2 = actionCard.GetSkillElement2();
        if (skillElement2 == null)
        {
            return false;
        }

        return true;
    }

    private static bool ShouldRenderSecondarySkillValue(
        HoyolabHttpClient.Models.Action actionCard,
        [NotNullWhen(true)] out SkillElement? skillElement2)
    {
        skillElement2 = actionCard.GetSkillElement2();
        if (skillElement2 == null)
        {
            return false;
        }

        var arcaneLegend = HoyolabHttpClient.Extensions.SkillElements.ArcaneLegend;
        if (skillElement2 == arcaneLegend)
        {
            return false;
        }

        return true;
    }
}