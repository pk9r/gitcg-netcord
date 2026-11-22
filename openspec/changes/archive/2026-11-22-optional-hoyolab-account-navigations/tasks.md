# Tasks

## 1. Domain Entities & DbContext Configuration

- [x] 1.1 Update `CardSnapshot`, `ActiveHoyolabAccount`, and `DiscordDailyReportThread` entity definitions in `src/GitcgNetCord.MainApp/Entities/` to make `HoyolabAccountId` nullable (`Guid?`) and navigation properties optional (`HoyolabAccount?`), and verify compilation succeeds.
- [x] 1.2 Update entity configurations in `src/GitcgNetCord.MainApp/Infrastructure/AppDbContext.cs` to configure `HoyolabAccount` navigations as optional (`.IsRequired(false)`) with `DeleteBehavior.SetNull` on `CardSnapshot` and `ActiveHoyolabAccount` and `DeleteBehavior.Cascade` on `DiscordDailyReportThread`, and verify compilation succeeds.

## 2. Database Migration via EF Core CLI

- [x] 2.1 Proactively stop the running Aspire application resource (`aspire resource main stop` or MCP command) to release DLL file locks, generate migration via `dotnet ef migrations add MakeHoyolabAccountNavigationsOptional --project src/GitcgNetCord.MainApp --startup-project src/GitcgNetCord.MainApp`, apply it via `dotnet ef database update --project src/GitcgNetCord.MainApp --startup-project src/GitcgNetCord.MainApp`, and restart the resource (`aspire resource main start`).

## 3. Service & Domain Logic Adjustments

- [x] 3.1 Update `DiscordUser.cs` and `ActiveHoyolabAccountService.cs` to replace `Guid.Empty` sentinel comparisons with idiomatic nullable `Guid?` checks (`activeAccount.HoyolabAccountId == null`), and verify compilation succeeds.
- [x] 3.2 Update `CardSnapshotService.cs` and associated consumers to support nullable `HoyolabAccountId` where applicable, and verify compilation succeeds.

## 4. Verification & Testing

- [x] 4.1 Run all xUnit tests in `tests/GitcgNetCord.MainApp.Tests` via `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` and verify that all test suites pass cleanly.
- [x] 4.2 Inspect Aspire structured logs and console logs for resource `main` and verify that `Microsoft.EntityFrameworkCore.Model.Validation[10622]` warnings are completely eliminated.
