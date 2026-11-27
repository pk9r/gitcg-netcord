# Spec Delta

## MODIFIED Requirements

### Requirement: Automated daily thread report delivery
The system SHALL automatically compute and post daily card statistics reports into active subscribed Discord destinations (threads and Direct Message channels) upon capturing an account's daily snapshot. All report embeds, field titles, notices, and summary texts SHALL be rendered completely in English, and win-rate percentages SHALL format deterministically without locale-dependent whitespace between the numeric percentage and `%` (for example, `75.0%`) regardless of host OS culture. The report embed SHALL display a unified Characters Used section showing all characters used during the period with role avatar icons, game count, win rate, and win/loss record. The report embed SHALL display up to the Top 10 most used action cards, and format the Period field to reflect the exact game date or date range.

#### Scenario: Daily report delivery when matches were played
- **GIVEN** an account has active thread or DM subscriptions and delta games played since the previous snapshot is greater than zero
- **WHEN** the account's daily snapshot is captured
- **THEN** the system SHALL post an English embed to each subscribed thread and DM channel showing total games played, total wins, win rate, unified Characters Used section with role avatar icons, up to Top 10 most used action cards, and accurate game day period

#### Scenario: Daily report delivery to Direct Message channel
- **GIVEN** an account has an active Direct Message subscription
- **WHEN** the daily report delivery executes
- **THEN** the system SHALL send the report embed directly to the user's Discord DM channel

#### Scenario: Multi-day period representation when previous snapshots were missed
- **GIVEN** the span between the previous snapshot and current snapshot encompasses more than one game day
- **WHEN** report delivery executes
- **THEN** the system SHALL format the Period field to display start date, end date, and total number of days (e.g., YYYY-MM-DD ~ YYYY-MM-DD (N days))

#### Scenario: Daily report delivery when zero matches were played
- **GIVEN** an account has active subscriptions and delta games played since the previous day snapshot equals zero
- **WHEN** daily report delivery executes
- **THEN** the system SHALL post an English embed to each subscribed destination informing that no matches were recorded for that server day

#### Scenario: Graceful handling of inaccessible or archived threads
- **GIVEN** a subscribed thread is deleted, archived, or inaccessible, or a user has blocked bot DMs (403 Forbidden)
- **WHEN** the system attempts to deliver a daily report
- **THEN** the system SHALL log the delivery failure, mark the subscription as disabled, and continue delivering reports to remaining destinations without crashing

#### Scenario: Culture-invariant win-rate percentage formatting across host locales
- **GIVEN** the application runs under a host culture where standard percent formatting inserts whitespace before `%` (such as Linux ICU defaults or `InvariantCulture`)
- **WHEN** the daily report embed formats overall win rate and per-character win rates
- **THEN** the system SHALL render percentages using `.` as the decimal separator and no space before `%` (for example, `75.0%`)
