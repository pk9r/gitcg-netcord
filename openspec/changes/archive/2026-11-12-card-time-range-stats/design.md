# Design

## Context

The bot currently runs on .NET 9 with PostgreSQL (`AppDbContext`), NetCord for Discord slash commands, and an existing `HoyolabHttpClientService` communicating with Hoyolab APIs. Hoyolab returns cumulative lifetime counters (`use_count`, `proficiency`) for all cards. To support periodic and version-scoped statistics, the bot must store historical snapshots at designated points in time and compute differences (deltas) on demand.

## Goals / Non-Goals

**Goals:**
- Provide a storage schema in PostgreSQL for version milestones and user card snapshots with minimal storage footprint.
- Automatically capture card snapshots for all active accounts upon new game version releases via a throttled background service.
- Enable users to manage custom checkpoints via `/card-snapshot`.
- Extend `/card` with `version`, `from`, and `to` options, accompanied by autocomplete providers.
- Implement an in-memory delta calculator service for computing character and action card metrics between any two snapshots or between a snapshot and live API data.

**Non-Goals:**
- Reconstructing historical match records prior to when an account was first registered and snapshotted.
- Storing individual match history logs (Hoyolab does not provide match logs).
- Real-time card tracking during active matches.

## Decisions

### 1. JSONB Storage for `CardSnapshot.CardData`
- **Decision**: Store card metrics as a serialized JSONB column (`Dictionary<int, CardStatSummary>` mapping `CardId` to `{ UseCount, Proficiency }`) on the `CardSnapshot` entity.
- **Rationale**: An active player has ~200-300 cards. Storing each card as a row in a relational table would create hundreds of rows per snapshot. A JSONB payload is only ~3-4 KB per snapshot, allows fast point-in-time reads, and supports trivial $O(N)$ in-memory delta subtraction without database table joins.
- **Alternatives considered**:
  - *Relational `CardSnapshotItem` table*: Produces millions of rows quickly with higher index overhead, offering no analytical advantage since queries are strictly per-user and per-snapshot.

### 2. Database-Driven Game Version Milestones (`GameVersion`)
- **Decision**: Store game version definitions (`VersionNumber`, `ReleaseDate`, `IsProcessed`) in a `GameVersions` table for tracking future game versions. Historical past versions (e.g. 3.3–5.6) are explicitly not pre-seeded because retroactive snapshot data does not exist.
- **Rationale**: Version release dates and maintenance schedules can shift; having them in the database allows updating schedules and adding minor hotfix checkpoints without recompiling and redeploying the bot.
- **Alternatives considered**:
  - *Hardcoded static class*: Inflexible when maintenance or unexpected version shifts occur.
  - *Pre-seeding historical past versions*: Rejected because no retroactive card statistics exist for prior versions, which would mislead users into expecting past data.

### 3. Rate-Limited Background Worker (`VersionSnapshotWorker`)
- **Decision**: Implement a .NET `BackgroundService` that monitors `GameVersion` release dates. When a new version is due and unprocessed, it sequentially fetches and persists snapshots for active accounts with a 1.5–2.0 second throttle between requests.
- **Rationale**: Hoyolab aggressively throttles IP addresses and flags accounts if bombarded with concurrent requests. Sequential rate-limiting prevents 429 Too Many Requests errors. Account-level failures (e.g. invalid tokens) are logged and skipped without breaking the batch.
- **Alternatives considered**:
  - *Parallel batch fetch with `Task.WhenAll`*: High risk of triggering Cloudflare / Hoyolab rate limiting and token revocation.
  - *On-demand snapshot on first user interaction in new patch*: Misses users who don't run commands immediately upon patch release, resulting in skewed stats.

### 4. Modular Delta Calculation Service (`ICardStatsCalculator`)
- **Decision**: Extract delta math and ranking logic out of `CardSlashCommand` into an injectable service.
- **Rationale**: Computing $\Delta UseCount$, $\Delta Proficiency$, total matches $(\sum \Delta UseCount_{char} / 3)$, win rates, and ranking (best win rate, lowest win rate, top character use, top action use) should be reusable across live vs snapshot comparisons and easily unit-tested.

### 5. English Presentation for Card Command Embeds
- **Decision**: Standardize all Discord embeds, field labels, notice messages, parameter descriptions, and autocomplete choices (`Present (Live)`, `Version X.Y`) in English.
- **Rationale**: The bot does not yet have an internationalization / multi-language (i18n) framework. Outputting embeds entirely in English maintains clarity, consistency, and alignment with Discord community conventions.

## Risks / Trade-offs

- **[Risk] Hoyolab API Rate Limiting & Cloudflare Challenges during Batch Worker**
  → *Mitigation*: Process accounts sequentially with a 1.5s delay. Use exponential backoff if HTTP 429 is received.
- **[Risk] Invalid or Expired Hoyolab Tokens during Batch Capture**
  → *Mitigation*: Catch exceptions per account, log warning, record failure flag, and proceed to next account.
- **[Risk] User requests a version where no snapshot exists (e.g., account registered in 5.6 requests 4.7)**
  → *Mitigation*: Validate snapshot availability prior to calculation. If the start snapshot is missing, return an informative error showing the earliest available snapshot for that user.
- **[Risk] New Cards Added in Later Version**
  → *Mitigation*: When comparing $Snapshot_B$ (newer) to $Snapshot_A$ (older), cards present in $Snapshot_B$ but absent in $Snapshot_A$ are treated as having 0 use count and 0 proficiency in $Snapshot_A$.

## Migration Plan

1. Create EF Core migration: `AddCardSnapshotAndGameVersion` (without past version seeds).
2. Apply migration to PostgreSQL.
3. Register `VersionSnapshotWorker` and `ICardStatsCalculator` in dependency injection.
