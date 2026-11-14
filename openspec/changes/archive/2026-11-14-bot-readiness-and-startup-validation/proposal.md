# Proposal

## Why

Currently, when starting the GitcgNetCord application via .NET Aspire, the AppHost marks the `main` service resource as `Running` and `Healthy` as soon as the OS process starts, even if required configuration (such as the Discord bot token) is missing or while the Discord Gateway client is still attempting to connect in the background. Furthermore, `MapDefaultEndpoints()` is never invoked in `Program.cs`, meaning the Aspire orchestration has no visibility into the true readiness of the bot. This creates a misleading developer experience where the dashboard reports all systems nominal while the bot is actually non-functional or offline.

## What Changes

- **Fail-Fast Startup Configuration Validation**: Validate essential runtime configuration (specifically the Discord bot token) during application bootstrap. If required configuration is missing or invalid, fail immediately with an explicit exception so the process terminates and Aspire marks the resource state as `Failed` / `Finished`.
- **Bot Readiness State Tracking**: Introduce a centralized state tracker (`DiscordBotHealthTracker`) and an `IReadyGatewayHandler` implementation to monitor the active connection state of NetCord's `GatewayClient` (connected, ready, bot user, latency, disconnected).
- **Discord Bot Health Check**: Implement a custom ASP.NET Core `IHealthCheck` (`DiscordBotHealthCheck`) reporting `Unhealthy` while the bot is disconnected or negotiating connection, and `Healthy` only when the bot has received the `READY` event from Discord Gateway.
- **Expose Health Check Endpoints**: Wire up `app.MapDefaultEndpoints()` in `GitcgNetCord.MainApp/Program.cs` to expose `/health` (readiness) and `/alive` (liveness).
- **Aspire AppHost Health Probe Integration**: Attach `.WithHttpHealthCheck("/health")` to the `main` project resource in `AppHost.cs` so Aspire accurately reports `Starting`, `Unhealthy`, or `Healthy` based on actual bot connectivity.

## Capabilities

### New Capabilities

- `bot-readiness-monitoring`: Manages startup validation for essential bot credentials and exposes Discord Gateway readiness and liveness health status to Aspire AppHost orchestration.

### Modified Capabilities

None.

## In-Scope and Out-of-Scope

### In-Scope
- Enforcing fail-fast startup behavior when required bot environment variables/configuration (e.g., Discord token) are missing.
- Tracking NetCord GatewayClient lifecycle events (connecting, ready, disconnected).
- Exposing standard `/health` and `/alive` endpoints in `GitcgNetCord.MainApp`.
- Adding HTTP health probe configuration to `AppHost.cs` for the `main` resource.
- Unit and integration tests verifying health check results across different bot lifecycle states.

### Out-of-Scope
- Changing Hoyolab API authentication or credentials verification during startup.
- Modifying database or Redis startup wait behavior beyond existing Aspire `WaitFor` dependencies.
- Discord Gateway sharding lifecycle management (the bot currently uses single gateway client).

## SemVer Impact & Breaking Changes

- **SemVer Impact**: MINOR
- **Breaking Changes**: None. This is a non-breaking operational and observability enhancement. However, environments that previously ran without a valid `Discord:Token` will now fail fast on startup instead of idling in an unready state.

## Infrastructure Requirements

- No new external infrastructure (databases, caches, or cloud services) is required.
- Utilizes built-in ASP.NET Core Health Checks and .NET Aspire 13.x resource health probe capabilities (`WithHttpHealthCheck`).

## Impact

- `src/AppHost/AppHost.cs`: Attaches `.WithHttpHealthCheck("/health")` to `main`.
- `src/GitcgNetCord.MainApp/Program.cs`: Adds fail-fast validation for `Discord:Token` and calls `app.MapDefaultEndpoints()`.
- `src/GitcgNetCord.MainApp/Extensions/Extensions.cs`: Registers bot health check services, tracker, and gateway ready handler.
- `src/GitcgNetCord.MainApp/Infrastructure/HealthChecks/`: New health check implementation and state tracker.
- `tests/GitcgNetCord.MainApp.Tests/`: Unit tests for the bot health check and startup validation logic.
