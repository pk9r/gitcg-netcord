using GitcgNetCord.MainApp.Enums;
using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GitcgNetCord.MainApp.Entities.Repositories;

public class DiscordCardCodeChannelService(
    AppDbContext dbContext
)
{
    public async ValueTask<DiscordCardCodeChannel?> 
        FindAsync(ulong? channelId)
    {
        return await dbContext
            .DiscordCardCodeChannels
            .FindAsync(channelId);
    }

    public async Task<(DiscordCardCodeChannel Channel, bool IsNew)> AddAsync(
        ulong channelId,
        ulong guildId,
        SharingDecodeType decodeType = SharingDecodeType.ImageGameBackground,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await dbContext.DiscordCardCodeChannels
            .FindAsync([channelId], cancellationToken: cancellationToken);

        var isNew = existing == null;
        if (existing == null)
        {
            existing = new DiscordCardCodeChannel
            {
                Id = channelId,
                GuildId = guildId,
                SharingDecodeType = decodeType
            };
            await dbContext.DiscordCardCodeChannels.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.GuildId = guildId;
            existing.SharingDecodeType = decodeType;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return (existing, isNew);
    }

    public async Task<bool> RemoveAsync(
        ulong channelId,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await dbContext.DiscordCardCodeChannels
            .FindAsync([channelId], cancellationToken: cancellationToken);

        if (existing == null)
            return false;

        dbContext.DiscordCardCodeChannels.Remove(existing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<DiscordCardCodeChannel>> GetByGuildAsync(
        ulong guildId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordCardCodeChannels
            .Where(x => x.GuildId == guildId)
            .ToListAsync(cancellationToken);
    }
}