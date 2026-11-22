# Proposal

## Why

Application startup currently experiences a synchronous 7 to 9 second freeze during bootstrapping caused by `DefaultAzureCredential` probing local CLI processes and Azure Key Vault retrieving secrets over WAN. Migrating to an external OpenBao / HashiCorp Vault secrets engine via the community-standard `VaultSharp` client reduces secret resolution latency from ~8,000ms to ~200ms (a 40x speedup), eliminates heavy Azure SDK dependencies, removes cloud vendor lock-in, and enforces strict environment isolation across dev and prod engines without storing connection secrets in project repository files.

## What Changes

- **Remove Azure SDK Dependencies**: Remove `Azure.Identity` and `Azure.Extensions.AspNetCore.Configuration.Secrets` from project dependencies and Directory.Packages.props.
- **Remove Azure Key Vault Bootstrapping**: Remove Azure Key Vault configuration code and references from `Program.cs`.
- **Integrate VaultSharp Client & Configuration Provider**: Add the `VaultSharp` package (v1.17.5.1) and implement an OpenBao KV v2 configuration provider supporting HTTP REST endpoints, token authentication, and optional namespace isolation (`VaultClientSettings.Namespace`).
- **Flexible Engine Routing**: Support configurable secret engine mounts via `OPENBAO_MOUNT` / `VAULT_MOUNT` (defaulting to standard `secret`) and secret path via `OPENBAO_PATH` / `VAULT_PATH` (defaulting to `bot`), allowing dev and prod environments to consume distinct KV v2 mounts without hardcoded values in code.
- **Configuration Without Project JSON**: Support loading OpenBao connection parameters entirely from environment variables (`OPENBAO_ADDR`, `OPENBAO_TOKEN`, `OPENBAO_NAMESPACE`, `OPENBAO_MOUNT`, `OPENBAO_PATH`) and .NET user secrets, keeping Git repositories completely free of infrastructure connection details.
- **Local Secret Cleanup**: Remove the legacy `AzureVaultUri` entry from local development user secrets.

## Capabilities

### New Capabilities
- `secrets-management`: Secure, low-latency secret resolution from OpenBao / HashiCorp Vault KV v2 secret engines into ASP.NET Core `IConfiguration` using `VaultSharp` during application bootstrapping.

### Modified Capabilities
<!-- None. Existing capabilities such as bot-readiness-monitoring still consume IConfiguration keys (e.g., Discord:Token) identically. -->

## Impact

- **Affected Code**: `src/GitcgNetCord.MainApp/Program.cs`, new configuration extension/provider in `src/GitcgNetCord.MainApp/Configuration/OpenBaoConfigurationExtensions.cs`.
- **Dependencies**: Removed `Azure.Identity` and `Azure.Extensions.AspNetCore.Configuration.Secrets`. Added `VaultSharp` (v1.17.5.1) for OpenBao / Vault connectivity.
- **Infrastructure Requirements**: External OpenBao / HashiCorp Vault instance (configured via environment variables or user secrets). No new AppHost container resources are needed as OpenBao is hosted externally.
- **Deployment & Operations**: Production containers in Docker/Portainer supply `OPENBAO_ADDR`, `OPENBAO_TOKEN`, `OPENBAO_MOUNT`, etc. via environment variables.
