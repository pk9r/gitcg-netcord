using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using HoyolabHttpClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GitcgNetCord.MainApp.Infrastructure;

public class VersionSnapshotWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<VersionSnapshotWorker> logger
) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan AccountThrottleDelay = TimeSpan.FromMilliseconds(1500);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("VersionSnapshotWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueVersionsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unexpected error in VersionSnapshotWorker.");
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

        logger.LogInformation("VersionSnapshotWorker stopped.");
    }

    public async Task ProcessDueVersionsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var gameVersionService = scope.ServiceProvider.GetRequiredService<GameVersionService>();
        var cardSnapshotService = scope.ServiceProvider.GetRequiredService<CardSnapshotService>();
        var hoyolab = scope.ServiceProvider.GetRequiredService<HoyolabHttpClientService>();

        var dueVersions = await gameVersionService.GetDueUnprocessedVersionsAsync(DateTimeOffset.UtcNow, cancellationToken);
        if (dueVersions.Count == 0) return;

        foreach (var version in dueVersions)
        {
            logger.LogInformation(
                "Processing snapshot for game version {VersionNumber} ({VersionName})...",
                version.VersionNumber,
                version.Name ?? "Unnamed"
            );

            var activeAccounts = await dbContext.ActiveHoyolabAccounts
                .Include(x => x.HoyolabAccount)
                .ToListAsync(cancellationToken);

            foreach (var activeAccount in activeAccounts)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var account = activeAccount.HoyolabAccount;
                if (account == null || string.IsNullOrWhiteSpace(account.Token)) continue;

                try
                {
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

                    await cardSnapshotService.SaveSnapshotAsync(
                        hoyolabAccountId: account.Id,
                        discordUserId: activeAccount.DiscordUserId,
                        name: version.VersionNumber,
                        type: SnapshotType.GameVersion,
                        totalGames: totalGames,
                        totalWins: totalWins,
                        cardData: cardData,
                        cancellationToken: cancellationToken
                    );

                    logger.LogDebug(
                        "Captured snapshot for account {AccountId} (UID: {UID}) for version {VersionNumber}.",
                        account.Id,
                        account.GameRoleId,
                        version.VersionNumber
                    );
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Failed to capture snapshot for account {AccountId} (UID: {UID}) on version {VersionNumber}.",
                        account.Id,
                        account.GameRoleId,
                        version.VersionNumber
                    );
                }

                await Task.Delay(AccountThrottleDelay, cancellationToken);
            }

            await gameVersionService.MarkVersionAsProcessedAsync(version.Id, cancellationToken);
            logger.LogInformation("Completed snapshot for version {VersionNumber}.", version.VersionNumber);
        }
    }
}
