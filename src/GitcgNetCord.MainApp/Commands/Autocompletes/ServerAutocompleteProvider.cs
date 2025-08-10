using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Commands.Autocompletes;

public class ServerAutocompleteProvider
    : IAutocompleteProvider<AutocompleteInteractionContext>
{
    private static readonly IEnumerable<
        (string Value, string Display)> Suggestions =
    [
        ("os_asia", "Asia Server"),
        ("os_euro", "Europe Server"),
        ("os_usa", "America Server"),
        ("", "Auto")
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
