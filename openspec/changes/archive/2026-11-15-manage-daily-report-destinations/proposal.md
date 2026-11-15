# Proposal

## Why

Users who subscribe to daily Genius Invokation TCG statistics reports currently face a disconnected and asymmetric experience: Direct Message (DM) delivery can only be enabled during the initial post-account-link button prompt, with no command-based mechanism to subscribe, unsubscribe, or inspect DM report settings. Furthermore, existing subscription management commands are strictly scoped to Discord Server Threads (`/card-snapshot thread subscribe`, `/card-snapshot thread unsubscribe`, `/card-snapshot thread list`) and fail with an error when run inside Direct Messages or standard channels.

By replacing the thread-specific subcommand group with a context-aware `/card-snapshot report` group, users gain full control over both Direct Message and Server Thread report delivery, including the ability to view an aggregate summary of all active report subscriptions from their DMs.

## What Changes

- **BREAKING**: Replace the `/card-snapshot thread` slash command group with `/card-snapshot report` (`subscribe`, `unsubscribe`, `list`).
- **Context-Aware Subscriptions**:
  - When executed in a **Direct Message (DM)**:
    - `/card-snapshot report subscribe [account]`: Subscribes the user's active or specified Hoyolab account to daily report delivery in DMs (requiring daily snapshot capture to be enabled).
    - `/card-snapshot report unsubscribe [account]`: Unsubscribes the specified account from DM report delivery.
    - `/card-snapshot report list`: Lists all active report destinations (both DMs and server threads) registered for the executing Discord user.
  - When executed inside a **Server Thread**:
    - `/card-snapshot report subscribe [account]`: Subscribes the account to report delivery in the current thread (retaining existing parent-channel approval and ownership checks).
    - `/card-snapshot report unsubscribe [account]`: Unsubscribes the account from the current thread.
    - `/card-snapshot report list`: Lists accounts subscribed in the current thread and renders community report server suggestions.
  - When executed in a **Standard Server Channel (non-thread)**:
    - Provides descriptive guidance instructing the user to run the command inside a report thread or in Direct Messages.
- **Onboarding & Guidance Updates**:
  - Update interactive button prompts and skip messages to reference `/card-snapshot report subscribe` instead of `/card-snapshot thread subscribe`.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `card-daily-thread-report`: Generalizes thread-specific report subscription and unsubscription commands to context-aware `/card-snapshot report` commands supporting both Direct Message (DM) channels and Discord server threads, and enables users to inspect their aggregate personal report subscriptions from DMs.

## Impact

- **Affected Code**:
  - `CardSnapshotModule.cs`: Replaces `/card-snapshot thread` subcommand group with `/card-snapshot report`.
  - `CardSnapshotSlashCommand.cs`: Refactors `ThreadSubscribeAsync`, `ThreadUnsubscribeAsync`, and `ThreadListAsync` into context-aware `ReportSubscribeAsync`, `ReportUnsubscribeAsync`, and `ReportListAsync` handling both `GuildThread` and `DMChannel`/Direct Messages.
  - `DailyReportDestinationComponentInteraction.cs`: Updates destination skip notice text to reference `/card-snapshot report subscribe`.
- **APIs & Dependencies**:
  - No new NuGet dependencies or external APIs.
  - Utilizes existing `DiscordDailyReportService.GetSubscriptionsForUserAsync`, `SubscribeDmAsync`, `FindDmSubscriptionAsync`, and `UnsubscribeThreadAsync`.
- **Infrastructure Requirements**:
  - No new database tables, cache schemas, or Aspire AppHost resources required.
