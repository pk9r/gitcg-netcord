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

        if (paramName != "name")
        {
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
        }

        if (paramName != "version")
        {
            var activeAccount = await activeHoyolabAccountService.GetActiveHoyolabAccountAsync(context.User.Id);
            if (activeAccount != null)
            {
                var snapshots = await cardSnapshotService.GetSnapshotsForAccountAsync(activeAccount.Id);
                var isDailyQuery = !string.IsNullOrWhiteSpace(keyword) &&
                    (keyword.Contains("daily", StringComparison.OrdinalIgnoreCase) ||
                     char.IsDigit(keyword[0]));

                foreach (var s in snapshots)
                {
                    if (s.SnapshotType == SnapshotType.GameVersion) continue;

                    if (s.SnapshotType == SnapshotType.Daily)
                    {
                        // In delete command (paramName == "name"), daily snapshots cannot be deleted
                        if (paramName == "name") continue;

                        // In card query (paramName == "from" / "to"), only include if explicitly queried
                        if (!isDailyQuery) continue;

                        var label = $"[Daily] {s.Name} ({s.CreatedAt:yyyy-MM-dd})";
                        if (label.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add(new ApplicationCommandOptionChoiceProperties(label, s.Name));
                        }
                    }
                    else
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
        }

        return results.Take(25);
    }
}
