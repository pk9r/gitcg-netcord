# Tasks

## 1. Modal Extraction Helper

- [x] 1.1 Implement a reusable extension method (e.g. `GetTextInputs` or `GetTextInputValue`) in `GitcgNetCord.MainApp.Extensions` to extract `TextInput` components nested within `NetCord.Label` or direct `IModalComponent` instances, and verify with `dotnet build`.

## 2. Interaction Handlers Update

- [x] 2.1 Refactor `AddHoyolabAccountComponentInteraction.PostAsync` to use the modal extraction helper, trim `hoyolabUserId` and `token`, return an ephemeral error if credentials are empty, and verify with `dotnet build`.
- [x] 2.2 Refactor `UpdateActiveHoyolabAccountComponentInteraction.PostAsync` to use the modal extraction helper, trim `uid`, return an ephemeral error if the UID is empty, and verify with `dotnet build`.

## 3. Unit Tests and Verification

- [x] 3.1 Add unit tests in `tests/GitcgNetCord.MainApp.Tests/` validating `TextInput` extraction from `IModalComponent` collections (handling `Label` wrapper, direct `TextInput`, missing IDs, and whitespace trimming), and verify tests compile.
- [x] 3.2 Run `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` and verify that all test suites pass without failures.
