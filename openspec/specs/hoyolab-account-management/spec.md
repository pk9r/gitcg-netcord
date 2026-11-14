# hoyolab-account-management Specification

## Purpose

Enables Discord users to manage their linked Hoyolab accounts, including removing accounts via interactive UI menus or slash commands, handling active account fallback transitions, terminating daily report deliveries, and preserving historical statistics snapshots.

## Requirements

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

### Requirement: Active Account Fallback and Reassignment
When a removed Hoyolab account was currently set as the user's active account, the system SHALL automatically update the active account state according to the user's remaining accounts.

#### Scenario: Active account removed with no remaining accounts
- **GIVEN** a user has only one linked Hoyolab account which is active
- **WHEN** the user removes that account
- **THEN** the system SHALL clear the user's active account record and notify the user in English that no linked accounts remain

#### Scenario: Active account removed with exactly one remaining account
- **GIVEN** a user has two linked Hoyolab accounts (Account A active, Account B inactive)
- **WHEN** the user removes Account A
- **THEN** the system SHALL automatically set Account B as the new active account and state in the confirmation message in English that Account B is now active

#### Scenario: Active account removed with two or more remaining accounts
- **GIVEN** a user has three linked Hoyolab accounts (Account A active, Account B and Account C inactive)
- **WHEN** the user removes Account A
- **THEN** the system SHALL designate the first remaining account (Account B) as active, notify the user in English, and attach an interactive select menu in the response allowing immediate selection of an alternate active account

### Requirement: Snapshot Preservation and Subscription Cleanup
When a Hoyolab account is removed, the system SHALL preserve all existing card statistics snapshots associated with that account while terminating all recurring daily snapshot captures and daily report thread subscriptions.

#### Scenario: Removing account cleans up subscriptions and stops daily captures
- **GIVEN** a linked Hoyolab account with daily snapshots enabled and active thread report subscriptions
- **WHEN** the user removes the Hoyolab account
- **THEN** the system SHALL set `IsDailySnapshotEnabled` to false, delete or disable its `DiscordDailyReportThread` subscriptions, clear sensitive auth tokens, and preserve all associated `CardSnapshot` records in the database

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
