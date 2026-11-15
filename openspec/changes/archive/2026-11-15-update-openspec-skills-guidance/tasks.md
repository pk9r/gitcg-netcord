# Tasks

## 1. OpenSpec Configuration Updates

- [x] 1.1 Update `context` in `openspec/config.yaml` to include `xUnit` and note EF Core 10.0 pinning, and verify file parses cleanly as valid YAML
- [x] 1.2 Update `rules.design` and `rules.tasks` in `openspec/config.yaml` to integrate `dotnet-diag` for SkiaSharp allocations, `dotnet11` for modern BCL features, and `csharp-refactoring` / `dotnet-breaking-changes` for shared contracts, verifying formatting
- [x] 1.3 Update `operations.apply.guidance` in `openspec/config.yaml` to replace `pwsh` with `powershell -ExecutionPolicy Bypass -File`, expand `Dotnet Skills Utilization` with `dotnet-diag` and `dotnet11`, specify `run-tests`, and detail `aspire-monitoring` guidance

## 2. Verification & Validation

- [x] 2.1 Run `openspec validate update-openspec-skills-guidance` and verify that all change artifacts validate with 0 errors
- [x] 2.2 Run `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` using `dotnet-test` scoping rules and verify that tests pass cleanly

## Workflow follow-up

- Archive the change after verification using `/openspec-archive-change`.
