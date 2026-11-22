# Proposal

## Why

During application startup and execution under .NET Aspire, Entity Framework Core emits three `Microsoft.EntityFrameworkCore.Model.Validation[10622]` (`CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning`) warnings. These warnings occur because `HoyolabAccount` defines a soft-delete global query filter (`HasQueryFilter(x => !x.IsDeleted)`), while its dependent entities (`CardSnapshot`, `ActiveHoyolabAccount`, and `DiscordDailyReportThread`) are configured as required relationships with non-nullable `Guid HoyolabAccountId` foreign keys. This mismatch causes EF Core to emit warnings and can silently omit dependent historical entities (like card snapshots) during `INNER JOIN` queries when parent accounts are soft-deleted.

Configuring these relationships as optional navigations with nullable foreign keys decouples historical and dependent records, prevents unexpected `INNER JOIN` record dropping, and eliminates the runtime validation warnings.

## What Changes

- Modify `CardSnapshot`: Make `HoyolabAccountId` nullable (`Guid?`) and navigation `HoyolabAccount?` optional, with `OnDelete(DeleteBehavior.SetNull)` in `AppDbContext`.
- Modify `ActiveHoyolabAccount`: Make `HoyolabAccountId` nullable (`Guid?`) and navigation `HoyolabAccount?` optional in `AppDbContext`, replacing `Guid.Empty` sentinels with idiomatic nullable semantics.
- Modify `DiscordDailyReportThread`: Make `HoyolabAccountId` nullable (`Guid?`) and navigation `HoyolabAccount?` optional in `AppDbContext`.
- Generate and apply an EF Core migration (`MakeHoyolabAccountNavigationsOptional`) using `dotnet ef` CLI tooling.
- Update domain services (`CardSnapshotService`, `ActiveHoyolabAccountService`, `HoyolabAccountService`) and entities (`DiscordUser`) to handle nullable `HoyolabAccountId` values gracefully.
- Update unit tests to verify snapshot persistence and active account resolution with optional navigations.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
None. This change is an internal EF Core model mapping and database schema refactoring that preserves existing business behavior (`skip_specs: true`).

## Impact

- **Database**: Adds an EF Core migration that alters columns `"HoyolabAccountId"` in `"CardSnapshots"`, `"ActiveHoyolabAccounts"`, and `"DiscordDailyReportThreads"` to allow `NULL` values.
- **Entities & DbContext**: `src/GitcgNetCord.MainApp/Entities/CardSnapshot.cs`, `ActiveHoyolabAccount.cs`, `DiscordDailyReportThread.cs`, `DiscordUser.cs`, and `Infrastructure/AppDbContext.cs`.
- **Services**: `CardSnapshotService.cs`, `ActiveHoyolabAccountService.cs`, `HoyolabAccountService.cs`.
- **Tests**: `tests/GitcgNetCord.MainApp.Tests` test suites for account removal and card snapshots.
- **Infrastructure**: No new infrastructure or Aspire resources are required.
