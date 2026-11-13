# Proposal

## Why

Currently, the daily TCG thread report embed displays only plain text names for character roles, making it harder for users to quickly recognize characters at a glance. Additionally, the embed's `Date` field shows the capture date instead of the actual game day being reported (as the morning reset snapshot summarizes the previous day's gameplay), and does not indicate when statistics span multiple days due to missed daily snapshots. Adding character avatar icons and an accurate `Period` representation resolves these readability and clarity issues.

## What Changes

- Display character role avatar icons (Discord application custom emojis) before character names in the "Most Used Characters" and "Highest Win Rate Characters" embed sections.
- Gracefully fall back to plain text names if a character's emoji is not registered in the application.
- Cache Discord application emojis using `HybridCache` to avoid repeated REST API calls on report delivery.
- Replace the static `Date` embed field with a dynamic `Period` field:
  - Single game day: displays the game day date (e.g. `2026-11-12`).
  - Multiple game days (missed previous snapshots): displays the start and end dates along with the day count (e.g. `2026-11-10 ~ 2026-11-13 (3 days)`).
  - Same-day / in-progress capture: displays `YYYY-MM-DD (Today)`.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `card-daily-thread-report`: Updates the automated daily report delivery requirement to include character avatar icons and accurate date/period range formatting.

## Impact

- `src/GitcgNetCord.MainApp/Infrastructure/DailyReportDeliveryService.cs`: Update embed generation and emoji resolution.
- `tests/GitcgNetCord.MainApp.Tests/DailyReportDeliveryTests.cs`: Update unit tests for embed period and avatar rendering.
