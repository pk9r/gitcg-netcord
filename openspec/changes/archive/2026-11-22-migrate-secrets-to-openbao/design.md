# Design

## Context

Application bootstrapping currently calls `builder.Configuration.AddAzureKeyVault(...)` using `DefaultAzureCredential`. This causes a synchronous 7 to 9 second startup block because `DefaultAzureCredential` executes local Azure CLI commands (`az.cmd account get-access-token`) and performs multiple WAN REST requests to Azure Key Vault to enumerate and retrieve individual secrets. 

The target solution is an external OpenBao / HashiCorp Vault instance operating with KV v2 secrets engines, storing application secrets under a designated secret path (default `bot`). Environment-specific secrets engines (e.g. dev vs prod) are bound dynamically via configuration (`OPENBAO_MOUNT`). Benchmark testing confirms ~193ms secret fetch latency, yielding an immediate ~40x speedup.

## Goals / Non-Goals

**Goals:**
- Eliminate the 7-9 second startup freeze by fetching all secrets in a single low-latency request (< 300ms) using `VaultSharp`.
- Remove Azure SDK dependencies (`Azure.Identity`, `Azure.Extensions.AspNetCore.Configuration.Secrets`) from the project.
- Integrate `VaultSharp` (v1.17.5.1) to communicate with OpenBao / Vault KV v2, supporting optional namespace headers (`VaultClientSettings.Namespace`) and Token authentication.
- Support configurable engine mount routing via `OPENBAO_MOUNT` / `VAULT_MOUNT` (defaulting to `secret`) and secret path via `OPENBAO_PATH` / `VAULT_PATH` (defaulting to `bot`), with zero hardcoded environment names in code.
- Enforce clean Git hygiene: zero OpenBao connection parameters, endpoints, namespaces, or tokens stored in committed repository files.

**Non-Goals:**
- Containerizing OpenBao within Aspire AppHost (OpenBao is operated externally as a standalone service).
- Dynamic secret leasing or runtime token renewal background loops (secrets are read once during application startup configuration building).
- Database or EF Core schema migrations (configuration migration only).

## Decisions

### Decision 1: Using VaultSharp as the OpenBao Client
- **Choice**: Integrate `VaultSharp` (v1.17.5.1) inside an `OpenBaoConfigurationProvider` and `OpenBaoConfigurationSource`.
- **Rationale**: `VaultSharp` is the premier, battle-tested .NET library for HashiCorp Vault and OpenBao. It natively supports `VaultClientSettings.Namespace`, handles TLS/HTTP connections cleanly, provides strongly-typed KV v2 APIs (`vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync`), and offers seamless future migration to AppRole or other auth methods if required.
- **Alternatives Considered**: 
  - Direct raw `HttpClient` implementation: Lacks built-in Vault abstractions and future auth extensibility.

### Decision 2: Environment and Engine Routing via User Secrets and Environment Variables
- **Choice**: Resolve `MountPoint` from `OPENBAO_MOUNT` / `VAULT_MOUNT` / `OpenBao:MountPoint`, falling back to standard `secret`. Resolve `SecretPath` from `OPENBAO_PATH` / `VAULT_PATH` / `OpenBao:SecretPath`, falling back to `bot`.
  - In Development: Developers configure their development engine mount in local .NET User Secrets.
  - In Production: Container runtime or CI/CD supplies the production engine mount via environment variables.
- **Rationale**: Keeps the codebase 100% generic, open-source-safe, and free of proprietary or private infrastructure names while supporting separate secret engines per environment.

### Decision 3: Configuration Precedence and Zero-JSON Policy
- **Choice**: Read OpenBao bootstrap settings from environment variables (`OPENBAO_ADDR` / `VAULT_ADDR`, `OPENBAO_TOKEN` / `VAULT_TOKEN`, `OPENBAO_NAMESPACE` / `VAULT_NAMESPACE`, `OPENBAO_MOUNT`, `OPENBAO_PATH`) and .NET User Secrets (in Development).
- **Rationale**: Adheres strictly to 12-Factor App methodology. `appsettings.json` and `appsettings.Development.json` remain completely free of infrastructure connection details.

## Risks / Trade-offs

- **[Risk] OpenBao Token Expiration** → If the provided token expires, the application cannot start.
  - *Mitigation*: The configuration provider validates HTTP response codes and throws an explicit `InvalidOperationException` stating that the token is invalid or expired, enabling fast troubleshooting.
- **[Risk] Network Outage to External OpenBao Server** → If the network or OpenBao is unreachable during startup, the host could hang.
  - *Mitigation*: Configure an explicit 5-second timeout in `VaultClientSettings` with fail-fast exception handling. If OpenBao configuration is completely unconfigured (e.g. running unit tests without secrets), gracefully bypass remote loading.
- **[Risk] Missing Configuration Keys** → If a key is misspelled in OpenBao, runtime errors could occur later in service initialization.
  - *Mitigation*: Existing `StartupValidationExtensions.ValidateDiscordConfiguration()` and options validation (`AddOptionsWithValidateOnStart`) immediately verify required keys like `Discord:Token` upon startup.
