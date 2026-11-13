# Design

## Context

See [proposal.md](proposal.md) for motivation and [specs/card-daily-thread-report/spec.md](specs/card-daily-thread-report/spec.md) for requirements.

Currently, `DailyReportDeliveryService.BuildDailyReportEmbed` constructs two inline fields for character performance: `Most Used Characters` (up to 5 entries) and `Highest Win Rate Characters` (up to 5 entries). Actions are rendered under `Most Used Actions` with a cap of 5 entries via `Math.Min(5, result.TopUseActions.Count)`.

The underlying `ICardStatsCalculator.Calculate` currently computes `TopUseCharacters` by taking only `topUseCount` (default 10) items.

## Goals / Non-Goals

**Goals:**
- Provide a single, complete view of all character cards used during the report period (`delta UseCount > 0`).
- Display each character's avatar emoji, name, total games, win rate percentage, and explicit win/loss record (`{games} games ({winRate:P1} - {wins}W/{losses}L)`).
- Order used characters by `UseCount` descending, then by `WinRate` descending.
- Ensure embed field length safety under Discord's 1024-character per-field value limit via continuation fields (`Characters Used (Cont.)`).
- Display up to 10 action cards in the `Top Actions` field.
- Maintain full backwards compatibility for existing slash commands (`/card`) using `ICardStatsCalculator`.

**Non-Goals:**
- Modifying the `/card` slash command embed layout (that command retains its existing all-time/version embed format).
- Modifying how snapshot deltas are captured or calculated in `DailySnapshotProcessor`.
- Changing action card emoji rendering (action cards remain text-only names and counts).

## Decisions

### Decision 1: Add `UsedCharacters` to `PeriodicStatsResult`
- **Choice**: Add `IReadOnlyList<CardDeltaStat> UsedCharacters` to the `PeriodicStatsResult` record.
- **Rationale**: Keeps existing properties (`TopUseCharacters`, `BestWinRateCharacters`, `LowestWinRateCharacters`) untouched so `CardSlashCommand` and any existing consumer remain unaffected.
- **Details**: In `CardStatsCalculator.Calculate`, collect all characters where `deltaUse > 0`, sorted by `UseCount` descending, then `WinRate` descending. Assign this list to `UsedCharacters`.

### Decision 2: Embed field layout & Chunking for `Characters Used`
- **Choice**: Render character cards in full-width fields (`WithInline(false)`). Format each line as:
  `${index}. {emojiPrefix}**{card.Name}** - {card.UseCount} games ({card.WinRate:P1} - {card.Proficiency}W/{card.UseCount - card.Proficiency}L)`.
  If the character text exceeds Discord's 1024-character limit (e.g. if >15 characters were played), split across continuation fields titled `Characters Used (Cont.)`.
- **Alternatives considered**:
  - *Keep inline 2-column format*: Insufficient width for `Name - X games (Y% - ZW/WL)`, causing ugly line wrapping and truncation. Full-width is much cleaner.
  - *Hard truncation at 10 or 15*: Fails the requirement of showing all characters used in the day. Chunking guarantees all characters are visible without hitting Discord REST errors.

### Decision 3: Update `Top Actions` to Top 10
- **Choice**: Update `DailyReportDeliveryService.BuildDailyReportEmbed` to render up to 10 action cards from `result.TopUseActions` under the field title `Top Actions`.
- **Details**: `DailyReportDeliveryService.DeliverReportsAsync` already calls `calculator.Calculate(..., topUseCount: 10)`. The existing code had `Math.Min(5, result.TopUseActions.Count)`. We update this to `Math.Min(10, result.TopUseActions.Count)` and rename the field to `Top Actions`.

## Risks / Trade-offs

- **[Risk] High number of characters in a single day exceeds embed field length (1024 chars)**
  → *Mitigation*: Chunk the character list across multiple embed fields (`Characters Used`, `Characters Used (Cont.)`), ensuring each field never exceeds 1024 characters.
- **[Risk] File locks during local build when MainApp is running in Aspire**
  → *Mitigation*: Stop the `main` resource in Aspire (`aspire resource main stop`) before compiling/running tests.
