# Design: Enable Aspire CLI Bundle in AppHost

## Context

`src/AppHost/AppHost.csproj` currently uses `Aspire.AppHost.Sdk/13.6.5`. In Aspire 13.x, MSBuild defaults `AspireUseCliBundle` to `false` and triggers warning `ASPIRE010` during build unless explicitly enabled or suppressed.

See `proposal.md` for motivation.

## Goals / Non-Goals

**Goals:**
- Eliminate the `ASPIRE010` warning during `dotnet build`.
- Align AppHost orchestration tool resolution (DCP and Dashboard) with the installed Aspire CLI bundle.
- Ensure clean solution build with 0 warnings.

**Non-Goals:**
- Fully migrating `AppHost.csproj` from `Aspire.AppHost.Sdk` to `Microsoft.NET.Sdk` + `Aspire.Hosting.Dotnet` (deferred to a separate migration if needed).
- Changing any application code or service definitions in `AppHost.cs`.

## Decisions

### Decision: Configure `<AspireUseCliBundle>true</AspireUseCliBundle>`

* **Choice**: Add `<AspireUseCliBundle>true</AspireUseCliBundle>` inside the `<PropertyGroup>` of `src/AppHost/AppHost.csproj`.
* **Rationale**:
  - Eliminates the `ASPIRE010` warning.
  - Avoids downloading large platform-specific RID NuGet packages (`Aspire.Dashboard.Sdk.*`, `Aspire.Hosting.Orchestration.*`).
  - Aligns with Microsoft Aspire's forward-looking architecture where CLI bundles manage Developer Control Plane and Dashboard binaries.
* **Alternatives considered**:
  - *Suppress warning via `<NoWarn>$(NoWarn);ASPIRE010</NoWarn>`*: Rejected because it maintains legacy behavior and will be deprecated in future Aspire releases.
  - *Full migration off `Aspire.AppHost.Sdk`*: Rejected for now to keep the change focused and minimal.

### Decision: Align Aspire CLI Tool Version

* **Choice**: Run `dotnet tool update -g Aspire.Cli` to bring Aspire CLI from `13.6.4` to `13.6.5`, matching the NuGet package version.
* **Rationale**: Keeps CLI runtime components in exact version parity with the AppHost SDK packages.

## Risks / Trade-offs

- **[Risk] CI/CD environment missing Aspire CLI**:
  - *Mitigation*: Simple compilation (`dotnet build`) succeeds without launching the bundle. If integration tests or deployment workflows execute the AppHost binary in CI, ensure `dotnet tool update -g Aspire.Cli` or Aspire CLI installation script is present in the pipeline runner.
