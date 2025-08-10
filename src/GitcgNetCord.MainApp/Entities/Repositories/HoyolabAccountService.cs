using GitcgNetCord.MainApp.Infrastructure;

namespace GitcgNetCord.MainApp.Entities.Repositories;

public class HoyolabAccountService(
    AppDbContext dbContext,
    DiscordUserService discordUserService
)
{
    public async Task<IEnumerable<HoyolabAccount>>
        GetHoyolabAccountsAsync(ulong discordUserId)
    {
        var discordUser = await discordUserService
            .GetDiscordUserAsync(discordUserId: discordUserId);

        var hoyolabAccounts = dbContext.Attach(discordUser)
            .Collection(x => x.HoyolabAccounts)
            .Query();

        return hoyolabAccounts;
    }
    
    public async Task AddHoyolabAccountAsync(
        ulong discordUserId,
        string hoyolabUserId,
        string token,
        string gameRoleId,
        string region
    )
    {
        var discordUser = await discordUserService
            .GetDiscordUserAsync(discordUserId: discordUserId);

        await dbContext.Attach(discordUser)
            .Reference(x => x.ActiveHoyolabAccount)
            .LoadAsync();

        await discordUser.AddHoyolabAccountAsync(
            hoyolabUserId: hoyolabUserId,
            token: token,
            gameRoleId: gameRoleId,
            region: region
        );

        await dbContext.SaveChangesAsync();
    }
}
