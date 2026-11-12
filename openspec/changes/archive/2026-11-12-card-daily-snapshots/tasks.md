# Tasks

## 1. Database Model & Migrations

- [x] 1.1 Add `Daily` to `SnapshotType` enum in `GitcgNetCord.MainApp.Entities.SnapshotType`. Verify project compiles.
- [x] 1.2 Add `IsDailySnapshotEnabled` property to `HoyolabAccount` entity and configure property in `AppDbContext`.
- [x] 1.3 Generate and review EF Core migration for `IsDailySnapshotEnabled`. Verify schema builds cleanly.

## 2. Daily Snapshot Worker

- [x] 2.1 Implement `DailySnapshotWorker` in `GitcgNetCord.MainApp.Infrastructure` to periodically evaluate regional game reset times (04:00 AM for Asia, Europe, and America), fetch card statistics, and persist `daily-yyyyMMdd` snapshots for accounts with `IsDailySnapshotEnabled`. Verify rate limiting and error handling.
- [x] 2.2 Register `DailySnapshotWorker` as a hosted service in `Extensions.cs`. Verify worker initializes successfully on application startup.

## 3. Slash Command Updates

- [x] 3.1 Add `daily` subcommand with `enabled` parameter to `CardSnapshotModule.cs`.
- [x] 3.2 Implement `DailyAsync` in `CardSnapshotSlashCommand.cs` to toggle `IsDailySnapshotEnabled` on the active Hoyolab account. Verify toggle responds with correct status message.
- [x] 3.3 Update `ListAsync` in `CardSnapshotSlashCommand.cs` to show only `GameVersion` and `UserCustom` snapshots in the displayed embed, with a footer summarizing the count of daily snapshots.
- [x] 3.4 Update `DeleteAsync` in `CardSnapshotSlashCommand.cs` to check `SnapshotType` and reject deletion of non-custom snapshots (`Daily` or `GameVersion`).

## 4. Autocomplete Smart Filter & Verification

- [x] 4.1 Update `CardSnapshotAutocompleteProvider.cs` to exclude `SnapshotType.Daily` from default autocomplete suggestions, only returning matching `[Daily]` options when users type "daily" or date patterns.
- [x] 4.2 Verify `/card` range queries function seamlessly with daily snapshot identifiers (`from: daily-yyyyMMdd`, `to: daily-yyyyMMdd`) and unit/integration tests pass.
