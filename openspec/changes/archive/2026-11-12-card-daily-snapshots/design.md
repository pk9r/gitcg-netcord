# Design

## Context

The bot captures Genius Invokation TCG statistics snapshots from Hoyolab via `HoyolabHttpClientService` and persists them in `AppDbContext` as `CardSnapshot` records. Game version snapshots are captured on release dates by `VersionSnapshotWorker`, while custom snapshots are captured on-demand by users via `/card-snapshot save`. 

See `proposal.md` for user motivations and problem context.

## Goals / Non-Goals

**Goals:**
- Provide an opt-in toggle per Hoyolab account for automatic daily snapshot capture.
- Automate daily snapshot capture at game server reset time (04:00 AM server time per region) via an isolated background service.
- Implement a smart autocomplete filter in `/card` so daily snapshots do not overwhelm the 25-choice Discord autocomplete limit.
- Filter `/card-snapshot list` by default to preserve visibility for game versions and custom milestones.
- Restrict `/card-snapshot delete` to custom user snapshots only.

**Non-Goals:**
- Retention or automatic pruning of daily snapshots (daily snapshots are retained indefinitely at this stage).
- Role-based access or quota limits (daily snapshots are available to all users with active Hoyolab accounts).
- Modifying the core calculation logic in `ICardStatsCalculator` (it already computes deltas correctly given any start and end snapshot).

## Decisions

### 1. Database Model Additions
- **Field**: Add `bool IsDailySnapshotEnabled { get; set; } = false;` to `HoyolabAccount`.
- **Enum**: Add `Daily` to `SnapshotType` (`GameVersion`, `Patch`, `UserCustom`, `Daily`).
- **Rationale**: Storing the toggle on `HoyolabAccount` keeps configuration attached to the game account and region. When `ActiveHoyolabAccounts` are queried, EF Core includes `HoyolabAccount` directly.
- **Alternative considered**: Storing on `DiscordUser`. Rejected because a user may have multiple accounts in different regions or with different preferences.

### 2. Dedicated `DailySnapshotWorker`
- **Pattern**: Implement a new `BackgroundService` registered via `services.AddHostedService<DailySnapshotWorker>()`.
- **Execution Interval**: Ticks every 15 minutes.
- **Server Reset Time Handling**:
  - Genshin Impact daily reset is at 04:00 AM server local time:
    - `os_asia` & `os_cht`: UTC+8 (04:00 AM local = 20:00 UTC).
    - `os_euro` & `os_europe`: UTC+1 (04:00 AM local = 03:00 UTC).
    - `os_usa` & `os_america`: UTC-5 (04:00 AM local = 09:00 UTC).
  - The worker computes the current game date for each account's region:
    `serverGameDate = (utcNow + regionOffset - TimeSpan.FromHours(4)).Date`
  - Expected snapshot name: `$"daily-{serverGameDate:yyyyMMdd}"` (e.g. `daily-20261112`).
  - If a snapshot with this name already exists for the account, it is skipped.
  - If the current local server time is past 04:00 AM and no snapshot exists for today, the worker fetches stats from Hoyolab and saves the snapshot tagged as `SnapshotType.Daily`.
- **Rationale**: Isolates daily cadence logic from version checking. Resilient to bot restarts (idempotently captures missing snapshots if restarted after 04:00 AM).
- **Alternative considered**: Expanding `VersionSnapshotWorker`. Rejected to preserve separation of concerns and avoid combining version release polling with daily recurring sweeps.

### 3. Autocomplete Smart Filter (`CardSnapshotAutocompleteProvider`)
- **Default behavior**: When `option.Value` is empty or whitespace, return only `GameVersion` and `UserCustom` snapshots.
- **Query behavior**: When `option.Value` contains `"daily"` or starts with date patterns (e.g., `"202"` or numeric dates), include matching `SnapshotType.Daily` snapshots formatted as `[Daily] daily-yyyyMMdd (yyyy-MM-dd)`.
- **Rationale**: Discord caps interaction choices at 25 items. Without filtering, a user with 30 daily snapshots would lose all visibility into versions and custom snapshots in the autocomplete menu.

### 4. Slash Command Adjustments (`CardSnapshotSlashCommand`)
- **New Subcommand**: `/card-snapshot daily enabled:<bool>`:
  - Updates `hoyolabAccount.IsDailySnapshotEnabled`.
  - Responds with confirmation embed.
- **List Command (`ListAsync`)**:
  - Filter snapshot list query to display `GameVersion` and `UserCustom` in the 25 items shown.
  - Query total count of `Daily` snapshots and mention in the footer: `e.g. Total: X version/custom snapshots (+ Y daily snapshots)`.
- **Delete Command (`DeleteAsync`)**:
  - Before deletion, verify `snapshot.SnapshotType == SnapshotType.UserCustom`.
  - If `Daily` or `GameVersion`, reject deletion with error embed: `Only custom snapshots can be deleted manually.`

## Risks / Trade-offs

- **[Risk] Unbounded database growth with daily snapshots**  
  *Mitigation*: The current user base is small and card stat records are compact dictionaries (~80 character pairs). Indefinite retention is acceptable now; an automated pruning background task can be scheduled later if needed.

- **[Risk] Hoyolab API throttling or transient network failures during daily batch**  
  *Mitigation*: Maintain a 1500ms delay between processing accounts (`AccountThrottleDelay`). Wrap each account capture in try-catch so individual failures do not abort the worker cycle.
