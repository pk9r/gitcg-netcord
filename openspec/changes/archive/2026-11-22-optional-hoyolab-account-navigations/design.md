# Design

## Context

See `proposal.md` for background and problem motivation.

In the current schema, `HoyolabAccount` has a global query filter:
```csharp
config.HasQueryFilter(x => !x.IsDeleted);
```
Three entities maintain a foreign key to `HoyolabAccount`:
1. `CardSnapshot.HoyolabAccountId`: `Guid` (non-nullable)
2. `ActiveHoyolabAccount.HoyolabAccountId`: `Guid` (non-nullable, default `Guid.Empty`)
3. `DiscordDailyReportThread.HoyolabAccountId`: `Guid` (non-nullable)

Because all three foreign keys are non-nullable `Guid`, EF Core's `ModelValidator` marks the relationships as required. When `HoyolabAccount` is filtered out by `!IsDeleted`, EF Core issues `Microsoft.EntityFrameworkCore.Model.Validation[10622]` warning because queries with `.Include(x => x.HoyolabAccount)` will translate to `INNER JOIN` in SQL, silently omitting dependent entities.

## Goals / Non-Goals

**Goals:**
- Eliminate EF Core model validation warning `Microsoft.EntityFrameworkCore.Model.Validation[10622]` completely.
- Convert foreign key properties `HoyolabAccountId` on `CardSnapshot`, `ActiveHoyolabAccount`, and `DiscordDailyReportThread` to nullable `Guid?`.
- Configure EF Core navigations as optional (`.IsRequired(false)`).
- Replace `Guid.Empty` sentinel values in `ActiveHoyolabAccount` with idiomatic C# `null`.
- Maintain full compatibility with existing tests and services.
- Adhere strictly to the Database Migration Policy: all migrations executed via `dotnet ef` CLI, never in runtime application startup.

**Non-Goals:**
- Changing user-facing Discord command interactions or outputs.
- Modifying the global query filter on `HoyolabAccount`.
- Hard-deleting historical snapshots.

## Decisions

### Decision 1: Make `CardSnapshot.HoyolabAccountId` Nullable (`Guid?`)
- **Choice**: Change `CardSnapshot.HoyolabAccountId` to `Guid?` and navigation `HoyolabAccount?`. Configure in `AppDbContext`:
  ```csharp
  builder.Entity<CardSnapshot>(config =>
  {
      config.HasOne(x => x.HoyolabAccount)
          .WithMany(x => x.CardSnapshots)
          .HasForeignKey(x => x.HoyolabAccountId)
          .IsRequired(false)
          .OnDelete(DeleteBehavior.SetNull);
  });
  ```
- **Rationale**: Historical game performance and deck match records are audit data that should persist indefinitely even if the account is removed. Marking the relationship as optional prevents `INNER JOIN` record dropping and accurately reflects domain lifetime semantics.
- **Alternatives Considered**:
  - *Adding matching query filter `!x.HoyolabAccount.IsDeleted`*: Rejected because it would hide historical snapshots when an account is deleted, defeating the purpose of retaining snapshot history.
  - *Suppressing warning via `ConfigureWarnings`*: Rejected because it leaves latent bugs where `.Include()` drops rows.

### Decision 2: Make `ActiveHoyolabAccount.HoyolabAccountId` Nullable (`Guid?`)
- **Choice**: Change `ActiveHoyolabAccount.HoyolabAccountId` to `Guid?` (default `null`). In `AppDbContext`:
  ```csharp
  builder.Entity<ActiveHoyolabAccount>(config =>
  {
      config.HasOne(x => x.HoyolabAccount)
          .WithMany()
          .HasForeignKey(x => x.HoyolabAccountId)
          .IsRequired(false)
          .OnDelete(DeleteBehavior.SetNull);
  });
  ```
- **Rationale**: Currently, `ActiveHoyolabAccount` initializes `HoyolabAccountId` with `Guid.Empty` as a sentinel for "no active account". Moving to `Guid?` provides idiomatic nullability, allows EF Core to treat the navigation as optional, and removes all `Guid.Empty` checks across `DiscordUser` and `ActiveHoyolabAccountService`.
- **Alternatives Considered**:
  - *Keep non-nullable `Guid` and add query filter*: Still leaves `Guid.Empty` sentinel in place and risks confusion when account is soft-deleted.

### Decision 3: Make `DiscordDailyReportThread.HoyolabAccountId` Nullable (`Guid?`)
- **Choice**: Change `DiscordDailyReportThread.HoyolabAccountId` to `Guid?` and navigation `HoyolabAccount?`. In `AppDbContext`:
  ```csharp
  builder.Entity<DiscordDailyReportThread>(config =>
  {
      config.HasIndex(x => new { x.ThreadId, x.HoyolabAccountId }).IsUnique();
      config.HasIndex(x => x.GuildId);
      config.HasIndex(x => x.DiscordUserId);
      config.HasIndex(x => x.HoyolabAccountId);

      config.HasOne(x => x.HoyolabAccount)
          .WithMany(x => x.DailyReportThreads)
          .HasForeignKey(x => x.HoyolabAccountId)
          .IsRequired(false)
          .OnDelete(DeleteBehavior.Cascade);
  });
  ```
- **Rationale**: `HoyolabAccountService.RemoveHoyolabAccountAsync` already explicitly purges subscriptions when an account is deleted. Making the navigation optional satisfies EF Core's model validator without altering business behavior.

### Decision 4: Schema Evolution via EF Core CLI Tooling
- **Choice**: In accordance with the repository's `config.yaml` database migration policy:
  - Generate migration using:
    `dotnet ef migrations add MakeHoyolabAccountNavigationsOptional --project src/GitcgNetCord.MainApp --startup-project src/GitcgNetCord.MainApp`
  - Apply migration using:
    `dotnet ef database update --project src/GitcgNetCord.MainApp --startup-project src/GitcgNetCord.MainApp`
  - Under no circumstances will migration execution be placed in `Program.cs`.

## Architectural Diagram

```
+--------------------------------------------------------------------------+
|                  OPTIONAL NAVIGATION ARCHITECTURE                        |
+--------------------------------------------------------------------------+
|                                                                          |
|       +---------------------------------------------+                    |
|       |               HoyolabAccount                |                    |
|       |  Filter: HasQueryFilter(x => !x.IsDeleted)  |                    |
|       +---------------------------------------------+                    |
|                     :         :         :                                |
|        Optional     :         :         :  Optional                      |
|       (Guid? null)  :         :         :  (Guid? null)                  |
|                     :         : Optional                                 |
|                     :         : (Guid? null)                             |
|  +--------------------+  +--------------------+  +--------------------+  |
|  |ActiveHoyolabAccount|  |    CardSnapshot    |  |DailyReportThread   |  |
|  | HoyolabAccountId?  |  | HoyolabAccountId?  |  | HoyolabAccountId?  |  |
|  | IsRequired(false)  |  | IsRequired(false)  |  | IsRequired(false)  |  |
|  +--------------------+  +--------------------+  +--------------------+  |
|                                                                          |
|  Query translation: LEFT OUTER JOIN                                      |
|  EF Core validation status: 0 warnings (clean)                           |
+--------------------------------------------------------------------------+
```

## Risks / Trade-offs

- **[Risk] Nullable foreign keys in services expecting non-null `Guid`**  
  → **Mitigation**: Update signatures in `CardSnapshotService` and `ActiveHoyolabAccountService` where appropriate, ensuring callers check `.HasValue` or pass `Guid` when creating new entities.
- **[Risk] Active Aspire AppHost locking assemblies during build/migration**  
  → **Mitigation**: Follow repository guidance: stop the `main` resource (`aspire resource main stop`) before generating/updating migrations and building, then restart it (`aspire resource main start`) after completion.

## Migration Plan

1. Update domain entity definitions in `src/GitcgNetCord.MainApp/Entities/`.
2. Update entity configurations in `src/GitcgNetCord.MainApp/Infrastructure/AppDbContext.cs`.
3. Update services and helpers (`ActiveHoyolabAccountService`, `DiscordUser`, `CardSnapshotService`).
4. Stop `main` resource in Aspire if running to prevent file locks.
5. Run `dotnet ef migrations add MakeHoyolabAccountNavigationsOptional ...`.
6. Run `dotnet ef database update ...`.
7. Start `main` resource in Aspire.
8. Verify tests pass with `dotnet test`.
9. Inspect Aspire logs to confirm zero `Validation[10622]` warnings.
