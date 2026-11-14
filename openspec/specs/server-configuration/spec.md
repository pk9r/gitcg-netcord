# server-configuration Specification

## Purpose

Enables Discord server administrators to configure guild-level metadata including setting, viewing, and clearing a server invite URL used by the bot to facilitate community discovery and report subscriptions.

## Requirements

### Requirement: Configure Server Invite URL
The system SHALL provide a `/server invite set` slash command allowing guild administrators to configure a public invite URL for the server. All command descriptions, parameter descriptions, notices, and response messages SHALL be rendered completely in English.

#### Scenario: Successfully set server invite URL
- **GIVEN** a user with `ManageGuild` or `Administrator` permissions in a Discord guild
- **WHEN** the user executes `/server invite set url:https://discord.gg/example`
- **THEN** the system SHALL validate the URL format, persist the invite URL for the current guild in the database, and reply with an English confirmation message

#### Scenario: Set invite URL with invalid format
- **GIVEN** a user with `ManageGuild` permissions in a Discord guild
- **WHEN** the user executes `/server invite set url:not-a-valid-url`
- **THEN** the system SHALL reject the URL and reply with an English error message indicating that a valid HTTP or HTTPS Discord invite URL is required

#### Scenario: Non-admin invocation restriction
- **GIVEN** a user without `ManageGuild` or `Administrator` permissions in a Discord guild
- **WHEN** the user attempts to execute `/server invite set`
- **THEN** the system SHALL reject the invocation with an English error message stating that server management permissions are required

#### Scenario: Invocation outside a guild
- **GIVEN** a user executes `/server invite set` within a Direct Message (DM)
- **WHEN** the command is processed
- **THEN** the system SHALL reject the invocation with an English error message stating that the command can only be used inside a server

### Requirement: View and Remove Server Invite URL
The system SHALL allow guild administrators to view the current server invite URL or remove it. All responses, notices, and messages SHALL be rendered completely in English.

#### Scenario: View configured server invite URL
- **GIVEN** a guild has an invite URL configured
- **WHEN** an authorized user executes `/server invite view`
- **THEN** the system SHALL display an English embed showing the current server invite URL and last updated timestamp

#### Scenario: View when no invite URL is configured
- **GIVEN** a guild has no invite URL configured
- **WHEN** an authorized user executes `/server invite view`
- **THEN** the system SHALL reply with an English message stating that no invite URL is configured for this server

#### Scenario: Remove configured server invite URL
- **GIVEN** a guild has an invite URL configured
- **WHEN** an authorized user executes `/server invite remove`
- **THEN** the system SHALL remove the stored invite URL from the database and reply with an English confirmation message

#### Scenario: Remove when no invite URL exists
- **GIVEN** a guild has no invite URL configured
- **WHEN** an authorized user executes `/server invite remove`
- **THEN** the system SHALL reply with an English notice stating that no invite URL was set for this server
