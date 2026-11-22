# Spec Delta

## Purpose

Enables secure, low-latency configuration and secret retrieval from external OpenBao / HashiCorp Vault KV v2 secret engines into ASP.NET Core IConfiguration during application bootstrapping.

## ADDED Requirements

### Requirement: OpenBao KV v2 Secret Retrieval at Bootstrapping
The application SHALL retrieve configuration secrets from an external OpenBao KV v2 secrets engine during bootstrapping and populate them into the application configuration root before service container construction.

#### Scenario: Successful secret retrieval from OpenBao
- **GIVEN** valid OpenBao endpoint and authentication token are configured
- **WHEN** the application initializes configuration providers at startup
- **THEN** all key-value secrets under the specified secret path SHALL be populated into IConfiguration with hierarchical key delimiters

#### Scenario: Missing OpenBao endpoint configuration
- **GIVEN** neither `OPENBAO_ADDR` nor `VAULT_ADDR` is provided in the environment or user secrets
- **WHEN** the application starts up
- **THEN** the OpenBao provider SHALL bypass remote secret retrieval and log an informational message without failing host bootstrapping

#### Scenario: Invalid OpenBao credentials or unreachable endpoint
- **GIVEN** an OpenBao endpoint and token are configured but the endpoint is unreachable or authentication fails
- **WHEN** the application attempts to retrieve secrets during startup
- **THEN** the provider MUST fail fast by throwing an InvalidOperationException with connection details to prevent operating in an indeterminate state

### Requirement: OpenBao Namespace and Secret Engine Routing
The system SHALL dispatch requests with optional OpenBao namespace headers and route secret lookups to configured engine mount paths.

#### Scenario: Namespace header dispatch
- **GIVEN** an OpenBao namespace is configured (e.g. via `OPENBAO_NAMESPACE` or `VAULT_NAMESPACE`)
- **WHEN** an HTTP request is made to the OpenBao server
- **THEN** the request MUST contain the `X-Vault-Namespace` header with the configured namespace value

#### Scenario: Configured secret engine mount point
- **GIVEN** `OPENBAO_MOUNT` or `VAULT_MOUNT` is configured in user secrets or environment variables
- **WHEN** the OpenBao configuration provider initializes
- **THEN** the secret engine mount path SHALL use the configured mount point value

#### Scenario: Default secret engine mount point
- **GIVEN** neither `OPENBAO_MOUNT` nor `VAULT_MOUNT` is configured
- **WHEN** the OpenBao configuration provider initializes
- **THEN** the secret engine mount path SHALL default to `secret` and the secret path SHALL default to `bot`

### Requirement: External Configuration Binding Without Project JSON
The system SHALL resolve OpenBao connection parameters exclusively from environment variables or local user secrets, keeping committed project JSON files free of infrastructure secrets.

#### Scenario: Parameter resolution from environment variables
- **GIVEN** `OPENBAO_ADDR` and `OPENBAO_TOKEN` are set in the operating system environment
- **WHEN** the configuration builder executes
- **THEN** the OpenBao options SHALL bind directly from environment variables without requiring entries in `appsettings.json` or `appsettings.Development.json`
