using GitcgNetCord.MainApp.Enums;
using GitcgNetCord.MainApp.Models;
using HoyolabHttpClient;
using HoyolabHttpClient.Models.Interfaces;

namespace GitcgNetCord.MainApp.Infrastructure.HoyolabServices;

public class HoyolabDecoder(
    HoyolabHttpClientService hoyolab
)
{
    public async Task<DecodeResult> DecodeAsync(
        string sharingCode,
        SharingCodeValidationRuleType validationRule =
            SharingCodeValidationRuleType.Playable,
        string lang = "en-us"
    )
    {
        IDeckData deck;
        try
        {
            deck = await hoyolab.DecodeCardCodeAsync(
                code: sharingCode, lang: lang
            );
        }
        catch
        {
            return new DecodeResult(IsValid: false);
        }

        return new DecodeResult(
            IsValid: true, Deck: deck
        );
    }
}
