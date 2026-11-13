# Spec Delta

## MODIFIED Requirements

### Requirement: Remove Hoyolab Account via Interactive Menu
The system SHALL provide an interactive removal option on the `/hoyolab-accounts` overview message that allows users to select a linked Hoyolab account from a dropdown menu and requires explicit user confirmation before executing account deletion.

#### Scenario: User clicks remove button and selects an account
- **GIVEN** a user has one or more linked Hoyolab accounts displayed via `/hoyolab-accounts`
- **WHEN** the user clicks the "Remove account" button and chooses an account from the selection dropdown
- **THEN** the system SHALL display an ephemeral confirmation prompt in English showing the account UID, region, a warning about stopping daily automated report captures, and "Confirm Remove" and "Cancel" buttons without deleting the account yet

#### Scenario: User confirms account removal from interactive menu
- **GIVEN** a user is viewing the removal confirmation prompt for a selected Hoyolab account
- **WHEN** the user clicks the "Confirm Remove" button
- **THEN** the system SHALL remove the selected Hoyolab account, revoke its credentials, and update the message with an ephemeral removal summary in English including any active account reassignment details

#### Scenario: User cancels account removal from interactive menu
- **GIVEN** a user is viewing the removal confirmation prompt
- **WHEN** the user clicks the "Cancel" button
- **THEN** the system SHALL abort the removal operation, remove all button components, and display an ephemeral notice in English stating that the account removal was cancelled

#### Scenario: User clicks remove button with no accounts
- **GIVEN** a user has no linked Hoyolab accounts
- **WHEN** the user clicks the "Remove account" button
- **THEN** the system SHALL respond with an ephemeral message in English indicating that there are no accounts to remove

### Requirement: Remove Hoyolab Account via Slash Command
The system SHALL provide a `/remove-hoyolab-account` slash command allowing users to specify a linked Hoyolab account by UID and requiring explicit button confirmation before removing the account.

#### Scenario: Successful removal via slash command
- **GIVEN** a user has a linked Hoyolab account with UID `800123456`
- **WHEN** the user executes `/remove-hoyolab-account account:800123456` and clicks the "Confirm Remove" button on the confirmation prompt
- **THEN** the system SHALL remove the account, revoke its credentials, and update the response with an ephemeral confirmation summary message in English

#### Scenario: User cancels removal via slash command prompt
- **GIVEN** a user was prompted to confirm removal after executing `/remove-hoyolab-account`
- **WHEN** the user clicks the "Cancel" button
- **THEN** the system SHALL abort the removal operation, remove all button components, and display an ephemeral message in English stating that the account removal was cancelled

#### Scenario: Account not found or owned by another user
- **GIVEN** a user executes `/remove-hoyolab-account account:999999999`
- **WHEN** the specified UID does not exist or does not belong to the user
- **THEN** the system SHALL return an ephemeral error message in English indicating that the account was not found
