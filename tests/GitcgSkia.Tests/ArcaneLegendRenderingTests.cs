using System.Net;
using GitcgSkia;
using GitcgSkia.Extensions;
using GitcgSkia.ImageCreators.Deck.Simplest;
using HoyolabHttpClient.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using Xunit;
using Action = HoyolabHttpClient.Models.Action;

namespace GitcgSkia.Tests;

public class ArcaneLegendRenderingTests
{
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly byte[] _pngBytes;

        public FakeHttpMessageHandler()
        {
            using var bmp = new SKBitmap(100, 100);
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.Blue);
            using var data = bmp.Encode(SKEncodedImageFormat.Png, 100);
            _pngBytes = data.ToArray();
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(_pngBytes)
            };
            return Task.FromResult(response);
        }
    }

    private static (ImageCacheService CacheService, IServiceProvider ServiceProvider) CreateTestServices()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        services.AddSingleton(new HttpClient(new FakeHttpMessageHandler()));
        services.AddSingleton<ImageCacheService>();
        services.AddTransient<SimplestDeckImageCreator>();

        var sp = services.BuildServiceProvider();
        var cacheService = sp.GetRequiredService<ImageCacheService>();
        return (cacheService, sp);
    }

    [Fact]
    public async Task LoadArcaneBorderAsync_ReturnsValidNonDisposedBitmap()
    {
        var (cacheService, _) = CreateTestServices();

        var border = await cacheService.LoadArcaneBorderAsync();

        Assert.NotNull(border);
        Assert.NotEqual(IntPtr.Zero, border.Handle);
        Assert.Equal(ImageCacheServiceExtension.BorderSize.Width, border.Width);
        Assert.Equal(ImageCacheServiceExtension.BorderSize.Height, border.Height);

        // Accessing Info should not throw or cause 0xC0000005
        var info = border.Info;
        Assert.Equal(ImageCacheServiceExtension.BorderSize.Width, info.Width);

        border.Dispose();
    }

    [Fact]
    public async Task LoadBorderedIconAsync_WithArcaneCard_ReturnsValidBitmap()
    {
        var (cacheService, _) = CreateTestServices();

        var arcaneCard = new Action
        {
            CardTags = ["7"], // CardTag.ArcaneLegend
            Basic = new Basic
            {
                ItemId = 1001,
                Icon = "https://example.com/arcane.png",
                Name = "Arcane Legend Card"
            }
        };

        var borderedIcon = await cacheService.LoadBorderedIconAsync(arcaneCard, new SKSizeI(120, 200));

        Assert.NotNull(borderedIcon);
        Assert.NotEqual(IntPtr.Zero, borderedIcon.Handle);
        Assert.Equal(120, borderedIcon.Width);
        Assert.Equal(200, borderedIcon.Height);

        borderedIcon.Dispose();
    }

    [Fact]
    public async Task SimplestDeckImageCreator_WithArcaneLegendDeck_RendersSuccessfullyWithoutCrash()
    {
        var (cacheService, sp) = CreateTestServices();
        var creator = sp.GetRequiredService<SimplestDeckImageCreator>();

        var roleCards = Enumerable.Range(1, 3).Select(i => new Role
        {
            Basic = new Basic
            {
                ItemId = i,
                Icon = $"https://example.com/role_{i}.png",
                Name = $"Role {i}"
            }
        }).ToList();

        var actionCards = new List<Action>();
        // Add 1 Arcane Legend card
        actionCards.Add(new Action
        {
            CardTags = ["7"],
            Basic = new Basic
            {
                ItemId = 9999,
                Icon = "https://example.com/arcane.png",
                Name = "Arcane Legend"
            }
        });
        // Add 29 normal action cards
        for (int i = 2; i <= 30; i++)
        {
            actionCards.Add(new Action
            {
                CardTags = [],
                Basic = new Basic
                {
                    ItemId = i,
                    Icon = $"https://example.com/action_{i}.png",
                    Name = $"Action {i}"
                }
            });
        }

        var deckData = new DeckData(roleCards, actionCards);

        await using var stream = await creator.CreateImageAsync(deckData, new SimplestDeckImageOptions());

        Assert.NotNull(stream);
        Assert.True(stream.Length > 0);

        using var renderedBitmap = SKBitmap.Decode(stream);
        Assert.NotNull(renderedBitmap);
        Assert.True(renderedBitmap.Width > 0);
        Assert.True(renderedBitmap.Height > 0);
    }
}
