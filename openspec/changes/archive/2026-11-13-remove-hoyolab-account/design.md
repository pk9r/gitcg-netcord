# Design

## Context

Users can currently add Hoyolab accounts and select an active account for card stats and daily report snapshots. However, account removal was left incomplete in `HoyolabAccountsSlashCommand` with a commented-out button and no backing domain service or NetCord interaction handlers. 

Existing `CardSnapshot` entities hold historical statistics tied via foreign key to `HoyolabAccount`. Simply hard-deleting an account would either cascade-delete years of match history or cause foreign key violations. Furthermore, recurring daily snapshot workers and daily thread delivery services run on schedules, so removing an account must cleanly detach those background services.

See `proposal.md` for background motivation and `specs/hoyolab-account-management/spec.md` for behavioral requirements.

## Goals / Non-Goals

**Goals:**
- Provide both interactive UI (button + select dropdown) and direct slash command (`/remove-hoyolab-account`) removal pathways.
- Preserve all existing `CardSnapshot` records of the unlinked UID.
- Immediately terminate background daily snapshot captures and daily report deliveries for the unlinked account.
- Handle active account transitions smoothly with automated fallback and instant reassignment dropdown when $\ge 2$ accounts remain.
- Allow seamless reactivation if the user re-adds the same UID later.

**Non-Goals:**
- Modifying Hoyolab API authentication endpoints or changing token generation.
- Hard-deleting historical snapshots from PostgreSQL.
- Providing cross-server administrative account unlinking (this is strictly user self-service).

## Decisions

### Decision 1: Soft-Delete via `IsDeleted` on `HoyolabAccount`
- **Choice**: Add `bool IsDeleted { get; set; } = false;` to `HoyolabAccount` and configure a global query filter `HasQueryFilter(x => !x.IsDeleted)` in `AppDbContext`. When an account is removed, wipe `Token = string.Empty`, `HoyolabUserId = string.Empty`, and `IsDailySnapshotEnabled = false`.
- **Alternatives Considered**:
  - *Hard Delete with Cascade*: Would delete all `CardSnapshot` rows, destroying user history.
  - *Decoupling CardSnapshot schema*: Adding `GameRoleId` to `CardSnapshot` and making `HoyolabAccountId` nullable (`SetNull`). This requires modifying multiple existing services, tests, and queries, with high migration complexity and orphan record risks.
- **Rationale**: Soft-delete preserves relational integrity without schema refactoring. Erasing sensitive tokens guarantees privacy and security. If the user ever re-links the same account in `AddHoyolabAccountAsync`, `IgnoreQueryFilters()` enables effortless reactivation and reconnection to past snapshots.

### Decision 2: Dual Entry Removal UX (Interactive Select Menu & Slash Command)
- **Choice**:
  1. Primary UI: Enable the "Remove account" button in `/hoyolab-accounts`. Clicking it displays an ephemeral String Select Menu populated with the user's accounts (`UID`, `Region`, `Nickname`).
  2. Direct CLI: Top-level `/remove-hoyolab-account account:<UID>` slash command with autocomplete powered by `HoyolabAccountAutocompleteProvider`.
- **Alternatives Considered**:
  - *Modal Text Input*: Required users to manually type/paste a 9-10 digit UID. Rejected in favor of the dropdown for better UX, while keeping the slash command for power users.

### Decision 3: Automated Active Account Fallback with Interactive Reassignment
- **Choice**:
  When the active account is removed:
  - If 0 accounts remain: Remove `ActiveHoyolabAccount` entity.
  - If 1 account remains: Reassign `ActiveHoyolabAccount.HoyolabAccountId` to the remaining account.
  - If $\ge 2$ accounts remain: Reassign to the first remaining account and attach a String Select Menu (`select-fallback-active-account`) directly in the confirmation message so the user can switch to another preferred account with one click.
- **Rationale**: Eliminates the frustration of commands failing with "No active account found", while still giving multi-account users immediate control over their active profile.

```
+-----------------------------------------------------------------------------------------+
|                                    REMOVAL ARCHITECTURE                                 |
+-----------------------------------------------------------------------------------------+
|                                                                                         |
|  [User Actions]                                                                         |
|  - Click "Remove account" Button  ---> Display Account Dropdown                         |
|  - Or /remove-hoyolab-account UID                                                      |
|                                       |                                                 |
|                                       v                                                 |
|  [HoyolabAccountService.RemoveHoyolabAccountAsync]                                      |
|  - Wipe Token & HoyolabUserId                                                           |
|  - Set IsDeleted = true, IsDailySnapshotEnabled = false                                 |
|  - Delete associated DiscordDailyReportThread subscriptions                            |
|  - Evaluate ActiveHoyolabAccount fallback:                                              |
|      0 remaining -> Clear ActiveHoyolabAccount                                          |
|      1 remaining -> Auto-fallback to sole remaining                                     |
|      >= 2 remaining -> Auto-fallback to first + return remaining list                   |
|                                       |                                                 |
|                                       v                                                 |
|  [Discord Response]                                                                     |
|  - Ephemeral confirmation Embed in English                                              |
|  - If >= 2 remaining accounts: attach "Switch active account" String Select Menu        |
|                                                                                         |
+-----------------------------------------------------------------------------------------+
```

## Risks / Trade-offs

- **[Risk] User re-adds the same UID after soft-deleting**:
  - *Mitigation*: In `AddHoyolabAccountAsync`, query using `IgnoreQueryFilters()`. If a soft-deleted record exists for `(DiscordUserId, GameRoleId)`, reactivate it (`IsDeleted = false`), update its `Token`, `HoyolabUserId`, and `Region`, and re-link as active account if needed.
- **[Risk] Background workers attempting to process soft-deleted accounts**:
  - *Mitigation*: The global query filter `!x.IsDeleted` automatically excludes them from `ActiveHoyolabAccounts` includes and worker queries. Additionally, `Token` is wiped and `IsDailySnapshotEnabled` is explicitly set to `false`.
- **[Risk] Stale autocomplete choices in Discord client**:
  - *Mitigation*: `HoyolabAccountAutocompleteProvider` queries through `HoyolabAccountService.GetHoyolabAccountsAsync()`, which honors the `!x.IsDeleted` filter, ensuring removed accounts disappear immediately from autocomplete.
