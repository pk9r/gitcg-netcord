# Proposal: Update Aspire to Version 17.0 (Option A)

## Why

Microsoft released .NET Aspire 17.0.0, upgrading the CLI, AppHost SDK model, developer dashboard, and integration packages from the 13.x line. Upgrading the local tooling and project AppHost ensures alignment with the latest Aspire capabilities (dashboard improvements, decoupled project hosting, performance optimizations) while safeguarding existing local database volumes and EF Core PostgreSQL compatibility.

## What Changes

- **Aspire CLI Tooling**: Upgrade the global Aspire CLI from `13.6.5` to `17.0.0`.
- **AppHost Project Modernization**: **BREAKING**: Replace the obsolete `Aspire.AppHost.Sdk/13.6.5` with the standard `Microsoft.NET.Sdk` and explicit package references (`Aspire.Hosting.AppHost` 17.0.0 and `Aspire.Hosting.Dotnet` 17.0.0) to eliminate warning `ASPIRE012`.
- **Central Package Management**:
  - Update `Aspire.Hosting.PostgreSQL`, `Aspire.Hosting.Redis`, and `Aspire.StackExchange.Redis.DistributedCaching` to `17.0.0` in `Directory.Packages.props`.
  - Retain `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` at stable `13.6.5` to maintain 100% stability with EF Core 10.0 and existing migrations.
- **PostgreSQL Volume Protection**: **BREAKING**: Explicitly pin the PostgreSQL container image tag to `17.6` in `AppHost.cs` prior to calling `.WithDataVolume()` to prevent volume corruption/incompatibility caused by PostgreSQL 18's new on-disk layout.
- **Documentation & Context**: Update project documentation in `openspec/config.yaml` to reflect the .NET Aspire 17.0 stack.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
None. (This is a tooling, AppHost orchestration, and infrastructure package upgrade with no changes to Discord bot behavior or functional domain specifications; `skip_specs: true` is enabled).

## Impact

- **Affected Projects**:
  - `src/AppHost/AppHost.csproj`
  - `src/AppHost/AppHost.cs`
  - `Directory.Packages.props`
  - `openspec/config.yaml`
- **Infrastructure**: Local PostgreSQL container image is pinned to `17.6` to guarantee zero data loss on existing dev volumes; Redis cache container continues as standard.
- **Developer Environment**: Requires running `dotnet tool update -g Aspire.Cli` to bring the CLI to `17.0.0`.
