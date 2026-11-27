# Proposal

## Why

The repository currently only has CD workflows for building and deploying release-tagged Docker images (`docker-release.yml` and `gitcgvn-portainer-webhook.yml`), with no automated CI pipeline to verify compilation or run unit tests on pull requests and branch pushes. Adding an automated CI build-and-test workflow catches regressions early before code is merged into `dev` or `main`, while leaving the existing release pipeline untouched for future improvements.

## What Changes

- Add a standalone GitHub Actions CI workflow (`.github/workflows/ci.yml`) that restores, builds in `Release` configuration, and runs all xUnit tests across `GitcgNetCord.slnx`.
- Trigger the CI workflow on `push` and `pull_request` events targeting `dev` and `main`, as well as manual `workflow_dispatch`.
- Configure workflow concurrency cancellation for in-progress runs on the same branch/PR and upload `.trx` test results as workflow artifacts on completion.
- Verify the live GitHub Actions workflow execution directly on `dev` using the GitHub CLI (`gh`) after pushing.
- Leave `.github/workflows/docker-release.yml` and `.github/workflows/gitcgvn-portainer-webhook.yml` unchanged.

## Capabilities

### New Capabilities
None. This change adds repository CI tooling only; runtime bot behavior, Discord commands, database schemas, and external service contracts remain unchanged (`skip_specs: true`).

### Modified Capabilities
None.

## Impact

- **CI/CD & Tooling**: Adds `.github/workflows/ci.yml` using GitHub-hosted `ubuntu-latest` runners, .NET 11 SDK (`11.0.x`), and NuGet package caching keyed on `Directory.Packages.props` and `**/*.csproj`. Verification commits and pushes the change directly to `dev` and uses `gh` CLI to trigger/inspect the remote run.
- **Infrastructure Requirements**: No new databases, caches, or Aspire AppHost resources are required. Existing unit tests (`GitcgNetCord.MainApp.Tests` using EF Core InMemory and `GitcgSkia.Tests` using `FakeHttpMessageHandler` and embedded SkiaSharp assets/Linux native binaries) execute without external service containers.
- **Release Pipeline**: Existing release workflows (`docker-release.yml`, `gitcgvn-portainer-webhook.yml`) are unaffected.
