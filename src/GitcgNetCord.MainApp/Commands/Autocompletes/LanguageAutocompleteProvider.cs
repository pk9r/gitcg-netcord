using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Commands.Autocompletes;

public class LanguageAutocompleteProvider
    : IAutocompleteProvider<AutocompleteInteractionContext>
{
    private static readonly IEnumerable<
        (string Value, string Display)> Suggestions =
    [
        ("zh-cn", "zh-cn / 中文（简体）"),
        ("zh-tw", "zh-tw / 中文（繁體）"),
        ("de-de", "de-de / Deutsch"),
        ("en-us", "en-us / English"),
        ("es-es", "es-es / Español"),
        ("fr-fr", "fr-fr / Français"),
        ("id-id", "id-id / Indonesia"),
        ("it-it", "it-it / Italiano"),
        ("ja-jp", "ja-jp / 日本語"),
        ("ko-kr", "ko-kr / 한국어"),
        ("pt-pt", "pt-pt / Português"),
        ("ru-ru", "ru-ru / Pусский"),
        ("th-th", "th-th / ภาษาไทย"),
        ("tr-tr", "tr-tr / Türkçe"),
        ("vi-vn", "vi-vn / Tiếng Việt")
    ];

    public ValueTask<
        IEnumerable<ApplicationCommandOptionChoiceProperties>?
    > GetChoicesAsync(
        ApplicationCommandInteractionDataOption option,
        AutocompleteInteractionContext context
    )
    {
        var keyword = option.Value!;

        var suggestions = Suggestions
            .Where(x => StartsWith(x.Display, keyword) || Contains(x.Display, keyword))
            .OrderBy(x => StartsWith(x.Display, keyword) ? 0 : 1)
            .Select(x => new ApplicationCommandOptionChoiceProperties(x.Display, x.Value));

        return ValueTask.FromResult<
            IEnumerable<ApplicationCommandOptionChoiceProperties>?
        >(result: suggestions);

        bool StartsWith(string x, string k)
        {
            return x.StartsWith(
                value: k,
                comparisonType: StringComparison.OrdinalIgnoreCase
            );
        }

        bool Contains(string x, string k)
        {
            return x.Contains(
                value: k,
                comparisonType: StringComparison.OrdinalIgnoreCase
            );
        }
    }
}
