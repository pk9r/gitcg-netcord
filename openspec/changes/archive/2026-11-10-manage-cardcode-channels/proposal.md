# Proposal

## Why

Currently, `CardCodeGatewayHandler` queries `DiscordCardCodeChannels` in PostgreSQL to determine whether an incoming Discord message or thread belongs to an authorized card-code channel before decoding deck codes. However, there is no in-Discord management command to configure these channels. Administrators or moderators must manually insert or modify channel records directly in the database, making bot deployment and server management inconvenient.

Providing an in-app slash command restricted to authorized server managers allows seamless registration and management of card code channels directly within Discord.

## What Changes

- Add a slash command group `/cardcode-channel` with subcommands:
  - `/cardcode-channel add [channel] [decode_type]`: Register a channel for card code processing (defaults to the current channel and `ImageGameBackground` decode type).
  - `/cardcode-channel remove [channel]`: Unregister a channel (defaults to the current channel).
  - `/cardcode-channel list`: List all card code channels configured for the current server.
- Restrict command execution to users with `ManageChannels` permission via Discord's native `DefaultGuildPermissions` and limit context to guilds.
- Add `GuildId` (`ulong`) to `DiscordCardCodeChannel` and create a database migration so channels can be associated with and listed per Discord server.
- Expand `DiscordCardCodeChannelService` to support adding, removing, and listing channels by guild ID.
- Update `CardCodeGatewayHandler` to honor the configured `SharingDecodeType` per channel when generating deck graphics.

## Capabilities

### New Capabilities

- `cardcode-channel-management`: Manages registered Discord channels where card codes and deck sharing links are recognized and automatically decoded.

### Modified Capabilities

<!-- None -->

## Impact

- **Database**: Adds `GuildId` column to `DiscordCardCodeChannels` via EF Core Migration.
- **Service Layer**: Adds CRUD methods to `DiscordCardCodeChannelService`.
- **Bot Commands**: Adds `/cardcode-channel` slash command in `CardCodeModule`.
- **Gateway Handlers**: `CardCodeGatewayHandler` uses channel-specific decode type.

