# Proposal

## Why

GitHub Dependabot version updates are ill-suited for this repository's Central Package Management (CPM) architecture (`Directory.Packages.props`) and tightly coupled dependencies (.NET 11, Aspire 17, and pinned EF Core 10 for Aspire Npgsql). Dependabot opens isolated PRs for individual packages, creating severe merge conflicts across all open PRs upon merging any single PR, while also risking broken builds when tightly coupled packages are bumped independently. Transitioning to deliberate manual dependency management resolves PR noise and preserves dependency compatibility.

## What Changes

- Remove `.github/dependabot.yml` to disable automated Dependabot version update scans and pull requests.
- Close existing Dependabot-created pull requests and prune stale `dependabot/*` tracking branches.
- Establish a standardized manual dependency upgrade procedure using `dotnet list package --outdated` and direct edits to `Directory.Packages.props`.

## Capabilities

### New Capabilities
None. This change alters repository tooling and maintenance procedures only; runtime system behaviors and user-facing capabilities remain unchanged.

### Modified Capabilities
None.

## Impact

- **Tooling & CI/CD**: Removal of `.github/dependabot.yml`. Dependabot version updates will stop triggering.
- **Repository Health**: Elimination of automated PR spam and merge conflicts on `Directory.Packages.props`.
- **Runtime & APIs**: No impact on application binaries, Discord bot commands, database models, or external services.
