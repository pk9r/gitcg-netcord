# Tasks

## 1. Remove Unused Constructor Parameters (CS9113)

- [x] 1.1 Remove unused `GatewayClient client` parameter from `DeckEditorGatewayHandler` primary constructor in `src/GitcgNetCord.MainApp/GatewayHandlers/DeckEditorGatewayHandler.cs` and verify CS9113 is resolved for this handler
- [x] 1.2 Remove unused `GatewayClient client` parameter from `DuelAssistantGatewayHandler` primary constructor in `src/GitcgNetCord.MainApp/GatewayHandlers/DuelAssistantGatewayHandler.cs` and verify CS9113 is resolved for this handler

## 2. Resolve Non-Nullable Property Initialization Warning (CS8618)

- [x] 2.1 Initialize `Message` property with `= null!;` in `GitcgGatewayHandlerContext` (`src/GitcgNetCord.MainApp/Models/GitcgGatewayHandlerContext.cs`) and verify CS8618 is resolved

## 3. Verification

- [x] 3.1 Run `dotnet build --no-incremental` across the solution and verify output reports `0 Warning(s), 0 Error(s)`
- [x] 3.2 Run `dotnet test` to verify all test suites continue to pass
