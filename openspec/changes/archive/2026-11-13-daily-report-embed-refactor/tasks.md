# Tasks

## 1. Data Model and Statistics Calculation

- [x] 1.1 Update `PeriodicStatsResult` to add `IReadOnlyList<CardDeltaStat> UsedCharacters` and update `CardStatsCalculator.Calculate` to populate all characters with `UseCount > 0` sorted by `UseCount` descending and `WinRate` descending. Verify existing and updated calculator unit tests pass.

## 2. Daily Report Embed Formatting

- [x] 2.1 Refactor `DailyReportDeliveryService.BuildDailyReportEmbed` to replace `Most Used Characters` and `Highest Win Rate Characters` with a unified `Characters Used` section (and `Characters Used (Cont.)` if character limit exceeds 1024 chars) formatting each character with emoji avatar icon, name, game count, win rate percentage, and win/loss count (`${i + 1}. {emojiPrefix}**{card.Name}** - {card.UseCount} games ({card.WinRate:P1} - {card.Proficiency}W/{card.UseCount - card.Proficiency}L)`). Verify with updated unit tests in `DailyReportDeliveryTests`.
- [x] 2.2 Update `DailyReportDeliveryService.BuildDailyReportEmbed` to display up to 10 action cards from `result.TopUseActions` under the field `Top Actions`. Verify with unit tests in `DailyReportDeliveryTests`.

## 3. End-to-End Verification

- [x] 3.1 Stop the Aspire `main` resource, run the full solution test suite (`dotnet test`), and verify all unit tests pass without errors or regressions.
- [x] 3.2 Restart the Aspire `main` resource, trigger the daily report delivery via `POST /api/reports/daily/trigger`, and verify report embeds deliver cleanly to subscribed threads.
