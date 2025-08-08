using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GitcgPainter.Extensions;
using HoyolabHttpClient.Models;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GitcgPainter.ImageCreators;

public class RolesImageCreator(ImageCacheService imageService)
{
    #region Constants
    private const int CARD_WIDTH = 140;
    private const int CARD_HEIGHT = 240;
    private const int GAP = 5;
    private const int PADDING = 10;
    private const float GOLDEN_RAITO = 1.618f;
    #endregion

    private readonly ImageCacheService _imageService = imageService;

    public async Task<byte[]> CreateImageAsync(
        IEnumerable<Role> roles,
        int nRows = 0, int nColumns = 0)
    {
        var nCard = roles.Count();

        // calculate nRows and nColumns
        if (nRows == 0 && nColumns == 0) // auto rows and columns
        {
            nColumns = CalculateDefaultColumns(roles);
        }
        else if (nColumns == 0) // calculate columns from rows
        {
            nColumns = (int)Math.Ceiling((double)nCard / nRows);
        }

        nRows = (int)Math.Ceiling((double)nCard / nColumns); // ensure nRows

        // calculate image size
        var (width, height) = CalcImageSize(nRows, nColumns);

        using var image = new Image<Rgba32>(width, height);

        await RenderRolesAsync(image, roles);

        using var memoryStream = new MemoryStream();
        await image.SaveAsPngAsync(memoryStream);
        memoryStream.Position = 0;
        var pngBytes = memoryStream.ToArray();

        return pngBytes;
    }

    private async Task RenderRolesAsync(Image image, IEnumerable<Role> roles)
    {
        var nCard = roles.Count();

        int x = 0, y = 0;
        for (var i = 0; i < nCard; i++)
        {
            if (x == 0)
            {
                x = PADDING;
            }
            else
            {
                x += CARD_WIDTH + GAP;
                if (x + CARD_WIDTH > image.Width)
                {
                    x = PADDING;
                    y += CARD_HEIGHT + GAP;
                }
            }
            if (y == 0)
            {
                y = PADDING;
            }

            // center last row
            if (x == PADDING && y + CARD_HEIGHT + PADDING >= image.Height)
            {
                var remain = nCard - i;
                x += (image.Width - PADDING * 2 - remain * CARD_WIDTH - (remain - 1) * GAP) / 2;
            }

            var cardImage = await _imageService.LoadIconAsync(roles.ElementAt(i));
            cardImage = cardImage.Clone(ctx =>
                ctx.Resize(
                    width: CARD_WIDTH,
                    height: CARD_HEIGHT));

            image.Mutate(ctx =>
                ctx.DrawImage(
                    foreground: cardImage,
                    new Point(x, y),
                    1f));
        }
    }

    private static int CalculateDefaultColumns<T>(IEnumerable<T> collection)
    {
        var collectionSize = collection.Count();
        var nColumnsDefault = 0;

        var goldenRatioDiff = float.MaxValue;

        for (var nRows = 1; nRows <= collectionSize; nRows++)
        {
            var nColumns = (int)Math.Ceiling((double)collectionSize / nRows);
            var (width, height) = CalcImageSize(nRows, nColumns);

            var newDiff = Math.Abs(width / (float)height - GOLDEN_RAITO);
            if (newDiff < goldenRatioDiff)
            {
                goldenRatioDiff = newDiff;
                nColumnsDefault = nColumns;
            }
        }

        return nColumnsDefault;
    }

    private static (int width, int height) CalcImageSize(int nRows, int nColumns)
    {
        var width = nColumns * CARD_WIDTH + (nColumns - 1) * GAP + 2 * PADDING;
        var height = nRows * CARD_HEIGHT + (nRows - 1) * GAP + 2 * PADDING;

        return (width, height);
    }
}
