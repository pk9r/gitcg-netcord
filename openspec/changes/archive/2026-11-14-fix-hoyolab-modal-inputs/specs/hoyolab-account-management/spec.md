# Spec Delta

## ADDED Requirements

### Requirement: Add Hoyolab Account via Interactive Modal
The system SHALL parse credentials from modal submissions triggered by the "Add account" button on `/hoyolab-accounts`, querying the Hoyolab API and linking the selected game character to the user's Discord profile.

#### Scenario: User successfully submits Add Account modal
- **GIVEN** a user opens the "Add account" modal from `/hoyolab-accounts`
- **WHEN** the user inputs valid Hoyolab user ID `ltuid_v2` and token `ltoken_v2` and submits the modal
- **THEN** the system SHALL extract the input values from the modal components, authenticate against the Hoyolab API without 10001 retcode errors, cache retrieved game records, and display an ephemeral select menu in English prompting character selection

#### Scenario: User submits modal with invalid or empty credentials
- **GIVEN** a user is viewing the "Add account" modal
- **WHEN** the user submits invalid credentials or the Hoyolab API rejects authentication
- **THEN** the system SHALL return an ephemeral error message in English describing the authentication failure without linking any account

### Requirement: Update Active Hoyolab Account via Interactive Modal
The system SHALL parse the game UID from modal submissions triggered by the "Update active account" button on `/hoyolab-accounts` and designate that linked account as the user's active account.

#### Scenario: User successfully updates active account via modal
- **GIVEN** a user has multiple linked accounts including UID `800123456`
- **WHEN** the user clicks "Update active account", enters `800123456` in the modal input, and submits
- **THEN** the system SHALL extract the UID from the modal components, set `800123456` as the active account in the database, and respond with an ephemeral confirmation message in English stating that the active account has been updated

#### Scenario: User submits non-existent UID in active account modal
- **GIVEN** a user submits the "Update active account" modal with UID `999999999` which is not linked to their profile
- **WHEN** the system attempts to update the active account
- **THEN** the system SHALL return an ephemeral error message in English indicating that the Hoyolab account with the specified UID was not found
