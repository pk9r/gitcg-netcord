using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Models;
using HoyolabHttpClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GitcgNetCord.MainApp.Infrastructure;

public class DailySnapshotProcessor(
    AppDbContext dbContext,
    CardSnapshotService cardSnapshotService,
    HoyolabHttpClientService hoyolab,
    DailyReportDeliveryService deliveryService,
    ILogger<DailySnapshotProcessor> logger
)
{
    private static readonly TimeSpan AccountThrottleDelay = TimeSpan.FromMilliseconds(1500);

    public async Task<DailyReportTriggerResponse> ProcessSnapshotsAsync(
        DailyReportTriggerRequest request,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ActiveHoyolabAccount> query = dbContext.ActiveHoyolabAccounts
            .Include(x => x.HoyolabAccount);

        if (!string.IsNullOrWhiteSpace(request.Uid))
        {
            var targetUid = request.Uid.Trim();
            query = query.Where(x => x.HoyolabAccount != null && x.HoyolabAccount.GameRoleId == targetUid);
        }
        else
        {
            query = query.Where(x => x.HoyolabAccount != null && x.HoyolabAccount.IsDailySnapshotEnabled);
        }

        var activeAccounts = await query.ToListAsync(cancellationToken);

        if (activeAccounts.Count == 0)
        {
            var notFoundMsg = !string.IsNullOrWhiteSpace(request.Uid)
                ? $"No active Hoyolab account found with UID '{request.Uid}'."
                : "No active accounts with daily snapshots enabled found.";

            return new DailyReportTriggerResponse(
                Success: string.IsNullOrWhiteSpace(request.Uid),
                Message: notFoundMsg,
                ProcessedCount: 0,
                Accounts: []
            );
        }

        var results = new List<AccountReportResult>();
        var nowUtc = DateTimeOffset.UtcNow;

        for (var i = 0; i < activeAccounts.Count; i++)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var activeAccount = activeAccounts[i];
            var account = activeAccount.HoyolabAccount;
            if (account == null || string.IsNullOrWhiteSpace(account.Token))
            {
                continue;
            }

            var offset = GetServerUtcOffset(account.Region);
            var localServerTime = nowUtc.ToOffset(offset);
            var localGameDate = (localServerTime - TimeSpan.FromHours(4)).Date;
            var snapshotName = $"daily-{localGameDate:yyyyMMdd}";

            try
            {
                var existing = await cardSnapshotService.GetSnapshotByNameAsync(
                    account.Id,
                    snapshotName,
                    cancellationToken
                );

                if (existing != null && !request.ForceUpdate)
                {
                    logger.LogInformation(
                        "Skipping daily snapshot for account {AccountId} (UID: {UID}): snapshot {SnapshotName} already exists.",
                        account.Id,
                        account.GameRoleId,
                        snapshotName
                    );
                    continue;
                }

                var authorize = new HoyolabAuthorize(
                    HoyolabUserId: account.HoyolabUserId,
                    Token: account.Token
                );

                var data = await hoyolab.GetGcgCardListAsync(
                    server: account.Region,
                    roleId: account.GameRoleId,
                    authorize: authorize
                );

                var cardData = data.CardList.ToDictionary(
                    x => x.Id,
                    x => new CardStatSummary(x.UseCount, x.Proficiency)
                );

                var characters = data.CardList
                    .Where(x => x.CardType == "CardTypeCharacter")
                    .ToList();

                var totalGames = characters.Sum(x => x.UseCount) / 3;
                var totalWins = characters.Sum(x => x.Proficiency) / 3;

                var snapshot = await cardSnapshotService.SaveSnapshotAsync(
                    hoyolabAccountId: account.Id,
                    discordUserId: activeAccount.DiscordUserId,
                    name: snapshotName,
                    type: SnapshotType.Daily,
                    totalGames: totalGames,
                    totalWins: totalWins,
                    cardData: cardData,
                    cancellationToken: cancellationToken
                );

                logger.LogInformation(
                    "Processed daily snapshot {SnapshotName} for account {AccountId} (UID: {UID}).",
                    snapshotName,
                    account.Id,
                    account.GameRoleId
                );

                var deliveredCount = await deliveryService.DeliverReportsAsync(
                    account: account,
                    allCardsMetadata: data.CardList,
                    currentSnapshot: snapshot,
                    localGameDate: localGameDate,
                    cancellationToken: cancellationToken
                );

                results.Add(new AccountReportResult(
                    AccountId: account.Id,
                    GameRoleId: account.GameRoleId,
                    Region: account.Region,
                    SnapshotName: snapshotName,
                    TotalGames: totalGames,
                    TotalWins: totalWins,
                    DeliveredThreadsCount: deliveredCount
                ));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(
                    ex,
                    "Failed to process daily snapshot for account {AccountId} (UID: {UID}).",
                    account.Id,
                    account.GameRoleId
                );

                results.Add(new AccountReportResult(
                    AccountId: account.Id,
                    GameRoleId: account.GameRoleId,
                    Region: account.Region,
                    SnapshotName: snapshotName,
                    TotalGames: 0,
                    TotalWins: 0,
                    DeliveredThreadsCount: 0,
                    Error: ex.Message
                ));
            }

            if (i < activeAccounts.Count - 1)
            {
                await Task.Delay(AccountThrottleDelay, cancellationToken);
            }
        }

        var successCount = results.Count(x => x.Error == null);
        return new DailyReportTriggerResponse(
            Success: true,
            Message: $"Processed {results.Count} account(s) ({successCount} succeeded).",
            ProcessedCount: results.Count,
            Accounts: results
        );
    }

    public static TimeSpan GetServerUtcOffset(string region)
    {
        return region.ToLowerInvariant() switch
        {
            "os_euro" or "os_europe" => TimeSpan.FromHours(1),
            "os_usa" or "os_america" => TimeSpan.FromHours(-5),
            _ => TimeSpan.FromHours(8) // os_asia, os_cht, or default
        };
    }
}
