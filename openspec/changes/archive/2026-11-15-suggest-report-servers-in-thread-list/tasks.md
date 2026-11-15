# Tasks

## 1. Service Layer Implementation

- [x] 1.1 Refactor `GetSuggestedReportGuildsAsync` in `DiscordDailyReportService.cs` to use Npgsql/PostgreSQL-translatable LINQ (filtering with `.Any()` on report channels and projecting anonymous type before mapping to `SuggestedReportGuild`). Verify with `dotnet build src/GitcgNetCord.MainApp`.
- [x] 1.2 Verify unit tests for `GetSuggestedReportGuildsAsync` in `tests/GitcgNetCord.MainApp.Tests/AutoReportThreadSubscriptionTests.cs`. Verify with `dotnet test tests/GitcgNetCord.MainApp.Tests --filter FullyQualifiedName~GetSuggestedReportGuilds`.

## 2. Slash Command Implementation

- [x] 2.1 Add `try...catch` error resilience to `CardSnapshotSlashCommand.ThreadListAsync` to safely handle exceptions after `DeferredMessage` and modify the response with an informative English error notice. Verify with `dotnet build src/GitcgNetCord.MainApp`.
- [x] 2.2 Update `CardSnapshotSlashCommand.ThreadListAsync` to render an informative embed with the empty notice, "Suggested Report Servers" embed field, and Link Buttons when inside a thread with zero subscriptions. Verify with `dotnet build src/GitcgNetCord.MainApp`.
- [x] 2.3 Update `CardSnapshotSlashCommand.ThreadListAsync` to render an informative embed explaining thread subscription requirements, along with "Suggested Report Servers" embed field and Link Buttons, when executed outside of a thread. Verify with `dotnet build src/GitcgNetCord.MainApp`.

## 3. Verification & Test Suite

- [x] 3.1 Run unit tests in `tests/GitcgNetCord.MainApp.Tests`. Verify with `dotnet test tests/GitcgNetCord.MainApp.Tests`.
- [x] 3.2 Run and pass all solution test suites using `dotnet test`.
