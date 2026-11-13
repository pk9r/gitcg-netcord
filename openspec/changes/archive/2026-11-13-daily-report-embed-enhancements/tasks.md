# Tasks

## 1. Application Emoji Caching

- [x] 1.1 Inject `HybridCache` into `DailyReportDeliveryService` and implement cached emoji map resolution `GetApplicationEmojisMapAsync`, and verify project build.

## 2. Embed Formatting Enhancements

- [x] 2.1 Update `DailyReportDeliveryService.BuildDailyReportEmbed` to format the `Period` field (single day, multi-day, or today) and prepend character role avatar emojis to character ranking names, and verify compilation.
- [x] 2.2 Wire the resolved emojis map into `DeliverReportsAsync` calls to `BuildDailyReportEmbed`, and verify with `dotnet build`.

## 3. Testing and Verification

- [x] 3.1 Update and expand unit tests in `DailyReportDeliveryTests.cs` covering single-day, multi-day, today period formats, and character emoji rendering, verifying all tests pass via `dotnet test`.
- [x] 3.2 Validate the change artifacts with `openspec validate daily-report-embed-enhancements --strict`.
