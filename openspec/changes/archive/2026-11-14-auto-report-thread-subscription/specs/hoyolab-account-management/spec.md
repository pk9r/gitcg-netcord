# Spec Delta

## MODIFIED Requirements

### Requirement: Add Hoyolab Account via Interactive Modal
The system SHALL parse credentials from modal submissions triggered by the "Add account" button on `/hoyolab-accounts`, query the Hoyolab API, link the selected game character to the user's Discord profile, and present interactive action buttons prompting the user to enable daily statistics snapshots upon successful character selection. All embed descriptions, field values, button labels, and notices SHALL be rendered completely in English.

#### Scenario: User successfully submits Add Account modal
- **GIVEN** a user opens the "Add account" modal from `/hoyolab-accounts`
- **WHEN** the user inputs valid Hoyolab user ID `ltuid_v2` and token `ltoken_v2` and submits the modal
- **THEN** the system SHALL extract the input values from the modal components, authenticate against the Hoyolab API without errors, cache retrieved game records, and display an ephemeral select menu in English prompting character selection

#### Scenario: Character selection attaches daily snapshot onboarding prompt
- **GIVEN** a user selects a game character from the account selection menu
- **WHEN** the account is successfully saved to the database
- **THEN** the system SHALL reply with an English confirmation embed accompanied by interactive buttons `[ Enable Daily Snapshot ]` and `[ No, Maybe Later ]`

#### Scenario: User submits modal with invalid or empty credentials
- **GIVEN** a user is viewing the "Add account" modal
- **WHEN** the user submits invalid credentials or the Hoyolab API rejects authentication
- **THEN** the system SHALL return an ephemeral error message in English describing the authentication failure without linking any account

## ADDED Requirements

### Requirement: Interactive Daily Snapshot and Destination Onboarding
The system SHALL process user button interactions on the post-account addition prompt, enabling daily snapshot capture when confirmed and presenting eligible report delivery destinations. All embed titles, descriptions, menu options, button labels, and messages SHALL be rendered completely in English.

#### Scenario: User clicks Enable Daily Snapshot button
- **GIVEN** a user is presented with the daily snapshot onboarding prompt for an account
- **WHEN** the user clicks the "Enable Daily Snapshot" button
- **THEN** the system SHALL set `IsDailySnapshotEnabled` to true for the account, update the response message with an English confirmation embed, and present interactive options to select report destinations (DM, available server channels, or skip)

#### Scenario: User clicks No Maybe Later button
- **GIVEN** a user is presented with the daily snapshot onboarding prompt for an account
- **WHEN** the user clicks the "No, Maybe Later" button
- **THEN** the system SHALL leave `IsDailySnapshotEnabled` as false, remove interactive buttons, and display an English notice indicating that daily snapshots remain disabled
