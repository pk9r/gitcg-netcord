# Tasks

## 1. VaultSharp Dependency & OpenBao Provider Implementation

- [x] 1.1 Add `VaultSharp` (v1.17.5.1) to `Directory.Packages.props` and `src/GitcgNetCord.MainApp/GitcgNetCord.MainApp.csproj`. Verify package restore succeeds.
- [x] 1.2 Implement `OpenBaoConfigurationProvider` and `OpenBaoConfigurationSource` in `src/GitcgNetCord.MainApp/Configuration/` using `VaultClient`, `TokenAuthMethodInfo`, and `VaultClientSettings.Namespace` to read KV v2 secrets from OpenBao and populate `IConfiguration`.
- [x] 1.3 Implement extension method `AddOpenBaoConfiguration` in `src/GitcgNetCord.MainApp/Configuration/OpenBaoConfigurationExtensions.cs` to resolve connection parameters from environment variables and user secrets (`OPENBAO_ADDR`, `OPENBAO_TOKEN`, `OPENBAO_NAMESPACE`, `OPENBAO_MOUNT`, `OPENBAO_PATH`), defaulting mount point to `secret` and secret path to `bot`.

## 2. Bootstrapping Refactor and Azure SDK Removal

- [x] 2.1 Refactor `src/GitcgNetCord.MainApp/Program.cs` to call `builder.Configuration.AddOpenBaoConfiguration()` and remove Azure Key Vault initialization and `Azure.Identity` namespaces.
- [x] 2.2 Remove package references `Azure.Identity` and `Azure.Extensions.AspNetCore.Configuration.Secrets` from `src/GitcgNetCord.MainApp/GitcgNetCord.MainApp.csproj` and `Directory.Packages.props`. Verify the solution compiles cleanly with `dotnet build src/GitcgNetCord.MainApp`.

## 3. Local Environment Configuration

- [x] 3.1 Configure local user secrets for `src/GitcgNetCord.MainApp` with `OPENBAO_ADDR`, `OPENBAO_NAMESPACE`, `OPENBAO_TOKEN`, and `OPENBAO_MOUNT`.
- [x] 3.2 Remove obsolete `AzureVaultUri` entry from local development user secrets.

## 4. Verification and Testing

- [x] 4.1 Run and pass xUnit tests in `tests/GitcgNetCord.MainApp.Tests` using `dotnet test` to confirm configuration validation and dependent services pass.
- [x] 4.2 Validate that the application starts up and loads all secrets from OpenBao via VaultSharp within 300ms, removing the previous 7-9 second freeze.
