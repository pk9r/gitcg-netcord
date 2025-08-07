namespace Gitcg.NetCord.MainApp.Entities;

public class DiscordUser
{
    public ulong Id { get; set; }

    public ICollection<HoyolabAccount> HoyolabAccounts { get; set; } = [];
    public ActiveHoyolabAccount? ActiveHoyolabAccount { get; set; }

    public Task AddHoyolabAccountAsync(
        string hoyolabUserId, string token, string gameRoleId, string region
    )
    {
        var hoyolabAccount = new HoyolabAccount
        {
            HoyolabUserId = hoyolabUserId, //
            Token = token, //
            GameRoleId = gameRoleId, //
            Region = region //
        };

        HoyolabAccounts.Add(hoyolabAccount);

        if (
            ActiveHoyolabAccount == null ||
            ActiveHoyolabAccount.HoyolabAccountId == Guid.Empty
        )
        {
            ActiveHoyolabAccount = new ActiveHoyolabAccount
            {
                HoyolabAccount = hoyolabAccount //
            };
        }

        return Task.CompletedTask;
    }
}
