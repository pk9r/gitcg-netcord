# Tasks

## 1. Database Model & Migration

- [x] 1.1 Create `CardSnapshot` and `GameVersion` entities in `src/GitcgNetCord.MainApp/Entities/` and configure mappings (including JSONB column for card data) in `AppDbContext`. Verify `dotnet build` compiles successfully.
- [x] 1.2 Generate an EF Core migration `AddCardSnapshotAndGameVersion` (without seeding past versions). Verify migration scripts compile and apply without database errors.

## 2. Core Calculation Service & Repositories

- [x] 2.1 Implement `CardSnapshotRepository` and `GameVersionRepository` to handle querying versions, creating snapshots, and listing user checkpoints. Verify repository methods compile and pass basic integration queries.
- [x] 2.2 Implement `ICardStatsCalculator` to compute delta use counts, delta proficiencies, total games, win rates, and rankings between two snapshot states or a snapshot and live data. Add unit tests verifying delta calculations and edge cases (new cards, 0 matches played).

## 3. Background Version Snapshot Worker

- [x] 3.1 Implement `VersionSnapshotWorker` background hosted service to periodically check for released `GameVersion` records, sequentially snapshot active `HoyolabAccount` records with rate-limiting, and mark versions as processed. Verify worker executes gracefully with failure logging on individual accounts.

## 4. Discord Slash Commands & Autocompletion

- [x] 4.1 Implement `CardSnapshotAutocompleteProvider` to provide autocomplete suggestions for `version`, `from`, and `to` parameters with English labels (displaying `Version <X.Y>`, user custom snapshots, and `Present (Live)`). Verify autocomplete returns valid choice properties.
- [x] 4.2 Create `/card-snapshot` command module with `save`, `list`, and `delete` subcommands for user-managed snapshots. Verify handlers compile and register in Discord bot modules.
- [x] 4.3 Update `CardSlashCommand.ExecuteAsync` to accept `version`, `from`, and `to` parameters, calculate periodic delta stats using `ICardStatsCalculator`, and render the updated Discord embed in full English. Verify command renders both all-time and periodic embeds correctly.

## 5. Integration Verification

- [x] 5.1 Run project build and execute all unit and integration tests. Verify that existing functionality (`/card` all-time mode) and new periodic modes pass without regressions.
