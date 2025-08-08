using System.Threading.Tasks;
using HoyolabHttpClient.Models;
using HoyolabHttpClient.Responses.Skills;

namespace HoyolabHttpClient.Extensions;

public static class HoyolabServiceExtension
{
    public static Task<Data>
        GetRoleSkillAsync(
            this HoyolabHttpClientService hoyolabService,
            Role role,
            string lang = "en-us"
        )
    {
        return hoyolabService.GetRoleSkillAsync(
            id: role.Basic.ItemId,
            lang: lang
        );
    }
}