# Design

## Context

Currently, `CardSnapshotModule` registers subcommands under `/card-snapshot thread` (`subscribe`, `unsubscribe`, `list`). Each subcommand immediately verifies `if (context.Channel is not GuildThread thread)` and rejects execution outside threads.

However, the bot already supports Direct Message (DM) reports at the storage and delivery layers:
- `DiscordDailyReportThread` models represent DMs with `GuildId = 0` and `ThreadId = dmChannel.Id`.
- `DiscordDailyReportService` already provides `SubscribeDmAsync`, `FindDmSubscriptionAsync`, `UnsubscribeThreadAsync`, and `GetSubscriptionsForUserAsync`.
- Currently, users can only enable DM reports through the interactive button prompt immediately after adding a Hoyolab account (`DailySnapshotPromptComponentInteraction`).

See `proposal.md` for motivation and `specs/card-daily-thread-report/spec.md` for requirement specifications.

## Goals / Non-Goals

**Goals:**
- Replace the `/card-snapshot thread` subcommand group with a context-aware `/card-snapshot report` group (`subscribe`, `unsubscribe`, `list`).
- Enable seamless subscription and unsubscription for Direct Messages (DM) directly from Discord slash commands.
- Provide a personal aggregate report overview in DM via `/card-snapshot report list` utilizing `GetSubscriptionsForUserAsync`.
- Maintain full parity for thread reporting in Discord servers with existing authorization and parent channel checks.
- Update all user-facing guidance referencing the legacy `/card-snapshot thread subscribe` command.

**Non-Goals:**
- Modifying EF Core database schemas or running EF migrations (existing schema fully supports DM subscriptions).
- Modifying `/server report-channel` administration commands or guild invite settings.
- Altering the daily snapshot generation or report delivery background workers (`DailySnapshotWorker`).

## Decisions

### Decision 1: Unified `/card-snapshot report` Command Group
We replace the `/card-snapshot thread` subcommand group with `/card-snapshot report`.

- **Rationale**: The word "report" naturally encompasses both DM delivery and thread delivery. Users think in terms of "subscribing to reports" rather than the technical channel container.
- **Alternatives considered**:
  - *Keep `/card-snapshot thread` and add `/card-snapshot dm`*: Causes command proliferation and redundant duplicate parameter schemas (`subscribe`, `unsubscribe`, `list` duplicated across both groups).
  - *`/card-snapshot destination`*: Verbose and technical; slower to type on Discord autocomplete.

### Decision 2: Context-Aware Channel Dispatching
Command handlers in `CardSnapshotSlashCommand.cs` (`ReportSubscribeAsync`, `ReportUnsubscribeAsync`, `ReportListAsync`) inspect `context.Channel` and `context.Interaction.GuildId` to dispatch behavior:

```
context.Channel
├── is GuildThread thread  ──> Thread report logic (Parent channel check, Thread ownership)
├── is DMChannel or        ──> DM report logic (GetDMChannelAsync, DM subscription lookup)
│   GuildId == null
└── is GuildChannel        ──> Non-thread guidance embed (Thread / DM instructions)
```

- **Rationale**: Eliminates cognitive load for users by allowing the exact same command syntax (`/card-snapshot report subscribe [account]`) to work intuitively wherever invoked.
- **Alternatives considered**: Separate parameters like `/card-snapshot report subscribe target:DM|Thread`, which adds friction and potential user error when specifying target while inside a thread or DM.

### Decision 3: Unified Personal Overview in DM List
When `/card-snapshot report list` is executed in a Direct Message channel:
- Queries `reportService.GetSubscriptionsForUserAsync(context.User.Id)`.
- Renders an English embed summarizing:
  - 📩 **Direct Messages**: Active accounts (UID, region) receiving DMs.
  - 🧵 **Server Threads**: Active accounts and their subscribed thread links `<#{ThreadId}>` across guilds.
- If no subscriptions exist, displays an informative English empty state with instructions to subscribe using `/card-snapshot report subscribe`.

- **Rationale**: Direct Messages serve as the user's personal private space with the bot. Showing all their report destinations in one place provides transparency and control.

### Decision 4: Update Guidance in Component Interactions
Update `DailyReportDestinationComponentInteraction.cs` skip messages:
- Replace: `You can configure a destination anytime using /card-snapshot thread subscribe.`
- With: `You can configure a destination anytime using /card-snapshot report subscribe.`

## Risks / Trade-offs

- **[Breaking Command Change] Removal of `/card-snapshot thread`**:
  - *Risk*: Users accustomed to typing `/card-snapshot thread` may initially encounter command not found.
  - *Mitigation*: Discord slash commands update globally upon bot registration; autocomplete will immediately suggest `/card-snapshot report`. Updated guidance strings instruct users on the new command.
- **[Direct Message Privacy Restriction (HTTP 403)]**:
  - *Risk*: A user executing `/card-snapshot report subscribe` in DM might have Discord privacy settings blocking bot DMs.
  - *Mitigation*: Wrap `GetDMChannelAsync` and DM interactions with catch blocks for `RestException` (HttpStatusCode.Forbidden) and return clear English instructions to enable server member direct messages.
