# Proposal: Customize OpenSpec Configuration for gitcg-netcord

## Why

The current OpenSpec configuration (`openspec/config.yaml`) contains only default placeholder comments without project-specific constraints, guidelines, or tool integrations. 

As a result, AI coding assistants lack explicit grounding regarding the project's technology stack (.NET 10, Aspire, NetCord, SkiaSharp, EF Core/Postgres), the strict requirement that all user-facing Discord messages and embeds must be in English, memory disposal safety for native SkiaSharp resources, and active guidance on leveraging Context7 MCP for up-to-date documentation and Aspire skills for AppHost lifecycle management. Establishing this configuration ensures higher quality proposals, designs, and task implementations that align consistently with project standards.

## What Changes

- Update `openspec/config.yaml` to define a comprehensive `context` block detailing project identity, stack versions, language rules, graphics safety, and Git commit conventions.
- Add artifact-specific `rules`:
  - `proposal`: SemVer impact assessment, explicit scope boundaries, and infrastructure/permission requirement checks.
  - `specs`: RFC 2119 keywords, testable GIVEN/WHEN/THEN scenarios, Discord permission validations, and English UI enforcement.
  - `design`: Mandatory Context7 MCP lookups for 3rd-party library APIs (NetCord, Aspire, SkiaSharp), architectural layer separation, Aspire AppHost orchestration alignment, and Skia native resource disposal.
  - `tasks`: Structured implementation sequencing (Data/Migrations -> AppHost -> Core/Skia -> NetCord -> Unit Tests), small testable units, and explicit `dotnet test` execution.
- Add workflow `operations` guidance:
  - `apply.guidance`: Context7 MCP fallback for API ambiguities, Aspire skill/CLI usage for AppHost health and telemetry, and test passing enforcement.
  - `archive.guidance`: Spec synchronization verification and cleanup of build artifacts (e.g., binlogs).

## Capabilities

### New Capabilities
None. This change configures repository development workflows and AI assistant behavior (`skip_specs: true`).

### Modified Capabilities
None.

## Impact

- **Affected Files**: `openspec/config.yaml`
- **APIs & Dependencies**: No runtime code, APIs, or software dependencies are modified.
- **Workflow & Tooling**: Directly improves all subsequent OpenSpec workflows (`propose`, `explore`, `apply`, `archive`) by supplying contextual rules and tool integrations (Context7 MCP, Aspire skill) to AI coding agents.
