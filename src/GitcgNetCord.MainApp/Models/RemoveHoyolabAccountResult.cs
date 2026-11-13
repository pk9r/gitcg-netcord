using GitcgNetCord.MainApp.Entities;

namespace GitcgNetCord.MainApp.Models;

public record RemoveHoyolabAccountResult(
    bool Success,
    string Message,
    HoyolabAccount? RemovedAccount = null,
    bool WasActive = false,
    HoyolabAccount? NewActiveAccount = null,
    IReadOnlyList<HoyolabAccount>? RemainingAccounts = null
)
{
    public IReadOnlyList<HoyolabAccount> RemainingAccounts { get; init; } = RemainingAccounts ?? [];
}
