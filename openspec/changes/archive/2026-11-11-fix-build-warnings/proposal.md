# Proposal

## Why

Building the solution emits 3 compiler warnings (`CS9113` for unread constructor parameters and `CS8618` for an uninitialized non-nullable property) in `GitcgNetCord.MainApp`. Eliminating these warnings ensures a clean build (`0 Warning(s), 0 Error(s)`), prevents dead parameter passing in primary constructors, and satisfies C# nullable reference type constraints.

## What Changes

- Remove unused `GatewayClient client` primary constructor parameter from `DeckEditorGatewayHandler`.
- Remove unused `GatewayClient client` primary constructor parameter from `DuelAssistantGatewayHandler`.
- Initialize `Message` property with `= null!;` in `GitcgGatewayHandlerContext` to satisfy compiler non-nullable initialization rules without breaking DI instantiation or downstream callers.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
None. This change is a code quality and warning cleanup task that preserves all existing runtime behavior; no functional system requirements are added or altered (`skip_specs: true`).

## Impact

- **Affected Files**:
  - `src/GitcgNetCord.MainApp/GatewayHandlers/DeckEditorGatewayHandler.cs`
  - `src/GitcgNetCord.MainApp/GatewayHandlers/DuelAssistantGatewayHandler.cs`
  - `src/GitcgNetCord.MainApp/Models/GitcgGatewayHandlerContext.cs`
- **APIs / Dependencies**: Internal constructor signatures and model defaults only. NetCord gateway handler activation via DI will resolve constructors cleanly.
- **Breaking Changes**: None.
