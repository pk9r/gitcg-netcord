# Tasks

## 1. Culture-Invariant Percentage Formatting

- [x] 1.1 Update `DailyReportDeliveryService.BuildDailyReportEmbed` in `src/GitcgNetCord.MainApp/Infrastructure/DailyReportDeliveryService.cs` (overall win rate and per-character win rate) to format percentages with `CultureInfo.InvariantCulture` using `"0.0%"` instead of `:P1`, and add/update a unit test in `tests/GitcgNetCord.MainApp.Tests/DailyReportDeliveryTests.cs` verifying `"75.0%"` output even under `CultureInfo.InvariantCulture` (and restoring `CultureInfo.CurrentCulture` in `finally`).
- [x] 1.2 Update `CardSlashCommand` in `src/GitcgNetCord.MainApp/Commands/Slash/CardSlashCommand.cs` (overall win rate, best win rate, and lowest win rate) to format percentages with `CultureInfo.InvariantCulture` using `"0.00%"` instead of `:P2` (via a pure testable helper method), and add unit tests in `tests/GitcgNetCord.MainApp.Tests/` verifying `"75.00%"` output under non-US / `InvariantCulture` locales.

## 2. Verification

- [x] 2.1 Run and pass all xUnit tests in `tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` (via `run-tests`) and confirm no remaining `:P1` / `:P2` locale-dependent format specifiers exist in `src/GitcgNetCord.MainApp`.

## Workflow follow-up

- Push to `dev` and verify the `CI` GitHub Actions workflow (`ubuntu-latest`) passes all 94+ tests, completing remote verification for `add-ci-test-workflow`.
- Archive `fix-culture-invariant-percent-formatting` via `/openspec-archive-change` once verified.
