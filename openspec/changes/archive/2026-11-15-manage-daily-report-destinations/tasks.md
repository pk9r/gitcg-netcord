# Tasks

## 1. Slash Command Handlers & Module Registration

- [x] 1.1 In `src/GitcgNetCord.MainApp/Commands/Slash/CardSnapshotSlashCommand.cs`, refactor `ThreadSubscribeAsync`, `ThreadUnsubscribeAsync`, and `ThreadListAsync` into context-aware `ReportSubscribeAsync`, `ReportUnsubscribeAsync`, and `ReportListAsync` supporting `GuildThread`, Direct Messages (`DMChannel` / `GuildId == null`), and non-thread guild channels. In DM `list`, call `reportService.GetSubscriptionsForUserAsync(context.User.Id)` to build a unified personal report destinations embed. Verify via `dotnet build`.
- [x] 1.2 In `src/GitcgNetCord.MainApp/Modules/Feats/CardSnapshotModule.cs`, replace the `thread` subcommand group with `report`, registering `subscribe`, `unsubscribe`, and `list` subcommands bound to the new handlers. Verify via `dotnet build`.

## 2. Guidance & Component Interaction Updates

- [x] 2.1 In `src/GitcgNetCord.MainApp/Commands/Interactions/DailyReportDestinationComponentInteraction.cs`, update destination skip messages to recommend `/card-snapshot report subscribe` instead of the legacy `/card-snapshot thread subscribe`. Verify via `dotnet build`.

## 3. Automated Testing & Verification

- [x] 3.1 In `tests/GitcgNetCord.MainApp.Tests/AutoReportThreadSubscriptionTests.cs` (or a dedicated test suite), write unit tests validating DM and Thread report subscription, unsubscription, and personal subscription listing logic. Verify tests compile.
- [x] 3.2 Execute xUnit test suite using `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` and verify that all test cases pass cleanly.
