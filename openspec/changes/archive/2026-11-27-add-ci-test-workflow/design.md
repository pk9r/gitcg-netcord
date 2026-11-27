# Design

## Context

See `proposal.md` for motivation. The repository targets `.NET 11` (`net11.0`) across all active projects and uses Central Package Management via `Directory.Packages.props`. Key solution characteristics shaping CI:
- `GitcgNetCord.slnx` includes all source projects (`GitcgNetCord.MainApp`, `AppHost`, `GitcgSkia`, `HoyolabHttpClient`, `GitcgSharp.Shared`, `ServiceDefaults`, `SharedUtils`) and both test projects (`tests/GitcgNetCord.MainApp.Tests`, `tests/GitcgSkia.Tests`), while explicitly excluding `src/GitcgPainter/GitcgPainter.csproj` (`<Build Project="false" />`).
- `GitcgNetCord.MainApp.Tests` uses `Microsoft.EntityFrameworkCore.InMemory` (`10.0.14`) and xUnit (`2.9.3`) with no external PostgreSQL or Redis runtime dependencies.
- `GitcgSkia.Tests` mocks network calls via `FakeHttpMessageHandler`, while `GitcgSkia` embeds all font/image resources under `assets/**` and conditionally references `SkiaSharp.NativeAssets.Linux.NoDependencies` (`4.154.0`) on Linux.
- `origin/HEAD` points to `dev`, and `main` also exists as a long-lived branch on the remote.

## Goals / Non-Goals

**Goals:**
- Provide fast, deterministic compilation and xUnit test verification on every pull request and push to `dev` and `main`.
- Ensure parity with the Linux production container target (`ubuntu-latest`, `Release` configuration, Linux SkiaSharp native assets).
- Keep workflow configuration self-contained and easy to inspect via uploaded `.trx` test result artifacts.

**Non-Goals:**
- Modifying `.github/workflows/docker-release.yml` or `.github/workflows/gitcgvn-portainer-webhook.yml` (deferred to a future release-pipeline improvement).
- Provisioning external PostgreSQL/Redis containers or running Aspire AppHost integration tests in CI.
- Integrating third-party coverage reporting services (e.g., Codecov) or PR comment bots at this stage.

## Decisions

### Decision 1: Standalone `.github/workflows/ci.yml` on `ubuntu-latest` with `.NET 11.0.x`
- **Choice**: Create a dedicated workflow running on `ubuntu-latest` using `actions/checkout@v7` and `actions/setup-dotnet@v6` configured with `dotnet-version: '11.0.x'`, `cache: true`, and `cache-dependency-path` covering `Directory.Packages.props` and `**/*.csproj`.
- **Rationale**: Linux runners start quickly, match the Linux deployment target (`src/GitcgNetCord.MainApp/Dockerfile` uses `11.0-alpine`), exercise `SkiaSharp.NativeAssets.Linux.NoDependencies`, and run on Node 24-native action majors (`@v7` / `@v6`) without Node 20 deprecation warnings. Specifying `cache-dependency-path` is required because `actions/setup-dotnet` defaults to `packages.lock.json`, which this repository does not use; keying on `Directory.Packages.props` and `**/*.csproj` ensures CPM or project dependency updates invalidate the NuGet cache cleanly.
- **Alternative Considered**: Matrix builds across `windows-latest` and `ubuntu-latest` (rejected for now to keep CI fast and runner usage minimal).

### Decision 2: Solution-Wide `restore` -> `build -c Release` -> `test` on `GitcgNetCord.slnx`
- **Choice**: Execute three sequential steps against `GitcgNetCord.slnx`:
  1. `dotnet restore GitcgNetCord.slnx`
  2. `dotnet build GitcgNetCord.slnx --configuration Release --no-restore`
  3. `dotnet test GitcgNetCord.slnx --configuration Release --no-build --verbosity normal --logger trx --results-directory TestResults`
- **Rationale**: Building the solution verifies that `AppHost` and non-test projects also compile cleanly alongside test projects (while `GitcgPainter` is automatically skipped by `.slnx`). Building in `Release` catches compiler/analyzer or `#if` differences vs. production images, and splitting `build` and `test` with `--no-restore` / `--no-build` avoids redundant work and isolates compilation failures from test failures. Omitting a hardcoded `LogFileName` lets each test project (`GitcgNetCord.MainApp.Tests` and `GitcgSkia.Tests`) write its own `.trx` file without overwriting the other.
- **Alternative Considered**: Running `dotnet test` only on `tests/*.csproj` (faster, but skips verifying `AppHost` compilation).

### Decision 3: Branch Triggers (`dev`, `main`), Concurrency Cancellation, and No `paths-ignore`
- **Choice**: Trigger on `push` (`branches: [dev, main]`), `pull_request` (`branches: [dev, main]`), and `workflow_dispatch`, with `concurrency: { group: "${{ github.workflow }}-${{ github.ref }}", cancel-in-progress: true }`. Omit `paths-ignore` filters.
- **Rationale**: Covers both `dev` (default branch) and `main` without running duplicate checks on pushes to feature branches before a PR is opened. Canceling superseded runs saves runner minutes. Omitting `paths-ignore` ensures required status checks (if enabled via branch protection) never hang on documentation-only or OpenSpec-only PRs.
- **Alternative Considered**: Triggering `push` on `**` or adding `paths-ignore: ['**.md', 'openspec/**']` (rejected due to duplicate feature-branch runs and branch-protection status-check deadlocks).

### Decision 4: Upload `.trx` Test Results as Artifacts
- **Choice**: Add an `actions/upload-artifact@v7` step with `if: always()`, `retention-days: 7`, and `if-no-files-found: ignore` to upload `TestResults/`.
- **Rationale**: Gives maintainers downloadable structured xUnit output whenever a test fails without introducing third-party marketplace actions, and prevents artifact upload noise if a build failure occurs before tests run.
- **Alternative Considered**: Adding a third-party PR test reporter action (deferred to keep permissions minimal—`contents: read` only).

### Decision 5: Direct `dev` Push and Remote Verification via GitHub CLI (`gh`)
- **Choice**: Commit and push the workflow directly to `dev`, then verify the live run on GitHub Actions using `gh` (`gh run list`, `gh workflow run ci.yml --ref dev`, `gh run watch --exit-status`, and `gh run view`).
- **Rationale**: GitHub Actions only registers `workflow_dispatch` for workflows present on the default branch (`dev`), and pushing directly to `dev` matches the maintainer's workflow preference while immediately exercising the `push` trigger as well.
- **Alternative Considered**: Opening a temporary feature branch and PR via `gh pr create --base dev` (rejected per maintainer preference to push directly to `dev`).

## Risks / Trade-offs

- [Risk: `11.0.x` SDK availability or pre-release channel quirks on `ubuntu-latest` runners] → **Mitigation**: Verify `actions/setup-dotnet` resolution on the first GitHub Actions run after merge (and enable `include-prerelease` only if a preview SDK is ever needed; currently `11.0.100` GA is in use).
- [Risk: Restoring `AppHost` Aspire workloads/packages adds build time to PR checks] → **Mitigation**: Enable `actions/setup-dotnet` NuGet caching; if restore/build time grows noticeably later, `AppHost` can be split into a separate job or filtered out.
- [Risk: Docs-only PRs still trigger a full build and test run] → **Mitigation**: Acceptable trade-off in favor of safe GitHub branch protection compatibility and quick (~1–2 min) cached builds.
