using System.Text;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Models;
using HoyolabHttpClient.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NetCord.Gateway;
using NetCord.Rest;

namespace GitcgNetCord.MainApp.Infrastructure;

public class DailyReportDeliveryService(
    AppDbContext dbContext,
    DiscordDailyReportService reportService,
    ICardStatsCalculator calculator,
    GatewayClient client,
    ILogger<DailyReportDeliveryService> logger,
    HybridCache? cache = null
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

        var emojis = await GetApplicationEmojisMapAsync(cancellationToken);
        var embed = BuildDailyReportEmbed(account, currentSnapshot, previousSnapshot, result, localGameDate, emojis);
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

                var destType = subscription.GuildId == 0 ? "DM" : "thread";
                deliveredCount++;
                logger.LogInformation(
                    "Delivered daily report for account {AccountId} (UID: {UID}) to {DestType} {DestinationId}.",
                    account.Id,
                    account.GameRoleId,
                    destType,
                    subscription.ThreadId
                );
            }
            catch (RestException ex) when (ex.StatusCode is System.Net.HttpStatusCode.NotFound or System.Net.HttpStatusCode.Forbidden)
            {
                var destType = subscription.GuildId == 0 ? "DM" : "thread";
                logger.LogWarning(
                    ex,
                    "{DestType} {DestinationId} is inaccessible ({StatusCode}). Disabling subscription {SubscriptionId}.",
                    destType,
                    subscription.ThreadId,
                    ex.StatusCode,
                    subscription.Id
                );
                await reportService.DisableSubscriptionAsync(subscription.Id, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                var destType = subscription.GuildId == 0 ? "DM" : "thread";
                logger.LogWarning(
                    ex,
                    "Failed to deliver daily report for account {AccountId} to {DestType} {DestinationId}.",
                    account.Id,
                    destType,
                    subscription.ThreadId
                );
            }

            await Task.Delay(500, cancellationToken);
        }

        return deliveredCount;
    }

    public async Task<Dictionary<string, string>> GetApplicationEmojisMapAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (cache != null)
            {
                return await cache.GetOrCreateAsync(
                    "discord:application_emojis",
                    async cancel =>
                    {
                        var appEmojis = await client.Rest.GetApplicationEmojisAsync(client.Id, cancellationToken: cancel);
                        return appEmojis.ToDictionary(x => x.Name, x => x.ToString());
                    },
                    new HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromHours(1)
                    },
                    cancellationToken: cancellationToken
                );
            }

            var emojis = await client.Rest.GetApplicationEmojisAsync(client.Id, cancellationToken: cancellationToken);
            return emojis.ToDictionary(x => x.Name, x => x.ToString());
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve application emojis for daily report delivery.");
            return [];
        }
    }

    public static DateTime GetSnapshotGameDate(CardSnapshot snapshot, TimeSpan offset, DateTime fallbackDate)
    {
        if (snapshot.Name.StartsWith("daily-", StringComparison.OrdinalIgnoreCase) &&
            DateTime.TryParseExact(snapshot.Name[6..], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
        {
            return parsedDate;
        }

        if (snapshot.CreatedAt != default)
        {
            return (snapshot.CreatedAt.ToOffset(offset) - TimeSpan.FromHours(4)).Date;
        }

        return fallbackDate;
    }

    public static EmbedProperties BuildDailyReportEmbed(
        HoyolabAccount account,
        CardSnapshot currentSnapshot,
        CardSnapshot? previousSnapshot,
        PeriodicStatsResult result,
        DateTime localGameDate,
        IReadOnlyDictionary<string, string>? emojis = null
    )
    {
        var offset = DailySnapshotProcessor.GetServerUtcOffset(account.Region);
        var currDate = GetSnapshotGameDate(currentSnapshot, offset, localGameDate);

        string periodText;
        if (previousSnapshot != null)
        {
            var prevDate = GetSnapshotGameDate(previousSnapshot, offset, localGameDate);
            var dayDiff = (currDate - prevDate).Days;

            if (dayDiff == 1)
            {
                periodText = $"{prevDate:yyyy-MM-dd}";
            }
            else if (dayDiff > 1)
            {
                periodText = $"{prevDate:yyyy-MM-dd} ~ {currDate:yyyy-MM-dd} ({dayDiff} days)";
            }
            else
            {
                periodText = $"{currDate:yyyy-MM-dd} (Today)";
            }
        }
        else
        {
            periodText = $"{currDate:yyyy-MM-dd}";
        }

        var embed = new EmbedProperties()
            .WithTitle($"Daily TCG Statistics Report - {periodText}")
            .WithColor(System.Drawing.Color.Purple.ToNetCordColor());

        embed.AddFields(
            new EmbedFieldProperties().WithName("UID").WithValue(account.GameRoleId).WithInline(),
            new EmbedFieldProperties().WithName("Region").WithValue(account.Region).WithInline(),
            new EmbedFieldProperties().WithName("Period").WithValue(periodText).WithInline()
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

        var charactersToDisplay = result.UsedCharacters.Count > 0 ? result.UsedCharacters : result.TopUseCharacters;
        if (charactersToDisplay.Count > 0)
        {
            const int maxFieldLength = 1024;
            var currentFieldName = "Characters Used";
            var sb = new StringBuilder();

            for (var i = 0; i < charactersToDisplay.Count; i++)
            {
                var card = charactersToDisplay[i];
                var emojiPrefix = emojis != null && emojis.TryGetValue(card.Id.ToString(), out var emoji)
                    ? $"{emoji} "
                    : string.Empty;
                var line = $"{i + 1}. {emojiPrefix}**{card.Name}** - {card.UseCount} games ({card.WinRate:P1} - {card.Proficiency}W/{card.UseCount - card.Proficiency}L)\n";

                if (sb.Length + line.Length > maxFieldLength && sb.Length > 0)
                {
                    embed.AddFields(new EmbedFieldProperties().WithName(currentFieldName).WithValue(sb.ToString().TrimEnd()));
                    sb.Clear();
                    currentFieldName = "Characters Used (Cont.)";
                }

                sb.Append(line);
            }

            if (sb.Length > 0)
            {
                embed.AddFields(new EmbedFieldProperties().WithName(currentFieldName).WithValue(sb.ToString().TrimEnd()));
            }
        }

        if (result.TopUseActions.Count > 0)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < Math.Min(10, result.TopUseActions.Count); i++)
            {
                var card = result.TopUseActions[i];
                sb.AppendLine($"{i + 1}. **{card.Name}** - {card.UseCount} times");
            }
            embed.AddFields(new EmbedFieldProperties().WithName("Top Actions").WithValue(sb.ToString().TrimEnd()));
        }

        embed.WithFooter(new EmbedFooterProperties().WithText($"Total to date: {currentSnapshot.TotalGames} games ({currentSnapshot.TotalWins} wins)"));
        return embed;
    }
}
