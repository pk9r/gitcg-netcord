namespace GitcgNetCord.MainApp.Models;

public record DailyReportTriggerRequest(
    string? Uid = null,
    bool ForceUpdate = true
);

public record DailyReportTriggerResponse(
    bool Success,
    string Message,
    int ProcessedCount,
    IReadOnlyList<AccountReportResult> Accounts
);

public record AccountReportResult(
    Guid AccountId,
    string GameRoleId,
    string Region,
    string SnapshotName,
    int TotalGames,
    int TotalWins,
    int DeliveredThreadsCount,
    string? Error = null
);
