# Tasks

## 1. Dependencies and OpenAPI/Scalar Setup

- [x] 1.1 Add `Microsoft.AspNetCore.OpenApi` and `Scalar.AspNetCore` packages to `Directory.Packages.props` and `src/GitcgNetCord.MainApp/GitcgNetCord.MainApp.csproj`, and verify `dotnet restore` succeeds.
- [x] 1.2 Configure `builder.Services.AddOpenApi()` and `app.MapOpenApi()`, `app.MapScalarApiReference()` in `Program.cs`, and verify the project builds without errors.

## 2. Core Service Refactoring

- [x] 2.1 Create `DailySnapshotProcessor` in `src/GitcgNetCord.MainApp/Infrastructure/DailySnapshotProcessor.cs` extracting snapshot calculation, persistence, and thread delivery with support for `uid` filtering and `forceUpdate`, and verify unit compilation.
- [x] 2.2 Register `DailySnapshotProcessor` in dependency injection via `src/GitcgNetCord.MainApp/Extensions/Extensions.cs`, and refactor `DailySnapshotWorker` to delegate execution to `DailySnapshotProcessor`. Verify project build.

## 3. Minimal API Endpoints

- [x] 3.1 Create DTO contracts (`DailyReportTriggerRequest`, `DailyReportTriggerResponse`, `AccountReportResult`) in `src/GitcgNetCord.MainApp/Models/DailyReportModels.cs`.
- [x] 3.2 Implement `DailyReportEndpoints` in `src/GitcgNetCord.MainApp/Endpoints/DailyReportEndpoints.cs` providing `POST /api/reports/daily/trigger` with OpenAPI summary, tags, and description.
- [x] 3.3 Map the report endpoints in `Program.cs` and verify with `dotnet build`.

## 4. Verification

- [x] 4.1 Run existing automated tests via `dotnet test` to confirm no regressions across `GitcgNetCord.MainApp.Tests`.
- [x] 4.2 Verify the change artifacts with `openspec validate manual-daily-report-api --strict`.
