# Proposal

## Why

Currently, users can add Hoyolab accounts and switch their active account, but there is no way to remove an added account when it is no longer used or needed. The "Remove account" button in `/hoyolab-accounts` is commented out, leaving users with no self-service mechanism to unlink an account, stop unwanted background daily snapshot reports for that account, or clean up their account list.

Providing a complete account removal flow empowers users to manage their linked accounts safely while preserving their historical card statistics snapshots and immediately terminating automated daily report deliveries.

## What Changes

- Enable the **"Remove account"** button in `/hoyolab-accounts` (`remove-hoyolab-account`).
- Implement an interactive **String Select Menu** when clicking "Remove account", listing all linked Hoyolab accounts (with UID, region, and nickname) for selection.
- Implement a new top-level slash command `/remove-hoyolab-account account:<UID>` with UID autocomplete via `HoyolabAccountAutocompleteProvider`.
- Implement account removal via **soft-delete** (`IsDeleted` flag) on `HoyolabAccount`:
  - Clears sensitive authentication tokens (`Token = string.Empty`, `HoyolabUserId = string.Empty`) to revoke Hoyolab API access.
  - Disables automated daily snapshots (`IsDailySnapshotEnabled = false`).
  - Cancels/disables all daily report thread subscriptions (`DiscordDailyReportThread`) associated with the account.
  - Preserves all historical `CardSnapshot` records tied to the account so stats and milestone comparisons remain intact.
  - Allows reactivation and credential refresh if the user re-adds the same UID in the future.
- Intelligently reassign the user's active account when the removed account was active:
  - If 0 accounts remain: delete/clear `ActiveHoyolabAccount`.
  - If 1 account remains: automatically switch `ActiveHoyolabAccount` to the sole remaining account and notify the user.
  - If $\ge 2$ accounts remain: automatically switch `ActiveHoyolabAccount` to the first remaining account and provide an in-message String Select Menu allowing the user to pick an alternate active account immediately.

### Scope Boundaries

- **In-Scope**:
  - Soft-delete account removal in `HoyolabAccountService` (`RemoveHoyolabAccountAsync`).
  - Active account reassignment and prompt logic in `ActiveHoyolabAccountService`.
  - Discord component interactions (Button, String Select Menu) for account removal and active account switching.
  - Slash command `/remove-hoyolab-account` with autocomplete.
  - EF Core migration adding `IsDeleted` to `HoyolabAccounts` and updating entity queries.
  - Unit tests covering account removal, active account fallback, and snapshot retention.
- **Out-of-Scope**:
  - Hard-deleting historical `CardSnapshot` rows.
  - Modifying the Hoyolab HTTP client authentication flow itself.

### SemVer Impact

- **PATCH / MINOR**: Backward-compatible feature enhancement (`MINOR`). No breaking changes to existing commands, database records, or public contracts.

## Capabilities

### New Capabilities
- `hoyolab-account-management`: Covers adding, listing, updating active, and removing linked Hoyolab accounts, including active account fallback transitions and retention of historical snapshots.

### Modified Capabilities
- *(None)*

## Impact

- **Database**: Adds boolean column `IsDeleted` (default `false`) to `HoyolabAccounts` table. Updates entity configuration and queries in `AppDbContext`.
- **Services**: `HoyolabAccountService` gains `RemoveHoyolabAccountAsync`; `GetHoyolabAccountsAsync` and `ActiveHoyolabAccountService` filter out deleted accounts. `DiscordDailyReportService` cleans up subscriptions for removed accounts.
- **Discord Commands & Interactions**:
  - `HoyolabAccountsSlashCommand`: Uncomments and connects the "Remove account" button.
  - `RemoveHoyolabAccountComponentInteraction`: Handles button click, account selection dropdown, and post-removal active account dropdown.
  - `RemoveHoyolabAccountSlashCommand`: Handles direct slash command execution.
  - `HoyolabAccountModule`: Registers the new slash command and component interaction handlers.
- **Infrastructure**: No new external infrastructure, databases, or Redis resources required. Uses existing PostgreSQL database and Aspire orchestration.
