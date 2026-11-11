# Tasks

## 1. Environment and AppHost Configuration

- [x] 1.1 Update Aspire CLI global tool via `dotnet tool update -g Aspire.Cli` and verify `aspire --version` matches `13.6.5`
- [x] 1.2 Add `<AspireUseCliBundle>true</AspireUseCliBundle>` to `<PropertyGroup>` in `src/AppHost/AppHost.csproj`

## 2. Verification

- [x] 2.1 Run `dotnet build` on the solution and verify build succeeds with 0 warnings (no `ASPIRE010`)
- [x] 2.2 Run `aspire doctor` and verify AppHost check passes with the bundle enabled
