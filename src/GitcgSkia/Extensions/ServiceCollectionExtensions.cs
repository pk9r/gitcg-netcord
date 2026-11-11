using GitcgSkia.ImageCreators.Deck.GameBackground;
using GitcgSkia.ImageCreators.Deck.Genshincards;
using GitcgSkia.ImageCreators.Deck.Simplest;
using Microsoft.Extensions.DependencyInjection;

namespace GitcgSkia.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGitcgSkia(
        this IServiceCollection services
    )
    {
        services
            .AddSingleton<ImageCacheService>()
            // .AddSingleton<RolesImageCreator>()
            // .AddTransient<DeckImageCreatorCollection>()
            .AddTransient<SimplestDeckImageCreator>()
            .AddTransient<GameBackgroundDeckImageCreator>()
            .AddTransient<GenshincardsDeckImageCreator>()
            ;

        return services;
    }
}