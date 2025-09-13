using HoyolabHttpClient.Models;
using System.Threading.Tasks;

namespace HoyolabHttpClient.Extensions;

public static class HoyolabServiceExtension
{
    public static Task<Responses.Skills.Data>
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