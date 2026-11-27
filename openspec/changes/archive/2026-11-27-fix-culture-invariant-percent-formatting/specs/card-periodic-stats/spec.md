# Spec Delta

## MODIFIED Requirements

### Requirement: Time-range statistics display in card command
The system SHALL calculate and display delta card statistics between two points in time when `version`, `from`, or `to` parameters are specified in the `/card` command. All embeds, field headers, notices, and messages SHALL be rendered completely in English, and win-rate percentages SHALL format deterministically without locale-dependent whitespace between the numeric percentage and `%` (for example, `75.00%`) regardless of host OS culture.

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

#### Scenario: Culture-invariant win-rate percentage formatting in periodic statistics embeds
- **GIVEN** the application runs under a host culture where standard percent formatting inserts whitespace before `%`
- **WHEN** the `/card` command formats overall win rate and character best/lowest win rates
- **THEN** the system SHALL render percentages with two decimal places, `.` as the decimal separator, and no space before `%` (for example, `75.00%`)
