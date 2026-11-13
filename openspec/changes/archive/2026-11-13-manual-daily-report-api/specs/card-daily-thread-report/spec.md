# Spec Delta

## ADDED Requirements

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
