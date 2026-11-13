# Spec Delta

## Purpose

Enables Discord users to manage their linked Hoyolab accounts, including removing accounts via interactive UI menus or slash commands, handling active account fallback transitions, terminating daily report deliveries, and preserving historical statistics snapshots.

## ADDED Requirements

### Requirement: Remove Hoyolab Account via Interactive Menu
The system SHALL provide an interactive removal option on the `/hoyolab-accounts` overview message that allows users to select and remove a linked Hoyolab account from a dropdown menu.

#### Scenario: User clicks remove button and selects an account
- **GIVEN** a user has one or more linked Hoyolab accounts displayed via `/hoyolab-accounts`
- **WHEN** the user clicks the "Remove account" button and chooses an account from the selection dropdown
- **THEN** the system SHALL remove the selected Hoyolab account, revoke its credentials, and send an ephemeral confirmation message in English

#### Scenario: User clicks remove button with no accounts
- **GIVEN** a user has no linked Hoyolab accounts
- **WHEN** the user clicks the "Remove account" button
- **THEN** the system SHALL respond with an ephemeral message in English indicating that there are no accounts to remove

### Requirement: Remove Hoyolab Account via Slash Command
The system SHALL provide a `/remove-hoyolab-account` slash command allowing users to remove a linked Hoyolab account by its Genshin Impact UID with autocomplete support.

#### Scenario: Successful removal via slash command
- **GIVEN** a user has a linked Hoyolab account with UID `800123456`
- **WHEN** the user executes `/remove-hoyolab-account account:800123456`
- **THEN** the system SHALL remove the account and respond with an ephemeral confirmation message in English

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
