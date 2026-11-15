# Proposal: Prune Redundant and Unnecessary Descriptions from OpenSpec Configuration

## Why

Over time, `openspec/config.yaml` has accumulated redundant rules, verbose documentation, and duplicate constraints across `context`, `rules`, and `operations.apply.guidance`. Notably:
1. OpenSpec schema built-ins (RFC 2119 keyword enforcement, GIVEN/WHEN/THEN testable scenarios, SemVer impact, In-Scope/Out-of-Scope boundaries) are already enforced by default templates and `openspec validate`.
2. Developer environment management commands (such as a 730-character explanation of `scripts/optimize-agent-skills.ps1` with 12 disabled plugin names) were placed inside runtime `operations.apply.guidance`, bloating agent context on every task execution.
3. Multiple duplicate directives (English UI requirement, SkiaSharp disposal, and Context7 MCP usage) are reiterated across multiple sections despite already being strictly enforced by repository context or global rules.

Pruning these unnecessary descriptions reduces token overhead by over 50%, eliminates contradictory or redundant instructions, and delivers concise, high-signal guidance to agents.

## What Changes

- **Prune Built-in Schema Boilerplate**:
  - Remove RFC 2119 and GIVEN/WHEN/THEN format declarations from `rules.specs` (already enforced by OpenSpec validator).
  - Remove generic SemVer and In-Scope/Out-of-Scope boilerplate from `rules.proposal`.
  - Remove generic "keep tasks small" guideline from `rules.tasks`.
  - Remove delta spec sync reminder from `operations.archive.guidance` (handled natively by `openspec archive`).
- **Remove Misplaced Developer Operations Guidance**:
  - Remove the lengthy `Agent Skills & Context Optimization` entry from `operations.apply.guidance` (belongs to `README.md` and repository setup documentation, not task implementation).
  - Consolidate redundant skill mentions in `Dotnet Skills Utilization` so guidance focuses strictly on action-oriented rules.
- **Deduplicate Cross-Section Rules**:
  - Remove repetitive SkiaSharp disposal reminder at the end of `operations.apply.guidance` (already enforced by `context` and `rules.design`).
  - Remove duplicate English language rule from `rules.specs` (already enforced by `context`).
  - Remove duplicate generic test pass statement from `operations.apply.guidance` (covered by `rules.tasks` and `Scoped Testing`).
  - Remove redundant Context7 MCP reminder from `rules.design` and `operations.apply.guidance` (governed by Agent User Global Rules).

### Scope Boundaries

- **In-Scope**:
  - Streamlining `openspec/config.yaml` to retain only critical, repository-specific architectural and operational constraints.
  - Ensuring the resulting configuration passes OpenSpec validation cleanly.
- **Out-of-Scope**:
  - Modifying C# source code, project dependencies, or test suites.
  - Modifying OpenSpec specs under `openspec/specs/`.
  - Altering `scripts/optimize-agent-skills.ps1` or `README.md`.

### SemVer Impact
- **SemVer**: PATCH
- **Breaking Changes**: None. This is an internal workflow configuration optimization.

### Infrastructure Requirements
- **Infrastructure**: None.

## Capabilities

### New Capabilities
None (`skip_specs: true` enabled). This change refines internal repository agent configuration.

### Modified Capabilities
None (`skip_specs: true` enabled).

## Impact

- **Affected Files**: `openspec/config.yaml`
- **Agent Behavior**: Improved context density, faster instruction parsing, zero loss of engineering constraints (EF Core migration safety, Aspire lifecycle management, layer separation, and memory safety rules remain fully intact).
