using HoyolabHttpClient;
using HoyolabHttpClient.Responses.GcgBasicInfo;

namespace GitcgNetCord.MainApp.Infrastructure.HoyolabServices;

public class HoyolabGcgBasicInfoService(HoyolabHttpClientService hoyolab)
{
    public async Task<Data>
        GetGcgBasicInfoAsync(
            string server, string uid,
            HoyolabAuthorize? authorize = null
        )
    {
        var response = await hoyolab
            .GetGcgBasicInfoAsync(server, uid, authorize);
        
        return response;
    }
}
