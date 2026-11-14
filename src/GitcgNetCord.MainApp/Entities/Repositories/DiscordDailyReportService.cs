using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GitcgNetCord.MainApp.Entities.Repositories;

public class DiscordDailyReportService(
    AppDbContext dbContext
)
{
    // Parent Report Channels
    public async ValueTask<DiscordDailyReportChannel?> FindReportChannelAsync(
        ulong channelId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordDailyReportChannels
            .FindAsync([channelId], cancellationToken: cancellationToken);
    }

    public async Task<bool> IsApprovedParentChannelAsync(
        ulong channelId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordDailyReportChannels
            .AnyAsync(x => x.Id == channelId, cancellationToken);
    }

    public async Task<(DiscordDailyReportChannel Channel, bool IsNew)> AddReportChannelAsync(
        ulong channelId,
        ulong guildId,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await dbContext.DiscordDailyReportChannels
            .FindAsync([channelId], cancellationToken: cancellationToken);

        var isNew = existing == null;
        if (existing == null)
        {
            existing = new DiscordDailyReportChannel
            {
                Id = channelId,
                GuildId = guildId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await dbContext.DiscordDailyReportChannels.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.GuildId = guildId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return (existing, isNew);
    }

    public async Task<bool> RemoveReportChannelAsync(
        ulong channelId,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await dbContext.DiscordDailyReportChannels
            .FindAsync([channelId], cancellationToken: cancellationToken);

        if (existing == null)
            return false;

        dbContext.DiscordDailyReportChannels.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<DiscordDailyReportChannel>> GetReportChannelsByGuildAsync(
        ulong guildId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordDailyReportChannels
            .Where(x => x.GuildId == guildId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    // Thread Subscriptions
    public async Task<DiscordDailyReportThread> SubscribeThreadAsync(
        ulong threadId,
        ulong guildId,
        ulong discordUserId,
        Guid hoyolabAccountId,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await dbContext.DiscordDailyReportThreads
            .FirstOrDefaultAsync(
                x => x.ThreadId == threadId && x.HoyolabAccountId == hoyolabAccountId,
                cancellationToken
            );

        if (existing == null)
        {
            existing = new DiscordDailyReportThread
            {
                ThreadId = threadId,
                GuildId = guildId,
                DiscordUserId = discordUserId,
                HoyolabAccountId = hoyolabAccountId,
                IsEnabled = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await dbContext.DiscordDailyReportThreads.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.IsEnabled = true;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.DiscordUserId = discordUserId;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> UnsubscribeThreadAsync(
        ulong threadId,
        Guid hoyolabAccountId,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await dbContext.DiscordDailyReportThreads
            .FirstOrDefaultAsync(
                x => x.ThreadId == threadId && x.HoyolabAccountId == hoyolabAccountId,
                cancellationToken
            );

        if (existing == null)
            return false;

        dbContext.DiscordDailyReportThreads.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<DiscordDailyReportThread>> GetThreadSubscriptionsAsync(
        ulong threadId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordDailyReportThreads
            .Include(x => x.HoyolabAccount)
            .Where(x => x.ThreadId == threadId && x.IsEnabled)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DiscordDailyReportThread>> GetActiveSubscriptionsForAccountAsync(
        Guid hoyolabAccountId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordDailyReportThreads
            .Where(x => x.HoyolabAccountId == hoyolabAccountId && x.IsEnabled)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DisableSubscriptionAsync(
        int subscriptionId,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await dbContext.DiscordDailyReportThreads
            .FindAsync([subscriptionId], cancellationToken: cancellationToken);

        if (existing == null)
            return false;

        existing.IsEnabled = false;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsUserSubscriberAsync(
        ulong threadId,
        ulong discordUserId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordDailyReportThreads
            .AnyAsync(
                x => x.ThreadId == threadId && x.DiscordUserId == discordUserId && x.IsEnabled,
                cancellationToken
            );
    }

    public async Task<DiscordDailyReportThread> SubscribeDmAsync(
        ulong dmChannelId,
        ulong discordUserId,
        Guid hoyolabAccountId,
        CancellationToken cancellationToken = default
    )
    {
        return await SubscribeThreadAsync(
            threadId: dmChannelId,
            guildId: 0,
            discordUserId: discordUserId,
            hoyolabAccountId: hoyolabAccountId,
            cancellationToken: cancellationToken
        );
    }

    public async Task<DiscordDailyReportThread?> FindSubscriptionInGuildAsync(
        ulong guildId,
        Guid hoyolabAccountId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordDailyReportThreads
            .FirstOrDefaultAsync(
                x => x.GuildId == guildId && x.HoyolabAccountId == hoyolabAccountId && x.IsEnabled,
                cancellationToken
            );
    }

    public async Task<DiscordDailyReportThread?> FindDmSubscriptionAsync(
        ulong discordUserId,
        Guid hoyolabAccountId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordDailyReportThreads
            .FirstOrDefaultAsync(
                x => x.GuildId == 0 && x.DiscordUserId == discordUserId && x.HoyolabAccountId == hoyolabAccountId && x.IsEnabled,
                cancellationToken
            );
    }

    public async Task<List<DiscordDailyReportThread>> GetSubscriptionsForUserAsync(
        ulong discordUserId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordDailyReportThreads
            .Include(x => x.HoyolabAccount)
            .Where(x => x.DiscordUserId == discordUserId && x.IsEnabled)
            .ToListAsync(cancellationToken);
    }
}
