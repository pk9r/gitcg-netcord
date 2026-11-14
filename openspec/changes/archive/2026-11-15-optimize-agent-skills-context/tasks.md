# Tasks

## 1. Automation Scripting

- [x] 1.1 Update `scripts/optimize-agent-skills.ps1` to include `dotnet-blazor` in `$TargetPlugins` and implement `-EnableBlazor` and `-DisableBlazor` switches
- [x] 1.2 Test `-Status` mode to verify accurate reporting with the 12-plugin target list and a safe threshold (~65 skills)

## 2. Configuration & Documentation

- [x] 2.1 Update `openspec/config.yaml` operations guidance to document Strategy B (12 disabled plugins, 6 active plugins, on-demand Blazor toggle)
- [x] 2.2 Update `README.md` Agent Environment section to document Strategy B and `-EnableBlazor` / `-DisableBlazor` commands

## 3. Optimization Execution & Verification

- [x] 3.1 Execute `pwsh scripts/optimize-agent-skills.ps1 -Apply` to disable the 12 non-essential plugins and verify via `agy plugin list` that exactly 61 skills remain active across 6 core plugins
- [x] 3.2 Run scoped unit tests via `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` to confirm solution health

## Workflow follow-up

- Archive the change after implementation and verification are complete.
- Verify delta specs sync is clean.
