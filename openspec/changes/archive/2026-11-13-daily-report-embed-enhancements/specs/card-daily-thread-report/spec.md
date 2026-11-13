# Spec Delta

## MODIFIED Requirements

### Requirement: Automated daily thread report delivery
The system SHALL automatically compute and post daily card statistics reports into active subscribed Discord threads upon capturing an account's daily snapshot. All report embeds, field titles, notices, and summary texts SHALL be rendered completely in English. The report embed SHALL display character role avatar icons before character names in character rankings and format the Period field to reflect the exact game date or date range.

#### Scenario: Daily report delivery when matches were played
- **WHEN** an account's daily snapshot is captured and there are active thread subscriptions, and delta games played since the previous snapshot is greater than zero
- **THEN** the system posts an English embed to each subscribed thread showing total games played, total wins, win rate, character rankings with role avatar icons, and the accurate game day period

#### Scenario: Multi-day period representation when previous snapshots were missed
- **WHEN** the span between the previous snapshot and current snapshot encompasses more than one game day
- **THEN** the system formats the Period field to display the start date, end date, and total number of days (e.g., YYYY-MM-DD ~ YYYY-MM-DD (N days))

#### Scenario: Daily report delivery when zero matches were played
- **WHEN** an account's daily snapshot is captured and there are active thread subscriptions, and delta games played since the previous day snapshot equals zero
- **THEN** the system posts an English embed to each subscribed thread informing that no matches were recorded for that server day

#### Scenario: Graceful handling of inaccessible or archived threads
- **WHEN** the system attempts to send a daily report to a thread that is deleted, archived, or inaccessible due to missing permissions
- **THEN** the system logs the delivery error without crashing or failing report delivery to remaining threads
