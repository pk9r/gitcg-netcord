# Tasks

## 1. OpenSpec Configuration Updates

- [x] 1.1 Update `openspec/config.yaml` rules (`design`, `tasks`) to forbid runtime database migrations (`Database.Migrate()`, `EnsureCreated()`, or ad-hoc DDL) during application startup, mandate EF Core CLI tooling (`dotnet ef`), and integrate `dotnet-data` (`optimizing-ef-core-queries`) and `dotnet-test` skills; verify configuration validity with `openspec doctor`.
- [x] 1.2 Update `openspec/config.yaml` operations apply guidance with explicit `dotnet ef migrations add` and `dotnet ef database update` commands, and define `dbx` MCP inspection workflows (`dbx_describe_table`, `dbx_list_tables`, `dbx_execute_query`); verify rendered guidance with `openspec doctor`.

## 2. Application Startup Cleanup

- [x] 2.1 Remove the `await dbContext.Database.MigrateAsync();` scope block from `src/GitcgNetCord.MainApp/Program.cs` and verify compilation with `dotnet build src/GitcgNetCord.MainApp/GitcgNetCord.MainApp.csproj`.
- [x] 2.2 Rebuild the solution with `dotnet build GitcgNetCord.slnx` and verify zero compilation errors or warnings.

## 3. Verification & Testing

- [x] 3.1 Run tests using `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` and verify all tests pass.
- [x] 3.2 Verify database connectivity and schema visibility with `dbx` MCP (`dbx_list_tables`) confirming that database inspection works out-of-band without runtime startup code.
