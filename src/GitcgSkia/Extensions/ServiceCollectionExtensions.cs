using GitcgSkia.ImageCreators.Deck;
using GitcgSkia.ImageCreators.Deck.GameBackground;
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
            // .AddTransient<SimplestDeckImageCreator>()
            .AddTransient<GameBackgroundDeckImageCreator>()
            // .AddTransient<GenshincardsDeckImageCreator>()
            ;

        return services;
    }
}