# Proposal: Enable Aspire CLI Bundle in AppHost

## Why

During `dotnet build`, MSBuild raises warning `ASPIRE010` stating that `AppHost is configured with AspireUseCliBundle=false`.
In Aspire 13.x, AppHost projects transition from fetching bulky platform-specific RID NuGet packages (for Developer Control Plane and Dashboard) to leveraging the Aspire CLI bundle installed in the environment. Enabling the bundle eliminates the build warning, aligns the project with modern Aspire best practices, and speeds up local restores and builds.

## What Changes

- Enable the Aspire CLI Bundle by setting `<AspireUseCliBundle>true</AspireUseCliBundle>` in `src/AppHost/AppHost.csproj`.
- Verify the build succeeds with 0 warnings.
- Document any environment prerequisites (requiring `aspire` CLI for local development/running AppHost).

## Capabilities

### New Capabilities
None.

### Modified Capabilities
None.

## Impact

- **Affected code**: `src/AppHost/AppHost.csproj`.
- **APIs & Behavior**: No changes to user-facing bot capabilities or application logic.
- **Dependencies & Tooling**: Local developers running the AppHost rely on the globally installed Aspire CLI (`dotnet tool update -g Aspire.Cli`).
