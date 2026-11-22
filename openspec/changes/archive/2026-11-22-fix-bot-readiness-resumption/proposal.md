# Proposal

## Why

When the Discord Gateway connection temporarily disconnects and successfully resumes (emitting a `RESUMED` / `Resume` event rather than a fresh `READY` event), the application does not restore the bot's readiness state. As a result, `DiscordBotHealthTracker.IsReady` remains false indefinitely despite the bot being fully functional and processing commands, causing Aspire's `/health` probe to continuously fail with HTTP 503 and flood logs with error entries.

## What Changes

- Update `DiscordBotReadyGatewayHandler` to listen for Discord Gateway session resumption events by implementing `IResumeGatewayHandler` (or subscribing to `GatewayClient.Resume`).
- Update `DiscordBotReadyGatewayHandler` to implement `IDisconnectGatewayHandler` (or maintain `GatewayClient.Disconnect`) to cleanly set disconnected state on connection drops.
- When session resumption occurs, restore `DiscordBotHealthTracker.IsReady` to true with preserved bot identity and updated gateway latency.
- Add unit tests verifying that session resumption transitions the health tracker back to ready and that health checks return `Healthy`.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `bot-readiness-monitoring`: Update requirement `Bot Readiness State Tracking` to specify the transition back to ready upon Gateway session resumption (`RESUMED`).

## Impact

- Affected Code:
  - `src/GitcgNetCord.MainApp/GatewayHandlers/DiscordBotReadyGatewayHandler.cs`
  - `src/GitcgNetCord.MainApp/Extensions/Extensions.cs` (if registration adjustments are needed)
  - `tests/GitcgNetCord.MainApp.Tests/HealthChecks/DiscordBotHealthCheckTests.cs`
- Infrastructure: No new databases, caches, or Aspire AppHost resources required.
- External APIs/Compatibility: No breaking changes to Discord commands or public endpoints.
