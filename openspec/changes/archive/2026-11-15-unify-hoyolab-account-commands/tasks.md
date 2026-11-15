# Tasks: Unify HoYoLAB Account Commands

## 1. Handlers & Slash Command Methods

- [x] 1.1 Implement `SwitchAsync` in `HoyolabAccountsSlashCommand.cs` taking an `account` parameter with `HoyolabAccountAutocompleteProvider` to switch the user's active account. Verify via `dotnet build`.
- [x] 1.2 Implement `AddAsync` in `HoyolabAccountsSlashCommand.cs` to directly launch the "Add account" modal via `InteractionCallback.Modal`. Verify via `dotnet build`.

## 2. Slash Command Routing & Module Restructuring

- [x] 2.1 Refactor `HoyolabAccountModule.cs` to register `/hoyolab` slash command group with subcommands `list`, `add`, `remove`, and `switch` enabled in `Guild`, `BotDMChannel`, and `DMChannel` contexts. Verify via `dotnet build`.
- [x] 2.2 Remove legacy standalone commands `/hoyolab-accounts`, `/add-hoyolab-account`, and `/remove-hoyolab-account` from `HoyolabAccountModule.cs`. Verify via `dotnet build`.
- [x] 2.3 Update command references and hint messages across `CardSlashCommand.cs`, `TcgSlashCommand.cs`, and `CardSnapshotSlashCommand.cs` to reference `/hoyolab`. Verify via `dotnet build`.

## 3. Verification & Testing

- [x] 3.1 Update and add unit tests in `tests/GitcgNetCord.MainApp.Tests` covering `/hoyolab` module registration, active account switching service logic, and autocomplete choices.
- [x] 3.2 Run and pass all xUnit tests using `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` and verify zero test regressions.

## Workflow follow-up

- Review and archive the change via `/openspec-archive-change` after verification succeeds.
