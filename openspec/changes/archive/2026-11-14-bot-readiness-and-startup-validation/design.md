# Design

## Context

See `proposal.md` for problem motivation and background.

Currently:
1. `src/AppHost/AppHost.cs` registers `builder.AddProject<GitcgNetCord_MainApp>("main")` without any health check probes (`.WithHttpHealthCheck(...)`). Aspire determines status solely by OS process existence.
2. `src/GitcgNetCord.MainApp/Program.cs` calls `builder.AddServiceDefaults()`, but never calls `app.MapDefaultEndpoints()`. Even though `ServiceDefaults` defines `/health` and `/alive`, those routes are never mapped in the HTTP pipeline.
3. The default health checks in `ServiceDefaults` only register a trivial check `"self"` that unconditionally returns `Healthy`.
4. NetCord's `GatewayClient` starts asynchronously as a hosted background service. If `Discord:Token` is missing or invalid, the WebApplication still binds Kestrel and runs, while gateway connection attempts fail in the background.

## Goals / Non-Goals

**Goals:**
- Provide a fail-fast startup validator that ensures `Discord:Token` (or configured credentials) is non-empty before the host starts serving traffic, crashing immediately with an informative message if missing.
- Implement a dedicated `DiscordBotHealthTracker` and `DiscordBotHealthCheck` (implementing `IHealthCheck`) reflecting true Discord Gateway readiness (verifying that the gateway connection is active and the `READY` event has been handled).
- Expose `/health` (readiness probe) and `/alive` (liveness probe) endpoints via `app.MapDefaultEndpoints()`.
- Configure Aspire AppHost to probe `/health` using `.WithHttpHealthCheck("/health")`, transitioning the Aspire Dashboard state from `Starting` -> `Healthy` upon real Discord connectivity, and showing `Failed` if startup validation fails.

**Non-Goals:**
- Validating external Hoyolab API tokens on bot startup (Hoyolab tokens are managed on a per-user basis in the database).
- Modifying Postgres or Redis health checks already managed by Aspire's `AddNpgsqlDbContext` and `AddRedisDistributedCache`.
- Discord Gateway sharding support (current deployment uses a single gateway connection).

## Decisions

### Decision 1: Fail-Fast Startup Validation in Bootstrapping Phase
- **Choice**: Validate `builder.Configuration["Discord:Token"]` (and/or `options.Token`) during service configuration or immediately prior to building the application. If missing or whitespace, throw `InvalidOperationException("Discord bot token is missing. Please configure 'Discord:Token' via environment variables, user secrets, or Azure Key Vault.")`.
- **Rationale**: If the token is missing, the bot cannot perform its primary function. Allowing Kestrel to start while the bot is silently broken misleads monitoring tools and developers. Exiting with a non-zero exit code immediately turns the Aspire resource status to `Failed` (Red).
- **Alternatives Considered**:
  - *Fallback to idle mode without throwing*: Rejected because the user explicitly requested fail-fast behavior, and Aspire would otherwise show `Running`.

### Decision 2: State Tracking via `DiscordBotHealthTracker` & `IReadyGatewayHandler`
- **Choice**: Introduce a singleton `DiscordBotHealthTracker` service that records:
  - `IsReady` (bool)
  - `BotUsername` (string?)
  - `ReadyAt` (DateTimeOffset?)
  - `LastDisconnectAt` (DateTimeOffset?)
  Implement `IReadyGatewayHandler` (`DiscordBotReadyGatewayHandler`) to set `IsReady = true` when Discord dispatches the `READY` event. Additionally, subscribe to `GatewayClient.Disconnect` (or gateway lifecycle events) to mark `IsReady = false` upon disconnect.
- **Rationale**: Decouples the health check evaluation from the internal state of NetCord, providing thread-safe, lock-free queries (`volatile` or atomic flags) for the ASP.NET Core health check runner.
- **Alternatives Considered**:
  - *Directly checking `GatewayClient.User != null` inside `IHealthCheck`*: Possible, but inspecting `User` alone does not reliably capture disconnection/reconnection cycles without state tracking.

### Decision 3: Custom `DiscordBotHealthCheck` Registered in `ServiceDefaults` / `MainApp`
- **Choice**: Implement `DiscordBotHealthCheck : IHealthCheck`.
  - When `tracker.IsReady == false`: Return `HealthCheckResult.Unhealthy("Discord Gateway client is not ready.")` with diagnostic metadata.
  - When `tracker.IsReady == true`: Return `HealthCheckResult.Healthy($"Online as {tracker.BotUsername}")` including gateway latency.
  Register this check with tag `["ready"]`.
- **Rationale**: Allows ASP.NET Core's `/health` endpoint to evaluate both system health and bot readiness, while keeping `/alive` scoped to basic process liveness (`["live"]`).

### Decision 4: Activating `MapDefaultEndpoints()` in `Program.cs`
- **Choice**: Call `app.MapDefaultEndpoints()` right before `app.Run()` in `GitcgNetCord.MainApp/Program.cs`.
- **Rationale**: Activates `/health` and `/alive` as defined in `ServiceDefaults/Extensions.cs`.

### Decision 5: Aspire Resource Probe via `.WithHttpHealthCheck("/health")`
- **Choice**: In `src/AppHost/AppHost.cs`, append `.WithHttpHealthCheck("/health")` to the `main` resource definition:
  ```csharp
  var main = builder
      .AddProject<GitcgNetCord_MainApp>("main")
      .WithHttpHealthCheck("/health");
  ```
- **Rationale**: In Aspire 13.x, `WithHttpHealthCheck` registers an HTTP polling health check against the project's endpoint. Aspire Dashboard displays the resource as `Starting` until `/health` returns 200 OK.

## Architectural Flow

```
+-------------------------------------------------------------------------------+
|                            SYSTEM DESIGN TOPOLOGY                             |
+-------------------------------------------------------------------------------+

 [Aspire AppHost]
        |
        | Probes GET /health every N seconds
        v
 [GitcgNetCord.MainApp (Kestrel Server)]
        |
        +---> /alive  --> Checks "self" (Liveness)
        |
        +---> /health --> Evaluates:
                            1. DB / Redis connectivity (ServiceDefaults)
                            2. DiscordBotHealthCheck
                                    |
                                    v
                         [DiscordBotHealthTracker]
                                    ^
                                    | Updates state
                         [DiscordBotReadyGatewayHandler]
                                    ^
                                    | Dispatches READY
                         [NetCord GatewayClient] <=======> [Discord Gateway]
```

## Risks / Trade-offs

- **[Risk] Slower Startup State Transition**:
  - *Details*: Aspire will display the resource as `Starting` (yellow) for 1-3 seconds after launch until Discord Gateway finishes the handshake.
  - *Mitigation*: This is the intended behavior. The dashboard provides transparent visibility into what is actually happening.
- **[Risk] Flapping Health on Network Glitches**:
  - *Details*: If the bot temporarily loses connection to Discord Gateway, `/health` will return 503 until reconnected.
  - *Mitigation*: Aspire and container orchestrators (like Kubernetes or Azure Container Apps) tolerate transient unhealthy states when configured with appropriate failure thresholds.
- **[Risk] Test Environments Without Real Discord Token**:
  - *Details*: Unit and integration tests that spin up `WebApplicationFactory` might fail if `Discord:Token` is strictly validated.
  - *Mitigation*: In test fixtures, inject a dummy token or bypass gateway connection.

## Migration Plan

1. Create `DiscordBotHealthTracker` and `DiscordBotReadyGatewayHandler` in `src/GitcgNetCord.MainApp/Infrastructure/HealthChecks/`.
2. Register the tracker, handler, and `DiscordBotHealthCheck` in `Extensions.cs`.
3. Add fail-fast token check and `app.MapDefaultEndpoints()` in `Program.cs`.
4. Update `AppHost.cs` to add `.WithHttpHealthCheck("/health")`.
5. Add unit tests in `GitcgNetCord.MainApp.Tests` to verify validation and health check reporting.
