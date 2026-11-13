using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Models;
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
        var processor = scope.ServiceProvider.GetRequiredService<DailySnapshotProcessor>();
        await processor.ProcessSnapshotsAsync(new DailyReportTriggerRequest(ForceUpdate: false), cancellationToken);
    }

    public static TimeSpan GetServerUtcOffset(string region) =>
        DailySnapshotProcessor.GetServerUtcOffset(region);
}
