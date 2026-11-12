using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using HoyolabHttpClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GitcgNetCord.MainApp.Infrastructure;

public class DailySnapshotWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<DailySnapshotWorker> logger
) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan AccountThrottleDelay = TimeSpan.FromMilliseconds(1500);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DailySnapshotWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDailySnapshotsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error in DailySnapshotWorker.");
            }

            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("DailySnapshotWorker stopped.");
    }

    public async Task ProcessDailySnapshotsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var cardSnapshotService = scope.ServiceProvider.GetRequiredService<CardSnapshotService>();
        var hoyolab = scope.ServiceProvider.GetRequiredService<HoyolabHttpClientService>();

        var activeAccounts = await dbContext.ActiveHoyolabAccounts
            .Include(x => x.HoyolabAccount)
            .Where(x => x.HoyolabAccount != null && x.HoyolabAccount.IsDailySnapshotEnabled)
            .ToListAsync(cancellationToken);

        if (activeAccounts.Count == 0) return;

        var nowUtc = DateTimeOffset.UtcNow;

        foreach (var activeAccount in activeAccounts)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var account = activeAccount.HoyolabAccount;
            if (account == null || string.IsNullOrWhiteSpace(account.Token)) continue;

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

                if (existing != null) continue;

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
                    "Captured daily snapshot {SnapshotName} for account {AccountId} (UID: {UID}).",
                    snapshotName,
                    account.Id,
                    account.GameRoleId
                );

                // Deliver daily reports to subscribed threads
                var deliveryService = scope.ServiceProvider.GetRequiredService<DailyReportDeliveryService>();
                await deliveryService.DeliverReportsAsync(
                    account: account,
                    allCardsMetadata: data.CardList,
                    currentSnapshot: snapshot,
                    localGameDate: localGameDate,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Failed to capture daily snapshot for account {AccountId} (UID: {UID}) for date {SnapshotName}.",
                    account.Id,
                    account.GameRoleId,
                    snapshotName
                );
            }

            await Task.Delay(AccountThrottleDelay, cancellationToken);
        }
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
