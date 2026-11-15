# Tasks: Unify Server Admin Commands

## 1. Handlers & Permission Alignment

- [x] 1.1 Update `CardCodeChannelSlashCommand.cs` to check `Permissions.ManageGuild` (and `Permissions.Administrator`) instead of `ManageChannels`, keeping response notices in English. Verify via `dotnet build`.
- [x] 1.2 Update `CardSnapshotSlashCommand.cs` (`ChannelAddAsync`, `ChannelRemoveAsync`, `ChannelListAsync`) to check `Permissions.ManageGuild` (and `Permissions.Administrator`) instead of `ManageChannels`. Verify via `dotnet build`.
- [x] 1.3 Implement `ServerSlashCommand.OverviewShowAsync` in `ServerSlashCommand.cs` to query `DiscordGuildConfigService`, `DiscordCardCodeChannelService`, and `DiscordDailyReportService`, returning a comprehensive English embed dashboard with quick admin command references. Verify via `dotnet build`.

## 2. Slash Command Routing & Module Restructuring

- [x] 2.1 Refactor `ServerModule.cs` to register `/server` with four subcommand groups (`overview`, `invite`, `cardcode`, and `report-channel`), enforcing `Permissions.ManageGuild` and guild context at the root group level. Verify via `dotnet build`.
- [x] 2.2 Remove legacy `/cardcode-channel` command group from `CardCodeModule.cs`. Verify via `dotnet build`.
- [x] 2.3 Remove legacy `/card-snapshot channel` subcommand group from `CardSnapshotModule.cs`. Verify via `dotnet build`.

## 3. Verification & Testing

- [x] 3.1 Update and add unit tests in `tests/GitcgNetCord.MainApp.Tests` covering `/server` command registration, permission checks (`ManageGuild`), and overview formatting.
- [x] 3.2 Run and pass all xUnit tests using `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` and verify zero test regressions.

## Workflow follow-up

- Review and archive the change via `/openspec-archive-change` after verification succeeds.
