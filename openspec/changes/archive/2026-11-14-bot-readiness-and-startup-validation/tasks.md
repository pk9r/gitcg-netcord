# Tasks

## 1. Aspire AppHost & Endpoint Wiring

- [x] 1.1 Attach HTTP health probe `.WithHttpHealthCheck("/health")` to the `main` resource in `src/AppHost/AppHost.cs` and verify compilation with `dotnet build src/AppHost/AppHost.csproj`.
- [x] 1.2 Enable health check and aliveness endpoints by invoking `app.MapDefaultEndpoints()` in `src/GitcgNetCord.MainApp/Program.cs` and verify compilation with `dotnet build src/GitcgNetCord.MainApp/GitcgNetCord.MainApp.csproj`.

## 2. Bot State Tracking & Readiness Health Check

- [x] 2.1 Create `DiscordBotHealthTracker` in `src/GitcgNetCord.MainApp/Infrastructure/HealthChecks/DiscordBotHealthTracker.cs` tracking `IsReady`, `BotUsername`, `Latency`, and transition timestamps, and verify compilation.
- [x] 2.2 Create `DiscordBotReadyGatewayHandler` in `src/GitcgNetCord.MainApp/GatewayHandlers/DiscordBotReadyGatewayHandler.cs` implementing `IReadyGatewayHandler` to update the tracker upon the `READY` event, and subscribe to gateway disconnect events.
- [x] 2.3 Create `DiscordBotHealthCheck` (implementing `IHealthCheck`) in `src/GitcgNetCord.MainApp/Infrastructure/HealthChecks/DiscordBotHealthCheck.cs` returning `Unhealthy` when `IsReady` is false and `Healthy` with bot user and latency metadata when `IsReady` is true.
- [x] 2.4 Register `DiscordBotHealthTracker`, `DiscordBotReadyGatewayHandler`, and `DiscordBotHealthCheck` with tag `["ready"]` in `src/GitcgNetCord.MainApp/Extensions/Extensions.cs` and verify DI wiring builds cleanly.

## 3. Fail-Fast Startup Configuration Validation

- [x] 3.1 Add fail-fast validation for `Discord:Token` during bootstrapping in `src/GitcgNetCord.MainApp/Program.cs`, throwing an `InvalidOperationException` with actionable remediation guidance if the token is null, empty, or whitespace.

## 4. Verification & Testing

- [x] 4.1 Add unit tests in `tests/GitcgNetCord.MainApp.Tests/HealthChecks/DiscordBotHealthCheckTests.cs` covering unready, ready, and disconnected states.
- [x] 4.2 Add unit tests in `tests/GitcgNetCord.MainApp.Tests/Configuration/StartupValidationTests.cs` verifying exception throwing when `Discord:Token` is missing or whitespace.
- [x] 4.3 Execute `dotnet test` on target test projects and verify all tests pass without errors.
