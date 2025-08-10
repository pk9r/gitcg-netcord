using HoyolabHttpClient;
using HoyolabHttpClient.Responses.DeckList;
using Microsoft.Extensions.Caching.Hybrid;

namespace GitcgNetCord.MainApp.Infrastructure.HoyolabServices;

public class HoyolabDeckAccountService(
    HoyolabHttpClientService hoyolab,
    HybridCache cache
)
{
    private static readonly
        HybridCacheEntryOptions CacheEntryOptions = new()
        {
            Flags = HybridCacheEntryFlags.DisableDistributedCache,
            Expiration = TimeSpan.FromSeconds(60) // Cache for 1 minute
        };

    public async Task<Data>
        GetDeckListAsync(
            HoyolabAuthorize authorize,
            string gameRoleId, string region
        )
    {
        var cacheKey = $"decklist:{authorize.HoyolabUserId}";

        var cachedDeckList = await cache
            .GetOrCreateAsync(
                key: cacheKey,
                factory: FetchDeckListAsync,
                options: CacheEntryOptions
            );

        return cachedDeckList;

        async ValueTask<Data>
            FetchDeckListAsync(
                CancellationToken cancellationToken = default
            )
        {
            var deckList = await hoyolab
                .GetDeckListAsync(
                    authorize: authorize,
                    roleId: gameRoleId,
                    server: region
                );

            return deckList;
        }
    }
}
