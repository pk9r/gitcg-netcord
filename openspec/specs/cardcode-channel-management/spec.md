# Cardcode Channel Management Specification

## Purpose

Enables Discord server administrators to configure and manage channels where Genius Invokation TCG card codes are recognized and decoded automatically.

## Requirements

### Requirement: Register Card Code Channel
The system SHALL provide a command to register a Discord channel for card code decoding within the current server, optionally setting the deck visual style.

#### Scenario: Register current channel with default settings
- **WHEN** an authorized user executes `/cardcode-channel add` without parameters
- **THEN** the system registers the current channel with `ImageGameBackground` decode style and sends an ephemeral confirmation message

#### Scenario: Register specific channel and custom decode style
- **WHEN** an authorized user executes `/cardcode-channel add` providing a target channel and a decode type
- **THEN** the system stores the specified channel ID, guild ID, and decode type, confirming completion ephemerally

#### Scenario: Update an existing registered channel
- **WHEN** an authorized user executes `/cardcode-channel add` for a channel that is already registered
- **THEN** the system updates the channel's decode type to the new selection and confirms the update

### Requirement: Unregister Card Code Channel
The system SHALL provide a command to unregister a channel so that card codes posted in it are no longer processed.

#### Scenario: Unregister an active channel
- **WHEN** an authorized user executes `/cardcode-channel remove` for a registered channel
- **THEN** the system removes the channel from the database and confirms deletion ephemerally

#### Scenario: Unregister a channel that is not registered
- **WHEN** an authorized user executes `/cardcode-channel remove` for an unregistered channel
- **THEN** the system informs the user ephemerally that the channel was not registered

### Requirement: List Registered Card Code Channels
The system SHALL allow authorized users to view all card code channels configured for the current Discord server.

#### Scenario: List when channels are registered
- **WHEN** an authorized user executes `/cardcode-channel list` on a server with registered channels
- **THEN** the system displays an ephemeral embed listing each registered channel and its configured decode style

#### Scenario: List when no channels are registered
- **WHEN** an authorized user executes `/cardcode-channel list` on a server with no registered channels
- **THEN** the system replies ephemerally indicating that no channels are currently configured for card code processing

### Requirement: Permission and Context Restrictions
The system SHALL restrict execution of card code channel management commands to guild environments and users holding channel management privileges.

#### Scenario: Execution outside a guild
- **WHEN** a user attempts to execute `/cardcode-channel` commands in direct messages
- **THEN** Discord rejects the execution because the command is restricted to guild contexts

#### Scenario: Execution without ManageChannels permission
- **WHEN** a user without `ManageChannels` permission attempts to invoke `/cardcode-channel`
- **THEN** Discord prevents invocation based on default guild user permission restrictions

