# Design: Prune Redundant and Unnecessary Descriptions from OpenSpec Configuration

## Context

See `proposal.md` for problem context and motivation.
The `openspec/config.yaml` file defines project background (`context`), per-artifact generation rules (`rules`), and lifecycle operation guidance (`operations.apply.guidance` and `operations.archive.guidance`). Currently, several entries duplicate OpenSpec schema primitives, repeat rules defined elsewhere, or contain developer-facing administrative instructions that bloat agent context during task execution.

## Goals / Non-Goals

**Goals:**
- Eliminate schema-level redundancies that OpenSpec already enforces by default (`proposal` template sections, RFC 2119 keyword enforcement, GIVEN/WHEN/THEN scenarios, archive spec sync).
- Purge misplaced administrative text (specifically the ~730-character `Agent Skills & Context Optimization` script guide) from `operations.apply.guidance`.
- Consolidate cross-section duplicates (SkiaSharp IDisposable, English UI language, generic `dotnet test` statements, and Context7 MCP reminders).
- Preserve 100% of domain-critical guardrails: layer separation, EF Core migration CLI mandate, Aspire AppHost process ownership and session-level stopping, SkiaSharp memory safety, and scoped test execution.

**Non-Goals:**
- Modifying C# projects, solution configuration, or test suites.
- Changing OpenSpec specs in `openspec/specs/`.
- Removing developer scripts or altering `README.md`.

## Decisions

### Decision 1: Anchor General Constraints Solely in `context`
- **Choice**: Keep English UI language requirement and SkiaSharp disposal rules strictly in the top-level `context` block.
- **Rationale**: The `context` block is automatically injected into all OpenSpec operations (planning, designing, task generation, and application). Re-stating `"Reiterate that all user-facing responses in scenarios must be rendered in English"` in `rules.specs` or `"Check for proper IDisposable usage on all SkiaSharp resources"` at the end of `operations.apply.guidance` creates noise without adding new semantic constraints.
- **Alternatives Considered**: Keeping them in rules or guidance; rejected because it increases prompt tokens across every artifact generation step.

### Decision 2: Remove Built-in OpenSpec Schema Enforcements
- **Choice**: Remove RFC 2119 keywords and GIVEN/WHEN/THEN scenario rules from `rules.specs`, SemVer/Scope boundaries from `rules.proposal`, and "keep tasks small" from `rules.tasks`.
- **Rationale**: `openspec validate --specs` programmatically validates RFC 2119 keywords and GIVEN/WHEN/THEN blocks. Similarly, the built-in `proposal` template already generates In-Scope/Out-of-Scope boundaries and SemVer impact sections.
- **Alternatives Considered**: Keeping them as reminders; rejected because automated validation already guarantees compliance.

### Decision 3: Remove Developer Plugin Management from `apply.guidance`
- **Choice**: Remove the `Agent Skills & Context Optimization` entry from `operations.apply.guidance`.
- **Rationale**: This entry describes how human developers run `powershell scripts/optimize-agent-skills.ps1` to disable 12 global plugins. When an AI agent executes tasks via `openspec apply` (e.g., modifying card codes, fixing rendering logic), running environment reconfiguration scripts is out-of-scope and distracting. This guidance is already permanently documented in `README.md`.
- **Alternatives Considered**: Condensing the entry; rejected because agent runtime apply operations do not and should not reconfigure IDE plugins.

### Decision 4: Streamline Specialized Skills Guidance into Actionable Directives
- **Choice**: Remove the broad `Dotnet Skills Utilization` laundry list and redundant Context7 MCP reminders, retaining direct, action-oriented directives for EF Core (`dotnet-data` / `dbx`), testing (`run-tests`), and Aspire telemetry (`aspire-monitoring`).
- **Rationale**: Agent User Global Rules already mandate Context7 MCP for all library lookups. In `apply.guidance`, actionable directives (such as using `run-tests` for scoped testing or `dbx` for schema inspection) give clear, immediate instructions without needing a secondary summary bullet.

## Target Configuration Preview

The planned pruned `openspec/config.yaml`:

```yaml
schema: spec-driven

context: |
  Project: gitcg-netcord (Discord Bot for Genshin Impact Genius Invokation TCG).
  Stack: .NET 11, C# 15, .NET Aspire 13.6, NetCord (1.0.0-beta.29), SkiaSharp, EF Core 10.0 (PostgreSQL, pinned for Aspire Npgsql), Redis, xUnit.
  User Interface Language: ALL user-facing Discord elements (commands, parameter descriptions, embed titles/fields/footers, notices, error messages) MUST be written in English.
  Graphics Safety: Strictly ensure all SkiaSharp native objects (SKBitmap, SKCanvas, SKImage, SKPaint) are properly disposed.
  Git Conventions: Use Conventional Commits (feat, fix, refactor, docs) adhering to project history.

rules:
  proposal:
    - "Identify any new infrastructure requirements (databases, caches, Aspire AppHost resources)"
  specs:
    - "Scenarios must explicitly verify permission restrictions (ManageChannels, Guild-only, Thread owner) and edge cases"
  design:
    - "Separate architectural layers: Domain models, Service/Rendering logic, and NetCord command modules"
    - "When adding dependencies or services, define their integration within the Aspire AppHost and ServiceDefaults"
    - "Detail SkiaSharp canvas allocation and image stream lifetimes to prevent native memory leaks; utilize `dotnet-diag` (`analyzing-dotnet-performance`) on rendering hot paths"
    - "C# & .NET 11 Evolution: Utilize `dotnet11` skills (`system-text-json-net11`, `process-api-net11`) for new BCL capabilities; apply `csharp-refactoring` and `dotnet-breaking-changes` before refactoring shared contracts"
    - "Database Design & Optimization: When reviewing, modifying, or creating EF Core models, queries, and DbContext configurations, utilize `dotnet-data` (`optimizing-ef-core-queries`) and `dbx` MCP tools (`dbx_describe_table`, `dbx_list_tables`)"
    - "Database Migration Policy: NEVER design application startup (`Program.cs`, service registration, or background workers) to execute runtime database migrations (`Database.Migrate()`, `Database.EnsureCreated()`, or DDL). All database schema evolutions MUST be handled via EF Core CLI migration tooling (`dotnet ef`)"
  tasks:
    - "Order implementation: EF Core Migrations (via dotnet ef) -> Aspire AppHost Wiring -> Core/Skia Logic -> NetCord Handlers/Endpoints -> Unit Tests"
    - "Always include an explicit task for running and passing xUnit tests (using `run-tests` from `dotnet-test`)"
    - "Database Tasks: Tasks for database schema changes MUST specify `dotnet ef migrations add` and `dotnet ef database update` with explicit `--project` and `--startup-project` flags; NEVER create tasks that add runtime migration code to `Program.cs`"

operations:
  apply:
    guidance:
      - "Aspire Process Ownership: NEVER run `aspire stop` or `aspire start`. The developer owns the Aspire AppHost lifecycle"
      - "Session-Level Aspire Resource Lifecycle: If Aspire is active (`aspire ps`), proactively stop the application resource once at the beginning using `aspire resource main stop` to avoid file locks (MSB3491/CS2012). Restart it once when tasks complete using `aspire resource main start`"
      - "Database Schema Migrations: NEVER execute migrations at runtime in `Program.cs`. ALWAYS use: `dotnet ef migrations add <MigrationName> --project src/GitcgNetCord.MainApp --startup-project src/GitcgNetCord.MainApp` and apply with `dotnet ef database update --project src/GitcgNetCord.MainApp --startup-project src/GitcgNetCord.MainApp`"
      - "Database State Inspection: Use `dbx` MCP tools (`dbx_describe_table`, `dbx_list_tables`, `dbx_execute_query`) to verify existing schemas and check `__EFMigrationsHistory` out-of-band"
      - "Scoped Testing: Prefer scoping `dotnet test` to the target test project using `run-tests` rather than running solution-wide tests"
      - "Telemetry & Health Monitoring: Utilize Aspire CLI (`aspire ps`, `aspire describe`) or `aspire-monitoring` skill (`aspire logs`, `aspire otel logs`, `aspire otel traces`) to inspect AppHost health and telemetry"
  archive:
    guidance:
      - "Ensure no lingering temporary build logs (e.g., msbuild.binlog) or uncommitted scratch files remain"
```

## Risks / Trade-offs

- **[Risk: Agents writing specs might forget permission scenarios without general reminders]**  
  → **Mitigation**: Kept the explicit rule in `rules.specs` requiring scenarios to test `ManageChannels`, `Guild-only`, and `Thread owner` permissions.
- **[Risk: Agents skipping xUnit test execution]**  
  → **Mitigation**: Preserved the mandatory task rule in `rules.tasks` and `Scoped Testing` guidance in `operations.apply.guidance`.
