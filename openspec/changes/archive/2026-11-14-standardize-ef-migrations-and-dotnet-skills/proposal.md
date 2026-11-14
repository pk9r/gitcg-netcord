# Proposal: Standardize EF Core Migrations and Integrate Dotnet Skills & DBX MCP

## Why

During bug fixing and feature development, AI coding agents frequently encountered database schema changes or data persistence issues. Lacking explicit procedural constraints, agents resorted to modifying application startup code in `Program.cs` by injecting runtime migration logic (`await dbContext.Database.MigrateAsync()`). This runtime anti-pattern risks startup timeouts, concurrent migration race conditions in multi-instance deployments, and unintended database schema locks upon application launch.

Furthermore, official `.NET Agent Skills` (`dotnet-data`, `dotnet-msbuild`, `dotnet-test`) and the `dbx` MCP server (providing database inspection and query execution) are now installed and available in the environment. However, OpenSpec configuration (`openspec/config.yaml`) does not currently reference or instruct agents on how to leverage these specialized tools. Redesigning OpenSpec configuration to mandate EF Core CLI tooling (`dotnet ef`), integrate `dotnet` skills, utilize `dbx` MCP for database inspection, and remove runtime startup migration code restores clean application architecture and ensures deterministic database operations.

## What Changes

- **Enforce EF Core Migration Standards**: Update `openspec/config.yaml` (`rules` and `operations.apply.guidance`) to strictly forbid modifying `Program.cs`, service registrations, or worker lifecycle methods to execute runtime schema migrations (`Database.Migrate()`, `Database.EnsureCreated()`, or raw DDL execution).
- **Mandate EF CLI Tooling**: Establish explicit requirements for generating and applying schema migrations via `dotnet ef` (e.g., `dotnet ef migrations add <Name> --project src/GitcgNetCord.MainApp --startup-project src/GitcgNetCord.MainApp` and `dotnet ef database update`).
- **Integrate DBX MCP Tooling**: Add guidance in `openspec/config.yaml` directing agents to use the `dbx` MCP server (`dbx_describe_table`, `dbx_list_tables`, `dbx_execute_query`) to inspect table structures, verify applied migrations in `__EFMigrationsHistory`, and validate database states without modifying production source code.
- **Integrate Dotnet Skills**: Embed explicit references in OpenSpec `design` and `tasks` rules to leverage installed `.NET Agent Skills`:
  - `dotnet-data`: Apply `optimizing-ef-core-queries` for query efficiency, index verification, and DbContext usage.
  - `dotnet-msbuild`: Use build diagnostics and target troubleshooting when project references or migration builds encounter issues.
  - `dotnet-test`: Run scoped test execution and ensure regression safety across data access changes.
- **Remove Runtime Migration Anti-pattern**: Plan the removal of `await dbContext.Database.MigrateAsync();` from `src/GitcgNetCord.MainApp/Program.cs` during implementation to restore clean startup behavior.
- **SemVer Impact**: PATCH (enhances repository development rules, AI agent guidelines, and cleans up startup code; zero breaking changes to public Discord bot commands).

## Scope Boundaries

- **In-Scope**:
  - Updating `openspec/config.yaml` with explicit database migration policies, `dotnet-data` skill guidance, and `dbx` MCP inspection workflows.
  - Removing `await dbContext.Database.MigrateAsync();` from `src/GitcgNetCord.MainApp/Program.cs`.
  - Verifying the application builds and tests pass cleanly without runtime startup migrations.
- **Out-of-Scope**:
  - Modifying EF Core entity models or adding new database tables/columns in this change.
  - Altering Discord bot commands, handlers, embed builders, or SkiaSharp rendering logic.
  - Installing new external package dependencies.

## Capabilities

### New Capabilities
None (`skip_specs: true` declared). This change refines developer workflow configuration, AI assistant constraints, and removes startup anti-patterns.

### Modified Capabilities
None.

## Impact

- **Affected Code**: `openspec/config.yaml`, `src/GitcgNetCord.MainApp/Program.cs`.
- **APIs & Dependencies**: No new dependencies. Uses existing `dotnet ef` CLI tool, installed `dotnet` skills, and `dbx` MCP server.
- **System Stability**: Eliminates startup schema lock contention and prevents unintended runtime schema mutations across application deployments.
