# card-periodic-stats Specification

## Purpose

Enables tracking and displaying Genius Invokation TCG card statistics over specific time intervals, including game versions and user-created snapshot milestones.

## Requirements

### Requirement: Periodic snapshot capture for game versions
The system SHALL maintain a catalog of game versions and automatically capture a card statistics snapshot for all active Hoyolab accounts upon the release of each new version.

#### Scenario: Scheduled version release snapshot
- **WHEN** the current time reaches or exceeds a game version's release date and that version has not been snapshotted
- **THEN** the system fetches card statistics for all active accounts with rate limiting and saves each account's snapshot tagged with the version number

#### Scenario: Account failure resilience during batch capture
- **WHEN** fetching stats for an individual account fails due to expired tokens, network errors, or API rate limits
- **THEN** the system logs the failure, marks the account error, and continues processing remaining accounts without aborting the batch

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

### Requirement: Time-range statistics display in card command
The system SHALL calculate and display delta card statistics between two points in time when `version`, `from`, or `to` parameters are specified in the `/card` command. All embeds, field headers, notices, and messages SHALL be rendered completely in English.

#### Scenario: View statistics for a single game version
- **WHEN** a user runs `/card` with `version: "4.7"`
- **THEN** the system retrieves the snapshot at version 4.7 as start and version 4.8 as end, computes the difference in use counts and wins, and renders the periodic embed in English

#### Scenario: View statistics from a snapshot to current time
- **WHEN** a user runs `/card` with `from: "5.6"` and no `to` parameter
- **THEN** the system uses the version 5.6 snapshot as start and live Hoyolab data as end, computing delta statistics to the present and rendering the embed in English

#### Scenario: No matches played in selected period
- **WHEN** a user requests a period during which no matches were played (delta games equals zero)
- **THEN** the system displays an English embed informing the user that no matches were recorded in that timeframe ("No matches were recorded during this period.")

#### Scenario: Missing start snapshot for account
- **WHEN** a user requests a period for which their account has no recorded start snapshot
- **THEN** the system responds with an English error embed indicating no snapshot exists ("Data Not Found") and suggests the earliest available snapshot

### Requirement: Snapshot autocomplete support
The system SHALL provide autocomplete suggestions for version and snapshot parameters in the `/card` command using English labels.

#### Scenario: Autocomplete suggestions for range parameters
- **WHEN** a user focuses or types into the `from`, `to`, or `version` parameters in `/card`
- **THEN** the system returns matching game versions (labeled `Version <X.Y>`), user custom snapshot names, and the "Present (Live)" option where applicable

#### Scenario: Smart filter for daily snapshots in autocomplete
- **WHEN** a user types a query matching "daily" or date patterns into `from` or `to` parameters
- **THEN** the system includes matching daily snapshots in autocomplete suggestions, but excludes them when the input is empty or does not match daily patterns
