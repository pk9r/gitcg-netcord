using GitcgNetCord.MainApp.Entities.Repositories;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Commands.Autocompletes;

public class HoyolabAccountAutocompleteProvider(
    HoyolabAccountService hoyolabAccountService
) : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
        ApplicationCommandInteractionDataOption option,
        AutocompleteInteractionContext context
    )
    {
        var accounts = await hoyolabAccountService.GetHoyolabAccountsAsync(context.User.Id);
        var keyword = option.Value ?? string.Empty;

        var choices = new List<ApplicationCommandOptionChoiceProperties>();
        foreach (var acc in accounts)
        {
            var dailyStatus = acc.IsDailySnapshotEnabled ? "Daily: ON" : "Daily: OFF";
            var label = $"{acc.GameRoleId} ({acc.Region}) - [{dailyStatus}]";
            if (string.IsNullOrWhiteSpace(keyword) ||
                label.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                acc.GameRoleId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                choices.Add(new ApplicationCommandOptionChoiceProperties(label, acc.GameRoleId));
            }
        }

        return choices.Take(25);
    }
}
