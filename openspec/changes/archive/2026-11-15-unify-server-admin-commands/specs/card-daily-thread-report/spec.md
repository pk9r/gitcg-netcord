# Spec Delta: Card Daily Thread Report

## MODIFIED Requirements

### Requirement: Register and unregister parent report channels
The system SHALL allow guild administrators with guild management permissions to register, unregister, and list parent channels approved for daily thread reporting via `/server report-channel` commands. All command descriptions, parameter descriptions, embed titles, fields, notices, and response messages SHALL be rendered completely in English.

#### Scenario: Register approved parent channel
- **WHEN** an authorized user executes `/server report-channel add` with a specified channel or the current channel in a guild
- **THEN** the system registers the channel as an approved daily report parent channel and replies with an English confirmation message

#### Scenario: Unregister parent channel
- **WHEN** an authorized user executes `/server report-channel remove` with a registered channel
- **THEN** the system removes the channel from the approved parent channels and replies with an English confirmation message

#### Scenario: List registered parent channels
- **WHEN** an authorized user executes `/server report-channel list` in a guild
- **THEN** the system displays an English embed listing all registered parent channels for the current guild

#### Scenario: Non-admin invocation restriction
- **WHEN** a user without `ManageGuild` or `Administrator` permissions attempts to execute `/server report-channel` commands
- **THEN** the system prevents invocation or replies with an English error message indicating server management permissions are required

### Requirement: Automated Thread Creation and Subscription
The system SHALL automatically create a Discord thread within an approved parent channel and subscribe the user's Hoyolab account to daily reports when selected from the report destination picker. All thread names, initial messages, and notifications SHALL be rendered completely in English.

#### Scenario: Auto-create thread in approved channel
- **GIVEN** a user selects an approved parent channel in a guild to receive daily reports for a Hoyolab account
- **WHEN** the automated thread creation executes
- **THEN** the system SHALL create a public thread named `📊 TCG Daily - {Nickname} ({UID})`, post an initial welcome message mentioning the user, subscribe the account to the newly created thread, and return a link to the thread

#### Scenario: Reuse existing thread when account is already subscribed in guild
- **GIVEN** an account already has an active thread subscription in the target guild
- **WHEN** the user selects the guild for thread report setup
- **THEN** the system SHALL avoid creating a duplicate thread and return a message referencing the existing subscribed thread

#### Scenario: Rejection when target guild has no approved parent channels
- **GIVEN** a guild with no registered approved daily report channels
- **WHEN** automated thread creation is requested for that guild
- **THEN** the system SHALL reject the operation with an English message explaining that an administrator must first register an approved channel using `/server report-channel add`
