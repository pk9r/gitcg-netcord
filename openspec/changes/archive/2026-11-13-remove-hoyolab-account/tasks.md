# Tasks

## 1. Data Model & EF Core Migration

- [x] 1.1 Add `IsDeleted` property to `HoyolabAccount` entity and configure entity property and global query filter `HasQueryFilter(x => !x.IsDeleted)` in `AppDbContext`
- [x] 1.2 Create EF Core migration `AddIsDeletedToHoyolabAccount` and verify migration applies cleanly to the PostgreSQL database schema

## 2. Service Layer Implementation

- [x] 2.1 Define `RemoveHoyolabAccountResult` model and implement `RemoveHoyolabAccountAsync` in `HoyolabAccountService` handling token wiping, thread subscription removal, and active account fallback resolution
- [x] 2.2 Update `AddHoyolabAccountAsync` in `HoyolabAccountService` using `IgnoreQueryFilters()` to reactivate soft-deleted accounts and refresh credentials if the user re-adds an existing UID
- [x] 2.3 Verify `ActiveHoyolabAccountService` and `HoyolabAccountAutocompleteProvider` correctly exclude soft-deleted accounts

## 3. NetCord Handlers & Discord Interactions

- [x] 3.1 Uncomment and activate the "Remove account" button in `HoyolabAccountsSlashCommand`
- [x] 3.2 Implement `RemoveHoyolabAccountComponentInteraction` handling the initial removal dropdown, account deletion execution, and fallback active account select menu when $\ge 2$ accounts remain
- [x] 3.3 Implement `RemoveHoyolabAccountSlashCommand` providing `/remove-hoyolab-account account:<UID>` with autocomplete via `HoyolabAccountAutocompleteProvider`
- [x] 3.4 Register the new slash command and component interaction handlers in `HoyolabAccountModule`

## 4. Testing & Verification

- [x] 4.1 Write comprehensive unit tests for `RemoveHoyolabAccountAsync` testing single account removal, multi-account active fallback, snapshot preservation, and re-linking reactivation
- [x] 4.2 Execute and pass the full test suite with `dotnet test`
