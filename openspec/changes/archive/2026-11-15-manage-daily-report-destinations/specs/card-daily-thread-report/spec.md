# Spec Delta

## MODIFIED Requirements

### Requirement: Thread daily report subscription
The system SHALL allow the owner of a thread or an authorized subscriber to subscribe a Hoyolab account with daily snapshots enabled to daily statistics reports in that thread via `/card-snapshot report subscribe`, and SHALL permit an account to be subscribed to multiple destinations simultaneously. All command descriptions, parameter descriptions, notices, and response messages SHALL be rendered completely in English.

#### Scenario: Successful thread subscription with active account
- **GIVEN** a thread created under an approved parent channel
- **WHEN** the thread owner executes `/card-snapshot report subscribe` without specifying an account, and their active Hoyolab account has daily snapshots enabled
- **THEN** the system SHALL register the thread subscription for the active account and reply with an English confirmation message

#### Scenario: Successful thread subscription with specific account
- **GIVEN** a thread created under an approved parent channel
- **WHEN** the thread owner executes `/card-snapshot report subscribe` specifying an account UID they own with daily snapshots enabled
- **THEN** the system SHALL register the thread subscription for the specified account and reply with an English confirmation message

#### Scenario: Rejection when account has daily snapshots disabled
- **GIVEN** a user owns a Hoyolab account with daily snapshots disabled
- **WHEN** the user executes `/card-snapshot report subscribe` in a thread for that account
- **THEN** the system SHALL reject the subscription with an English error message instructing the user to enable daily snapshots first using `/card-snapshot daily enabled:True`

#### Scenario: Rejection when user is not thread owner
- **GIVEN** a user who is neither the thread owner nor the subscriber of an account in the thread
- **WHEN** the user attempts to execute `/card-snapshot report subscribe`
- **THEN** the system SHALL reject the request with an English error message stating only the thread owner or subscriber can manage subscriptions

#### Scenario: Rejection when thread is not in an approved parent channel
- **GIVEN** a thread whose parent channel is not registered as an approved report channel
- **WHEN** a thread owner executes `/card-snapshot report subscribe` inside that thread
- **THEN** the system SHALL reject the request with an English error message stating the thread does not belong to an approved report channel

#### Scenario: Rejection when executed outside a thread
- **GIVEN** a user executes `/card-snapshot report subscribe` in a regular server channel that is neither a thread nor a Direct Message channel
- **WHEN** the command is processed
- **THEN** the system SHALL reject the request with an English guidance message explaining that server subscriptions must be run inside a report thread, or in Direct Messages for private reports

#### Scenario: Multiple destination subscription allowed for same account
- **GIVEN** a Hoyolab account is already subscribed to reports in a Direct Message channel or another thread
- **WHEN** the user subscribes the account to a new approved thread
- **THEN** the system SHALL register the new subscription alongside existing subscriptions without overwriting previous destinations

### Requirement: Thread daily report unsubscription and listing
The system SHALL allow thread owners or the user who subscribed an account to unsubscribe accounts from receiving daily reports in the current thread and list currently subscribed accounts via `/card-snapshot report` subcommands. When listing subscriptions inside a thread via `/card-snapshot report list`, the system SHALL also provide community server suggestions to assist users in finding report destinations. All command descriptions, parameter descriptions, embed titles, fields, notices, and response messages SHALL be rendered completely in English.

#### Scenario: Unsubscribe account from thread
- **GIVEN** a thread has an active account subscription
- **WHEN** the thread owner or the user who registered the subscription executes `/card-snapshot report unsubscribe` with that account
- **THEN** the system SHALL remove the subscription for that account and reply with an English confirmation message

#### Scenario: Rejection when user is neither thread owner nor subscriber
- **GIVEN** a user who is neither the thread owner nor the subscriber of an account in the thread
- **WHEN** the user attempts to execute `/card-snapshot report unsubscribe`
- **THEN** the system SHALL reject the request with an English error message stating that only the thread owner or subscriber can manage subscriptions

#### Scenario: List subscribed accounts in thread
- **GIVEN** a thread that has active subscriptions
- **WHEN** a user executes `/card-snapshot report list` inside that thread
- **THEN** the system SHALL display an English embed listing subscribed Hoyolab accounts with their UIDs and regions, append a section highlighting community servers configured for daily reports if available, and attach interactive Link Buttons for those community servers (up to 3)

#### Scenario: List when no accounts are subscribed
- **GIVEN** a thread with no active subscriptions
- **WHEN** a user executes `/card-snapshot report list` inside that thread
- **THEN** the system SHALL display an English embed stating that no accounts are currently subscribed in this thread, include a section with suggested report community servers if available, and attach interactive Link Buttons for those community servers

#### Scenario: List executed outside of a thread
- **GIVEN** a user executes `/card-snapshot report list` in a standard guild channel that is neither a thread nor a Direct Message channel
- **WHEN** the command is processed
- **THEN** the system SHALL return an informative English embed explaining that thread report listings are scoped to individual threads and personal listings are available in DMs, display available suggested community servers with report channels enabled, and attach interactive Link Buttons for those servers

#### Scenario: Filtering and eligibility for suggested report servers
- **GIVEN** candidate guilds configured in the database
- **WHEN** the system queries for report server suggestions
- **THEN** the system SHALL only include guilds that have both a non-empty public invite URL and at least one approved parent report channel, excluding the current guild if executed within a server

#### Scenario: Graceful behavior when no suggested servers exist
- **GIVEN** no registered guilds meet the criteria for public invites and approved report channels
- **WHEN** a user executes `/card-snapshot report list`
- **THEN** the system SHALL display the subscription status or thread guidance without rendering empty suggestion fields or empty button action rows

### Requirement: Direct Message Report Subscription
The system SHALL allow users to subscribe their Hoyolab accounts to receive daily statistics reports directly in their Discord Direct Messages, unsubscribe accounts from DM reports, and inspect all personal report destinations via `/card-snapshot report` commands or interactive destination pickers without requiring server membership. All confirmation and notice messages SHALL be rendered completely in English.

#### Scenario: Successful DM report subscription
- **GIVEN** a user with daily snapshots enabled on a Hoyolab account
- **WHEN** the user selects the Direct Message destination option from an interactive prompt
- **THEN** the system SHALL open or retrieve the DM channel with the user, register the subscription with the DM channel ID, and reply with an English confirmation message

#### Scenario: Successful DM report subscription via slash command
- **GIVEN** a user executing commands inside their Discord Direct Message channel with the bot
- **WHEN** the user executes `/card-snapshot report subscribe` with an active or specified Hoyolab account that has daily snapshots enabled
- **THEN** the system SHALL register the DM subscription for that account and reply with an English confirmation message

#### Scenario: Rejection when daily snapshots are disabled for account
- **GIVEN** a user attempts to subscribe to DM reports for an account with daily snapshots disabled
- **WHEN** the DM subscription is requested via command or picker
- **THEN** the system SHALL reject the subscription with an English error message instructing the user to enable daily snapshots first

#### Scenario: Successful DM report unsubscription via slash command
- **GIVEN** a user with an active DM subscription for a Hoyolab account
- **WHEN** the user executes `/card-snapshot report unsubscribe` inside their Direct Message channel specifying that account
- **THEN** the system SHALL remove the DM subscription for that account and reply with an English confirmation message

#### Scenario: Unsubscribe non-subscribed account in DM
- **GIVEN** a user executing `/card-snapshot report unsubscribe` in DM for an account that is not subscribed to DM reports
- **WHEN** the command is processed
- **THEN** the system SHALL reply with an English message indicating that the account is not currently subscribed to daily reports in Direct Messages

#### Scenario: List personal report subscriptions in DM
- **GIVEN** a user executing `/card-snapshot report list` inside their Direct Message channel
- **WHEN** the user has active subscriptions in DMs or server threads
- **THEN** the system SHALL return an English embed listing all active report destinations associated with the user, categorized by Direct Messages and Server Threads with guild and channel references

#### Scenario: List personal report subscriptions in DM when none exist
- **GIVEN** a user executing `/card-snapshot report list` inside their Direct Message channel who has no active report subscriptions
- **WHEN** the command is processed
- **THEN** the system SHALL return an English embed stating that no active report destinations are configured, along with instructions on how to subscribe via `/card-snapshot report subscribe`
