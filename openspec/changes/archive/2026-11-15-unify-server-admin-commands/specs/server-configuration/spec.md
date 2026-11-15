# Spec Delta: Server Configuration

## ADDED Requirements

### Requirement: View Server Configuration Overview
The system SHALL provide a `/server overview show` slash command displaying an English embed dashboard summarizing the server's invite URL, active card code channels, approved daily report channels, and administrative quick references. All titles, fields, and notices SHALL be rendered completely in English.

#### Scenario: View overview with configured settings
- **GIVEN** a user with `ManageGuild` or `Administrator` permissions in a Discord guild with configured settings
- **WHEN** the user executes `/server overview show`
- **THEN** the system SHALL display an English embed showing the server invite URL, the list of configured card code channels with their decode styles, the list of approved daily report channels, and quick admin command hints

#### Scenario: View overview with empty configuration
- **GIVEN** a user with `ManageGuild` or `Administrator` permissions in a Discord guild with no invite URL and no registered channels
- **WHEN** the user executes `/server overview show`
- **THEN** the system SHALL display an English embed indicating that each setting is not configured, along with command guidance on how to configure them

#### Scenario: Non-admin invocation restriction
- **GIVEN** a user without `ManageGuild` or `Administrator` permissions in a Discord guild
- **WHEN** the user attempts to execute `/server overview show`
- **THEN** the system SHALL reject the invocation with an English error message stating that server management permissions are required

#### Scenario: Invocation outside a guild
- **GIVEN** a user executes `/server overview show` within a Direct Message (DM)
- **WHEN** the command is processed
- **THEN** the system SHALL reject the invocation with an English error message stating that the command can only be used inside a server
