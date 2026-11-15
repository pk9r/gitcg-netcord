# Proposal

## Why

When users run `/card-snapshot thread list` to inspect daily report subscriptions, they may find that the current thread has no active subscriptions, or they may accidentally invoke the command outside of a Discord thread. Currently, the bot outputs a minimal message ("No Hoyolab accounts are currently subscribed to daily reports in this thread") or rejects the command with a rigid error ("This command can only be used inside a Thread") without providing discoverability or actionable paths to join communities that support daily Genius Invokation TCG statistics reports.

Displaying server suggestions (servers with public invites and approved daily report channels) in `/card-snapshot thread list` significantly enhances community engagement, improves user onboarding, and helps players discover active TCG Discord servers where they can track their stats.

## What Changes

- **Server Suggestions in Thread Subscriptions**: In `/card-snapshot thread list`, when active subscriptions exist in the current thread, append a "Suggested Report Servers" section in the response embed and include interactive Link Buttons for public community servers.
- **Guidance & Server Suggestions in Empty Threads**: When `/card-snapshot thread list` is run in a thread with no active subscriptions, display an informative embed detailing that no accounts are subscribed in this thread alongside the list of suggested community servers and invite link buttons.
- **Graceful Non-Thread Handling**: When `/card-snapshot thread list` is invoked outside of a thread (e.g., in a standard guild text channel or DM), instead of abruptly rejecting with an error, return an explanatory embed informing the user how thread reporting works, accompanied by the suggested community servers and link buttons.
- **Server Suggestion Filtering**: Identify suggested servers by checking `DiscordGuildConfig` for configured public invite URLs (`InviteUrl`) and ensuring each candidate server has at least one approved parent report channel (`DiscordDailyReportChannel`). If executed within a guild, prioritize other community guilds while filtering out the current guild if it is already the host.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `card-daily-thread-report`: Modifies the thread listing requirement to display community server suggestions (servers with public invite URLs and approved report channels) alongside thread subscription data, when no subscriptions exist, and as informative guidance when invoked outside a thread.

## Impact

- **Affected Code**:
  - `CardSnapshotSlashCommand.cs`: Enhance `ThreadListAsync` to fetch server suggestions and construct embeds with suggested community servers and Link Buttons.
  - `DiscordGuildConfigService.cs` / `DiscordDailyReportService.cs`: Provide or reuse query methods to retrieve guilds that have both a public invite URL and registered report channels.
- **User Interface Language**: All embeds, fields, notices, and button labels are strictly English according to project rules.
- **Infrastructure Requirements**:
  - Databases: No new tables or migrations required (uses existing `DiscordGuildConfigs` and `DiscordDailyReportChannels`).
  - Caches: No new cache dependencies.
  - Aspire AppHost: No AppHost resource changes required.
