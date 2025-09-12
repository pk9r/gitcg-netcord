using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Commands.Autocompletes;

public class CardAutocompleteProvider(
    HoyolabCardActionService cardActionService
) : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public async ValueTask<
        IEnumerable<ApplicationCommandOptionChoiceProperties>?
    > GetChoicesAsync(
        ApplicationCommandInteractionDataOption option,
        AutocompleteInteractionContext context
    )
    {
        var keyword = option.Value!;

        var cardActions = await cardActionService.GetCardActionsAsync("vi-vn");

        var suggestions = cardActions.Actions
            .Where(x => StartsWith(x.Basic.Name, keyword) || Contains(x.Basic.Name, keyword))
            .OrderBy(x => StartsWith(x.Basic.Name, keyword) ? 0 : 1)
            .Select(x => new ApplicationCommandOptionChoiceProperties(x.Basic.Name, x.Basic.ItemId));

        return suggestions.Take(25);

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
