# Design

## Context

See `proposal.md` for motivation. Standard `.NET` percent format strings (`"P1"`, `"P2"`) rely on `CultureInfo.CurrentCulture.NumberFormat.PercentPositivePattern`. While Windows `en-US` uses pattern `1` (`75.0%`), `CultureInfo.InvariantCulture` and Linux/ICU defaults often use pattern `0` (`75.0 %` with a space), and non-English locales can also change the decimal separator. Two production components format win-rate percentages for Discord embeds:
- `src/GitcgNetCord.MainApp/Infrastructure/DailyReportDeliveryService.cs`: lines 213 (`OverallWinRate:P1`) and 230 (`card.WinRate:P1`).
- `src/GitcgNetCord.MainApp/Commands/Slash/CardSlashCommand.cs`: lines 259 (`x.WinRate:P2`), 273 (`x.WinRate:P2`), and 329 (`OverallWinRate:P2`).

## Goals / Non-Goals

**Goals:**
- Guarantee identical win-rate percentage formatting (`0.0%` for daily reports, `0.00%` for `/card` periodic stats) across Windows, Linux CI runners, and Linux Alpine production containers.
- Cover the behavior with deterministic unit tests that explicitly exercise `PercentPositivePattern = 0` cultures.

**Non-Goals:**
- Modifying global `CultureInfo.DefaultThreadCurrentCulture` across the entire process (which could hide locale bugs elsewhere or affect unrelated parsing).
- Changing decimal precision (`1` decimal place in daily reports vs. `2` decimal places in `/card`).

## Decisions

### Decision 1: Explicit Custom Format Strings with `CultureInfo.InvariantCulture`
- **Choice**: Format win rates using `string.Create(CultureInfo.InvariantCulture, $"{value:0.0%}")` (or `value.ToString("0.0%", CultureInfo.InvariantCulture)`) in `DailyReportDeliveryService` and `0.00%` with `CultureInfo.InvariantCulture` in `CardSlashCommand` (optionally extracted into a small shared formatting helper if appropriate to avoid duplication).
- **Rationale**: In .NET custom numeric format strings, `%` multiplies the value by 100 and emits `PercentSymbol` (`%` in `InvariantCulture`) immediately adjacent to the digits without consulting `PercentPositivePattern`, while `InvariantCulture` fixes the decimal separator to `.`.
- **Alternative Considered**: Passing `CultureInfo.GetCultureInfo("en-US")` to `:P1` / `:P2` (rejected because `en-US` formatting can still vary between Windows NLS and Linux ICU/CLDR versions).

### Decision 2: Extract a Small Formatting Helper or Keep Local Consistency
- **Choice**: Extract a lightweight, testable formatting helper (or static helper method) for `FormatWinRate` / win-rate display in `CardSlashCommand` and `DailyReportDeliveryService` if both can share it cleanly without coupling command modules to infrastructure, or keep minimal static helpers per component.
- **Rationale**: Extracting a small pure helper makes it trivial to unit test `CardSlashCommand`'s `0.00%` formatting directly without needing to mock a full NetCord `ApplicationCommandContext` and Discord REST client.
- **Alternative Considered**: Constructing full NetCord interaction mocks solely to assert `0.00%` strings in `CardSlashCommand` (unnecessarily heavy for a pure formatting bug).

## Risks / Trade-offs

- [Risk: Changing `CultureInfo.CurrentCulture` inside a unit test could affect parallel xUnit test classes] → **Mitigation**: Save and restore `CultureInfo.CurrentCulture` inside a `try/finally` block in the test, and keep the production fix independent of `CurrentCulture` altogether.
