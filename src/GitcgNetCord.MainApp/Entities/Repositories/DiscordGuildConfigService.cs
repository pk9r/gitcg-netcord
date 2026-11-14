using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GitcgNetCord.MainApp.Entities.Repositories;

public class DiscordGuildConfigService(
    AppDbContext dbContext
)
{
    public async ValueTask<DiscordGuildConfig?> GetConfigAsync(
        ulong guildId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordGuildConfigs
            .FindAsync([guildId], cancellationToken: cancellationToken);
    }

    public async Task<string?> GetInviteUrlAsync(
        ulong guildId,
        CancellationToken cancellationToken = default
    )
    {
        var config = await dbContext.DiscordGuildConfigs
            .FindAsync([guildId], cancellationToken: cancellationToken);

        return config?.InviteUrl;
    }

    public async Task<DiscordGuildConfig> SetInviteUrlAsync(
        ulong guildId,
        string inviteUrl,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await dbContext.DiscordGuildConfigs
            .FindAsync([guildId], cancellationToken: cancellationToken);

        if (existing == null)
        {
            existing = new DiscordGuildConfig
            {
                GuildId = guildId,
                InviteUrl = inviteUrl.Trim(),
                CreatedAt = DateTimeOffset.UtcNow
            };
            await dbContext.DiscordGuildConfigs.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.InviteUrl = inviteUrl.Trim();
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> RemoveInviteUrlAsync(
        ulong guildId,
        CancellationToken cancellationToken = default
    )
    {
        var existing = await dbContext.DiscordGuildConfigs
            .FindAsync([guildId], cancellationToken: cancellationToken);

        if (existing == null || string.IsNullOrWhiteSpace(existing.InviteUrl))
            return false;

        existing.InviteUrl = null;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<DiscordGuildConfig>> GetGuildsWithInvitesAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext.DiscordGuildConfigs
            .Where(x => x.InviteUrl != null && x.InviteUrl != "")
            .ToListAsync(cancellationToken);
    }
}
