# Tasks

## 1. Response Helper & Component Extraction

- [x] 1.1 Implement `HoyolabAccountResponseHelper.CreateAccountAddedResponse` to generate the English confirmation embed and Daily Snapshot onboarding action row (`[ Enable Daily Snapshot ]`, `[ No, Maybe Later ]`), and refactor `SelectGameRecordComponentInteraction.ExecuteAsync` to use it.
- [x] 1.2 Verify that the solution compiles without errors using `dotnet build src/GitcgNetCord.MainApp/GitcgNetCord.MainApp.csproj`.

## 2. Character Flow Branching & Selection Annotation

- [x] 2.1 Update `AddHoyolabAccountSlashCommand.ExecuteCoreAsync` to detect when zero Genshin Impact characters are found (`count == 0`) and respond with an ephemeral notice without creating a select menu.
- [x] 2.2 Update `AddHoyolabAccountSlashCommand.ExecuteCoreAsync` to auto-link when exactly one character is found (`count == 1`), calling `HoyolabAccountService.AddHoyolabAccountAsync` directly and returning the confirmation response without select menu or cache writes.
- [x] 2.3 Update `AddHoyolabAccountSlashCommand.ExecuteCoreAsync` when multiple characters are found (`count > 1`) to query `HoyolabAccountService.GetHoyolabAccountsAsync` and annotate existing UIDs with `[Already Linked - Select to update token]`.

## 3. Unit Testing & Verification

- [x] 3.1 Implement unit tests in `tests/GitcgNetCord.MainApp.Tests/HoyolabAddAccountFlowTests.cs` verifying zero-character handling, single-character auto-link behavior, and multi-character option descriptions.
- [x] 3.2 Execute scoped unit tests with `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` and verify all tests pass.
