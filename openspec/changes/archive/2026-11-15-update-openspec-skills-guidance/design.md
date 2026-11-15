# Design: Synchronize OpenSpec Configuration with Restored .NET and Aspire Skills

## Context

See `proposal.md` for background and motivation.

The project currently operates under Strategy B (Zero-Truncation), with 6 core plugins active (`dotnet11`, `dotnet-data`, `dotnet`, `dotnet-msbuild`, `dotnet-test`, `dotnet-diag`) and 12 auxiliary plugins temporarily disabled. This setup resolved the prior context overflow, restoring all previously dropped skills (`aspire`, `aspire-monitoring`, `csharp-refactoring`, `dotnet-breaking-changes`, `msbuild`, `dotnet-diag`).

However, `openspec/config.yaml` was authored when these skills were excluded. This design outlines the precise schema updates to `openspec/config.yaml` to ensure AI agents proactively utilize all active skills and follow reliable cross-environment shell practices.

## Goals / Non-Goals

**Goals:**
- Update `openspec/config.yaml` to replace failing `pwsh` command strings with reliable Windows PowerShell invocations.
- Explicitly declare `xUnit` and EF Core version pinning in the `context` block.
- Establish rules for `dotnet-diag` (`analyzing-dotnet-performance`, `microbenchmarking`) for SkiaSharp graphics rendering and performance-critical paths.
- Incorporate `dotnet11` (`system-text-json-net11`, `process-api-net11`) and safe refactoring skills (`csharp-refactoring`, `dotnet-breaking-changes`) into design rules.
- Enrich `operations.apply.guidance` with `run-tests` for test command building and `aspire-monitoring` for telemetry/log debugging.

**Non-Goals:**
- Do not modify C# source files or project dependencies.
- Do not alter the active plugin profile or re-enable auxiliary plugins.

## Decisions

### Decision 1: Cross-Environment PowerShell Invocation Syntax
- **Choice**: Specify `powershell -ExecutionPolicy Bypass -File scripts/optimize-agent-skills.ps1 [-Status|-Apply|-Restore|-EnableBlazor|-DisableBlazor]` in `operations.apply.guidance`.
- **Rationale**: While PowerShell Core (`pwsh`) is cross-platform, standard developer workstations and CI runners on Windows often have only Windows PowerShell (`powershell.exe`) pre-installed. Using `powershell -ExecutionPolicy Bypass -File` guarantees seamless execution across all Windows sessions without encountering `pwsh: CommandNotFoundException`.
- **Alternative Considered**: Keeping `pwsh` only. *Rejected*: Causes reproducible execution failures on systems without PowerShell 7 in PATH.

### Decision 2: Stack Definition & Precision in `context`
- **Choice**: Update `context` to explicitly specify `xUnit` and note EF Core 10.0 pinning:
  `Stack: .NET 11, C# 15, .NET Aspire 13.6, NetCord (1.0.0-beta.29), SkiaSharp, EF Core 10.0 (PostgreSQL, pinned for Aspire Npgsql), Redis, xUnit.`
- **Rationale**: Prevents agents from attempting to bump EF Core to 11.0 before Aspire Npgsql supports it, and prevents agents from mis-activating MSTest skills (`writing-mstest-tests`) or generating MSTest attributes.
- **Alternative Considered**: Leaving `context` unchanged. *Rejected*: Leads to recurring test framework confusion and dependency drift during future changes.

### Decision 3: Embed `dotnet-diag` in Graphics & Rendering Design Rules
- **Choice**: Add an explicit clause in `rules.design`:
  `"Detail SkiaSharp canvas allocation and image stream lifetimes to prevent native memory leaks; utilize dotnet-diag (analyzing-dotnet-performance) on rendering hot paths"`
- **Rationale**: SkiaSharp relies on unmanaged native C++ allocations (`SKBitmap`, `SKCanvas`, `SKImage`, `SKPaint`). Memory leaks or improper disposal in a long-running bot cause process bloat. `analyzing-dotnet-performance` specifically detects un-disposed resources, memory leaks, and async allocation hot paths.

### Decision 4: Embed .NET 11 and Safe Refactoring Skills in Design Rules
- **Choice**: Add explicit guidance for .NET 11 and refactoring in `rules.design`:
  `"C# & .NET 11 Evolution: Utilize dotnet11 skills (system-text-json-net11, process-api-net11) for new BCL capabilities; apply csharp-refactoring and dotnet-breaking-changes before refactoring shared contracts"`
- **Rationale**: Guides agents to use modern .NET 11 JSON policy APIs (`JsonNamingPolicy.PascalCase`) and generic `JsonTypeInfo` rather than ad-hoc custom policies, and prevents breaking contracts between `GitcgNetCord.MainApp` and shared projects.

### Decision 5: Complete Dotnet Skills Utilization and Monitoring in Apply Guidance
- **Choice**: Update `operations.apply.guidance`:
  - Update `Dotnet Skills Utilization` to include `dotnet-diag` and `dotnet11`.
  - Add explicit reference to `run-tests` for constructing scoped `dotnet test` commands.
  - Update Aspire monitoring guidance to explicitly reference `aspire-monitoring` (`aspire logs`, `aspire otel logs`, `aspire otel traces`).
- **Rationale**: Agents consult `operations.apply.guidance` at the start of implementation. Explicitly naming the tools ensures they are invoked without prompting.

## Risks / Trade-offs

- **[Risk]** Developers on Linux/macOS running PowerShell Core might prefer typing `pwsh`.
  - **Mitigation**: Mentioning `powershell -ExecutionPolicy Bypass -File scripts/optimize-agent-skills.ps1` works across all Windows hosts, and Linux/macOS developers can naturally invoke `pwsh scripts/optimize-agent-skills.ps1`.
- **[Risk]** Too many skill mentions could clutter the config.
  - **Mitigation**: The proposed additions are concise, scoped to the 6 core active plugins, and directly relevant to the Discord bot architecture.

## Migration Plan

1. Edit `openspec/config.yaml` to apply the updated `context`, `rules`, and `operations.apply.guidance`.
2. Run `openspec validate update-openspec-skills-guidance` to confirm schema validity.
3. Verify that all 10 projects continue to build and pass tests cleanly.
