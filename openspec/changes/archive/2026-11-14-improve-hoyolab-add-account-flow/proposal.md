# Proposal: Improve Hoyolab Add Account Flow

## Why

Currently, when users add a Hoyolab account via the "Add account" button on `/hoyolab-accounts` or via the `/add-hoyolab-account` slash command, the system always forces them through a Discord select dropdown menu to pick a Genshin Impact character, even when their Hoyolab account contains exactly one character (~80-90% of user accounts). Furthermore, if a Hoyolab account has no linked Genshin Impact characters, constructing an empty Discord select menu fails with an API validation error (select menus require between 1 and 25 items), hanging the interaction. When users have multiple characters, the dropdown provides no visual indication of which characters are already linked versus newly detected.

Streamlining this flow by auto-linking single-character accounts, handling zero-character accounts with a clear message, and annotating already-linked characters in the multi-character dropdown significantly improves onboarding efficiency and prevents interaction crashes.

## What Changes

- **Automatic Linking for Single-Character Accounts**: If a verified Hoyolab account has exactly one Genshin Impact character (`GameId == 2`), the system immediately links that character, skips the intermediate select menu and caching step, and responds directly with the success embed and the `[ Enable Daily Snapshot ]` / `[ No, Maybe Later ]` onboarding prompt.
- **Graceful Zero-Character Handling**: If a verified Hoyolab account contains zero Genshin Impact characters, the system aborts menu generation and responds with a clear, user-friendly English notification explaining that no Genshin Impact records were found.
- **Annotated Multi-Character Dropdown Selection**: If a Hoyolab account has two or more Genshin Impact characters, the system compares the fetched game records against the user's existing linked accounts. Options for previously linked accounts are explicitly annotated with `[Already Linked - Select to update token]`, allowing users to refresh expired tokens while distinguishing new characters.
- **Shared Code Path Enhancement**: Both the modal submission flow from `/hoyolab-accounts` and the direct `/add-hoyolab-account` command benefit from this streamlined behavior via `AddHoyolabAccountSlashCommand.ExecuteCoreAsync`.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `hoyolab-account-management`: Update `Requirement: Add Hoyolab Account via Interactive Modal` and associated scenarios to specify auto-linking behavior when exactly one character exists, graceful error messaging when zero characters exist, and annotated dropdown options when multiple characters exist.

## In-Scope and Out-of-Scope

### In-Scope
- Modifying `AddHoyolabAccountSlashCommand.ExecuteCoreAsync` to branch based on Genshin Impact character count (0, 1, >1).
- Direct linking and response generation in `ExecuteCoreAsync` when `count == 1`.
- Annotating already-linked UIDs in `SelectGameRecordComponentInteraction` select menu options when `count > 1`.
- Adding unit tests verifying zero, single, and multi-character flows and option descriptions.
- Updating delta spec and planning artifacts under `openspec/changes/improve-hoyolab-add-account-flow`.

### Out-of-Scope
- Changing modal text inputs (keeping `ltuid_v2` and `ltoken_v2` inputs as-is).
- Changes to `/hoyolab-accounts` initial menu components or `/remove-hoyolab-account`.
- Changes to Hoyolab API client authentication protocols or cookie serialization.
- Custom mapping of Hoyolab retcodes to localized error messages.

## Impact

- **SemVer Impact**: MINOR (backward-compatible feature enhancement with improved user experience and error handling). No breaking changes.
- **Affected Code**:
  - `src/GitcgNetCord.MainApp/Commands/Slash/AddHoyolabAccountSlashCommand.cs`
  - `src/GitcgNetCord.MainApp/Commands/Interactions/SelectGameRecordComponentInteraction.cs`
  - `tests/GitcgNetCord.MainApp.Tests`
- **Dependencies & Infrastructure**:
  - No new database tables or EF Core migrations required.
  - No new caches, message queues, or Aspire AppHost resource changes required.
