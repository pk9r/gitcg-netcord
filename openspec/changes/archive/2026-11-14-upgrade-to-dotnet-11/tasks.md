# Tasks

## 1. Target Framework & Package Upgrades

- [x] 1.1 Configure `Directory.Packages.props` to keep `Microsoft.EntityFrameworkCore.Design` and `Microsoft.EntityFrameworkCore.InMemory` at `10.0.14` while setting BCL/Web packages to `11.0.0`, remove EF Core 11 transitive pins, remove direct EF Core package reference in `src/GitcgNetCord.MainApp/GitcgNetCord.MainApp.csproj`, and verify file contents.
- [x] 1.2 Update `<TargetFramework>net11.0</TargetFramework>` across all 8 projects in `src/` (`src/AppHost/AppHost.csproj`, `src/GitcgNetCord.MainApp/GitcgNetCord.MainApp.csproj`, `src/GitcgPainter/GitcgPainter.csproj`, `src/GitcgSharp.Shared/GitcgSharp.Shared.csproj`, `src/GitcgSkia/GitcgSkia.csproj`, `src/HoyolabHttpClient/HoyolabHttpClient.csproj`, `src/ServiceDefaults/ServiceDefaults.csproj`, `src/SharedUtils/SharedUtils.csproj`) and verify with file check.
- [x] 1.3 Update `<TargetFramework>net11.0</TargetFramework>` in both test projects (`tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` and `tests/GitcgSkia.Tests/GitcgSkia.Tests.csproj`) and verify with file check.

## 2. Infrastructure & Configuration Updates

- [x] 2.1 Update `src/GitcgNetCord.MainApp/Dockerfile` to use `mcr.microsoft.com/dotnet/aspnet:11.0-alpine` and `mcr.microsoft.com/dotnet/sdk:11.0-alpine` and verify file content.
- [x] 2.2 Update `openspec/config.yaml` to specify `.NET 11, C# 15` in the stack context and verify YAML validity.

## 3. Build & Test Verification

- [x] 3.1 Run `dotnet restore` and `dotnet build` across the solution and verify clean compilation with 0 errors and zero NU1608/NU1107 warnings.
- [x] 3.2 Run `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` and verify all 72 tests pass under .NET 11.
- [x] 3.3 Run `dotnet test tests/GitcgSkia.Tests/GitcgSkia.Tests.csproj` and verify all 4 tests pass under .NET 11.
- [x] 3.4 Restart the application under Aspire and verify through Aspire CLI / health checks that the resource is Healthy with HTTP 200.
