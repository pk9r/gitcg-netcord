using HoyolabHttpClient;
using HoyolabHttpClient.Responses.CardRoles;
using Microsoft.Extensions.Caching.Hybrid;

namespace GitcgNetCord.MainApp.Infrastructure.HoyolabServices;

public class HoyolabCardRoleService(
    HoyolabHttpClientService hoyolab,
    HybridCache cache
)
{
    public async Task<Data>
        GetCardRolesAsync(string lang = "en-us")
    {
        var cacheKey = $"card-roles:{lang}";
        var cachedCardRoles = await cache
            .GetOrCreateAsync(
                key: cacheKey,
                factory: FetchCardRolesAsync
            );

        return cachedCardRoles;

        async ValueTask<Data>
            FetchCardRolesAsync(
                CancellationToken cancellationToken = default
            )
        {
            var cardRoles = await hoyolab
                .GetCardRolesAsync(lang);
            return cardRoles;
        }
    }
}
