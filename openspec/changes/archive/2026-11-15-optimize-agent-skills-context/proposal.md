# Proposal: Optimize Agent Skills Context (Strategy B: Zero-Truncation)

## Why

Even after applying Strategy A+ (reducing active plugin skills from 136 to 71 across 7 plugins), a slight context budget overflow persisted, with exactly 4 skills excluded: `aspire`, `msbuild`, `coordinate-components`, and `use-igniteui-blazor`. Significantly, two of the excluded skills belong to `dotnet-blazor`, while simultaneously crowding out critical local and build skills (`aspire` and `msbuild`). Since `gitcg-netcord` currently contains exclusively Discord bot, SkiaSharp rendering, EF Core data access, and Aspire orchestration code without any active `.razor` components, temporarily disabling `dotnet-blazor` eliminates the remaining overflow completely, bringing active plugin skills down to 61 and achieving 100% skill availability with zero exclusions.

## What Changes

- Transition from Strategy A+ to **Strategy B (Zero-Truncation)**:
  - **Active Core Profile (6 plugins / 61 skills)**:
    - `dotnet11` (2): .NET 11 Process APIs and System.Text.Json.
    - `dotnet-data` (2): EF Core query optimization and data access.
    - `dotnet` (5): C# 15 language refactoring, diagnostics, breaking changes.
    - `dotnet-msbuild` (21): Project build diagnostics, native asset integration, and binlog analysis.
    - `dotnet-test` (24): Unit test execution, test quality analysis, and xUnit runner.
    - `dotnet-diag` (7): Diagnostics, dump collection, memory and performance profiling.
  - **Temporarily Disabled (12 plugins / 75 skills eliminated)**:
    - `dotnet-blazor` (-10): Temporarily disabled until active Blazor web frontend/dashboard development begins.
    - Previously disabled auxiliary plugins: `azure` (-28), `dotnet-maui` (-8), `dotnet-template-engine` (-6), `dotnet-winforms` (-1), `dotnet-nuget` (-1), `dotnet-test-migration` (-6), `dotnet-upgrade` (-6), `dotnet-advanced` (-4), `dotnet-aspnetcore` (-2), `dotnet-ai` (-1), `dotnet-experimental` (-2).
- Add on-demand Blazor toggling to `scripts/optimize-agent-skills.ps1` via `-EnableBlazor` and `-DisableBlazor` switches so developers can re-enable Blazor in 1 second when building web components.
- Update `openspec/config.yaml` operations guidance and `README.md` to reflect Strategy B.

### Boundaries

- **In-Scope**:
  - Disabling 12 unused/auxiliary plugins (including `dotnet-blazor`).
  - Updating `scripts/optimize-agent-skills.ps1` with `-EnableBlazor` and `-DisableBlazor` switches.
  - Updating `openspec/config.yaml` and `README.md`.
  - Verifying 0 excluded skills in agent context.
- **Out-of-Scope**:
  - Permanently uninstalling any plugins from disk.
  - Modifying application runtime code in `src/` or tests in `tests/`.

### SemVer Impact
- **SemVer**: PATCH
- **Breaking Changes**: None. Changes apply to agent environment and tooling configuration only.

### Infrastructure Requirements
- **Infrastructure**: None.

## Capabilities

### New Capabilities
None (`skip_specs: true` enabled). Tooling and configuration change only.

### Modified Capabilities
None (`skip_specs: true` enabled).

## Impact

- **Agent Context**: Total active global plugin skills reduced to 61 skills (from originally 136, a ~55% reduction). Eliminates the 4 remaining excluded skills and guarantees un-truncated access to `aspire` and `msbuild`.
- **Developer Workflow**: Developers run in an ultra-clean context for bot development, with an instant one-line toggle to enable Blazor when building web dashboards.
- **Dependencies**: No NuGet or package changes.
