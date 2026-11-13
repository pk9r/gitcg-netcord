# Proposal

## Why

Currently, daily card snapshot capturing and daily report delivery to Discord threads only trigger automatically on a 15-minute timer via `DailySnapshotWorker` when crossing the game server's daily reset time (04:00 AM server time). When developing, debugging, or verifying report templates and thread deliveries, developers must wait or manually manipulate timestamps. Providing a local HTTP API endpoint allows instant on-demand snapshot refreshing and report delivery. Additionally, adopting Scalar (`Scalar.AspNetCore`) alongside ASP.NET Core OpenAPI provides an interactive and modern API documentation interface in local development without legacy Swagger dependencies.

## What Changes

- Introduce a shared `DailySnapshotProcessor` service that encapsulates the snapshot capturing, calculation, and thread report delivery logic currently located in `DailySnapshotWorker`.
- Expose a local HTTP Minimal API endpoint `POST /api/reports/daily/trigger` supporting optional parameters (`uid`, `forceUpdate`) to trigger snapshot capture and report delivery on demand.
- Support force-updating today's daily snapshot so that newly played matches are recalculated and reported even if a daily snapshot was already recorded for the current server day.
- Integrate `Scalar.AspNetCore` and `Microsoft.AspNetCore.OpenApi` for interactive API reference documentation accessible locally at `/scalar/v1`.
- Refactor `DailySnapshotWorker` to delegate its periodic snapshot tasks to `DailySnapshotProcessor`.

## Capabilities

### New Capabilities
<!-- None: Extending existing daily thread report capability -->

### Modified Capabilities
- `card-daily-thread-report`: Adds requirements for manual HTTP triggering of daily snapshots and reports, and interactive local API documentation via Scalar.

## Impact

- **Source Code**:
  - `src/GitcgNetCord.MainApp/Infrastructure/DailySnapshotWorker.cs`: Refactored to delegate to `DailySnapshotProcessor`.
  - `src/GitcgNetCord.MainApp/Infrastructure/DailySnapshotProcessor.cs`: New service implementing the extraction.
  - `src/GitcgNetCord.MainApp/Endpoints/DailyReportEndpoints.cs`: New endpoint mapping for Minimal APIs.
  - `src/GitcgNetCord.MainApp/Program.cs`: Register OpenAPI, Scalar, and endpoint routes.
- **Dependencies**:
  - `Directory.Packages.props`: Add `Scalar.AspNetCore` and `Microsoft.AspNetCore.OpenApi`.
  - `src/GitcgNetCord.MainApp/GitcgNetCord.MainApp.csproj`: Reference the packages.
- **Environment**:
  - Local HTTP endpoint and Scalar UI available during local development.
