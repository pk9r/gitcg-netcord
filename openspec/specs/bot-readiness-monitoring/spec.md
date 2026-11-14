# bot-readiness-monitoring Specification

## Purpose

Provides comprehensive startup validation and real-time Discord Gateway health and readiness monitoring, enabling orchestration platforms like .NET Aspire to accurately determine application health.

## Requirements

### Requirement: Fail-Fast Startup Validation on Missing Bot Credentials
The application SHALL validate that required bot credentials (specifically the Discord bot token `Discord:Token`) are provided during bootstrapping. If the token is null, empty, or whitespace, the host startup MUST fail immediately by throwing an explicit exception to prevent running in a broken state.

#### Scenario: Missing or empty Discord token on startup
- **GIVEN** the application configuration lacks a valid `Discord:Token` value
- **WHEN** the application starts up
- **THEN** the application MUST abort immediately with an `InvalidOperationException` and the process exits with a non-zero exit code

#### Scenario: Valid Discord token provided on startup
- **GIVEN** the application configuration contains a valid `Discord:Token` value
- **WHEN** the application starts up
- **THEN** the application SHALL proceed with host construction and start background hosted services normally

### Requirement: Bot Readiness State Tracking
The system SHALL track the live Discord Gateway connectivity and readiness lifecycle. The readiness tracker MUST remain unready during initialization and SHALL transition to ready only after the Discord Gateway `READY` event has been received by the bot client.

#### Scenario: Initial state before Gateway handshake
- **GIVEN** the application has booted and Kestrel is listening
- **WHEN** the Discord Gateway connection handshake is in progress and `READY` has not yet fired
- **THEN** the readiness tracker MUST report that the bot is not ready

#### Scenario: Transition to ready upon Gateway READY event
- **GIVEN** the bot successfully establishes a WebSocket session with Discord Gateway
- **WHEN** the Gateway emits the `READY` event containing bot identity
- **THEN** the readiness tracker SHALL record the bot username, mark readiness as true, and capture gateway latency

#### Scenario: Gateway connection drop or disconnect
- **GIVEN** the bot was previously in a ready state
- **WHEN** the Discord Gateway connection drops or disconnects
- **THEN** the readiness tracker SHALL update readiness to false until reconnection and session resumption complete

### Requirement: Readiness and Liveness Health Check Probes
The system SHALL expose HTTP health check endpoints at `/health` for readiness and `/alive` for liveness. The `/health` endpoint MUST reflect the real-time bot readiness state as well as dependent service availability, returning HTTP 503 when unready and HTTP 200 when ready.

#### Scenario: Readiness probe while bot is connecting
- **GIVEN** the application is running but the Discord bot is not yet ready
- **WHEN** an HTTP GET request is received at `/health`
- **THEN** the endpoint MUST return HTTP status 503 (Service Unavailable) with an `Unhealthy` payload describing that the Discord bot is connecting

#### Scenario: Readiness probe after bot is ready
- **GIVEN** the bot is connected and ready on Discord Gateway
- **WHEN** an HTTP GET request is received at `/health`
- **THEN** the endpoint MUST return HTTP status 200 (OK) with a `Healthy` payload containing the bot username and latency

#### Scenario: Liveness probe during process lifetime
- **GIVEN** the web host process is active and responsive
- **WHEN** an HTTP GET request is received at `/alive`
- **THEN** the endpoint MUST return HTTP status 200 (OK) with a `Healthy` payload regardless of Discord Gateway connectivity state
