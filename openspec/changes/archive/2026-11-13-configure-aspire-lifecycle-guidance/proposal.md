# Proposal: Configure Aspire Process Lifecycle & File-Lock Guidance

## Why

When agents execute tasks during `/openspec-apply-change`, running solution-wide `dotnet build` or `dotnet test` while Aspire is running triggers Windows file locks (MSB3491/CS2012) on `GitcgNetCord.MainApp.dll`. Default agent recovery rules attempt to shut down the entire Aspire AppHost (`aspire stop`), terminating the developer's interactive session and dashboard, and subsequently launch unmanageable detached background Aspire processes (`aspire start`). Explicit operation guidance in `openspec/config.yaml` is needed to establish that the developer owns the AppHost process and enforce resource-level isolation (`aspire resource main stop/start`) instead of AppHost kills.

## What Changes

- Add **Aspire Process Ownership** rules to `openspec/config.yaml` under `operations.apply.guidance`:
  - FORBID `aspire stop` (prevents terminating user's active session, terminal, containers, and dashboard).
  - FORBID `aspire start` (prevents spawning unmanageable detached background daemon processes).
- Add **Aspire File-Lock Prevention & Recovery** guidance:
  - When encountering file locks or preparing to build/test while Aspire is active (`aspire ps`), target ONLY the specific application resource: `aspire resource main stop`.
  - Scope tests directly to the target test project (e.g., `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj`).
  - Restart the resource inside the running AppHost using `aspire resource main start`.
- **SemVer Impact**: PATCH (workflow guidance refinement, zero breaking changes).

## Scope Boundaries

- **In-Scope**:
  - Customizing `openspec/config.yaml` guidance for apply operations.
  - Validating configuration syntax and ensuring OpenSpec CLI consumes the updated guidance.
- **Out-of-Scope**:
  - Changes to application code, database migrations, or test projects.
  - Changes to `src/AppHost/AppHost.cs`.
  - Overwriting global built-in skills outside this repository.

## Capabilities

### New Capabilities
None (`skip_specs: true` declared).

### Modified Capabilities
None (`skip_specs: true` declared).

## Impact

- **Affected Systems**: OpenSpec apply workflow (`/openspec-apply-change`).
- **Infrastructure Requirements**: None.
- **Breaking Changes**: None.
