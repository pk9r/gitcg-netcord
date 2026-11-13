# Proposal

## Why

The current Daily TCG Statistics Report embed separates character stats into two distinct 5-entry fields ("Most Used Characters" and "Highest Win Rate Characters"), while restricting action cards to 5 entries. This splits character performance data across two partial lists, obscures characters that were played outside the top 5, and doesn't show win rate and game count together in context. Combining all played characters into a single unified list with both game count and win rate, along with expanding action cards to the top 10, provides a comprehensive, clearer overview of the day's matches.

## What Changes

- Consolidate character statistics in the daily report embed into a single full-width field (`Characters Used`) showing **all characters used during the reported period** (`delta UseCount > 0`).
- Format each character entry to include their Discord application emoji avatar icon, character name, games played, win rate percentage, and record details: `${i + 1}. {emojiPrefix}**{card.Name}** - {card.UseCount} games ({card.WinRate:P1} - {card.Proficiency}W/{card.UseCount - card.Proficiency}L)`.
- Sort all used characters by `UseCount` descending, breaking ties with `WinRate` descending.
- Safeguard against Discord's 1024-character field limit by splitting the character list into continuation fields (`Characters Used (Cont.)`) if many unique characters were played in a single period.
- Increase the displayed action cards ranking from Top 5 to **Top 10** under the field `Top Actions`.
- Update `CardStatsCalculator` and `PeriodicStatsResult` to ensure all characters with `UseCount > 0` are accessible for daily reporting without being prematurely capped by `topUseCount`.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `card-daily-thread-report`: Update automated daily report delivery to display a unified list of all used characters with game count and win rate details, and display the top 10 action cards.

## Impact

- `GitcgNetCord.MainApp.Models.PeriodicStatsResult`: Add or ensure access to all characters with `UseCount > 0` (e.g. `IReadOnlyList<CardDeltaStat> UsedCharacters`).
- `GitcgNetCord.MainApp.Infrastructure.CardStatsCalculator`: Populate `UsedCharacters` containing all characters with `UseCount > 0` ordered by `UseCount` desc and `WinRate` desc, while preserving backwards compatibility for `TopUseCharacters`, `BestWinRateCharacters`, etc.
- `GitcgNetCord.MainApp.Infrastructure.DailyReportDeliveryService`: Update `BuildDailyReportEmbed` to render the unified `Characters Used` section and top 10 `Top Actions`.
- `tests/GitcgNetCord.MainApp.Tests/DailyReportDeliveryTests.cs`: Update unit test assertions for the new embed structure and field contents.
