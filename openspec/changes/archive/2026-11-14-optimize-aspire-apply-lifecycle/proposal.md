# Proposal: Optimize Aspire Apply Lifecycle via Session-Level Proactive Management

## Why

Currently, during `/openspec-apply-change`, the guidance directs agents to stop the `main` resource reactively after detecting a build lock failure. This results in agents running `dotnet build` or `dotnet test` against the active process, waiting through MSBuild's 15–30 second file lock retry loop, failing with MSB3491/CS2012, and only then executing `aspire resource main stop`. Shifting to a proactive session-level lifecycle—stopping `main` once at the very start of the apply session and restarting it once when all tasks finish—eliminates file-lock delays entirely and prevents Discord Gateway reconnect churn during intermediate task builds.

## What Changes

- Replace the reactive file-lock instruction in `openspec/config.yaml` (`operations.apply.guidance`) with proactive **Session-Level Aspire Resource Lifecycle** guidance:
  - If Aspire is active (`aspire ps`), stop the application resource once at the start of implementation: `aspire resource main stop`.
  - Maintain the resource in stopped state across all implementation tasks, intermediate builds, and unit tests to ensure zero file contention.
  - Restart the resource once upon task completion or pause: `aspire resource main start`.
- **SemVer Impact**: PATCH (workflow operation guidance enhancement, no breaking changes).

## Scope Boundaries

- **In-Scope**:
  - Updating `operations.apply.guidance` in `openspec/config.yaml`.
  - Verifying guidance rendering via `openspec instructions apply`.
- **Out-of-Scope**:
  - Modifying application source code or tests.
  - Modifying `src/AppHost/AppHost.cs`.
  - Modifying external agent skills.

## Capabilities

### New Capabilities
None (`skip_specs: true` declared).

### Modified Capabilities
None (`skip_specs: true` declared).

## Impact

- **Affected Systems**: OpenSpec apply workflow (`/openspec-apply-change`).
- **Performance Impact**: Eliminates 15–30s MSBuild lock retry timeouts on builds/tests during apply.
- **Breaking Changes**: None.
