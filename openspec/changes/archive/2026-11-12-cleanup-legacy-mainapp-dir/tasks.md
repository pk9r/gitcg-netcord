# Tasks

## 1. Remove Legacy Directory

- [x] 1.1 Remove the orphaned `src/Gitcg.NetCord.MainApp` directory using `git rm -r src/Gitcg.NetCord.MainApp` and verify the directory is removed from the git index and disk.
- [x] 1.2 Clean up any stale configurations in `.idea/workspace.xml` referencing `Gitcg.NetCord.MainApp` and verify with `git grep "Gitcg.NetCord.MainApp"`.

## 2. Verification

- [x] 2.1 Verify that the active solution `GitcgNetCord.slnx` builds cleanly using `dotnet build GitcgNetCord.slnx`.
