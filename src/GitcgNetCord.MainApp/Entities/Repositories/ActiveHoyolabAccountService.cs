using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GitcgNetCord.MainApp.Entities.Repositories;

public class ActiveHoyolabAccountService(
    HoyolabAccountService hoyolabAccountService,
    AppDbContext dbContext
)
{
    public async Task<HoyolabAccount?>
        GetActiveHoyolabAccountAsync(ulong discordUserId)
    {
        var activeAccount = await dbContext
            .ActiveHoyolabAccounts
            .FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);

        if (activeAccount == null)
            return null;

        if (activeAccount.HoyolabAccountId == Guid.Empty)
            return null;

        await dbContext.Entry(activeAccount)
            .Reference(x => x.HoyolabAccount)
            .LoadAsync();

        return activeAccount.HoyolabAccount;
    }

    public async Task
        UpdateActiveHoyolabAccountAsync(
            ulong discordUserId,
            string gameRoleId
        )
    {
        var hoyolabAccounts = await hoyolabAccountService
            .GetHoyolabAccountsAsync(discordUserId);

        var newActiveAccount = hoyolabAccounts
            .FirstOrDefault(x => x.GameRoleId == gameRoleId);

        if (newActiveAccount == null)
            throw new InvalidOperationException(
                $"Hoyolab account with UID '{gameRoleId}' not found for user."
            );
     
        var activeAccount = await dbContext.ActiveHoyolabAccounts
            .FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);
        
        if (activeAccount == null)
        {
            activeAccount = new ActiveHoyolabAccount { 
                DiscordUserId = discordUserId,
            };
            
            await dbContext
                .ActiveHoyolabAccounts
                .AddAsync(activeAccount);
        }

        activeAccount.HoyolabAccountId = newActiveAccount.Id;

        await dbContext.SaveChangesAsync();
    }
}
