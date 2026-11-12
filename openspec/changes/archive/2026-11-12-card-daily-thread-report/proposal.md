# Proposal

## Why

Users who track their Genius Invokation TCG progression in Discord often maintain personal journal threads in dedicated guild channels (such as forum channels or text channels). Manually invoking `/card` each day to check progression is cumbersome, and guild admins need control over which parent channels permit automated daily postings. Providing automated daily report delivery to user-owned threads keeps members engaged with their daily card progress without cluttering general chat or spamming unapproved channels.

## What Changes

- Add server administrator commands to manage allowed parent channels for daily thread reports:
  - `/card-snapshot channel add [channel]`: Registers a channel as an approved parent for thread reporting.
  - `/card-snapshot channel remove [channel]`: Unregisters an approved parent channel.
  - `/card-snapshot channel list`: Lists registered parent channels in the current guild.
- Add user commands inside threads to manage daily report subscriptions:
  - `/card-snapshot thread subscribe [account]`: Subscribes a Hoyolab account (defaults to active account) to daily reports in the current thread. Requires the executing user to be the thread owner and the thread's parent channel to be registered.
  - `/card-snapshot thread unsubscribe [account]`: Unsubscribes an account from daily reports in the current thread.
  - `/card-snapshot thread list`: Lists all Hoyolab accounts currently subscribed to receive daily reports in the current thread.
- Enforce validation:
  - Subscribing requires that the target Hoyolab account has `IsDailySnapshotEnabled == true`. If disabled, the command rejects the subscription and instructs the user to enable daily snapshots first.
- Automated daily report delivery:
  - Upon capturing a daily snapshot (`daily-yyyyMMdd`) after server reset (04:00 AM server time), the system computes the day's delta statistics against the previous snapshot.
  - The system posts a formatted Discord embed to all active subscribed threads for that account.
  - If no matches were played during that server day (0 delta games), the system still delivers a report indicating zero matches were recorded.
- Language standard:
  - All command descriptions, parameter descriptions, ephemeral confirmations, error notices, and report embeds are rendered completely in English.

## Capabilities

### New Capabilities
- `card-daily-thread-report`: Covers guild parent channel registration for daily reporting, thread ownership and account subscription management, and automated daily delta report delivery into Discord threads.

### Modified Capabilities
None.

## Impact

- **Database**: New entities `DiscordDailyReportChannel` and `DiscordDailyReportThread` with EF Core migration.
- **Discord Commands**: New subcommands under `/card-snapshot` (`channel add/remove/list` requiring `ManageChannels`, `thread subscribe/unsubscribe/list` restricted to thread creators).
- **Background Worker**: Integration in `DailySnapshotWorker` (or accompanying delivery service) to format and send daily embeds to subscribed threads via NetCord GatewayClient.
