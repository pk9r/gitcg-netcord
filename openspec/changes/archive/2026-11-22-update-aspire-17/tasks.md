# Tasks: Update Aspire to Version 17.0 (Option A)

## 1. Tooling and Dependencies Setup

- [x] 1.1 Update the global Aspire CLI tool to `17.0.0` using `dotnet tool update -g Aspire.Cli` and verify with `aspire --version`.
- [x] 1.2 Update `Directory.Packages.props` to bump `Aspire.Hosting.PostgreSQL`, `Aspire.Hosting.Redis`, and `Aspire.StackExchange.Redis.DistributedCaching` to `17.0.0`, add package versions for `Aspire.Hosting.AppHost` (`17.0.0`) and `Aspire.Hosting.Dotnet` (`17.0.0`), while keeping `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` at `13.6.5`. Verify restore via `dotnet restore`.

## 2. AppHost Modernization

- [x] 2.1 Migrate `src/AppHost/AppHost.csproj` from `Aspire.AppHost.Sdk` to `Microsoft.NET.Sdk`, add package references for `Aspire.Hosting.AppHost` and `Aspire.Hosting.Dotnet`, and verify the project compiles without `ASPIRE012` warnings.
- [x] 2.2 In `src/AppHost/AppHost.cs`, chain `.WithImageTag("17.6")` on `builder.AddPostgres(...)` before `.WithDataVolume()` and `.WithPgAdmin()` to preserve existing dev data volume compatibility.

## 3. Configuration and Verification

- [x] 3.1 Update the stack description in `openspec/config.yaml` to reflect `.NET Aspire 17.0`.
- [x] 3.2 Build the entire solution with `dotnet build` to verify clean compilation across all projects.
- [x] 3.3 Execute existing unit tests across `tests/GitcgSkia.Tests` and `tests/GitcgNetCord.MainApp.Tests` using `dotnet test` and verify all tests pass.
