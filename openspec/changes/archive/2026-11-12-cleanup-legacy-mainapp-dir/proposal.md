# Proposal: Cleanup Legacy MainApp Directory

## Why

During a previous project rename refactor (commit `78c3fe31`), the main application was renamed from `Gitcg.NetCord.MainApp` to `GitcgNetCord.MainApp`. However, a later commit (`822c3b59`) accidentally committed an obsolete copy of `DeckSlashCommand.cs` back into `src/Gitcg.NetCord.MainApp/Commands/Slash/`.

Currently, `src/Gitcg.NetCord.MainApp` is an unreferenced, orphaned directory containing only this outdated file, which causes confusion and clutters the workspace. Removing it ensures clean repository structure and avoids developer mistake.

## What Changes

- Delete the entire orphaned directory `src/Gitcg.NetCord.MainApp/` (including `src/Gitcg.NetCord.MainApp/Commands/Slash/DeckSlashCommand.cs`).
- Clean up any obsolete IDE workspace references pointing to `Gitcg.NetCord.MainApp` in `.idea/` if present.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
None. (`skip_specs: true` is configured as this change is a purely structural cleanup and does not alter system behaviors or specifications).

## Impact

- **Affected Code**: `src/Gitcg.NetCord.MainApp` removed.
- **APIs & Dependencies**: None. The active application, solution (`GitcgNetCord.slnx`), AppHost, and test projects already reference `src/GitcgNetCord.MainApp` exclusively.
- **Build / CI**: No impact on build or CI since `src/Gitcg.NetCord.MainApp` is not part of any project or solution.
