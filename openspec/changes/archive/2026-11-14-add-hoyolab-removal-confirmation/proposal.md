# Proposal

## Why

Currently, when a user selects a Hoyolab account to remove from the interactive dropdown menu in `/hoyolab-accounts` or runs `/remove-hoyolab-account`, the account is removed immediately without an explicit confirmation step. Users risk accidentally deleting the wrong linked Hoyolab account through a single misclick or incorrect autocomplete selection, resulting in unlinking credentials and terminating active daily report subscriptions unexpectedly.

Introducing a mandatory confirmation step with explicit "Confirm Remove" (Danger) and "Cancel" (Secondary) buttons protects users from accidental deletions and improves overall system safety.

## What Changes

- Add a confirmation prompt when a user selects an account to remove from the interactive dropdown in `/hoyolab-accounts`.
- Add a confirmation prompt when a user executes the `/remove-hoyolab-account` slash command.
- Display account details (UID, Region) in the confirmation prompt warning that unlinking will stop automated daily snapshot captures and report deliveries while preserving historical card statistics.
- Provide interactive buttons:
  - **Confirm Remove**: Danger button (`ButtonStyle.Danger`) that executes the removal and returns the removal summary along with fallback active account selection if needed.
  - **Cancel**: Secondary button (`ButtonStyle.Secondary`) that aborts the removal and updates the message to state that the operation was cancelled.
- **SemVer Impact**: MINOR (user-facing UX enhancement adding confirmation safeguards to existing command workflows without breaking backend API contracts).

### In-Scope
- Updating `RemoveHoyolabAccountComponentInteraction` to show a confirmation prompt upon select menu choice.
- Adding button interaction handlers for confirming and cancelling Hoyolab account removal.
- Updating `RemoveHoyolabAccountSlashCommand` to validate account presence and present the same confirmation prompt.
- Updating `HoyolabAccountModule` to register the new component interaction handlers.
- Adding unit and integration tests covering the confirmation and cancellation workflows.

### Out-of-Scope
- Modifying underlying database entities or repository methods (`HoyolabAccountService.RemoveHoyolabAccountAsync` remains untouched).
- Adding or modifying other account management interactions (e.g., adding accounts, switching active accounts).
- Modifying Aspire AppHost or infrastructure configurations.

## Capabilities

### Modified Capabilities
- `hoyolab-account-management`: Update requirements for interactive menu and slash command removal to require two-step confirmation (prompt followed by explicit Confirm/Cancel action) before unlinking accounts.

## Impact

- **Affected Code**:
  - `src/GitcgNetCord.MainApp/Commands/Interactions/RemoveHoyolabAccountComponentInteraction.cs`
  - `src/GitcgNetCord.MainApp/Commands/Slash/RemoveHoyolabAccountSlashCommand.cs`
  - `src/GitcgNetCord.MainApp/Modules/Feats/HoyolabAccountModule.cs`
  - `tests/GitcgNetCord.MainApp.Tests/`
- **Dependencies & Infrastructure**:
  - No new external packages, database schemas, caches, or Aspire AppHost resources required.
