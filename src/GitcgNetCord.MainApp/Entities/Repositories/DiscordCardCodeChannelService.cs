using GitcgNetCord.MainApp.Infrastructure;

namespace GitcgNetCord.MainApp.Entities.Repositories;

public class DiscordCardCodeChannelService(
    AppDbContext dbContext
)
{
    public async ValueTask<DiscordCardCodeChannel?> FindAsync(ulong channelId)
    {
        return await dbContext
            .DiscordCardCodeChannels
            .FindAsync(channelId);
    }
}