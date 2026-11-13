# Design: Configure Aspire Process Lifecycle & File-Lock Guidance

## Context

In `openspec/config.yaml`, the `operations.apply.guidance` section defines prompt-level operational instructions that are automatically returned by `openspec instructions apply` and consumed by agents implementing tasks. Currently, guidance advises running `dotnet test` and utilizing Aspire skills without setting process boundaries, leading agents to invoke destructive global commands (`aspire stop`, `aspire start`). See `proposal.md` for background context.

## Goals / Non-Goals

**Goals:**
- Guarantee that agents never terminate the developer's interactive Aspire AppHost session, terminal, or dashboard.
- Guarantee that agents never spawn detached, headless background Aspire processes.
- Provide a clear, resource-scoped procedure (`aspire resource main stop/start`) to resolve Windows DLL locking during builds and tests.
- Promote scoped test execution (targeting specific test projects) instead of full solution compilation.

**Non-Goals:**
- Modifying C# code in `src/AppHost/AppHost.cs` or `GitcgNetCord.MainApp`.
- Rewriting global agent skills in `.agents/skills/`.
- Disabling tests or skipping compilation steps.

## Decisions

### Decision 1: Enforce Aspire Process Ownership in `operations.apply.guidance`
- **Choice**: Add explicit negative constraints forbidding `aspire stop` and `aspire start`.
- **Rationale**: Agents prioritize negative guardrails in operation guidance over generic skill recommendations. Explicitly stating "The developer owns the Aspire AppHost lifecycle" prevents agents from assuming orchestration responsibility.
- **Alternatives Considered**:
  - *Completely forbidding all `aspire` commands*: Too restrictive; inspecting health/logs via `aspire ps` or `aspire describe` is beneficial.
  - *Modifying global skills*: Fragile and harder to maintain across workspace resets or updates.

### Decision 2: Resource-Level Isolation (`main`) for File-Lock Recovery
- **Choice**: Guide agents to execute `aspire resource main stop` before building/testing when locks occur, followed by `aspire resource main start`.
- **Rationale**: In `src/AppHost/AppHost.cs`, the service is registered with `builder.AddProject<GitcgNetCord_MainApp>("main")`. Stopping `main` terminates the executing `GitcgNetCord.MainApp.dll` process and releases the OS file lock immediately, while keeping Postgres, Redis, PgAdmin, and the Aspire Developer Dashboard alive.
- **Alternatives Considered**:
  - *Asking the developer manually every time*: Safe but halts autonomous task execution unnecessarily when a standard non-destructive CLI command exists.
  - *Full solution clean / kill dotnet*: Unsafe, leads to orphaned processes and port collisions.

### Decision 3: Project-Scoped Testing Guidance
- **Choice**: Direct agents to target `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` (or `tests/GitcgSkia.Tests/GitcgSkia.Tests.csproj`) rather than root-level `dotnet test`.
- **Rationale**: Root `dotnet test` evaluates the entire `GitcgNetCord.slnx`, attempting to build `AppHost` and other projects, which amplifies file contention risks.

## Risks / Trade-offs

- **[Risk] Aspire CLI commands fail if AppHost is not running** → **Mitigation**: Guidance advises checking `aspire ps` first or only invoking `aspire resource main stop` when file lock errors (`MSB3491` / `CS2012`) actually occur.
- **[Risk] Resource name mismatch if AppHost configuration changes** → **Mitigation**: The resource name `"main"` is established in `AppHost.cs` (line 7). If ever renamed, `openspec/config.yaml` can be updated accordingly.
