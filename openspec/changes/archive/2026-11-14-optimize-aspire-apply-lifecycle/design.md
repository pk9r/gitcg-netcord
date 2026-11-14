# Design: Optimize Aspire Apply Lifecycle via Session-Level Proactive Management

## Context

In `openspec/config.yaml`, the `operations.apply.guidance` section instructs agents on operational behavior during task execution. The previous guidance operated reactively: waiting for `dotnet build` or `dotnet test` to fail with file locks before running `aspire resource main stop`. Because MSBuild implements an internal retry loop that waits 15–30 seconds before timing out with `MSB3491`/`CS2012`, this reactive loop wastes substantial time on every build. See `proposal.md` for motivation.

## Goals / Non-Goals

**Goals:**
- Eliminate the 15–30 second MSBuild retry delay by proactively stopping the `main` resource before implementation begins.
- Minimize Aspire CLI invocation overhead to exactly one stop at the beginning and one start at the end of the apply session.
- Protect the bot against Discord Gateway reconnect churn caused by stopping and restarting after every minor task.

**Non-Goals:**
- Modifying application C# code, solution files, or database schema.
- Modifying Aspire AppHost wiring in `src/AppHost/AppHost.cs`.

## Decisions

### Decision 1: Proactive Session-Level Resource Lifecycle
- **Choice**: If Aspire is active (`aspire ps`), stop the `main` resource once at the very start of the apply session (`aspire resource main stop`).
- **Rationale**: While an agent is implementing changes, the bot's assemblies are continually modified. Keeping `main` stopped throughout the session ensures zero file contention for any `dotnet build` or `dotnet test` calls, allowing them to compile at native speed.
- **Alternatives Considered**:
  - *Per-command proactive stop/start*: Stopping before every build and restarting immediately after. Rejected because it incurs CLI overhead on every task and spams Discord Gateway connect/disconnect cycles.
  - *Reactive error recovery*: Current behavior. Rejected because MSBuild takes 15–30s to fail on locked files.

### Decision 2: Single Clean Restart on Completion or Pause
- **Choice**: Execute `aspire resource main start` once when all tasks are marked complete (or when implementation pauses).
- **Rationale**: Ensures the service starts cleanly only when the full suite of tasks has been applied and validated.

## Risks / Trade-offs

- **[Risk] Bot remains stopped if apply is interrupted mid-way** → **Mitigation**: Guidance advises restarting on pause; the developer can also restart `main` with one click in the Aspire Dashboard or via `aspire resource main start`. Supporting services (Postgres, Redis, PgAdmin, Dashboard) remain running throughout.
