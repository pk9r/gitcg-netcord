using GitcgPainter.ImageCreators;
using GitcgPainter.ImageCreators.Deck;
using GitcgPainter.ImageCreators.Deck.GameBackground;
using GitcgPainter.ImageCreators.Deck.Genshincards;
using GitcgPainter.ImageCreators.Deck.Simplest;
using Microsoft.Extensions.DependencyInjection;

namespace GitcgPainter.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGitcgPainter(this IServiceCollection services)
    {
        services
            .AddSingleton<ImageCacheService>()
            .AddSingleton<RolesImageCreator>()
            .AddTransient<DeckImageCreatorCollection>()
            .AddTransient<SimplestDeckImageCreator>()
            .AddTransient<GameBackgroundDeckImageCreator>()
            .AddTransient<GenshincardsDeckImageCreator>();

        return services;
    }
}
