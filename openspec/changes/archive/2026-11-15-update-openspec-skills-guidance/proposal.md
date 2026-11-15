# Proposal: Synchronize OpenSpec Configuration with Restored .NET and Aspire Skills

## Why

During prior configuration updates (`standardize-ef-migrations-and-dotnet-skills` and the initial pass of `optimize-agent-skills-context`), excessive global plugin skills caused context budget overflow in Antigravity, which led to the silent exclusion of 12 critical skills (`aspire`, `aspire-monitoring`, `msbuild`, `msbuild-antipatterns`, `property-patterns`, `csharp-refactoring`, `dotnet-breaking-changes`, `test-anti-patterns`, and others). Consequently, `openspec/config.yaml` was crafted under truncated visibility, omitting guidance for newly restored diagnostic (`dotnet-diag`), performance profiling, and .NET 11 specific features, while leaving an incompatible `pwsh` command invocation that fails on standard Windows environments lacking PowerShell Core.

Following the successful implementation of Strategy B (Zero-Truncation), exactly 6 core plugins (61 skills) plus all workspace and built-in skills are fully active with zero dropped skills. Updating `openspec/config.yaml` now ensures AI agents have explicit, comprehensive instructions to utilize the full breadth of active .NET, SkiaSharp diagnostic, xUnit testing, and Aspire monitoring skills.

## What Changes

- **Fix Shell Invocations in Operations Guidance**:
  - Replace `pwsh scripts/optimize-agent-skills.ps1` with Windows-compatible `powershell -ExecutionPolicy Bypass -File scripts/optimize-agent-skills.ps1` (or `powershell / pwsh`) to avoid `CommandNotFoundException` on standard Windows installations.
- **Clarify Context Stack & Constraints**:
  - Explicitly document **xUnit** as the repository test framework in `context` to prevent agents from mistakenly applying MSTest conventions or invoking `writing-mstest-tests`.
  - Document EF Core version pinning (**10.0.14**) for compatibility with Aspire Npgsql on .NET 11 preview.
- **Integrate Diagnostic & Performance Skills (`dotnet-diag`)**:
  - Add explicit rules in `rules.design` to mandate using `dotnet-diag` (`analyzing-dotnet-performance`) on SkiaSharp image rendering and hot paths to inspect allocations, disposable lifetimes, and async anti-patterns.
  - Reference `microbenchmarking` for performance-sensitive rendering or algorithm changes.
- **Integrate .NET 11 BCL Capabilities (`dotnet11`)**:
  - Direct agents to utilize `system-text-json-net11` and `process-api-net11` for modern JSON serialization options and process execution patterns introduced in .NET 11.
- **Integrate Safe Refactoring Rules (`dotnet`)**:
  - Require the use of `csharp-refactoring` and `dotnet-breaking-changes` before refactoring shared domain types across `GitcgSharp.Shared`, `GitcgSkia`, and `GitcgNetCord.MainApp`.
- **Enhance Testing and Monitoring Guidance (`operations.apply.guidance`)**:
  - Specify `run-tests` from `dotnet-test` for accurate test command construction and project-level scoping.
  - Detail `aspire-monitoring` (`aspire logs`, `aspire otel logs`, `aspire otel traces`) alongside `aspire ps` / `aspire describe` for troubleshooting runtime errors and telemetry inspection.

### Scope Boundaries

- **In-Scope**:
  - Updating `openspec/config.yaml` (`context`, `rules.design`, `rules.tasks`, and `operations.apply.guidance`).
  - Verifying OpenSpec validation passes cleanly (`openspec validate update-openspec-skills-guidance`).
- **Out-of-Scope**:
  - Modifying C# code in `src/` or tests in `tests/`.
  - Changing project dependencies or modifying `Directory.Packages.props`.
  - Changing the installed plugin or skill configuration.

### SemVer Impact
- **SemVer**: PATCH
- **Breaking Changes**: None. Changes are confined to repository-level agent guidance and OpenSpec workflow configuration.

### Infrastructure Requirements
- **Infrastructure**: None.

## Capabilities

### New Capabilities
None (`skip_specs: true` enabled). This change updates agent governance and workflow configuration.

### Modified Capabilities
None (`skip_specs: true` enabled).

## Impact

- **Affected Files**: `openspec/config.yaml`.
- **Workflow Impact**: AI agents will accurately leverage the complete set of restored skills (`dotnet-diag`, `dotnet11`, `dotnet`, `dotnet-test`, `aspire-monitoring`) without hallucinating missing tools or using failing shell commands.
- **Runtime/Product Code**: Zero impact on Discord bot execution, SkiaSharp rendering, or EF Core persistence.
