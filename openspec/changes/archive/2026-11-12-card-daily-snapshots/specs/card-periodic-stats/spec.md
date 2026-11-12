# Spec Delta

## ADDED Requirements

### Requirement: Periodic daily snapshot capture for opt-in accounts
The system SHALL periodically check and capture daily card statistics snapshots for active Hoyolab accounts that have opted into daily snapshots, timed according to each account's game server daily reset.

#### Scenario: Scheduled daily snapshot capture after server reset
- **WHEN** the current time passes the game server reset time (04:00 AM server time) for an active account with daily snapshots enabled and no snapshot exists for that server date
- **THEN** the system fetches the account's card statistics and persists a snapshot tagged as Daily named `daily-yyyyMMdd`

#### Scenario: Skip duplicate daily capture
- **WHEN** an account already has a snapshot recorded with the name `daily-yyyyMMdd` for the current server date
- **THEN** the system skips capturing for that date

### Requirement: User daily snapshot configuration
The system SHALL allow users to enable or disable automatic daily snapshot capture for their currently active Hoyolab account.

#### Scenario: User enables daily snapshots
- **WHEN** a user executes `/card-snapshot daily enabled:True` with an active Hoyolab account
- **THEN** the system enables daily snapshot capture for that account and confirms with a success message

#### Scenario: User disables daily snapshots
- **WHEN** a user executes `/card-snapshot daily enabled:False` with an active Hoyolab account
- **THEN** the system disables daily snapshot capture for that account and confirms with a success message

## MODIFIED Requirements

### Requirement: User custom card snapshots
The system SHALL allow users to manually capture, list, and delete custom named snapshots of their current card statistics.

#### Scenario: User creates custom snapshot
- **WHEN** a user executes `/card-snapshot save` with a custom name
- **THEN** the system retrieves the user's current Hoyolab card stats and persists a new snapshot tagged with the provided name

#### Scenario: User lists snapshots
- **WHEN** a user executes `/card-snapshot list`
- **THEN** the system displays available version and custom snapshots for their active account with creation dates and displays the count of daily snapshots in the footer

#### Scenario: User deletes custom snapshot
- **WHEN** a user executes `/card-snapshot delete` with a valid custom snapshot name
- **THEN** the system removes the snapshot from the database and confirms deletion

#### Scenario: User attempts to delete non-custom snapshot
- **WHEN** a user executes `/card-snapshot delete` with a daily or game version snapshot name
- **THEN** the system rejects the deletion with an error message stating that only custom snapshots can be deleted

### Requirement: Snapshot autocomplete support
The system SHALL provide autocomplete suggestions for version and snapshot parameters in the `/card` command using English labels.

#### Scenario: Autocomplete suggestions for range parameters
- **WHEN** a user focuses or types into the `from`, `to`, or `version` parameters in `/card`
- **THEN** the system returns matching game versions (labeled `Version <X.Y>`), user custom snapshot names, and the "Present (Live)" option where applicable

#### Scenario: Smart filter for daily snapshots in autocomplete
- **WHEN** a user types a query matching "daily" or date patterns into `from` or `to` parameters
- **THEN** the system includes matching daily snapshots in autocomplete suggestions, but excludes them when the input is empty or does not match daily patterns
