# card-daily-thread-report Specification

## Purpose

Enables Discord servers to configure approved report channels and allows thread owners to subscribe their Hoyolab accounts to automated daily Genius Invokation TCG statistics reports.

## Requirements

### Requirement: Register and unregister parent report channels
The system SHALL allow guild administrators with channel management permissions to register, unregister, and list parent channels approved for daily thread reporting. All command descriptions, parameter descriptions, embed titles, fields, notices, and response messages SHALL be rendered completely in English.

#### Scenario: Register approved parent channel
- **WHEN** an authorized user executes `/card-snapshot channel add` with a specified channel or the current channel in a guild
- **THEN** the system registers the channel as an approved daily report parent channel and replies with an English confirmation message

#### Scenario: Unregister parent channel
- **WHEN** an authorized user executes `/card-snapshot channel remove` with a registered channel
- **THEN** the system removes the channel from the approved parent channels and replies with an English confirmation message

#### Scenario: List registered parent channels
- **WHEN** an authorized user executes `/card-snapshot channel list` in a guild
- **THEN** the system displays an English embed listing all registered parent channels for the current guild

#### Scenario: Non-admin invocation restriction
- **WHEN** a user without channel management permissions attempts to execute `/card-snapshot channel` commands
- **THEN** the system prevents invocation or replies with an English error message indicating channel management permissions are required

### Requirement: Thread daily report subscription
The system SHALL allow the owner of a thread created within an approved parent channel to subscribe a Hoyolab account to daily statistics reports in that thread. All command descriptions, parameter descriptions, notices, and response messages SHALL be rendered completely in English.

#### Scenario: Successful thread subscription with active account
- **WHEN** the thread owner executes `/card-snapshot thread subscribe` without specifying an account inside a thread under an approved parent channel, and their active Hoyolab account has daily snapshots enabled
- **THEN** the system registers the thread subscription for the active account and replies with an English confirmation message

#### Scenario: Successful thread subscription with specific account
- **WHEN** the thread owner executes `/card-snapshot thread subscribe` specifying an account UID they own with daily snapshots enabled
- **THEN** the system registers the thread subscription for the specified account and replies with an English confirmation message

#### Scenario: Rejection when account has daily snapshots disabled
- **WHEN** a user executes `/card-snapshot thread subscribe` for a Hoyolab account that does not have daily snapshots enabled
- **THEN** the system rejects the subscription with an English error message instructing the user to enable daily snapshots first using `/card-snapshot daily enabled:True`

#### Scenario: Rejection when user is not thread owner
- **WHEN** a user who is not the creator or owner of the thread attempts to execute `/card-snapshot thread subscribe`
- **THEN** the system rejects the request with an English error message stating only the thread owner can manage subscriptions

#### Scenario: Rejection when thread is not in an approved parent channel
- **WHEN** a thread owner executes `/card-snapshot thread subscribe` inside a thread whose parent channel is not registered as an approved report channel
- **THEN** the system rejects the request with an English error message stating the thread does not belong to an approved report channel

#### Scenario: Rejection when executed outside a thread
- **WHEN** a user executes `/card-snapshot thread subscribe` in a regular channel that is not a thread
- **THEN** the system rejects the request with an English error message stating the command can only be used inside a thread

### Requirement: Thread daily report unsubscription and listing
The system SHALL allow thread owners to unsubscribe accounts from receiving daily reports in the current thread and list currently subscribed accounts. All command descriptions, parameter descriptions, embed titles, fields, notices, and response messages SHALL be rendered completely in English.

#### Scenario: Unsubscribe account from thread
- **WHEN** the thread owner executes `/card-snapshot thread unsubscribe` with an account in the thread
- **THEN** the system removes the subscription for that account and replies with an English confirmation message

#### Scenario: List subscribed accounts in thread
- **WHEN** a user executes `/card-snapshot thread list` inside a thread that has active subscriptions
- **THEN** the system displays an English embed listing subscribed Hoyolab accounts with their UIDs and regions

#### Scenario: List when no accounts are subscribed
- **WHEN** a user executes `/card-snapshot thread list` inside a thread with no subscriptions
- **THEN** the system replies with an English message stating that no accounts are currently subscribed in this thread

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

### Requirement: Manual HTTP trigger for daily snapshot and report delivery
The system SHALL provide an HTTP endpoint to manually trigger capturing or updating the daily snapshot for the current server day and delivering daily reports to subscribed Discord threads.

#### Scenario: Manual trigger for all active accounts with force update
- **WHEN** an authorized client sends a POST request to `/api/reports/daily/trigger` without specifying a UID
- **THEN** the system processes all active accounts with daily snapshots enabled, updates today's snapshot with current game statistics, delivers reports to active subscribed threads, and returns a JSON summary of the processed accounts

#### Scenario: Manual trigger for a specific Hoyolab account
- **WHEN** a client sends a POST request to `/api/reports/daily/trigger` specifying a valid `uid`
- **THEN** the system processes only the specified Hoyolab account, updates today's snapshot, delivers reports to subscribed threads for that account, and returns a JSON response indicating the results

#### Scenario: Manual trigger for non-existent or unmanaged UID
- **WHEN** a client sends a POST request to `/api/reports/daily/trigger` with a `uid` that is not registered or found
- **THEN** the system returns an error status indicating the account was not found

### Requirement: Interactive local API documentation with Scalar
The system SHALL expose interactive OpenAPI documentation powered by Scalar at `/scalar/v1` in local development mode.

#### Scenario: Access Scalar API reference interface
- **WHEN** a user navigates to `/scalar/v1` in a browser during local execution
- **THEN** the system serves the interactive Scalar API documentation interface listing available endpoints including the daily report trigger
