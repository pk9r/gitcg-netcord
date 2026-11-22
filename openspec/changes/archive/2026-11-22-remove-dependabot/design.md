# Design

## Context

See `proposal.md` for motivation. The project manages all NuGet package versions centrally via `Directory.Packages.props` (Central Package Management / CPM) and configures weekly NuGet update checks via `.github/dependabot.yml`. Several key dependencies are constrained by compatibility:
- `Microsoft.EntityFrameworkCore.*` is pinned to `10.0.14` for compatibility with `Aspire.Npgsql.EntityFrameworkCore.PostgreSQL` (`13.6.5`).
- The project targets .NET 11 and Aspire 17.0 with components that must be updated in lockstep rather than individually.

## Goals / Non-Goals

**Goals:**
- Completely remove automated Dependabot version update configuration from the repository.
- Provide a clear, actionable checklist for repository cleanup (closing Dependabot PRs and deleting orphaned branches).
- Document the standard manual package upgrade workflow using standard .NET CLI tooling.

**Non-Goals:**
- Adopting alternative bot systems (such as Renovate or custom action workflows).
- Changing any package versions in `Directory.Packages.props` as part of this change.
- Disabling repository-level security vulnerability alerts (GitHub Dependabot security alerts operate independently of `dependabot.yml`).

## Decisions

### Decision 1: Manual Dependency Management over Alternative Bot Tooling
- **Choice**: Transition to purely manual updates performed on-demand or during scheduled maintenance.
- **Rationale**: The user explicitly requested no alternative tools. Central Package Management combined with pinned versions makes manual batching safer, avoiding automated PR spam and merge conflicts.
- **Alternative Considered**: Renovate with package grouping rules (rejected per user requirement).

### Decision 2: Complete Removal of `.github/dependabot.yml`
- **Choice**: Delete the file rather than commenting out or disabling intervals.
- **Rationale**: GitHub recognizes the absence of `dependabot.yml` on the default branch as the definitive signal to halt version update jobs. Removing unused files maintains a clean `.github/` folder.

## Risks / Trade-offs

- [Risk: Dependencies may fall behind without automated reminders] → **Mitigation**: Run `dotnet list package --outdated` periodically during sprint cycles or pre-release maintenance.
- [Risk: Unnoticed security vulnerabilities in transitive packages] → **Mitigation**: GitHub Dependabot Alerts (configured in repo `Settings > Code security and analysis`) continue to monitor for known CVEs independently of `dependabot.yml`.

## Migration Plan

1. Remove `.github/dependabot.yml`.
2. Commit and merge the deletion to the default branch (`dev`).
3. On GitHub web / CLI:
   - Close any remaining open pull requests created by Dependabot.
   - Delete `dependabot/nuget/*` remote branches.
4. Rollback: If automated version updates are ever needed again, `.github/dependabot.yml` can be restored from Git history.
