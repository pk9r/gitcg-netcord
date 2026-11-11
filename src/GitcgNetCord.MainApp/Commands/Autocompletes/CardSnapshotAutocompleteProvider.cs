using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Commands.Autocompletes;

public class CardSnapshotAutocompleteProvider(
    GameVersionService gameVersionService,
    CardSnapshotService cardSnapshotService,
    ActiveHoyolabAccountService activeHoyolabAccountService
) : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
        ApplicationCommandInteractionDataOption option,
        AutocompleteInteractionContext context
    )
    {
        var keyword = option.Value ?? string.Empty;
        var paramName = option.Name.ToLowerInvariant();
        var results = new List<ApplicationCommandOptionChoiceProperties>();

        if (paramName == "to")
        {
            if (string.IsNullOrWhiteSpace(keyword) ||
                "present (live)".Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                "current (live)".Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                "current".Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                "present".Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new ApplicationCommandOptionChoiceProperties("Present (Live)", "current"));
            }
        }

        var versions = await gameVersionService.GetGameVersionsAsync();
        foreach (var v in versions)
        {
            var label = $"Version {v.VersionNumber}" + (!string.IsNullOrWhiteSpace(v.Name) ? $" - {v.Name}" : "");
            if (string.IsNullOrWhiteSpace(keyword) ||
                label.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                v.VersionNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new ApplicationCommandOptionChoiceProperties(label, v.VersionNumber));
            }
        }

        if (paramName != "version")
        {
            var activeAccount = await activeHoyolabAccountService.GetActiveHoyolabAccountAsync(context.User.Id);
            if (activeAccount != null)
            {
                var snapshots = await cardSnapshotService.GetSnapshotsForAccountAsync(activeAccount.Id);
                foreach (var s in snapshots.Where(x => x.SnapshotType != SnapshotType.GameVersion))
                {
                    var label = $"[Custom] {s.Name} ({s.CreatedAt:yyyy-MM-dd})";
                    if (string.IsNullOrWhiteSpace(keyword) ||
                        label.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new ApplicationCommandOptionChoiceProperties(label, s.Name));
                    }
                }
            }
        }

        return results.Take(25);
    }
}
