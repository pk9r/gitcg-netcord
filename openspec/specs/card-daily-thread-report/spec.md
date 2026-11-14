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
The system SHALL allow the owner of a thread or an authorized subscriber to subscribe a Hoyolab account with daily snapshots enabled to daily statistics reports in that thread, and SHALL permit an account to be subscribed to multiple destinations simultaneously. All command descriptions, parameter descriptions, notices, and response messages SHALL be rendered completely in English.

#### Scenario: Successful thread subscription with active account
- **GIVEN** a thread created under an approved parent channel
- **WHEN** the thread owner executes `/card-snapshot thread subscribe` without specifying an account, and their active Hoyolab account has daily snapshots enabled
- **THEN** the system SHALL register the thread subscription for the active account and reply with an English confirmation message

#### Scenario: Successful thread subscription with specific account
- **GIVEN** a thread created under an approved parent channel
- **WHEN** the thread owner executes `/card-snapshot thread subscribe` specifying an account UID they own with daily snapshots enabled
- **THEN** the system SHALL register the thread subscription for the specified account and reply with an English confirmation message

#### Scenario: Rejection when account has daily snapshots disabled
- **GIVEN** a user owns a Hoyolab account with daily snapshots disabled
- **WHEN** the user executes `/card-snapshot thread subscribe` for that account
- **THEN** the system SHALL reject the subscription with an English error message instructing the user to enable daily snapshots first using `/card-snapshot daily enabled:True`

#### Scenario: Rejection when user is not thread owner
- **GIVEN** a user who is neither the thread owner nor the subscriber of an account in the thread
- **WHEN** the user attempts to execute `/card-snapshot thread subscribe`
- **THEN** the system SHALL reject the request with an English error message stating only the thread owner or subscriber can manage subscriptions

#### Scenario: Rejection when thread is not in an approved parent channel
- **GIVEN** a thread whose parent channel is not registered as an approved report channel
- **WHEN** a thread owner executes `/card-snapshot thread subscribe` inside that thread
- **THEN** the system SHALL reject the request with an English error message stating the thread does not belong to an approved report channel

#### Scenario: Rejection when executed outside a thread
- **GIVEN** a user executes `/card-snapshot thread subscribe` in a regular channel that is not a thread
- **WHEN** the command is processed
- **THEN** the system SHALL reject the request with an English error message stating the command can only be used inside a thread

#### Scenario: Multiple destination subscription allowed for same account
- **GIVEN** a Hoyolab account is already subscribed to reports in a Direct Message channel or another thread
- **WHEN** the user subscribes the account to a new approved thread
- **THEN** the system SHALL register the new subscription alongside existing subscriptions without overwriting previous destinations

### Requirement: Thread daily report unsubscription and listing
The system SHALL allow thread owners or the user who subscribed an account to unsubscribe accounts from receiving daily reports in the current thread and list currently subscribed accounts. All command descriptions, parameter descriptions, embed titles, fields, notices, and response messages SHALL be rendered completely in English.

#### Scenario: Unsubscribe account from thread
- **GIVEN** a thread has an active account subscription
- **WHEN** the thread owner or the user who registered the subscription executes `/card-snapshot thread unsubscribe` with that account
- **THEN** the system SHALL remove the subscription for that account and reply with an English confirmation message

#### Scenario: Rejection when user is neither thread owner nor subscriber
- **GIVEN** a user who is neither the thread owner nor the subscriber of an account in the thread
- **WHEN** the user attempts to execute `/card-snapshot thread unsubscribe`
- **THEN** the system SHALL reject the request with an English error message stating that only the thread owner or subscriber can manage subscriptions

#### Scenario: List subscribed accounts in thread
- **GIVEN** a thread that has active subscriptions
- **WHEN** a user executes `/card-snapshot thread list` inside that thread
- **THEN** the system SHALL display an English embed listing subscribed Hoyolab accounts with their UIDs and regions

#### Scenario: List when no accounts are subscribed
- **GIVEN** a thread with no active subscriptions
- **WHEN** a user executes `/card-snapshot thread list` inside that thread
- **THEN** the system SHALL reply with an English message stating that no accounts are currently subscribed in this thread

### Requirement: Automated daily thread report delivery
The system SHALL automatically compute and post daily card statistics reports into active subscribed Discord destinations (threads and Direct Message channels) upon capturing an account's daily snapshot. All report embeds, field titles, notices, and summary texts SHALL be rendered completely in English. The report embed SHALL display a unified Characters Used section showing all characters used during the period with role avatar icons, game count, win rate, and win/loss record. The report embed SHALL display up to the Top 10 most used action cards, and format the Period field to reflect the exact game date or date range.

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

### Requirement: Automated Thread Creation and Subscription
The system SHALL automatically create a Discord thread within an approved parent channel and subscribe the user's Hoyolab account to daily reports when selected from the report destination picker. All thread names, initial messages, and notifications SHALL be rendered completely in English.

#### Scenario: Auto-create thread in approved channel
- **GIVEN** a user selects an approved parent channel in a guild to receive daily reports for a Hoyolab account
- **WHEN** the automated thread creation executes
- **THEN** the system SHALL create a public thread named `📊 TCG Daily - {Nickname} ({UID})`, post an initial welcome message mentioning the user, subscribe the account to the newly created thread, and return a link to the thread

#### Scenario: Reuse existing thread when account is already subscribed in guild
- **GIVEN** an account already has an active thread subscription in the target guild
- **WHEN** the user selects the guild for thread report setup
- **THEN** the system SHALL avoid creating a duplicate thread and return a message referencing the existing subscribed thread

#### Scenario: Rejection when target guild has no approved parent channels
- **GIVEN** a guild with no registered approved daily report channels
- **WHEN** automated thread creation is requested for that guild
- **THEN** the system SHALL reject the operation with an English message explaining that an administrator must first register an approved channel using `/card-snapshot channel add`

### Requirement: Direct Message Report Subscription
The system SHALL allow users to subscribe their Hoyolab account to receive daily statistics reports directly in their Discord Direct Messages without requiring server membership. All confirmation and notice messages SHALL be rendered completely in English.

#### Scenario: Successful DM report subscription
- **GIVEN** a user with daily snapshots enabled on a Hoyolab account
- **WHEN** the user selects the Direct Message destination option
- **THEN** the system SHALL open or retrieve the DM channel with the user, register the subscription with the DM channel ID, and reply with an English confirmation message

#### Scenario: Rejection when daily snapshots are disabled for account
- **GIVEN** a user attempts to subscribe to DM reports for an account with daily snapshots disabled
- **WHEN** the DM subscription is requested
- **THEN** the system SHALL reject the subscription with an English error message instructing the user to enable daily snapshots first

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
