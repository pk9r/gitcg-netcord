# Proposal

## Why

Currently, the bot only automatically records Genius Invokation TCG card statistics snapshots upon new game version releases (`VersionSnapshotWorker`), and users must manually execute `/card-snapshot save` to record intermediate checkpoints. Users wishing to track daily progression and day-over-day statistics have to manually save snapshots every day.

Enabling an automated daily snapshot capture option per Hoyolab account allows dedicated players to record daily match and proficiency progress automatically. Furthermore, to avoid cluttering Discord's 25-choice autocomplete menu and snapshot lists with hundreds of daily entries, daily snapshots must be filtered intelligently so that regular version and custom milestone tracking remains clean and usable.

## What Changes

- Add an opt-in toggle per Hoyolab account (`IsDailySnapshotEnabled`) to allow automatic daily snapshot capture.
- Add `/card-snapshot daily enabled:<true/false>` slash command allowing users to toggle daily snapshots for their currently active Hoyolab account.
- Add a dedicated background hosted service (`DailySnapshotWorker`) that periodically evaluates game server reset times (04:00 AM server time for Asia, Europe, and America regions) and automatically captures a snapshot named `daily-yyyyMMdd` tagged as `SnapshotType.Daily`.
- Update `CardSnapshotAutocompleteProvider` with a smart filter: daily snapshots are excluded from default `/card` autocomplete recommendations and only suggested when users search with keywords like `daily` or a specific date/year.
- Update `/card-snapshot list` to display only `GameVersion` and `UserCustom` snapshots by default, while summarizing the total count of daily snapshots in the footer.
- Restrict `/card-snapshot delete` so users can only delete `UserCustom` snapshots; attempts to delete system-managed `GameVersion` or `Daily` snapshots are rejected with an informative message.

## Capabilities

### New Capabilities
<!-- None -->

### Modified Capabilities
- `card-periodic-stats`: Adds daily automated snapshot tracking for opt-in accounts, daily snapshot toggle command, smart autocomplete filtering for daily snapshots, default list filtering, and snapshot deletion restrictions.

## Impact

- **Database**: Add `IsDailySnapshotEnabled` column to `HoyolabAccount` and add `Daily` to the `SnapshotType` enum.
- **Background Tasks**: New hosted service `DailySnapshotWorker` registered in dependency injection alongside `VersionSnapshotWorker`.
- **Discord Slash Commands**:
  - `CardSnapshotModule`: Add `/card-snapshot daily enabled:<bool>` subcommand.
  - `CardSnapshotSlashCommand`: Handle daily toggle, update `ListAsync` filtering, and update `DeleteAsync` to reject non-custom snapshots.
  - `CardSnapshotAutocompleteProvider`: Filter out daily snapshots unless query matches.
- **Hoyolab API**: Accounts with daily snapshots enabled will issue one card stats query per day after server reset. Rate limiting is preserved.
