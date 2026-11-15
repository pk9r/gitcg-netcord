# Spec Delta

## MODIFIED Requirements

### Requirement: Thread daily report unsubscription and listing
The system SHALL allow thread owners or the user who subscribed an account to unsubscribe accounts from receiving daily reports in the current thread and list currently subscribed accounts. When listing subscriptions via `/card-snapshot thread list`, the system SHALL also provide community server suggestions (servers with configured public invite URLs and approved report channels) to assist users in finding report destinations. All command descriptions, parameter descriptions, embed titles, fields, notices, and response messages SHALL be rendered completely in English.

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
- **THEN** the system SHALL display an English embed listing subscribed Hoyolab accounts with their UIDs and regions, append a section highlighting community servers configured for daily reports if available, and attach interactive Link Buttons for those community servers (up to 3)

#### Scenario: List when no accounts are subscribed
- **GIVEN** a thread with no active subscriptions
- **WHEN** a user executes `/card-snapshot thread list` inside that thread
- **THEN** the system SHALL display an English embed stating that no accounts are currently subscribed in this thread, include a section with suggested report community servers if available, and attach interactive Link Buttons for those community servers

#### Scenario: List executed outside of a thread
- **GIVEN** a user executes `/card-snapshot thread list` in a standard guild channel or Direct Message channel
- **WHEN** the command is processed
- **THEN** the system SHALL return an informative English embed explaining that thread report listings are scoped to individual threads, display available suggested community servers with report channels enabled, and attach interactive Link Buttons for those servers

#### Scenario: Filtering and eligibility for suggested report servers
- **GIVEN** candidate guilds configured in the database
- **WHEN** the system queries for report server suggestions
- **THEN** the system SHALL only include guilds that have both a non-empty public invite URL and at least one approved parent report channel, excluding the current guild if executed within a server

#### Scenario: Graceful behavior when no suggested servers exist
- **GIVEN** no registered guilds meet the criteria for public invites and approved report channels
- **WHEN** a user executes `/card-snapshot thread list`
- **THEN** the system SHALL display the subscription status or thread guidance without rendering empty suggestion fields or empty button action rows
