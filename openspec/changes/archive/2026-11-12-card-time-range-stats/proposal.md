# Proposal

## Why

Currently, the bot's `/card` command only calculates and displays lifetime cumulative Genius Invokation TCG statistics retrieved from Hoyolab. Players cannot evaluate their win rate, character usage, or deck adjustments for a specific game version (e.g., version 4.7), between balance patches, or within custom training/tournament timeframes. Providing snapshot-based periodic analytics allows players to track progression, meta shifts, and performance across specific time windows.

## What Changes

- Add periodic card snapshot capture and storage in PostgreSQL (`CardSnapshot` with JSONB payload for card usage and proficiency).
- Add game version scheduling and tracking (`GameVersion` entity) for future releases (historical past versions are not pre-seeded due to lack of retrospective data).
- Add background worker (`VersionSnapshotWorker`) that automatically snapshots all active Hoyolab accounts upon new version releases with safe rate-limiting.
- Add user-facing snapshot management commands (`/card-snapshot save`, `/card-snapshot list`, `/card-snapshot delete`).
- Extend the `/card` slash command with optional parameters: `version`, `from`, and `to` supporting autocomplete for snapshot labels and game versions, rendering all output embeds and notices completely in English.
- Implement differential calculation logic (delta use count, delta proficiency/wins, and win rate) between any two snapshots or between a snapshot and live Hoyolab data.

## Capabilities

### New Capabilities
- `card-periodic-stats`: Capturing card snapshots, scheduling version checkpoints, calculating delta statistics across intervals, and displaying periodic stats in `/card`.

### Modified Capabilities
<!-- None: No existing specs are affected -->

## Impact

- **Database**: New entities `CardSnapshot` and `GameVersion` registered in `AppDbContext`, requiring an EF Core migration.
- **Discord Bot**:
  - `CardSlashCommand` extended with new parameters (`version`, `from`, `to`) and autocomplete providers.
  - New slash command module `/card-snapshot` for manual checkpoint management.
- **Background Tasks**: New hosted background service `VersionSnapshotWorker` registered in dependency injection.
- **External Dependencies**: Utilizes existing `HoyolabHttpClientService` with rate limiting during batch captures.
