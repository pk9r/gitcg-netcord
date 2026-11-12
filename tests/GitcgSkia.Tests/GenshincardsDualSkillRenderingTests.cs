using System.Net;
using GitcgSharp.Shared.ImageCreators.Deck.Abstractions;
using GitcgSkia;
using GitcgSkia.ImageCreators.Deck.Genshincards;
using HoyolabHttpClient.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using Xunit;
using Action = HoyolabHttpClient.Models.Action;
using SkillElements = HoyolabHttpClient.Extensions.SkillElements;

namespace GitcgSkia.Tests;

public class GenshincardsDualSkillRenderingTests
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
        services.AddTransient<GenshincardsDeckImageCreator>();

        var sp = services.BuildServiceProvider();
        var cacheService = sp.GetRequiredService<ImageCacheService>();
        return (cacheService, sp);
    }

    [Fact]
    public async Task GenshincardsDeckImageCreator_WithDualSkillValues_RendersSuccessfully()
    {
        var (_, sp) = CreateTestServices();
        var creator = sp.GetRequiredService<GenshincardsDeckImageCreator>();

        var roleCards = Enumerable.Range(1, 3).Select(i => new Role
        {
            Basic = new Basic
            {
                ItemId = i,
                Icon = $"https://example.com/role_{i}.png",
                IconSmall = $"https://example.com/role_small_{i}.png",
                Name = $"Role {i}"
            }
        }).ToList();

        var actionCards = new List<Action>();

        // Thẻ đầu tiên có 2 skill value (ví dụ Pyro + Unaligned)
        actionCards.Add(new Action
        {
            CardTags = [],
            Basic = new Basic
            {
                ItemId = 100,
                Icon = "https://example.com/action_dual_skill.png",
                Name = "Dual Skill Action Card"
            },
            SkillValue = 2,
            SkillValue2 = 1,
            SkillElement = SkillElements.Pyro.Value,
            SkillElement2 = SkillElements.Unaligned.Value
        });

        // 29 thẻ hành động thông thường khác
        for (int i = 2; i <= 30; i++)
        {
            actionCards.Add(new Action
            {
                CardTags = [],
                Basic = new Basic
                {
                    ItemId = 100 + i,
                    Icon = $"https://example.com/action_{i}.png",
                    Name = $"Action {i}"
                },
                SkillValue = 3,
                SkillElement = SkillElements.Hydro.Value
            });
        }

        var deckData = new DeckData(roleCards, actionCards);

        await using var stream = await ((IDeckImageCreationService)creator).CreateImageAsync(deckData);

        Assert.NotNull(stream);
        Assert.True(stream.Length > 0);

        using var renderedBitmap = SKBitmap.Decode(stream);
        Assert.NotNull(renderedBitmap);
        Assert.True(renderedBitmap.Width > 0);
        Assert.True(renderedBitmap.Height > 0);
    }
}
