# Tasks

## 1. Data Model and Storage

- [x] 1.1 Create `DiscordDailyReportChannel` and `DiscordDailyReportThread` entities in `src/GitcgNetCord.MainApp/Entities/` and verify the project builds
- [x] 1.2 Register DbSets and configure entity constraints (unique index on `(ThreadId, HoyolabAccountId)`) in `AppDbContext`, generate EF Core migration `AddDiscordDailyReportTables`, and verify migration builds cleanly
- [x] 1.3 Implement `DiscordDailyReportService` in `src/GitcgNetCord.MainApp/Entities/Repositories/` to manage parent report channels and thread subscriptions, and register it in `Extensions.cs`

## 2. Parent Channel Admin Commands

- [x] 2.1 Implement `/card-snapshot channel add [channel]`, `/card-snapshot channel remove [channel]`, and `/card-snapshot channel list` methods in `CardSnapshotSlashCommand.cs` requiring `ManageChannels` permission in guild contexts
- [x] 2.2 Wire the channel subcommands in `CardSnapshotModule.cs` and verify slash command tree registration
- [x] 2.3 Refactor channel admin command descriptions and responses to English

## 3. Thread Subscription Commands

- [x] 3.1 Implement `/card-snapshot thread subscribe [account]`, `/card-snapshot thread unsubscribe [account]`, and `/card-snapshot thread list` in `CardSnapshotSlashCommand.cs`
- [x] 3.2 Add validation in `CardSnapshotSlashCommand.cs` ensuring execution occurs in a thread, the user is the thread owner, the thread's parent is an approved report channel, and the target account has daily snapshots enabled
- [x] 3.3 Register thread subcommands in `CardSnapshotModule.cs` and verify parameter autocomplete and command registration
- [x] 3.4 Refactor thread subscription command descriptions, parameter descriptions, and response messages to English

## 4. Daily Report Delivery and Integration

- [x] 4.1 Implement `DailyReportDeliveryService` to calculate daily delta stats using `ICardStatsCalculator` and generate Discord embeds for both active match days and 0-match days
- [x] 4.2 Integrate report delivery into `DailySnapshotWorker` immediately following successful daily snapshot capture, including REST error handling (for deleted/archived threads) and message send throttling
- [x] 4.3 Add unit tests in `tests/GitcgNetCord.MainApp.Tests/` covering thread validation, daily report embed formatting, and delivery dispatch, and verify all tests pass (`dotnet test`)
