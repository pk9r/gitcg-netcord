# Design

## Context

The application currently has a `DailySnapshotWorker` that runs on a 15-minute polling interval, identifying active Hoyolab accounts configured with `IsDailySnapshotEnabled == true` whose local game server time has passed 04:00 AM. It saves snapshots named `daily-yyyyMMdd` with `SnapshotType.Daily`.

The application also has an `ICardStatsCalculator` for calculating win counts, use counts, and card stats deltas between snapshots, and uses NetCord's `GatewayClient` for Discord gateway and REST interactions.

Guild admins and members now need a way to connect Discord threads to this daily snapshot pipeline so that personal progression threads automatically receive a daily summary embed after reset.

## Goals / Non-Goals

**Goals:**
- Store and manage approved parent channels per Discord guild via database entity `DiscordDailyReportChannel`.
- Store and manage thread subscriptions per thread and Hoyolab account via entity `DiscordDailyReportThread`.
- Provide admin commands under `/card-snapshot channel` (`add`, `remove`, `list`) with `ManageChannels` permission requirements.
- Provide thread commands under `/card-snapshot thread` (`subscribe`, `unsubscribe`, `list`) enforcing thread ownership, parent channel registration, and active daily snapshot requirements.
- Automatically format and send daily delta embeds (including 0-match reports) to subscribed threads when daily snapshots are captured.

**Non-Goals:**
- Creating Discord threads automatically (threads are created by users).
- Delivering reports via direct messages (DMs).
- Real-time match notifications outside of the daily snapshot reset cycle.

## Decisions

### 1. Data Model & Entities

Add two entities to `AppDbContext`:

```csharp
public class DiscordDailyReportChannel
{
    public ulong Id { get; set; } // ChannelId (PK)
    public ulong GuildId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class DiscordDailyReportThread
{
    public int Id { get; set; }
    public ulong ThreadId { get; set; }
    public ulong GuildId { get; set; }
    public ulong DiscordUserId { get; set; }
    public int HoyolabAccountId { get; set; }
    public HoyolabAccount? HoyolabAccount { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
```

- Configure a unique index on `(ThreadId, HoyolabAccountId)` to prevent duplicate subscriptions for the same account in the same thread.
- Configure index on `HoyolabAccountId` and `GuildId` for fast lookup during snapshot delivery.

*Alternative considered:* Storing a JSON array of thread IDs on `HoyolabAccount`. Rejected because it prevents relational querying by guild/thread, complicates cascading deletes, and bypasses EF Core relational constraints.

### 2. Thread Validation & Permission Flow

When a user executes `/card-snapshot thread subscribe`:
1. Check context: `context.Channel is GuildThread thread`. If false, reply with an ephemeral error stating the command is only valid inside threads.
2. Check thread owner: `thread.OwnerId == context.User.Id`. If false, reply that only the thread creator can manage subscriptions in this thread.
3. Check parent channel: Query `DiscordDailyReportChannel` for `thread.ParentId`. If not found, reply that the thread's parent channel is not approved for daily reports.
4. Resolve Hoyolab account: If specified, verify it belongs to `context.User.Id`; otherwise resolve user's active Hoyolab account.
5. Check daily snapshot prerequisite: Verify `account.IsDailySnapshotEnabled == true`. If false, reject and prompt the user to enable daily snapshots first with `/card-snapshot daily enabled:True`.
6. Save or reactivate subscription in `DiscordDailyReportThread`.

### 3. Report Delivery Pipeline

Encapsulate delivery in a `DailyReportDeliveryService` invoked by `DailySnapshotWorker` immediately after successfully saving `daily-yyyyMMdd`:
1. Query active subscriptions: `_dbContext.DailyReportThreads.Where(t => t.HoyolabAccountId == account.Id && t.IsEnabled).ToListAsync()`.
2. Find preceding daily snapshot: Query the most recent snapshot for `account.Id` created prior to today's snapshot (typically yesterday's `daily-yyyyMMdd`).
3. Compute delta statistics via `ICardStatsCalculator`:
   - If previous snapshot exists: compute delta between previous and current card list.
   - If no previous snapshot exists: compute delta against 0 (treat current snapshot as baseline).
4. Build report embed:
   - Header: `Daily TCG Statistics Report - {serverDate:yyyy-MM-dd}`.
   - Account info: Hoyolab UID / Game Role ID.
   - Matches played: If `deltaGames > 0`, display total games, wins, win rate percentage, top 3-5 used cards, and top win rate cards.
   - Zero matches: If `deltaGames == 0`, display message: `No matches were recorded during this period.`
5. Send message: Call `GatewayClient.Rest.CreateMessageAsync(subscription.ThreadId, messageProperties)`.

### 4. Language and Localization Standard

All slash command names, group descriptions, parameter descriptions, ephemeral confirmations, error notices, embed titles, and field names are standardized in English. This maintains consistency with the bot's broader command structure prior to the introduction of a dynamic multi-language localization system.

## Risks / Trade-offs

- **[Risk] Thread deleted, archived, or bot lacks SendMessages permission**
  → *Mitigation*: Wrap each thread delivery in `try/catch`. On `RestException` with Discord error codes (e.g., `10003 Unknown Channel`, `50001 Missing Access`), log a warning and mark the subscription disabled or remove it to avoid future redundant calls.
- **[Risk] REST Rate Limits with multiple threads**
  → *Mitigation*: Introduce a 500ms throttle between thread message dispatches.
- **[Risk] Ambiguity with multiple Hoyolab accounts**
  → *Mitigation*: Provide an optional `account` parameter with an autocomplete provider showing the user's Hoyolab accounts (with indication of daily snapshot status).
