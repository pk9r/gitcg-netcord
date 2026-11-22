# Spec Delta

## MODIFIED Requirements

### Requirement: Bot Readiness State Tracking
The system SHALL track the live Discord Gateway connectivity and readiness lifecycle. The readiness tracker MUST remain unready during initialization and SHALL transition to ready after the Discord Gateway `READY` event is received or after a session resumption (`RESUMED`) completes.

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

#### Scenario: Transition to ready upon Gateway session resumption
- **GIVEN** the bot was marked unready due to a Gateway connection drop or disconnect
- **WHEN** the Discord Gateway session is successfully resumed and the Gateway emits the `RESUMED` event
- **THEN** the readiness tracker SHALL transition readiness to true, retaining the authenticated bot username and updating gateway latency
