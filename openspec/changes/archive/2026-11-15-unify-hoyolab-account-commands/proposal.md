# Proposal: Unify HoYoLAB Account Commands

## Why

Currently, HoYoLAB account management is fragmented across disparate top-level slash commands (`/hoyolab-accounts`, `/add-hoyolab-account`, `/remove-hoyolab-account`), with naming inconsistencies between singular and plural forms, and lacking a dedicated slash command to switch active accounts directly without navigating modal dialogues. Consolidating all account management actions into a single, cohesive `/hoyolab` command group (`list`, `add`, `remove`, `switch`) streamlines the user experience, cleans up the Discord root command space, and allows fast switching between linked game accounts using Discord autocomplete.

## What Changes

- **BREAKING**: Replace top-level slash commands `/hoyolab-accounts`, `/add-hoyolab-account`, and `/remove-hoyolab-account` with a single unified `/hoyolab` slash command group available in Guild, Bot DM, and DM contexts.
- **NEW**: Add `/hoyolab switch [account]` slash command allowing users to quickly change their active HoYoLAB account using UID autocomplete (`HoyolabAccountAutocompleteProvider`).
- **NEW**: Add `/hoyolab add` slash command that opens the secure "Add account" modal directly from chat without exposing authentication tokens in command parameters.
- **MODIFIED**: Move `/hoyolab-accounts` list overview functionality to `/hoyolab list`, retaining interactive components (`[Add account]`, `[Update active account]`, `[Remove account]`).
- **MODIFIED**: Move `/remove-hoyolab-account` functionality to `/hoyolab remove [account]`, retaining UID autocomplete and the interactive confirmation prompt.
- Update bot notices and error messages across features (such as `/tcg`, `/card`, and `/card-snapshot`) that referenced `/hoyolab-accounts` to reference the new `/hoyolab` commands.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `hoyolab-account-management`: Updates command routing from legacy flat commands (`/hoyolab-accounts`, `/remove-hoyolab-account`) to the unified `/hoyolab` command group (`/hoyolab list`, `/hoyolab add`, `/hoyolab remove`), and adds the `/hoyolab switch [account]` active account switching requirement.

## Impact

- **Bot Commands / Slash Routing**:
  - `HoyolabAccountModule.cs` registers `/hoyolab` slash command group with subcommands `list`, `add`, `remove`, and `switch`.
  - Removes top-level commands `/hoyolab-accounts`, `/add-hoyolab-account`, and `/remove-hoyolab-account`.
- **User Experience**:
  - Direct Modal invocation on `/hoyolab add` for token security.
  - Fast active account switching via autocomplete on `/hoyolab switch`.
- **References in Other Modules**:
  - Updates command hint strings in `CardSlashCommand.cs`, `TcgSlashCommand.cs`, and `CardSnapshotSlashCommand.cs`.
- **Infrastructure**: No database migrations, cache additions, or Aspire AppHost changes required.
