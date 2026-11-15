# Design: Unify Server Admin Commands

## Context

Discord server administration commands are currently registered across three disparate modules (`ServerModule`, `CardCodeModule`, and `CardSnapshotModule`). Discord's Application Command API enforces strict nesting constraints: a top-level command that contains a `SUB_COMMAND_GROUP` cannot mix `SUB_COMMAND` and `SUB_COMMAND_GROUP` at the root level. All underlying entity services (`DiscordGuildConfigService`, `DiscordCardCodeChannelService`, `DiscordDailyReportService`) are already registered in DI and operate on the current database schema without requiring entity or migration updates.

## Goals / Non-Goals

**Goals:**
- Centralize all server administrative commands under `/server`.
- Provide an aggregated overview command `/server overview show` displaying invite URL, cardcode channels, and report parent channels.
- Enforce `Permissions.ManageGuild` and `Permissions.Administrator` consistently at both Discord registration level and runtime handler checks.
- Cleanly eliminate `/cardcode-channel` and `/card-snapshot channel` commands.

**Non-Goals:**
- Interactive Discord components (Buttons, Select Menus) for configuration in this phase.
- Modifying EF Core models, database schema, or running database migrations.
- Altering user-facing `/card-snapshot` subcommands (`save`, `list`, `daily`, `delete`, `thread`).

## Decisions

### 1. Unified Subcommand Group Hierarchy under `/server`
- `/server` is registered as a slash command group with `.WithDefaultGuildPermissions(Permissions.ManageGuild)` and `.AddContexts([InteractionContextType.Guild])`.
- It defines four subcommand groups:
  1. `overview`: contains `show` (handler: `ServerSlashCommand.OverviewShowAsync`).
  2. `invite`: contains `set`, `view`, `remove` (handlers in `ServerSlashCommand`).
  3. `cardcode`: contains `add`, `remove`, `list` (handlers in `CardCodeChannelSlashCommand`).
  4. `report-channel`: contains `add`, `remove`, `list` (handlers in `CardSnapshotSlashCommand`).
- *Rationale*: Satisfies Discord API nesting constraints while providing a consistent, discoverable namespace.
- *Alternatives considered*: Flat commands (`/server-overview`, `/server-cardcode`). Rejected to maintain a clean command namespace.

### 2. Handler Reuse and Permission Alignment
- Existing command handlers (`CardCodeChannelSlashCommand` and `CardSnapshotSlashCommand`) will be updated to check `Permissions.ManageGuild` instead of `Permissions.ManageChannels`.
- Handlers are wired into `ServerModule.cs`, centralizing guild administrative route definitions in one place.
- *Rationale*: Minimizes code churn, reuses validated logic, and ensures consistent error messaging across commands.

### 3. Aggregated Overview Command Implementation
- `ServerSlashCommand.OverviewShowAsync` will inject:
  - `DiscordGuildConfigService`
  - `DiscordCardCodeChannelService`
  - `DiscordDailyReportService`
- In a single ephemeral response, it retrieves:
  - Guild invite configuration via `configService.GetConfigAsync(guildId)`.
  - Registered card code channels via `channelService.GetByGuildAsync(guildId)`.
  - Registered daily report channels via `reportService.GetReportChannelsByGuildAsync(guildId)`.
- It formats the results into an English embed with sections for Invite URL, Card Code Channels (with decode style), Daily Report Channels, and quick admin command hints.
- *Rationale*: Provides administrators with complete visibility into guild-level bot behavior in one glance.

## Risks / Trade-offs

- **[Risk] Breaking Change for Existing Server Admins** → *Mitigation*: The overview command (`/server overview show`) includes quick admin command references so administrators immediately discover the new `/server` syntax.
- **[Risk] Multiple Service Injections in Overview Handler** → *Mitigation*: All three services are lightweight scoped repositories backed by DbContext and execute straightforward index-backed queries by `guildId`.
