# Tasks

## 1. Create GitHub Actions CI Workflow

- [x] 1.1 Create `.github/workflows/ci.yml` with `push` and `pull_request` triggers on `dev` and `main`, `workflow_dispatch`, `contents: read` permissions, and branch/PR `concurrency` cancellation; verify YAML syntax and trigger configuration match `design.md`.
- [x] 1.2 Configure the `build-and-test` job on `ubuntu-latest` with `actions/checkout@v7`, `actions/setup-dotnet@v6` (`dotnet-version: '11.0.x'`, `cache: true`, `cache-dependency-path` for `Directory.Packages.props` and `**/*.csproj`), sequential `dotnet restore`, `dotnet build --configuration Release --no-restore`, and `dotnet test --configuration Release --no-build --logger trx --results-directory TestResults` targeting `GitcgNetCord.slnx`, plus `actions/upload-artifact@v7` (`if: always()`, `retention-days: 7`, `if-no-files-found: ignore`) for `TestResults/`; verify all referenced paths and action inputs align with `Directory.Packages.props` and `GitcgNetCord.slnx`.

## 2. Local Verification

- [x] 2.1 Run `dotnet restore GitcgNetCord.slnx` and `dotnet build GitcgNetCord.slnx --configuration Release --no-restore` locally and verify the solution builds with zero errors (with `GitcgPainter` skipped per `GitcgNetCord.slnx`).
- [x] 2.2 Run and pass all xUnit tests in `GitcgNetCord.MainApp.Tests` and `GitcgSkia.Tests` using `dotnet test GitcgNetCord.slnx --configuration Release --no-build --logger trx --results-directory TestResults` (via `run-tests`) and confirm `.trx` files are generated for both test projects without overwriting each other, then remove the temporary local `TestResults/` directory if untracked.
- [x] 2.3 Verify `.github/workflows/docker-release.yml` and `.github/workflows/gitcgvn-portainer-webhook.yml` remain unmodified via `git status` / `git diff`.

## 3. Remote Verification via GitHub CLI

- [x] 3.1 Verify `gh auth status` has `repo` and `workflow` scopes, commit `.github/workflows/ci.yml` (and change artifacts if appropriate) on `dev` following Conventional Commits, push to `origin/dev`, and verify GitHub Actions registers and triggers the `CI` workflow (`gh workflow list` / `gh run list --workflow ci.yml`, plus `gh workflow run ci.yml --ref dev` if testing manual dispatch).
- [x] 3.2 Monitor the remote `CI` workflow run with `gh run watch <run-id> --exit-status` and inspect details via `gh run view <run-id>` to confirm `.NET 11.0.x` setup on `ubuntu-latest`, NuGet caching, passing xUnit tests (`94/94`), and `test-results` artifact upload.

## Workflow follow-up

- Archive the change via `/openspec-archive-change` once remote verification on GitHub Actions succeeds.
