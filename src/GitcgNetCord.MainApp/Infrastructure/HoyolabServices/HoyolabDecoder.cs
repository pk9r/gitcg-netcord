using GitcgNetCord.MainApp.Enums;
using GitcgNetCord.MainApp.Models;
using HoyolabHttpClient;
using Microsoft.Extensions.Options;
using SharedUtils;

namespace GitcgNetCord.MainApp.Infrastructure.HoyolabServices;

public class HoyolabDecoder(
    HoyolabHttpClientService hoyolab
)
{
    public async ValueTask<DecodeResult> DecodeAsync(
        string sharingCode,
        SharingCodeValidationRuleType validationRule =
            SharingCodeValidationRuleType.Playable,
        string lang = "en-us"
    )
    {
        var resultBuilder = new ValidateOptionsResultBuilder();

        if (sharingCode.Length != SharedConstants.SharingCodeLength)
        {
            resultBuilder.AddError(
                error: $"Sharing code must be {SharedConstants.SharingCodeLength} characters long.",
                propertyName: nameof(sharingCode)
            );

            return new DecodeResult(Validate: resultBuilder.Build());
        }

        var decodeResult = await hoyolab.DecodeCardCodeAsync(
            code: sharingCode, lang: lang
        );

        if (decodeResult.Validate.Failed)
        {
            resultBuilder.AddResult(decodeResult.Validate);
            return new DecodeResult(resultBuilder.Build());
        }

        ExecutePlayableValidateRule();

        return new DecodeResult(
            resultBuilder.Build(),
            Deck: decodeResult.Data
        );

        void ExecutePlayableValidateRule()
        {
            if (validationRule != SharingCodeValidationRuleType.Playable) 
                return;
            
            if (decodeResult.Data.RoleCards.Count != 3)
            {
                resultBuilder.AddError(
                    error: "Deck must have exactly 3 role cards."
                );
            }

            if (decodeResult.Data.ActionCards.Count != 30)
            {
                resultBuilder.AddError(
                    "Deck must have exactly 30 action cards."
                );
            }
        }
    }
}