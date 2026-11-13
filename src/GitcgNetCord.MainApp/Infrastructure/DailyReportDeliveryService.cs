using System.Text;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Models;
using HoyolabHttpClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Rest;

namespace GitcgNetCord.MainApp.Infrastructure;

public class DailyReportDeliveryService(
    AppDbContext dbContext,
    DiscordDailyReportService reportService,
    ICardStatsCalculator calculator,
    GatewayClient client,
    ILogger<DailyReportDeliveryService> logger
)
{
    public async Task<int> DeliverReportsAsync(
        HoyolabAccount account,
        IReadOnlyCollection<Card> allCardsMetadata,
        CardSnapshot currentSnapshot,
        DateTime localGameDate,
        CancellationToken cancellationToken = default
    )
    {
        var subscriptions = await reportService.GetActiveSubscriptionsForAccountAsync(account.Id, cancellationToken);
        if (subscriptions.Count == 0) return 0;

        var previousSnapshot = await dbContext.CardSnapshots
            .Where(x => x.HoyolabAccountId == account.Id && x.Id != currentSnapshot.Id && x.CreatedAt < currentSnapshot.CreatedAt)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var result = calculator.Calculate(
            allCardsMetadata: allCardsMetadata,
            startSnapshot: previousSnapshot?.CardData,
            endSnapshot: currentSnapshot.CardData,
            minUseCount: "1%",
            topWinRateCount: 5,
            topUseCount: 10
        );

        var embed = BuildDailyReportEmbed(account, currentSnapshot, previousSnapshot, result, localGameDate);
        var messageProperties = new MessageProperties().AddEmbeds(embed);

        var deliveredCount = 0;
        foreach (var subscription in subscriptions)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                await client.Rest.SendMessageAsync(
                    subscription.ThreadId,
                    messageProperties,
                    cancellationToken: cancellationToken
                );

                deliveredCount++;
                logger.LogInformation(
                    "Delivered daily report for account {AccountId} (UID: {UID}) to thread {ThreadId}.",
                    account.Id,
                    account.GameRoleId,
                    subscription.ThreadId
                );
            }
            catch (RestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Forbidden)
            {
                logger.LogWarning(
                    ex,
                    "Thread {ThreadId} is inaccessible ({StatusCode}). Disabling subscription {SubscriptionId}.",
                    subscription.ThreadId,
                    ex.StatusCode,
                    subscription.Id
                );
                await reportService.DisableSubscriptionAsync(subscription.Id, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(
                    ex,
                    "Failed to deliver daily report for account {AccountId} to thread {ThreadId}.",
                    account.Id,
                    subscription.ThreadId
                );
            }

            await Task.Delay(500, cancellationToken);
        }

        return deliveredCount;
    }

    public static EmbedProperties BuildDailyReportEmbed(
        HoyolabAccount account,
        CardSnapshot currentSnapshot,
        CardSnapshot? previousSnapshot,
        PeriodicStatsResult result,
        DateTime localGameDate
    )
    {
        var embed = new EmbedProperties()
            .WithTitle($"Daily TCG Statistics Report - {localGameDate:yyyy-MM-dd}")
            .WithColor(System.Drawing.Color.Purple.ToNetCordColor());

        embed.AddFields(
            new EmbedFieldProperties().WithName("UID").WithValue(account.GameRoleId).WithInline(),
            new EmbedFieldProperties().WithName("Region").WithValue(account.Region).WithInline(),
            new EmbedFieldProperties().WithName("Date").WithValue($"{localGameDate:yyyy-MM-dd}").WithInline()
        );

        if (previousSnapshot == null || result.TotalGames == 0)
        {
            embed.AddFields(
                new EmbedFieldProperties()
                    .WithName("Notice")
                    .WithValue("No matches were recorded for this server day.")
            );
            embed.WithFooter(new EmbedFooterProperties().WithText($"Total recorded to date: {currentSnapshot.TotalGames} games ({currentSnapshot.TotalWins} wins)"));
            return embed;
        }

        embed.AddFields(
            new EmbedFieldProperties().WithName("Games Played").WithValue($"{result.TotalGames}").WithInline(),
            new EmbedFieldProperties().WithName("Wins").WithValue($"{result.TotalWins} ({result.OverallWinRate:P1})").WithInline(),
            new EmbedFieldProperties().WithName("Losses").WithValue($"{result.TotalGames - result.TotalWins}").WithInline()
        );

        if (result.TopUseCharacters.Count > 0)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < Math.Min(5, result.TopUseCharacters.Count); i++)
            {
                var card = result.TopUseCharacters[i];
                sb.AppendLine($"{i + 1}. **{card.Name}** - {card.UseCount} games");
            }
            embed.AddFields(new EmbedFieldProperties().WithName("Most Used Characters").WithValue(sb.ToString()).WithInline());
        }

        if (result.BestWinRateCharacters.Count > 0)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < Math.Min(5, result.BestWinRateCharacters.Count); i++)
            {
                var card = result.BestWinRateCharacters[i];
                sb.AppendLine($"{i + 1}. **{card.Name}** - {card.WinRate:P1} ({card.Proficiency}/{card.UseCount})");
            }
            embed.AddFields(new EmbedFieldProperties().WithName("Highest Win Rate Characters").WithValue(sb.ToString()).WithInline());
        }

        if (result.TopUseActions.Count > 0)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < Math.Min(5, result.TopUseActions.Count); i++)
            {
                var card = result.TopUseActions[i];
                sb.AppendLine($"{i + 1}. **{card.Name}** - {card.UseCount} times");
            }
            embed.AddFields(new EmbedFieldProperties().WithName("Most Used Actions").WithValue(sb.ToString()));
        }

        embed.WithFooter(new EmbedFooterProperties().WithText($"Total to date: {currentSnapshot.TotalGames} games ({currentSnapshot.TotalWins} wins)"));
        return embed;
    }
}
