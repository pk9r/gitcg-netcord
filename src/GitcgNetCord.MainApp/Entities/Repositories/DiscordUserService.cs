using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GitcgNetCord.MainApp.Entities.Repositories;

public class DiscordUserService(
    AppDbContext dbContext
)
{
    public async Task<DiscordUser>
        GetDiscordUserAsync(ulong discordUserId)
    {
        var discordUser =
            await dbContext.DiscordUsers
                .FirstOrDefaultAsync(x => x.Id == discordUserId) ??
            await AddDiscordUserAsync(discordUserId);

        return discordUser;
    }

    private async Task<DiscordUser>
        AddDiscordUserAsync(ulong discordUserId)
    {
        var discordUser = new DiscordUser { Id = discordUserId };
        await dbContext.DiscordUsers.AddAsync(discordUser);

        await dbContext.SaveChangesAsync();

        return discordUser;
    }
}
