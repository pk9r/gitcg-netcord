# Design: Standardize EF Core Migrations and Integrate Dotnet Skills & DBX MCP

## Context

See [proposal.md](proposal.md) for background and motivation.

The `gitcg-netcord` project uses .NET 10, C# 13, EF Core 10, and Npgsql (PostgreSQL). The design-time factory `AppDbContextFactory` is configured in `src/GitcgNetCord.MainApp/Infrastructure/AppDbContextFactory.cs`. The local environment has `dotnet ef` (v11.0.0) installed and available on PATH, the `dbx` MCP server configured with active PostgreSQL connections, and official `dotnet/skills` (`dotnet-data`, `dotnet-msbuild`, `dotnet-test`) installed globally.

Currently, `src/GitcgNetCord.MainApp/Program.cs` contains an ad-hoc database migration block:
```csharp
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}
```
This block was introduced as a workaround by an agent when handling schema changes without utilizing EF Core migration tools. This design defines the configuration updates and code cleanup needed to prevent this issue permanently.

## Goals / Non-Goals

**Goals:**
- Update `openspec/config.yaml` to enforce a strict policy prohibiting runtime schema migrations (`Database.Migrate()`, `EnsureCreated()`, or DDL) during application startup.
- Document and enforce standard `dotnet ef` CLI commands for generating and applying migrations in this repository.
- Incorporate `dbx` MCP workflows into OpenSpec guidance for schema inspection, table structure verification, and checking applied migrations in `__EFMigrationsHistory`.
- Incorporate `dotnet` skills (notably `dotnet-data: optimizing-ef-core-queries`, `dotnet-msbuild`, `dotnet-test`) into OpenSpec design and task guidance.
- Remove the runtime `MigrateAsync()` invocation from `src/GitcgNetCord.MainApp/Program.cs`.

**Non-Goals:**
- Creating new EF Core entities, changing table schemas, or generating new migration files in this change.
- Altering the PostgreSQL container resource configuration in `src/AppHost/AppHost.cs`.
- Introducing automated build-time or git-hook migrations.

## Decisions

### Decision 1: Strict Prohibition of Runtime Database Migration in Application Startup

- **Choice**: Forbid placing `dbContext.Database.MigrateAsync()`, `dbContext.Database.EnsureCreated()`, or raw schema creation queries in `Program.cs`, service initialization, or background workers.
- **Rationale**: Runtime migrations create startup bottlenecks, fail unpredictably if the database container is still warming up during Aspire startup, cause concurrency races in multi-process/multi-replica deployments, and blur the boundary between schema versioning and application runtime logic.
- **Alternative Considered**: Allowing `MigrateAsync()` only in `Development` environment. *Rejected*: This encourages agents to skip creating and verifying formal EF migration files, leading to discrepancies when deploying or running in production or isolated test environments.

### Decision 2: Mandate Explicit `dotnet ef` CLI Invocations

- **Choice**: Require all database schema changes to be handled via `dotnet ef` commands with explicit `--project` and `--startup-project` flags:
  - Add migration:
    ```powershell
    dotnet ef migrations add <MigrationName> --project src/GitcgNetCord.MainApp --startup-project src/GitcgNetCord.MainApp
    ```
  - Update database:
    ```powershell
    dotnet ef database update --project src/GitcgNetCord.MainApp --startup-project src/GitcgNetCord.MainApp
    ```
- **Rationale**: In a solution with multiple projects (`AppHost`, `MainApp`, `GitcgSharp.Shared`), running `dotnet ef` without explicit project paths can fail due to ambiguous entry points or failure to locate `AppDbContextFactory`.
- **Alternative Considered**: Running raw SQL scripts directly against PostgreSQL. *Rejected*: EF Core Code-First migrations update `AppDbContextModelSnapshot.cs` and track applied versions in `__EFMigrationsHistory`, preserving schema integrity across team members.

### Decision 3: Leverage `dbx` MCP for Database Inspection & State Verification

- **Choice**: Direct agents in `openspec/config.yaml` (`rules.design` and `operations.apply.guidance`) to utilize the `dbx` MCP server tools:
  - `dbx_list_tables`: Enumerate current database tables.
  - `dbx_describe_table`: Inspect column types, nullability, primary/foreign keys, and indexes.
  - `dbx_execute_query`: Query `__EFMigrationsHistory` or verify sample data state.
- **Rationale**: Agents previously guessed schema structure or wrote throwaway test code. `dbx` MCP provides direct, non-intrusive DBMS inspection without modifying repository source files.
- **Alternative Considered**: Using shell `psql` commands. *Rejected*: `dbx` MCP is cross-platform, natively available to the agent, handles authentication automatically, and avoids shell escaping/environment variable risks.

### Decision 4: Leverage Dotnet Skills in OpenSpec Rules

- **Choice**: Embed explicit references in `openspec/config.yaml`:
  - `dotnet-data` (`optimizing-ef-core-queries`): Activate when writing queries, designing entity relations, adding indexes, or reviewing DbContext performance.
  - `dotnet-msbuild`: Activate when diagnosing build, target, or project reference issues.
  - `dotnet-test`: Activate for test scoping and test quality enforcement.
- **Rationale**: Having skills installed is only useful if agents know when and where to trigger them during planning and implementation phases.

## Risks / Trade-offs

- **[Risk]** Developer starts the bot on a fresh environment where PostgreSQL has not had `dotnet ef database update` run yet.
  - **Mitigation**: Document the single `dotnet ef database update` command clearly in developer instructions and project context. The Aspire environment maintains persistent container volumes between runs, so migrations are only needed when schema actually changes.
- **[Risk]** Agent attempts to run `dotnet ef database update` when the PostgreSQL container is not running.
  - **Mitigation**: Guidance specifies ensuring Aspire AppHost is running or testing connection availability with `dbx` before applying database updates.

## Migration Plan

1. **Update OpenSpec Configuration**: Edit `openspec/config.yaml` to update `rules` and `operations.apply.guidance`.
2. **Clean Up Startup Code**: Edit `src/GitcgNetCord.MainApp/Program.cs` to remove the `await dbContext.Database.MigrateAsync();` scope block.
3. **Verify Build & Tests**:
   - Run `dotnet build src/GitcgNetCord.MainApp/GitcgNetCord.MainApp.csproj`.
   - Run `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj`.
   - Confirm clean compilation with no warnings or errors.
