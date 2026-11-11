# Tasks

## 1. Update Aspire Package & SDK Definitions

- [x] 1.1 Update `Aspire.AppHost.Sdk` from `13.6.4` to `13.6.5` in `src/AppHost/AppHost.csproj` and verify the file content specifies `<Project Sdk="Aspire.AppHost.Sdk/13.6.5">`
- [x] 1.2 Update Aspire package versions from `13.6.4` to `13.6.5` in `Directory.Packages.props` (`Aspire.Hosting.PostgreSQL`, `Aspire.Hosting.Redis`, `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL`, `Aspire.StackExchange.Redis.DistributedCaching`) and verify all Aspire entries specify `Version="13.6.5"`

## 2. Restore and Vulnerability Verification

- [x] 2.1 Restore NuGet packages using `dotnet restore` and verify the command succeeds with exit code 0
- [x] 2.2 Audit vulnerable packages using `dotnet list package --vulnerable --include-transitive` and verify neither `Aspire.Hosting.AppHost` nor `Aspire.Hosting` appears in the vulnerability report

## 3. Build & Integration Verification

- [x] 3.1 Build the solution with `dotnet build` and verify compilation succeeds without errors
- [x] 3.2 Build the `AppHost` project specifically (`dotnet build src/AppHost/AppHost.csproj`) and verify no build warnings NU1903 are emitted for Aspire packages

## Workflow follow-up

- Archive the change after implementation and verification are complete.

