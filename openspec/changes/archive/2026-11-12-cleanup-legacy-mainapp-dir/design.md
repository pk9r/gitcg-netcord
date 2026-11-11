# Design: Cleanup Legacy MainApp Directory

## Context

See `proposal.md` for background. The directory `src/Gitcg.NetCord.MainApp` exists as an artifact of historical renaming. It contains one file `src/Gitcg.NetCord.MainApp/Commands/Slash/DeckSlashCommand.cs`. The active project is located at `src/GitcgNetCord.MainApp/` and is referenced by `GitcgNetCord.slnx` and `src/AppHost/AppHost.csproj`.

## Goals / Non-Goals

**Goals:**
- Completely remove `src/Gitcg.NetCord.MainApp/` from source control and disk.
- Clean up any stale IDE configuration references if present in `.idea/workspace.xml`.
- Confirm that `GitcgNetCord.slnx` and `src/AppHost` compile without errors or warnings related to the removed path.

**Non-Goals:**
- No changes to `src/GitcgNetCord.MainApp/` source code.
- No changes to application behavior or functionality.

## Decisions

- **Decision: Remove via Git (`git rm -r`)**
  - *Rationale*: Ensures proper git tracking removal and disk removal in one operation.
  - *Alternative Considered*: Manual file deletion, but `git rm` is cleaner and tracks the change explicitly.

- **Decision: Verify no references remain via global grep**
  - *Rationale*: Double check codebase to confirm no lingering references to `Gitcg.NetCord.MainApp`.

## Risks / Trade-offs

- [Accidental deletion of active code] → Mitigated by verified inspection showing `GitcgNetCord.MainApp` is the active project and `Gitcg.NetCord.MainApp` only contains a single outdated duplicate file.
