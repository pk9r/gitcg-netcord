using System.Collections.Immutable;
using System.Drawing;
using System.Text;
using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Infrastructure;
using HoyolabHttpClient;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class CardSlashCommand
{
    public static async Task ExecuteAsync(
        HoyolabHttpClientService hoyolab,
        IServiceProvider serviceProvider,
        ApplicationCommandContext context,
        [
            SlashCommandParameter(
                Description = "Stats for a specific game version (e.g. 4.7)",
                AutocompleteProviderType = typeof(CardSnapshotAutocompleteProvider)
            )
        ]
        string? version = null,
        [
            SlashCommandParameter(
                Description = "Stats starting from this milestone (version or snapshot name)",
                AutocompleteProviderType = typeof(CardSnapshotAutocompleteProvider)
            )
        ]
        string? from = null,
        [
            SlashCommandParameter(
                Description = "Stats up to this milestone (default: Present)",
                AutocompleteProviderType = typeof(CardSnapshotAutocompleteProvider)
            )
        ]
        string? to = null,
        [
            SlashCommandParameter(
                Description = "Minimum use count for a card to be considered. Default: 1%."
            )
        ]
        string minUseCount = "1%",
        [
            SlashCommandParameter(
                Description = "Number of top win rates to display. Default: 5."
            )
        ]
        int topWinRateCount = 5,
        [
            SlashCommandParameter(
                Description = "Number of top use counts to display. Default: 10."
            )
        ]
        int topUseCount = 10
    )
    {
        var activeHoyolabAccountService = serviceProvider
            .GetRequiredService<ActiveHoyolabAccountService>();
        var cardSnapshotService = serviceProvider
            .GetRequiredService<CardSnapshotService>();
        var gameVersionService = serviceProvider
            .GetRequiredService<GameVersionService>();
        var calculator = serviceProvider
            .GetRequiredService<ICardStatsCalculator>();

        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage()
        );

        var hoyolabAccount = await activeHoyolabAccountService
            .GetActiveHoyolabAccountAsync(context.User.Id);

        if (hoyolabAccount == null)
        {
            var commands = await context.Client.Rest
                .GetGlobalApplicationCommandsAsync(context.Client.Id);

            var accountCommand = commands
                .First(x => x.Name == "hoyolab-accounts");

            await context.Interaction.ModifyResponseAsync(message =>
            {
                message.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription(
                        $"""
                         You don't have an active Hoyolab account.
                         Please use the command {accountCommand} to set one up.
                         """
                    )
                    .WithColor(Color.Red.ToNetCordColor())
                );
            });

            return;
        }

        var authorize = new HoyolabAuthorize(
            HoyolabUserId: hoyolabAccount.HoyolabUserId,
            Token: hoyolabAccount.Token
        );

        var uid = hoyolabAccount.GameRoleId;
        var server = hoyolabAccount.Region;

        HoyolabHttpClient.Responses.GcgCardList.Data data;
        try
        {
            data = await hoyolab.GetGcgCardListAsync(
                server: server,
                roleId: uid,
                authorize: authorize
            );
        }
        catch (Exception e)
        {
            await context.Interaction.ModifyResponseAsync(message =>
            {
                message.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription(e.Message)
                    .WithColor(Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        CardSnapshot? startSnapshot = null;
        CardSnapshot? endSnapshot = null;
        string periodDescription = "All-time";

        if (!string.IsNullOrWhiteSpace(version))
        {
            var ver = version.Trim();
            startSnapshot = await cardSnapshotService.GetSnapshotByNameAsync(hoyolabAccount.Id, ver);
            if (startSnapshot == null)
            {
                var earliest = await cardSnapshotService.GetEarliestSnapshotAsync(hoyolabAccount.Id);
                var earliestMsg = earliest != null
                    ? $" Your earliest available snapshot is '{earliest.Name}' ({earliest.CreatedAt:yyyy-MM-dd})."
                    : " You do not have any snapshots yet.";

                await context.Interaction.ModifyResponseAsync(message =>
                {
                    message.AddEmbeds(new EmbedProperties()
                        .WithTitle("Data Not Found")
                        .WithDescription($"No snapshot found for version **{ver}**.{earliestMsg}")
                        .WithColor(Color.Orange.ToNetCordColor())
                    );
                });
                return;
            }

            var nextVersion = await gameVersionService.GetNextVersionAsync(ver);
            if (nextVersion != null)
            {
                endSnapshot = await cardSnapshotService.GetSnapshotByNameAsync(hoyolabAccount.Id, nextVersion.VersionNumber);
            }

            periodDescription = endSnapshot != null
                ? $"Version {ver} ({ver} to {nextVersion?.VersionNumber})"
                : $"Version {ver} ({ver} to Present)";
        }
        else if (!string.IsNullOrWhiteSpace(from))
        {
            var fromName = from.Trim();
            startSnapshot = await cardSnapshotService.GetSnapshotByNameAsync(hoyolabAccount.Id, fromName);
            if (startSnapshot == null)
            {
                var earliest = await cardSnapshotService.GetEarliestSnapshotAsync(hoyolabAccount.Id);
                var earliestMsg = earliest != null
                    ? $" Your earliest available snapshot is '{earliest.Name}' ({earliest.CreatedAt:yyyy-MM-dd})."
                    : " You do not have any snapshots yet.";

                await context.Interaction.ModifyResponseAsync(message =>
                {
                    message.AddEmbeds(new EmbedProperties()
                        .WithTitle("Data Not Found")
                        .WithDescription($"No snapshot found for starting point **{fromName}**.{earliestMsg}")
                        .WithColor(Color.Orange.ToNetCordColor())
                    );
                });
                return;
            }

            var toName = to?.Trim();
            if (!string.IsNullOrWhiteSpace(toName) &&
                !toName.Equals("current", StringComparison.OrdinalIgnoreCase) &&
                !toName.Equals("live", StringComparison.OrdinalIgnoreCase) &&
                !toName.Equals("present", StringComparison.OrdinalIgnoreCase))
            {
                endSnapshot = await cardSnapshotService.GetSnapshotByNameAsync(hoyolabAccount.Id, toName);
                if (endSnapshot == null)
                {
                    await context.Interaction.ModifyResponseAsync(message =>
                    {
                        message.AddEmbeds(new EmbedProperties()
                            .WithTitle("Data Not Found")
                            .WithDescription($"No snapshot found for end point **{toName}**.")
                            .WithColor(Color.Orange.ToNetCordColor())
                        );
                    });
                    return;
                }

                periodDescription = $"From {startSnapshot.Name} ({startSnapshot.CreatedAt:yyyy-MM-dd}) to {endSnapshot.Name} ({endSnapshot.CreatedAt:yyyy-MM-dd})";
            }
            else
            {
                periodDescription = $"From {startSnapshot.Name} ({startSnapshot.CreatedAt:yyyy-MM-dd}) to Present";
            }
        }

        var result = calculator.Calculate(
            allCardsMetadata: data.CardList,
            startSnapshot: startSnapshot?.CardData,
            endSnapshot: endSnapshot?.CardData,
            minUseCount: minUseCount,
            topWinRateCount: topWinRateCount,
            topUseCount: topUseCount
        );

        if (result.TotalGames == 0)
        {
            await context.Interaction.ModifyResponseAsync(message =>
            {
                message.AddEmbeds(new EmbedProperties()
                    .WithTitle($"{data.Stats.Nickname} - Statistics")
                    .WithColor(Color.Purple.ToNetCordColor())
                    .AddFields(
                        new EmbedFieldProperties().WithName("UID").WithValue(uid).WithInline(),
                        new EmbedFieldProperties().WithName("Time Period").WithValue(periodDescription).WithInline(),
                        new EmbedFieldProperties().WithName("Notice").WithValue("No matches were recorded during this period.")
                    )
                );
            });
            return;
        }

        var appEmojis = await context.Client.Rest
            .GetApplicationEmojisAsync(context.Client.Id);
        var emojis = appEmojis.ToImmutableDictionary(x => x.Name);

        var bestWinRateStringBuilder = new StringBuilder();
        for (var i = 0; i < result.BestWinRateCharacters.Count; i++)
        {
            var x = result.BestWinRateCharacters[i];
            var emojiStr = emojis.TryGetValue(x.Id.ToString(), out var emoji) ? $"{emoji} " : "";
            bestWinRateStringBuilder.AppendLine(
                $"{i + 1}. {emojiStr}{x.Name} - {x.WinRate:P2} ({x.Proficiency}/{x.UseCount})"
            );
        }
        if (result.BestWinRateCharacters.Count == 0)
        {
            bestWinRateStringBuilder.AppendLine("No matching data for current filters.");
        }

        var lowestWinRateStringBuilder = new StringBuilder();
        for (var i = 0; i < result.LowestWinRateCharacters.Count; i++)
        {
            var x = result.LowestWinRateCharacters[i];
            var emojiStr = emojis.TryGetValue(x.Id.ToString(), out var emoji) ? $"{emoji} " : "";
            lowestWinRateStringBuilder.AppendLine(
                $"{i + 1}. {emojiStr}{x.Name} - {x.WinRate:P2} ({x.Proficiency}/{x.UseCount})"
            );
        }
        if (result.LowestWinRateCharacters.Count == 0)
        {
            lowestWinRateStringBuilder.AppendLine("No matching data for current filters.");
        }

        var topUseCountsStringBuilder = new StringBuilder();
        for (var i = 0; i < result.TopUseCharacters.Count; i++)
        {
            var x = result.TopUseCharacters[i];
            var emojiStr = emojis.TryGetValue(x.Id.ToString(), out var emoji) ? $"{emoji} " : "";
            topUseCountsStringBuilder.AppendLine(
                $"{i + 1}. {emojiStr}{x.Name} - {x.UseCount}"
            );
        }
        if (result.TopUseCharacters.Count == 0)
        {
            topUseCountsStringBuilder.AppendLine("No data.");
        }

        var topActionUseCountsStringBuilder = new StringBuilder();
        for (var i = 0; i < result.TopUseActions.Count; i++)
        {
            var card = result.TopUseActions[i];
            topActionUseCountsStringBuilder.AppendLine(
                $"{i + 1}. {card.Name} - {card.UseCount}"
            );
        }
        if (result.TopUseActions.Count == 0)
        {
            topActionUseCountsStringBuilder.AppendLine("No data.");
        }

        await context.Interaction.ModifyResponseAsync(message =>
        {
            message.AddEmbeds(
                new EmbedProperties()
                    .WithColor(Color.Purple.ToNetCordColor())
                    .WithTitle($"{data.Stats.Nickname}")
                    .AddFields(
                        new EmbedFieldProperties()
                            .WithName("UID")
                            .WithValue($"{uid}")
                            .WithInline(),
                        new EmbedFieldProperties()
                            .WithName("Time Period")
                            .WithValue(periodDescription)
                            .WithInline(),
                        new EmbedFieldProperties()
                            .WithName("Total games")
                            .WithValue($"{result.TotalGames}")
                            .WithInline(),
                        new EmbedFieldProperties()
                            .WithName("Total wins")
                            .WithValue($"{result.TotalWins} ({result.OverallWinRate:P2})")
                            .WithInline()
                    ),
                new EmbedProperties()
                    .WithColor(Color.Purple.ToNetCordColor())
                    .AddFields(
                        new EmbedFieldProperties()
                            .WithName("Best win rate")
                            .WithValue(bestWinRateStringBuilder.ToString())
                            .WithInline(),
                        new EmbedFieldProperties()
                            .WithName("Lowest win rate")
                            .WithValue(lowestWinRateStringBuilder.ToString())
                            .WithInline()
                    )
                    .WithFooter(new EmbedFooterProperties()
                        .WithText(
                            "Win rate stats only include cards " +
                            (result.IsPercentThreshold
                                ? $"with use count > {result.MinUseCountThreshold} ({result.MinUseCountInput} total games)."
                                : $"with use count > {result.MinUseCountThreshold}.")
                        )
                    ),
                new EmbedProperties()
                    .WithColor(Color.Purple.ToNetCordColor())
                    .AddFields(
                        new EmbedFieldProperties()
                            .WithName("Top use count characters")
                            .WithValue(topUseCountsStringBuilder.ToString())
                            .WithInline(),
                        new EmbedFieldProperties()
                            .WithName("Top use count actions")
                            .WithValue(topActionUseCountsStringBuilder.ToString())
                            .WithInline()
                    )
            );
        });
    }
}
