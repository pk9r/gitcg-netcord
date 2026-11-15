# Design: Unify HoYoLAB Account Commands

## Context

HoYoLAB account interactions in `GitcgNetCord.MainApp` are currently split across three independent top-level commands: `/hoyolab-accounts`, `/add-hoyolab-account`, and `/remove-hoyolab-account`. Furthermore, users who want to switch active accounts must open `/hoyolab-accounts`, click "Update active account", and type the 9-digit UID into a modal text input. Consolidating these actions into a `/hoyolab` slash command group with subcommands `list`, `add`, `remove`, and `switch` unifies the account experience and integrates UID autocomplete for both removal and active account switching.

## Goals / Non-Goals

**Goals:**
- Replace flat commands with `/hoyolab` slash command group supporting contexts: `Guild`, `BotDMChannel`, `DMChannel`.
- Provide four subcommands under `/hoyolab`:
  - `list`: Render account overview embed with interactive buttons (`Add account`, `Update active account`, `Remove account`).
  - `add`: Launch the secure "Add account" modal directly.
  - `remove`: Prompt account removal with UID autocomplete and interactive confirmation buttons.
  - `switch`: Directly set the active account by UID with autocomplete.
- Update other command modules (`TcgSlashCommand`, `CardSlashCommand`, `CardSnapshotSlashCommand`) that refer to `/hoyolab-accounts`.
- Preserve all existing component interactions (modals, dropdown menus, button callbacks).

**Non-Goals:**
- Modifying EF Core models, database tables, or repository contracts (`HoyolabAccountService`, `ActiveHoyolabAccountService`).
- Changing encryption or authentication token handling for HoYoLAB credentials.

## Decisions

### 1. Single-Level Subcommand Group Structure
- Register `/hoyolab` as a top-level `SlashCommandGroup` with `.AddContexts(InteractionContextType.Guild, InteractionContextType.BotDMChannel, InteractionContextType.DMChannel)`.
- Register subcommands directly on the group:
  - `list` -> `HoyolabAccountsSlashCommand.ListAsync`
  - `add` -> `HoyolabAccountsSlashCommand.AddAsync` (launches modal)
  - `remove` -> `RemoveHoyolabAccountSlashCommand.ExecuteAsync`
  - `switch` -> `HoyolabAccountsSlashCommand.SwitchAsync`
- *Rationale*: Avoids unnecessary nesting depth while keeping all account management operations discoverable under `/hoyolab`.

### 2. Fast Active Account Switching (`/hoyolab switch`)
- Parameter `account` uses `[SlashCommandParameter(AutocompleteProviderType = typeof(HoyolabAccountAutocompleteProvider))]`.
- Handler queries `HoyolabAccountService.GetHoyolabAccountsAsync(userId)`, locates the matching account by `GameRoleId == account.Trim()`, updates active status via `ActiveHoyolabAccountService.SetActiveHoyolabAccountAsync(...)`, and replies with an ephemeral confirmation notice in English.
- *Rationale*: Eliminates manual UID typing, providing instant switching with keyboard autocomplete.

### 3. Modal Launch for `/hoyolab add`
- Responds directly with `InteractionCallback.Modal(AddHoyolabAccountComponentInteraction.CreateModal())` without sending a deferred message first.
- The submitted modal uses existing `AddHoyolabAccountComponentInteraction.PostAsync` modal handler.
- *Rationale*: Guarantees credential privacy and reuses proven modal handling logic.

### 4. Coordinated Module References
- In `CardSlashCommand.cs` and `TcgSlashCommand.cs`, update resolution of the help command to reference `hoyolab` (e.g. `</hoyolab list:...>` or `</hoyolab add:...>`).
- In `CardSnapshotSlashCommand.cs`, update hardcoded string notices from `/hoyolab-accounts` to `/hoyolab list`.

## Risks / Trade-offs

- **[Risk] Modal response cannot be deferred**: Discord requires modal interactions to be the immediate initial response. → *Mitigation*: The `/hoyolab add` handler will send `InteractionCallback.Modal(...)` immediately without calling `DeferredMessage()`.
- **[Risk] Autocomplete in DM context**: In DMs, Discord autocomplete functions identically to guilds. → *Mitigation*: `HoyolabAccountAutocompleteProvider` relies only on `context.User.Id`, which is available in all contexts.
