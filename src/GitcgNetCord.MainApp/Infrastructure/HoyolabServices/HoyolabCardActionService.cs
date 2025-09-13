using HoyolabHttpClient;
using HoyolabHttpClient.Responses.CardActions;
using Microsoft.Extensions.Caching.Hybrid;

namespace GitcgNetCord.MainApp.Infrastructure.HoyolabServices;

public class HoyolabCardActionService(
    HoyolabCardRoleService cardRoleService,
    HoyolabHttpClientService hoyolab,
    HybridCache cache
)
{
    public async Task<Data>
        GetCardActionsAsync(string lang = "en-us")
    {
        var cacheKey = $"card-action:{lang}";
        var cached = await cache
            .GetOrCreateAsync(
                key: cacheKey,
                factory: FetchAsync
            );

        return cached;

        async ValueTask<Data> FetchAsync(
            CancellationToken cancellationToken = default
        )
        {
            var roleData = await cardRoleService.GetCardRolesAsync(lang);
            var roleIds = roleData.Roles
                .Select(x => x.Basic.ItemId);

            var cardActions = await hoyolab
                .GetCardActionsAsync(roleIds, lang);
            return cardActions;
        }
    }
}
