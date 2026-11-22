# Tasks

## 1. Gateway Lifecycle Event Handler Implementation

- [x] 1.1 Update `DiscordBotReadyGatewayHandler` to implement `IResumeGatewayHandler` and `IDisconnectGatewayHandler`, restoring readiness on session resumption with preserved bot username and gateway latency, and marking unready on disconnect. Verify compilation succeeds with `dotnet build src/GitcgNetCord.MainApp/GitcgNetCord.MainApp.csproj`.

## 2. Unit Testing & Verification

- [x] 2.1 Add unit tests in `tests/GitcgNetCord.MainApp.Tests/HealthChecks/DiscordBotHealthCheckTests.cs` verifying Gateway resumption transitions the health tracker back to `Healthy` with retained bot identity, and disconnect transitions to `Unhealthy`. Verify test execution with `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj --filter FullyQualifiedName~HealthChecks`.
- [x] 2.2 Run and pass the complete xUnit test suite for `GitcgNetCord.MainApp.Tests` using `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj`.
