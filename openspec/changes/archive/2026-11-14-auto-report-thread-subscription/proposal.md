# Proposal

## Why

Currently, users who want to receive automated daily Genius Invokation TCG statistics reports must navigate a cumbersome, disconnected multi-step process: they must independently know to enable daily snapshots, manually locate an approved server channel, manually create a Discord thread, and run `/card-snapshot thread subscribe`. Furthermore, users cannot receive reports directly in their Direct Messages (DMs) without joining a server, and server administrators have no way to associate an invite link with their guild so the bot can introduce community members to their report channels.

This change automates the onboarding and report subscription workflow immediately after adding a Hoyolab account, introduces Direct Messages (DM) and auto-created Server Threads as flexible report destinations, supports multi-destination delivery, and provides a dedicated server configuration capability to manage guild invite URLs.

## What Changes

- **Post-Account Setup Prompt**: Automatically prompts the user with interactive buttons (`[ Enable Daily Snapshot ]` / `[ No, Maybe Later ]`) immediately after successfully adding a Hoyolab account.
- **Report Destination Picker**: Upon enabling daily snapshot, presents an interactive destination picker allowing the user to select where to receive reports:
  - Direct Message (DM) to receive reports privately without needing a server.
  - Server Thread in an approved report channel (the bot automatically creates the thread, welcomes/mentions the user, and registers the subscription).
  - Community Server suggestions with invite links (for servers that configured a public invite URL).
  - Skip (capture snapshot data only).
- **Multi-Destination Report Subscriptions**: Allows a single Hoyolab account to be subscribed to multiple destinations simultaneously (e.g., both DM and server threads).
- **Bot-Created Thread Management Permission**: Updates thread subscription management (`unsubscribe`, `list`) so the user who initiated the subscription can manage it even though the bot is the Discord thread owner (`thread.OwnerId == bot.Id`).
- **Server Configuration Module**: Introduces a dedicated `/server invite` slash command group (`set`, `remove`, `view`) and database entity (`DiscordGuildConfig`) allowing server administrators to manage their server invite link independently from the card snapshot feature.

## Capabilities

### New Capabilities
- `server-configuration`: Manages guild-level settings and metadata, specifically configuring, viewing, and clearing the server's public invite URL for community discovery.

### Modified Capabilities
- `card-daily-thread-report`: Automates thread creation and subscription, supports Direct Message (DM) delivery alongside server threads, enables multiple report destinations per Hoyolab account, and allows subscribers to manage bot-created report threads.
- `hoyolab-account-management`: Enhances the account registration flow to prompt the user to enable daily snapshots with interactive buttons upon successful account linkage.

## Scope Boundaries

### In-Scope
- Database entity `DiscordGuildConfig` and migration for guild configurations.
- Slash command group `/server invite` with `set`, `remove`, and `view` subcommands restricted to users with `ManageGuild` or `Administrator` permissions.
- Interactive component handlers for the daily snapshot prompt and destination selection (DM vs. Server Thread vs. Community Guild).
- Automated Discord Thread creation (`client.Rest.CreateGuildThreadAsync`) in approved parent channels with user mention for auto-join.
- DM channel report delivery and error handling (403 Forbidden auto-disable).
- Permission check adjustments in `/card-snapshot thread` commands to support bot-created threads.
- Unit and integration tests for new services, component interactions, and report delivery.

### Out-of-Scope
- Server setup wizards that automatically create channels or categories in Discord.
- User-customizable report schedules (delivery remains bound to daily server reset at 04:00 AM game time).
- Web or dashboard UI for managing subscriptions.

## SemVer Impact & Breaking Changes

- **SemVer Assessment**: `MINOR` (v1.x.0)
- **Breaking Changes**: None. Existing database records, manual `/card-snapshot thread subscribe`, and `/card-snapshot daily` commands remain fully backward-compatible.

## Infrastructure Requirements

- **Database**: New PostgreSQL table `discord_guild_configs` via EF Core migration.
- **Cache**: No new cache instances required; HybridCache continues to be used.
- **Aspire AppHost**: No AppHost resource changes required.
