using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Infrastructure;
using GitcgNetCord.MainApp.Models;
using Microsoft.EntityFrameworkCore;

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
    
    public async Task<HoyolabAccount> AddHoyolabAccountAsync(
        ulong discordUserId,
        string hoyolabUserId,
        string token,
        string gameRoleId,
        string region
    )
    {
        var existing = await dbContext.HoyolabAccounts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId && x.GameRoleId == gameRoleId);

        if (existing != null)
        {
            existing.IsDeleted = false;
            existing.HoyolabUserId = hoyolabUserId;
            existing.Token = token;
            existing.Region = region;
            existing.IsDailySnapshotEnabled = false;

            var active = await dbContext.ActiveHoyolabAccounts
                .FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId);

            if (active == null || active.HoyolabAccountId == Guid.Empty)
            {
                if (active == null)
                {
                    active = new ActiveHoyolabAccount { DiscordUserId = discordUserId };
                    await dbContext.ActiveHoyolabAccounts.AddAsync(active);
                }
                active.HoyolabAccountId = existing.Id;
            }

            await dbContext.SaveChangesAsync();
            return existing;
        }

        var discordUser = await discordUserService
            .GetDiscordUserAsync(discordUserId: discordUserId);

        await dbContext.Attach(discordUser)
            .Reference(x => x.ActiveHoyolabAccount)
            .LoadAsync();

        var newAccount = new HoyolabAccount
        {
            DiscordUserId = discordUserId,
            HoyolabUserId = hoyolabUserId,
            Token = token,
            GameRoleId = gameRoleId,
            Region = region
        };

        discordUser.HoyolabAccounts.Add(newAccount);

        if (
            discordUser.ActiveHoyolabAccount == null ||
            discordUser.ActiveHoyolabAccount.HoyolabAccountId == Guid.Empty
        )
        {
            discordUser.ActiveHoyolabAccount = new ActiveHoyolabAccount
            {
                HoyolabAccount = newAccount
            };
        }

        await dbContext.SaveChangesAsync();
        return newAccount;
    }

    public async Task<RemoveHoyolabAccountResult> RemoveHoyolabAccountAsync(
        ulong discordUserId,
        string gameRoleId,
        CancellationToken cancellationToken = default
    )
    {
        var account = await dbContext.HoyolabAccounts
            .FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId && x.GameRoleId == gameRoleId, cancellationToken);

        if (account == null)
        {
            return new RemoveHoyolabAccountResult(
                Success: false,
                Message: $"Hoyolab account with UID '{gameRoleId}' not found for user."
            );
        }

        account.IsDeleted = true;
        account.Token = string.Empty;
        account.HoyolabUserId = string.Empty;
        account.IsDailySnapshotEnabled = false;

        var subscriptions = await dbContext.DiscordDailyReportThreads
            .Where(x => x.HoyolabAccountId == account.Id)
            .ToListAsync(cancellationToken);

        if (subscriptions.Count > 0)
        {
            dbContext.DiscordDailyReportThreads.RemoveRange(subscriptions);
        }

        var activeAccount = await dbContext.ActiveHoyolabAccounts
            .FirstOrDefaultAsync(x => x.DiscordUserId == discordUserId, cancellationToken);

        bool wasActive = activeAccount != null && activeAccount.HoyolabAccountId == account.Id;
        HoyolabAccount? newActiveAccount = null;

        var remainingAccounts = await dbContext.HoyolabAccounts
            .Where(x => x.DiscordUserId == discordUserId && x.Id != account.Id && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        if (wasActive)
        {
            if (remainingAccounts.Count == 0)
            {
                if (activeAccount != null)
                {
                    dbContext.ActiveHoyolabAccounts.Remove(activeAccount);
                }
                newActiveAccount = null;
            }
            else
            {
                newActiveAccount = remainingAccounts[0];
                activeAccount!.HoyolabAccountId = newActiveAccount.Id;
            }
        }
        else if (activeAccount != null && activeAccount.HoyolabAccountId != Guid.Empty)
        {
            newActiveAccount = remainingAccounts.FirstOrDefault(x => x.Id == activeAccount.HoyolabAccountId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RemoveHoyolabAccountResult(
            Success: true,
            Message: $"Successfully removed Hoyolab account {account.GameRoleId}.",
            RemovedAccount: account,
            WasActive: wasActive,
            NewActiveAccount: newActiveAccount,
            RemainingAccounts: remainingAccounts
        );
    }
}
