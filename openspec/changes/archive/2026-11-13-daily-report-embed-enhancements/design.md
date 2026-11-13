# Design

## Context

See `proposal.md` for background and motivation. `DailyReportDeliveryService` produces a Discord embed reporting TCG performance delta between the previous snapshot and current snapshot. Currently, the embed renders character lists using plain text formatting and displays a static `Date` field reflecting the date passed at invocation.

The bot application maintains character role icons on Discord as Application Emojis keyed by the card's numerical `ItemId` (created and updated via `/update-emojis`).

## Goals / Non-Goals

**Goals:**
- Include character role avatar icons (`<:itemId:emojiId>`) before character names in the "Most Used Characters" and "Highest Win Rate Characters" lists.
- Fall back cleanly to bold text if an emoji for a card ID is not found.
- Cache Discord application emojis in `HybridCache` with a 1-hour expiration.
- Dynamically format the `Period` field based on the game dates derived from `previousSnapshot` and `currentSnapshot`:
  - 1 game day: `YYYY-MM-DD`
  - Multiple game days: `YYYY-MM-DD ~ YYYY-MM-DD (N days)`
  - Same day (in-progress): `YYYY-MM-DD (Today)`
- Maintain full backward compatibility for existing unit tests.

**Non-Goals:**
- Adding emojis for action cards (only role cards are synced to Discord application emojis).
- Adding extra warning lines or disclaimers for 1-day reports.

## Decisions

### Decision 1: Discord Application Emoji Resolution and HybridCache
- **Choice:** Retrieve application emojis via `client.Rest.GetApplicationEmojisAsync(client.Id)` and store as a dictionary `Dictionary<string, string>` (mapping `cardId` to `<:name:id>`) in `HybridCache` with 1 hour TTL.
- **Rationale:** Application emojis rarely change, so caching avoids redundant HTTP roundtrips to Discord REST API on each daily report dispatch while remaining cluster-friendly with Redis.
- **Alternatives Considered:**
  - Calling Discord API per report: Increases latency and risks rate limits.
  - In-memory static dictionary: Works, but misses out on cache invalidation and distributed consistency supported by `HybridCache`.

### Decision 2: Period Calculation Logic
- **Choice:** Compute game dates using the server's UTC offset and 4:00 AM reset threshold:
  ```csharp
  var offset = DailySnapshotProcessor.GetServerUtcOffset(account.Region);
  var currDate = (currentSnapshot.CreatedAt.ToOffset(offset) - TimeSpan.FromHours(4)).Date;
  var prevDate = previousSnapshot != null
      ? (previousSnapshot.CreatedAt.ToOffset(offset) - TimeSpan.FromHours(4)).Date
      : currDate;
  var dayDiff = (currDate - prevDate).Days;
  ```
- **Formatting Rule:**
  - `dayDiff == 1`: `{prevDate:yyyy-MM-dd}` (the exact game day that completed).
  - `dayDiff > 1`: `{prevDate:yyyy-MM-dd} ~ {currDate:yyyy-MM-dd} ({dayDiff} days)`.
  - `dayDiff <= 0`: `{currDate:yyyy-MM-dd} (Today)`.
  - `previousSnapshot == null`: `{currDate:yyyy-MM-dd}`.
- **Rationale:** Hoyolab stats are cumulative. The delta between the morning reset snapshot of day D and day D+1 corresponds to day D. When multiple days elapse between snapshots, explicitly showing `{prevDate} ~ {currDate} ({N} days)` prevents misleading users without cluttering the UI with warning boxes.

### Decision 3: Embed Signature and Test Compatibility
- **Choice:** Update `BuildDailyReportEmbed` to take an optional `IReadOnlyDictionary<string, string>? emojis = null`.
- **Rationale:** Existing unit tests calling `BuildDailyReportEmbed` will continue to pass without modification, while tests can pass test dictionaries to verify emoji rendering.

## Risks / Trade-offs

- **[Risk] Missing or un-synced emojis**
  → **Mitigation**: Lookup uses `emojis?.TryGetValue(card.Id.ToString(), out var emoji)`. If absent, empty prefix is used, yielding plain bold text without error.
