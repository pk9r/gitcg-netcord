# Design: Update Aspire to Version 17.0 (Option A)

## Context

The solution uses Central Package Management (`Directory.Packages.props`) with `ManagePackageVersionsCentrally: true`. Currently:
- The AppHost project (`src/AppHost/AppHost.csproj`) is built on `Aspire.AppHost.Sdk/13.6.5`.
- Hosting integration packages (`Aspire.Hosting.PostgreSQL`, `Aspire.Hosting.Redis`, `Aspire.StackExchange.Redis.DistributedCaching`) are on version `13.6.5`.
- Client database package `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` is pinned at `13.6.5` with EF Core `10.0.14`.
- The developer's machine currently runs Aspire CLI `13.6.5`, triggering update notifications for `17.0.0`.

See `proposal.md` for motivation and scope boundaries.

## Goals / Non-Goals

**Goals:**
- Eliminate the obsolete `Aspire.AppHost.Sdk` pattern (`ASPIRE012`) by transitioning `src/AppHost/AppHost.csproj` to `Microsoft.NET.Sdk` and explicit package references (`Aspire.Hosting.AppHost` and `Aspire.Hosting.Dotnet` 17.0.0).
- Update central package versions for hosting packages to `17.0.0`.
- Preserve existing local PostgreSQL database volume integrity by pinning the container image tag to `17.6`.
- Update the global Aspire CLI to `17.0.0` for full compatibility with the new AppHost runtime and redesigned dashboard.

**Non-Goals:**
- Do not adopt `17.0.0-preview.*` packages for runtime client dependencies (`Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` remains stable `13.6.5`).
- Do not upgrade the underlying PostgreSQL major version to 18 (avoids breaking data storage layout).
- Do not alter Discord bot commands, business logic, or EF Core entity models.

## Decisions

### Decision 1: Migrate AppHost Project SDK to Standard `Microsoft.NET.Sdk`
- **Approach**: Replace `<Project Sdk="Aspire.AppHost.Sdk/13.6.5">` with `<Project Sdk="Microsoft.NET.Sdk">` in `src/AppHost/AppHost.csproj`. Add `<PackageReference Include="Aspire.Hosting.AppHost" />` and `<PackageReference Include="Aspire.Hosting.Dotnet" />` to the project file and manage their versions (17.0.0) in `Directory.Packages.props`.
- **Rationale**: In Aspire 17, `Aspire.AppHost.Sdk` is deprecated (`ASPIRE012`). Standardizing on `Microsoft.NET.Sdk` is the official modern pattern.
- **Alternatives Considered**: Suppressing `ASPIRE012`. Rejected because the SDK itself is being sunsetted and cannot take full advantage of Aspire 17 package updates.

### Decision 2: Pin PostgreSQL Container Image to `17.6` in `AppHost.cs`
- **Approach**: In `src/AppHost/AppHost.cs`, specify `.WithImageTag("17.6")` on `builder.AddPostgres(...)` prior to invoking `.WithDataVolume()`.
  ```csharp
  var postgres = builder.AddPostgres(
      name: "postgresql",
      password: postgresPassword
  ).WithImageTag("17.6");
  ```
- **Rationale**: Aspire 17 updates the default PostgreSQL image to PostgreSQL 18.x. PostgreSQL 18 uses a version-specific folder hierarchy (`/var/lib/postgresql/<version>/...`) which cannot mount existing volumes initialized by PostgreSQL 17 without manual cluster migration. Pinning to `17.6` ensures zero downtime or data loss for existing local databases.
- **Alternatives Considered**: Upgrading to Postgres 18 and clearing local data volumes. Rejected because it forces developers to recreate or seed local data.

### Decision 3: Retain Client Component `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` at Stable `13.6.5`
- **Approach**: Keep `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` at `13.6.5` in `Directory.Packages.props`. Update `Aspire.StackExchange.Redis.DistributedCaching` to `17.0.0`.
- **Rationale**: As of the Aspire 17 release, `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` version 17 is only available as a preview (`17.0.0-preview.1.26567.5`). The stable `13.6.5` package is fully compatible with EF Core 10.0 and communicates via standard connection strings passed by the AppHost environment.
- **Alternatives Considered**: Upgrading to `17.0.0-preview.1.26567.5`. Rejected due to preview status and stability concerns.

### Decision 4: Global CLI Tool Update
- **Approach**: Update the global .NET tool via `dotnet tool update -g Aspire.Cli`.
- **Rationale**: Ensures `aspire doctor`, `aspire ps`, and `aspire dashboard` function identically with the 17.0 AppHost.

## Risks / Trade-offs

- **[Risk] File locks during build if AppHost / MainApp process is active (MSB3026/MSB3027)**
  → *Mitigation*: Verify processes before building or rebuilding; prompt developer to pause or restart the application resource if active.
- **[Risk] Unpinned container image upgrading silently**
  → *Mitigation*: Explicitly place `.WithImageTag("17.6")` directly on the `builder.AddPostgres` call before `.WithDataVolume()`.
- **[Risk] CPM transitive package mismatch**
  → *Mitigation*: Ensure `Aspire.Hosting.AppHost` and `Aspire.Hosting.Dotnet` are registered in `Directory.Packages.props` with exact matching version `17.0.0`.

## Migration Plan

1. Upgrade CLI: `dotnet tool update -g Aspire.Cli`.
2. Update `Directory.Packages.props` with 17.0.0 hosting packages.
3. Update `src/AppHost/AppHost.csproj` SDK and package references.
4. Update `src/AppHost/AppHost.cs` with `.WithImageTag("17.6")`.
5. Update `openspec/config.yaml` context.
6. Verify compilation and tests via `dotnet build` and `dotnet test`.
7. Rollback strategy: Git revert restores project files to `13.6.5` state with no schema or database mutations.
