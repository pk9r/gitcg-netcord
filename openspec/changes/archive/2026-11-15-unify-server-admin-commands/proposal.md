# Proposal: Unify Server Admin Commands

## Why

Currently, Discord server administration commands are fragmented across three distinct command prefixes (`/server invite`, `/cardcode-channel`, and `/card-snapshot channel`), with `/card-snapshot channel` inappropriately nested inside user-facing deck snapshot features. Furthermore, administrators lack a central command or dashboard to inspect the bot's configured channels and settings in their server, and permission requirements vary inconsistently between `ManageGuild` and `ManageChannels`. Consolidating all guild administration commands under a single `/server` hierarchy with a unified `ManageGuild` permission check and an overview dashboard will significantly improve discoverability, clarity, and administrative UX.

## What Changes

- **BREAKING**: Move `/cardcode-channel add|remove|list` commands to `/server cardcode add|remove|list`.
- **BREAKING**: Move `/card-snapshot channel add|remove|list` commands to `/server report-channel add|remove|list`.
- **BREAKING**: Remove legacy top-level `/cardcode-channel` command group and `/card-snapshot channel` subcommand group.
- **NEW**: Add `/server overview show` command displaying a unified English embed dashboard summarizing all guild configurations (Server Invite URL, active Card Code Channels, approved Daily Report parent channels, and quick admin command references).
- Standardize all server admin commands under `/server` to enforce `Permissions.ManageGuild` (and `Administrator`) both via Discord default command permissions and in-command guild user checks.
- Keep `/server invite set|view|remove` under `/server invite`.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `server-configuration`: Adds the `/server overview show` requirement to view a comprehensive guild configuration summary, while retaining `/server invite` management under `ManageGuild`.
- `cardcode-channel-management`: Updates command path from `/cardcode-channel` to `/server cardcode` and updates required permission from `ManageChannels` to `ManageGuild`.
- `card-daily-thread-report`: Updates command path from `/card-snapshot channel` to `/server report-channel` and updates required permission from `ManageChannels` to `ManageGuild`.

## Impact

- **Bot Commands / Slash Routing**:
  - `ServerModule.cs` is expanded to register the root `/server` command with `overview`, `invite`, `cardcode`, and `report-channel` subcommand groups.
  - `CardCodeModule.cs` no longer registers `cardCodeChannelGroup`.
  - `CardSnapshotModule.cs` no longer registers `channelGroup`.
- **Permissions**:
  - Unifies permission checks in `CardCodeChannelSlashCommand` and `CardSnapshotSlashCommand` to check `Permissions.ManageGuild` instead of `Permissions.ManageChannels`.
- **User Interface**:
  - New `/server overview show` slash command returning an English embed dashboard.
- **Tests**:
  - Unit tests verifying command permissions and registration will be updated to reflect the new command hierarchy and `ManageGuild` requirement.
- **Infrastructure**: No new databases, caches, or Aspire AppHost resources are required.
