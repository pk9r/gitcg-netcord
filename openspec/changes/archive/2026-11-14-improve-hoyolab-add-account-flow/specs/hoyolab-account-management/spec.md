# Spec Delta

## MODIFIED Requirements

### Requirement: Add Hoyolab Account via Interactive Modal
The system SHALL parse credentials from modal submissions triggered by the "Add account" button on `/hoyolab-accounts`, query the Hoyolab API, link the selected game character to the user's Discord profile, and present interactive action buttons prompting the user to enable daily statistics snapshots upon successful character selection. All embed descriptions, field values, button labels, and notices SHALL be rendered completely in English.

#### Scenario: User successfully submits Add Account modal
- **GIVEN** a user opens the "Add account" modal from `/hoyolab-accounts`
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
- **GIVEN** a user submits valid Hoyolab credentials from `/hoyolab-accounts` or `/add-hoyolab-account`
- **WHEN** the Hoyolab API returns exactly one Genshin Impact character record
- **THEN** the system SHALL automatically link that character to the user's Discord profile without displaying an intermediate character selection menu, and SHALL respond with an ephemeral confirmation embed in English accompanied by interactive buttons `[ Enable Daily Snapshot ]` and `[ No, Maybe Later ]`

#### Scenario: User submits credentials for account with zero Genshin characters
- **GIVEN** a user submits valid Hoyolab credentials from `/hoyolab-accounts` or `/add-hoyolab-account`
- **WHEN** the Hoyolab API returns zero Genshin Impact character records
- **THEN** the system SHALL display an ephemeral message in English indicating that no Genshin Impact character records were found on the Hoyolab account without creating a select menu or linking any account
