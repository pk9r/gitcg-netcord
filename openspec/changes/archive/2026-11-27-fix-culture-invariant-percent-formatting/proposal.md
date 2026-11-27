# Proposal

## Why

Running the test suite on Linux (`ubuntu-latest` in GitHub Actions) revealed a culture-dependent test failure in `DailyReportDeliveryTests.BuildDailyReportEmbed_WithEmojis_PrefixesCharacterNames` (`89/90` passed, `1` failed). Both `DailyReportDeliveryService` and `CardSlashCommand` use standard `.NET` percent format specifiers (`:P1` / `:P2`) against `CultureInfo.CurrentCulture`, which renders `"75.0%"` on Windows (`PercentPositivePattern = 1`) but `"75.0 %"` (with a space before `%`) on Linux/ICU or `InvariantCulture` (`PercentPositivePattern = 0`)—affecting both Linux CI runners and the Linux Alpine production container.

## What Changes

- Replace culture-dependent `:P1` / `:P2` percent formatting in `DailyReportDeliveryService` and `CardSlashCommand` with explicit culture-invariant formatting (`0.0%` / `0.00%` using `CultureInfo.InvariantCulture`) so win rates always render consistently without a space before `%` across Windows and Linux runtimes.
- Add regression unit tests verifying percentage formatting produces `"75.0%"` / `"75.00%"` even when `CultureInfo.CurrentCulture` uses `PercentPositivePattern = 0` (such as `CultureInfo.InvariantCulture` or `fr-FR`).

## Capabilities

### New Capabilities
None.

### Modified Capabilities
- `card-daily-thread-report`: Clarify that win-rate percentages in daily thread/DM report embeds MUST render deterministically in English without locale-dependent spacing before `%` across host operating systems.
- `card-periodic-stats`: Clarify that win-rate percentages in `/card` periodic statistics embeds MUST render deterministically in English without locale-dependent spacing before `%` across host operating systems.

## Impact

- **Application Code**: `src/GitcgNetCord.MainApp/Infrastructure/DailyReportDeliveryService.cs` (lines 213, 230) and `src/GitcgNetCord.MainApp/Commands/Slash/CardSlashCommand.cs` (lines 259, 273, 329).
- **Tests**: `tests/GitcgNetCord.MainApp.Tests/DailyReportDeliveryTests.cs` and `tests/GitcgNetCord.MainApp.Tests/CardStatsCalculatorTests.cs` (or a dedicated formatting test in `DailyReportDeliveryTests` / `CardSlashCommand` embed formatting helper tests).
- **Infrastructure Requirements**: No new databases, caches, or Aspire AppHost resources are required.
