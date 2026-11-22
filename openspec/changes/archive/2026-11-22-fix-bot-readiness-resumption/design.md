# Design

## Context

See [proposal.md](proposal.md) for background and motivation.

NetCord's hosting abstraction (`NetCord.Hosting.Gateway`) provides interface-based gateway event handlers registered via `AddGatewayHandler<T>()`. `DiscordBotReadyGatewayHandler` currently implements only `IReadyGatewayHandler` and subscribes to `_client.Disconnect` in its constructor:
- When the bot initially connects, Discord sends `READY`, which invokes `IReadyGatewayHandler.HandleAsync(ReadyEventArgs)` and marks the bot ready in `DiscordBotHealthTracker`.
- On connection drop, `_client.Disconnect` fires, marking the bot disconnected.
- When NetCord reconnects and resumes the session, Discord sends `RESUMED` (`Resume` event in NetCord). Because `IResumeGatewayHandler` is not implemented, the resumption is unhandled, leaving `DiscordBotHealthTracker.IsReady == false` indefinitely while the bot is actually active.

## Goals / Non-Goals

**Goals:**
- Transition `DiscordBotHealthTracker` back to `IsReady == true` whenever Discord Gateway session resumption (`RESUMED`) completes.
- Utilize NetCord's hosting interfaces (`IResumeGatewayHandler` and `IDisconnectGatewayHandler`) for clean lifecycle event handling.
- Preserve known bot identity (`_tracker.BotUsername` or `_client.User?.Username`) and update `Latency` when resuming.
- Validate lifecycle transitions through automated unit tests.

**Non-Goals:**
- Modifying Aspire AppHost health check configuration or interval.
- Modifying Discord Gateway connection settings or intents.

## Decisions

### Decision 1: Implement `IResumeGatewayHandler` on `DiscordBotReadyGatewayHandler`
- **Choice**: Implement `IResumeGatewayHandler` alongside `IReadyGatewayHandler` on `DiscordBotReadyGatewayHandler`.
- **Rationale**: `AddGatewayHandler<T>()` in NetCord scans implemented `IGatewayHandler` interfaces on `T` and registers metadata for each event type. Placing `Ready` and `Resume` in the same handler ensures cohesive tracking of Gateway readiness without creating redundant classes or extra service registrations.
- **Alternatives Considered**:
  - *Create a separate `DiscordBotResumeGatewayHandler`*: Unnecessary class proliferation; both handlers would need injected `DiscordBotHealthTracker` and `GatewayClient`.
  - *Manual event subscription (`_client.Resume += ...` in constructor)*: Bypasses NetCord's hosting pipeline and DI scope conventions for gateway event handlers.

### Decision 2: Implement `IDisconnectGatewayHandler` vs Constructor Delegate
- **Choice**: Implement `IDisconnectGatewayHandler` on `DiscordBotReadyGatewayHandler` and remove manual `_client.Disconnect += OnDisconnectAsync` constructor wiring.
- **Rationale**: Keeps all gateway lifecycle event handling unified across NetCord's hosting interfaces (`IReadyGatewayHandler`, `IResumeGatewayHandler`, `IDisconnectGatewayHandler`).
- **Alternatives Considered**:
  - *Keep constructor subscription*: Works, but creates mixed paradigms (interface-based for `Ready`, manual delegate for `Disconnect`).

### Decision 3: Bot Username Resolution on Resumption
- **Choice**: Retrieve the bot username with fallback: `_tracker.BotUsername ?? _client.User?.Username ?? "Unknown"`.
- **Rationale**: During a session resumption, the Gateway does not send the user object again (that is only sent in `READY`). The tracker already holds `BotUsername` from the initial `READY` handshake, and `_client.User` is cached by NetCord.

## Risks / Trade-offs

- **[Risk] Latency may be zero or unmeasured immediately upon resumption** → Mitigation: `GatewayClient.Latency` remains accessible and `SetReady` accepts `TimeSpan? latency`; `LatencyUpdate` events continue to refresh it.
- **[Risk] Disconnect event might fire during graceful shutdown** → Mitigation: Marking unready on disconnect is expected behavior during shutdown.
