# Tasks

## 1. Prune OpenSpec Configuration

- [x] 1.1 Update `rules` in `openspec/config.yaml` to remove OpenSpec built-in boilerplate (RFC 2119, GIVEN/WHEN/THEN, SemVer/Scope boundaries, small tasks) and duplicate English UI rules.
- [x] 1.2 Update `operations.apply.guidance` and `operations.archive.guidance` in `openspec/config.yaml` to remove misplaced developer script documentation (`Agent Skills & Context Optimization`), duplicate IDisposable reminders, redundant Context7 rules, and generic test pass statements.

## 2. Verification

- [x] 2.1 Validate OpenSpec changes and specs using `openspec validate prune-openspec-config` and `openspec validate --specs` to confirm clean validation without errors.
- [x] 2.2 Run scoped xUnit tests with `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` to ensure repository test baseline remains intact.

## Workflow follow-up

- Archive the change via `/openspec-archive-change` once implemented and verified.
