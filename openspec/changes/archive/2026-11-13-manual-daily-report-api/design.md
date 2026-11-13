# Design

## Context

See `proposal.md` for background and motivation. The system currently captures snapshots and sends Discord thread reports solely via `DailySnapshotWorker`, which runs as a `BackgroundService` every 15 minutes. The worker executes logic that checks for existing snapshots for the current server day, and if absent, fetches Hoyolab card statistics, computes totals, persists the snapshot via `CardSnapshotService`, and coordinates report delivery via `DailyReportDeliveryService`.

The project runs on .NET 10 (`net10.0`) with centralized package management (`Directory.Packages.props`). HTTP endpoints in `Program.cs` use ASP.NET Core Minimal APIs.

## Goals / Non-Goals

**Goals:**
- Extract the snapshot capture and report dispatch pipeline into a reusable service `DailySnapshotProcessor`.
- Support both automatic execution (worker skips if today's snapshot exists) and manual execution (API forces refresh and report resend).
- Expose `POST /api/reports/daily/trigger` allowing developers to trigger all accounts or a single account by `uid`.
- Provide interactive API reference documentation via Scalar (`Scalar.AspNetCore`) and `Microsoft.AspNetCore.OpenApi` at `/scalar/v1`.
- Keep `DailySnapshotWorker` functioning identically, delegating to the new processor.

**Non-Goals:**
- Implementing production authentication, tokens, or role-based access control (this endpoint is restricted to local/development usage).
- Changing the embed formatting, stats calculation formulas, or Discord thread delivery logic.
- Adding third-party heavy frameworks (e.g., FastEndpoints or Swagger/Swashbuckle).

## Decisions

### Decision 1: Service Extraction (`DailySnapshotProcessor`)
- **Choice:** Create `DailySnapshotProcessor` (scoped service) taking `AppDbContext`, `HoyolabHttpClientService`, `CardSnapshotService`, `DailyReportDeliveryService`, and `ILogger`.
- **Rationale:** The worker and the HTTP endpoint share the exact same orchestration steps: determining game date offset, querying active accounts, calling Hoyolab API, saving snapshot, and invoking report delivery. Extracting this keeps the code DRY and testable.
- **Alternatives Considered:**
  - Putting trigger logic directly inside `DailyReportEndpoints`: Leads to code duplication with `DailySnapshotWorker`.
  - Adding methods to `DailyReportDeliveryService`: Violates single responsibility principle as `DailyReportDeliveryService` focuses on formatting and posting Discord messages.

### Decision 2: OpenAPI & Interactive UI with Scalar
- **Choice:** Use `Microsoft.AspNetCore.OpenApi` for schema generation and `Scalar.AspNetCore` for the reference UI.
- **Rationale:** .NET 10 has native OpenAPI document generation (`builder.Services.AddOpenApi()`). `Scalar.AspNetCore` provides an integrated API test client, dark mode, code snippet generators, and high performance without the legacy Swashbuckle dependency.
- **Alternatives Considered:**
  - `Swashbuckle.AspNetCore`: Deprecated from default .NET templates since .NET 9.
  - Raw JSON endpoint without UI: Inconvenient for quick manual testing compared to Scalar's in-browser execution panel.

### Decision 3: Endpoint Organization via Route Groups
- **Choice:** Encapsulate endpoint definition in `Endpoints/DailyReportEndpoints.cs` using `app.MapGroup("/api/reports")`.
- **Rationale:** Keeps `Program.cs` clean and modular while enabling rich OpenAPI metadata annotations (`.WithSummary()`, `.WithDescription()`, `.Produces<T>()`).

### Decision 4: Snapshot Overwriting Semantics on Manual Trigger
- **Choice:** Pass `forceUpdate: bool` (default `true` for manual API, `false` for worker).
- **Behavior:** When `forceUpdate` is `true`, even if a snapshot named `daily-yyyyMMdd` exists, `CardSnapshotService.SaveSnapshotAsync` will overwrite the `CardData`, `TotalGames`, and `TotalWins` and update `CreatedAt` to `UtcNow`. This allows `DailyReportDeliveryService` to compare against earlier snapshots and broadcast an updated report for the day.

## Risks / Trade-offs

- **[Risk] Multiple manual triggers causes Discord rate limits or spam**
  → **Mitigation**: The API allows specifying an optional `uid` parameter so developers can test a single target account. When running for all accounts, the existing 1500ms throttle delay between Hoyolab calls is preserved.
- **[Risk] Package version compatibility with .NET 10**
  → **Mitigation**: Use compatible versions of `Microsoft.AspNetCore.OpenApi` (matching 10.0.x) and `Scalar.AspNetCore` in `Directory.Packages.props`.
