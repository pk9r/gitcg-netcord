# Design: Optimize Agent Skills Context (Strategy B: Zero-Truncation)

## Context

See `proposal.md` for background. Following Strategy A+, 4 skills remained excluded by Antigravity: `aspire`, `msbuild`, `coordinate-components`, and `use-igniteui-blazor`. The 10 skills in `dotnet-blazor` occupy enough token weight to squeeze out `aspire` and `msbuild`.

Because the current repository codebase has no Blazor files, temporarily disabling `dotnet-blazor` drops active plugin skills from 71 to 61 across 6 core plugins. This completely clears the token budget deficit while providing dedicated `-EnableBlazor` and `-DisableBlazor` script flags for on-demand use.

## Goals / Non-Goals

**Goals:**
- Eliminate all 4 remaining skill exclusions, achieving 0 dropped skills.
- Focus the active skill set on the 6 core plugins needed for current development: `dotnet11`, `dotnet-data`, `dotnet`, `dotnet-msbuild`, `dotnet-test`, `dotnet-diag`.
- Enhance `scripts/optimize-agent-skills.ps1` with `-EnableBlazor` and `-DisableBlazor` switches alongside `-Apply`, `-Restore`, and `-Status`.
- Update repository documentation in `openspec/config.yaml` and `README.md`.

**Non-Goals:**
- Do not permanently delete or uninstall `dotnet-blazor` or any other plugin.
- Do not alter application code in `src/` or `tests/`.

## Decisions

### Decision 1: Strategy B (Zero-Truncation with 61 active plugin skills)
- **Choice**: Temporarily disable `dotnet-blazor`, expanding the disabled list to 12 plugins:
  `azure`, `dotnet-maui`, `dotnet-winforms`, `dotnet-template-engine`, `dotnet-nuget`, `dotnet-test-migration`, `dotnet-upgrade`, `dotnet-advanced`, `dotnet-aspnetcore`, `dotnet-ai`, `dotnet-experimental`, `dotnet-blazor`.
- **Active Plugin Set (6 plugins / 61 skills)**:
  1. `dotnet11` (2 skills)
  2. `dotnet-data` (2 skills)
  3. `dotnet` (5 skills)
  4. `dotnet-msbuild` (21 skills)
  5. `dotnet-test` (24 skills)
  6. `dotnet-diag` (7 skills)
- **Rationale**: 61 plugin skills + 12 workspace skills + 11 built-in = ~84 total skills, well beneath the threshold where prompt truncation triggers.

### Decision 2: Add `-EnableBlazor` and `-DisableBlazor` switches to `scripts/optimize-agent-skills.ps1`
- **Choice**: Add direct single-purpose switches to the PowerShell utility script:
  - `-EnableBlazor`: Calls `agy plugin enable dotnet-blazor` and reports status.
  - `-DisableBlazor`: Calls `agy plugin disable dotnet-blazor` and reports status.
- **Rationale**: When web frontend work begins in the future, developers can re-activate Blazor in a single command without remembering CLI syntax.

## Risks / Trade-offs

- **[Risk]** Developer starts writing Blazor components without re-enabling the plugin.
  - **Mitigation**: Documented in `README.md`, `openspec/config.yaml`, and script status output. The IDE customization link `[dotnet-blazor](customization://dotnet-blazor)` also allows instant UI toggle.

## Migration & Rollback Plan

1. **Apply**:
   ```powershell
   pwsh scripts/optimize-agent-skills.ps1 -Apply
   ```
2. **Verification**:
   Inspect `agy plugin list` and confirm 12 plugins are disabled, 6 remain enabled (61 active skills), and 0 skills are excluded.
3. **On-Demand Blazor Activation**:
   ```powershell
   pwsh scripts/optimize-agent-skills.ps1 -EnableBlazor
   ```
4. **Full Rollback**:
   ```powershell
   pwsh scripts/optimize-agent-skills.ps1 -Restore
   ```
