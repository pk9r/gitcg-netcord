# Spec Delta: Hoyolab Account Management

## ADDED Requirements

### Requirement: Switch Active Hoyolab Account via Slash Command
The system SHALL provide a `/hoyolab switch` slash command allowing users to specify a linked Hoyolab account by UID using autocomplete and immediately designate it as the active account. All responses and error messages SHALL be rendered completely in English.

#### Scenario: Successful active account switch via slash command
- **GIVEN** a user has multiple linked accounts including UID `800123456`
- **WHEN** the user executes `/hoyolab switch account:800123456`
- **THEN** the system SHALL set `800123456` as the active account for the user in the database, and respond with an ephemeral confirmation message in English stating that the active account has been updated

#### Scenario: Switch to non-existent or unlinked UID
- **GIVEN** a user executes `/hoyolab switch account:999999999` where `999999999` is not linked to their profile
- **WHEN** the command is processed
- **THEN** the system SHALL reject the request with an ephemeral error message in English stating that the account was not found

#### Scenario: Invocation without linked accounts
- **GIVEN** a user with no linked Hoyolab accounts
- **WHEN** the user executes `/hoyolab switch`
- **THEN** the system SHALL reply with an ephemeral error message in English instructing the user to link an account first

## MODIFIED Requirements

### Requirement: Remove Hoyolab Account via Interactive Menu
The system SHALL provide an interactive removal option on the `/hoyolab list` overview message that allows users to select a linked Hoyolab account from a dropdown menu and requires explicit user confirmation before executing account deletion.

#### Scenario: User clicks remove button and selects an account
- **GIVEN** a user has one or more linked Hoyolab accounts displayed via `/hoyolab list`
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
The system SHALL provide a `/hoyolab remove` slash command allowing users to specify a linked Hoyolab account by UID and requiring explicit button confirmation before removing the account.

#### Scenario: Successful removal via slash command
- **GIVEN** a user has a linked Hoyolab account with UID `800123456`
- **WHEN** the user executes `/hoyolab remove account:800123456` and clicks the "Confirm Remove" button on the confirmation prompt
- **THEN** the system SHALL remove the account, revoke its credentials, and update the response with an ephemeral confirmation summary message in English

#### Scenario: User cancels removal via slash command prompt
- **GIVEN** a user was prompted to confirm removal after executing `/hoyolab remove`
- **WHEN** the user clicks the "Cancel" button
- **THEN** the system SHALL abort the removal operation, remove all button components, and display an ephemeral message in English stating that the account removal was cancelled

#### Scenario: Account not found or owned by another user
- **GIVEN** a user executes `/hoyolab remove account:999999999`
- **WHEN** the specified UID does not exist or does not belong to the user
- **THEN** the system SHALL return an ephemeral error message in English indicating that the account was not found

### Requirement: Add Hoyolab Account via Interactive Modal
The system SHALL parse credentials from modal submissions triggered by the "Add account" button on `/hoyolab list` or the `/hoyolab add` slash command, query the Hoyolab API, link the selected game character to the user's Discord profile, and present interactive action buttons prompting the user to enable daily statistics snapshots upon successful character selection. All embed descriptions, field values, button labels, and notices SHALL be rendered completely in English.

#### Scenario: User successfully submits Add Account modal
- **GIVEN** a user opens the "Add account" modal from `/hoyolab list` or `/hoyolab add`
- **WHEN** the user inputs valid Hoyolab user ID `ltuid_v2` and token `ltoken_v2` for an account with two or more Genshin Impact characters and submits the modal
- **THEN** the system SHALL extract the input values from the modal components, authenticate against the Hoyolab API without errors, cache retrieved game records, and display an ephemeral select menu in English prompting character selection, wherein each option corresponding to a character already linked to the user's Discord profile is annotated with `[Already Linked - Select to update token]`

#### Scenario: Character selection attaches daily snapshot onboarding prompt
- **GIVEN** a user selects a game character from the account selection menu
- **WHEN** the account is successfully saved or updated in the database
- **THEN** the system SHALL reply with an English confirmation embed accompanied by interactive buttons `[ Enable Daily Snapshot ]` and `[ No, Maybe Later ]`

#### Scenario: User submits modal with invalid or empty credentials
- **GIVEN** a user is viewing the "Add account" modal
- **WHEN** the user submits invalid credentials or the Hoyolab API rejects authentication
- **THEN** the system SHALL return an ephemeral error message in English describing the authentication failure without linking any account

#### Scenario: User submits credentials for account with exactly one Genshin character
- **GIVEN** a user submits valid Hoyolab credentials from `/hoyolab list` or `/hoyolab add`
- **WHEN** the Hoyolab API returns exactly one Genshin Impact character record
- **THEN** the system SHALL automatically link that character to the user's Discord profile without displaying an intermediate character selection menu, and SHALL respond with an ephemeral confirmation embed in English accompanied by interactive buttons `[ Enable Daily Snapshot ]` and `[ No, Maybe Later ]`

#### Scenario: User submits credentials for account with zero Genshin characters
- **GIVEN** a user submits valid Hoyolab credentials from `/hoyolab list` or `/hoyolab add`
- **WHEN** the Hoyolab API returns zero Genshin Impact character records
- **THEN** the system SHALL display an ephemeral message in English indicating that no Genshin Impact character records were found on the Hoyolab account without creating a select menu or linking any account

### Requirement: Update Active Hoyolab Account via Interactive Modal
The system SHALL parse the game UID from modal submissions triggered by the "Update active account" button on `/hoyolab list` and designate that linked account as the user's active account.

#### Scenario: User successfully updates active account via modal
- **GIVEN** a user has multiple linked accounts including UID `800123456`
- **WHEN** the user clicks "Update active account", enters `800123456` in the modal input, and submits
- **THEN** the system SHALL extract the UID from the modal components, set `800123456` as the active account in the database, and respond with an ephemeral confirmation message in English stating that the active account has been updated

#### Scenario: User submits non-existent UID in active account modal
- **GIVEN** a user submits the "Update active account" modal with UID `999999999` which is not linked to their profile
- **WHEN** the system attempts to update the active account
- **THEN** the system SHALL return an ephemeral error message in English indicating that the Hoyolab account with the specified UID was not found
