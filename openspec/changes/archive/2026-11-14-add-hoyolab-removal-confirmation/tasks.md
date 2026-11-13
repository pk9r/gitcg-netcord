# Tasks

## 1. Interaction Handlers & UI Components

- [x] 1.1 Implement `PopulateConfirmationPrompt` helper and update `SelectAsync` in `RemoveHoyolabAccountComponentInteraction.cs` to show the confirmation prompt upon dropdown selection instead of deleting immediately, and verify compilation
- [x] 1.2 Implement `ConfirmRemovalAsync` and `CancelRemovalAsync` handlers in `RemoveHoyolabAccountComponentInteraction.cs` to handle Danger confirm and Secondary cancel actions, and verify compilation
- [x] 1.3 Update `RemoveHoyolabAccountSlashCommand.cs` to look up the target account and display the confirmation prompt with Confirm and Cancel buttons, and verify compilation
- [x] 1.4 Register button component interactions for `confirm-remove-hoyolab-account` and `cancel-remove-hoyolab-account` in `HoyolabAccountModule.cs`, and verify compilation

## 2. Verification & Testing

- [x] 2.1 Add unit tests covering removal confirmation prompt generation, cancel action, and confirm execution in `tests/GitcgNetCord.MainApp.Tests/HoyolabAccountRemovalTests.cs`, and verify tests pass
- [x] 2.2 Run `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` and ensure all test suites pass with zero failures
