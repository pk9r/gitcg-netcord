# Design: Customize OpenSpec Configuration

## Context

The repository uses OpenSpec to guide AI-assisted feature development and maintenance. By default, `openspec/config.yaml` contains empty placeholder comments under `schema: spec-driven`. OpenSpec supports injecting project-wide context (`context`), per-artifact instructions (`rules`), and execution guidance (`operations`).

See [proposal.md](file:///C:/Users/pk9r/source/repos/pk9r/gitcg-netcord/openspec/changes/customize-openspec-config/proposal.md) for motivation.

## Goals / Non-Goals

**Goals:**
- Provide a tailored `openspec/config.yaml` that enforces domain and technical constraints for `gitcg-netcord`.
- Explicitly integrate agent capabilities (Context7 MCP and Aspire skills) into planning and execution instructions.
- Ensure all Discord UI elements remain strictly in English while allowing multilingual collaboration.
- Prevent common technical pitfalls such as SkiaSharp native memory leaks and unverified API signatures.

**Non-Goals:**
- Forking or creating custom OpenSpec workflow schemas (the default `spec-driven` schema remains ideal and standard).
- Editing any C# application source files, build scripts, or project configurations.

## Decisions

### Decision 1: Customize via `config.yaml` instead of forking schema
- **Rationale**: The standard `spec-driven` schema (`proposal -> specs -> design -> tasks`) satisfies all requirements. OpenSpec's `config.yaml` provides dedicated extension points (`context`, `rules`, `operations`) to inject domain rules without having to maintain forked schema templates.
- **Alternatives considered**: `openspec schema fork spec-driven custom-schema` was evaluated, but schema forks introduce maintenance overhead when the upstream OpenSpec CLI updates.

### Decision 2: Enforce English for user-facing Discord artifacts
- **Rationale**: Previous specs (`card-daily-thread-report`, `deck-image-rendering`, `cardcode-channel-management`) mandate that all Discord command descriptions, options, embeds, and status messages are rendered in English. Placing this in both `context` and `rules.specs` ensures the agent adheres to this boundary regardless of prompt language.

### Decision 3: Explicitly bind Context7 MCP in `rules.design` and `operations.apply`
- **Rationale**: Libraries like NetCord (`1.0.0-beta.29`), SkiaSharp (`4.154`), and Aspire (`13.6`) frequently undergo breaking API changes. Guiding the agent to query Context7 MCP before drafting code designs or implementing unfamiliar APIs prevents syntax errors and deprecated patterns.

### Decision 4: Integrate Aspire Skill and CLI into `tasks` and `operations.apply`
- **Rationale**: `gitcg-netcord` manages its PostgreSQL, Redis, and service telemetry through .NET Aspire (`src/AppHost` and `src/ServiceDefaults`). Directing the agent to account for AppHost wiring in `tasks` and use Aspire tools for health/trace inspection during `apply` ensures end-to-end integration quality.

## Risks / Trade-offs

- **[Risk] Instruction verbosity causing agent drift** → *Mitigation*: Keep each rule focused and bulleted, avoiding general coding tutorials and specifying only project-specific constraints.
- **[Risk] Schema invalidation on CLI updates** → *Mitigation*: Restrict configuration to officially supported OpenSpec fields (`schema`, `context`, `rules`, `operations`) and test with `openspec doctor`.
