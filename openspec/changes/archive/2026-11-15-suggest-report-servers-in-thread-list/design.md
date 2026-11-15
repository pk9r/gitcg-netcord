# Design

## Context

See `proposal.md` for motivation.

Currently, `CardSnapshotSlashCommand.ThreadListAsync` handles `/card-snapshot thread list`. It verifies whether `context.Channel is GuildThread thread`. If not, it returns a plain error message. If it is a thread, it queries `DiscordDailyReportService.GetThreadSubscriptionsAsync(thread.Id)` and renders an embed with subscribed accounts or a message stating no accounts are subscribed.

The application already has:
- `DiscordGuildConfig` entity storing guild configuration, specifically public `InviteUrl`.
- `DiscordDailyReportChannel` entity storing approved parent channels for daily reports per guild.
- `DiscordGuildConfigService` and `DiscordDailyReportService` registered as scoped services in dependency injection.

## Goals / Non-Goals

**Goals:**
- Provide a clean query in the service layer to retrieve eligible community servers for daily reports (servers with both a configured public `InviteUrl` and at least one approved `DiscordDailyReportChannel`).
- Present server suggestions seamlessly in `/card-snapshot thread list` across all 3 execution contexts:
  1. Inside a thread with active subscriptions.
  2. Inside a thread with no active subscriptions.
  3. Outside a thread (in standard text channels or DMs) with helpful guidance instead of a dead-end error message.
- Enhance Discord UX with both an embed field (server names, report channel count, invite links) and an interactive `ActionRow` containing up to 3 `LinkButtonProperties` for one-click server joining.
- Comply strictly with all project guidelines: all user-facing strings in English, layer separation, and zero native/database resource leaks.

**Non-Goals:**
- Allowing subscription creation directly in non-thread channels (subscriptions remain strictly bound to threads and DMs).
- Creating interactive select menus or custom bot wizards for external guilds.
- Modifying the underlying database schema or EF Core entities (the existing entities have all required data).

## Decisions

### Decision 1: Query location and data model
- **Choice**: Add `GetSuggestedReportGuildsAsync(ulong? excludeGuildId = null, int limit = 3, CancellationToken cancellationToken = default)` to `DiscordDailyReportService`, returning a lightweight record `SuggestedReportGuild(ulong GuildId, string InviteUrl, int ReportChannelCount)`. The EF Core LINQ query filters candidate guilds directly in the `Where` clause using `dbContext.DiscordDailyReportChannels.Any(ch => ch.GuildId == c.GuildId)` (generating an efficient SQL `EXISTS` predicate in PostgreSQL) and performs database-side projection and ordering using an anonymous type before mapping in-memory to `SuggestedReportGuild`.
- **Rationale**: `DiscordDailyReportService` is the domain authority on report channels and destinations. Filtering on custom projected constructor parameters (e.g. `Where(x => x.ReportChannelCount > 0)`) cannot be translated to SQL by Npgsql/EF Core. Filtering via `Any` and projecting anonymous types ensures 100% translatability on PostgreSQL while maintaining a single query without N+1 overhead.
- **Alternatives Considered**:
  - *Query in `DiscordGuildConfigService`*: `DiscordGuildConfigService` only manages guild metadata (invite URLs). It does not own report channel business logic.
  - *Query separately in command handler*: Violates layer separation and risks multiple database roundtrips.

### Decision 2: Discord Embed and Component Layout
- **Choice**:
  - Inside Thread with Subscriptions: Keep existing subscription list in embed description; append an embed field titled `"Suggested Report Servers"` with formatted server details and invite URLs. If valid HTTP/HTTPS invite URLs exist, append an `ActionRowProperties` with `LinkButtonProperties` (up to 3 buttons).
  - Inside Thread without Subscriptions: Display an embed with title `"Thread Daily Report Subscriptions"` and description stating no accounts are subscribed, followed by the `"Suggested Report Servers"` embed field and Link Buttons.
  - Outside Thread (Non-thread channel or DM): Display an embed with title `"Daily Report Thread Information"` explaining that subscriptions are managed inside Discord threads under approved report channels, followed by the `"Suggested Report Servers"` embed field and Link Buttons.
- **Rationale**: Combining embed fields and Link Buttons satisfies Discord UX best practices: embed fields provide readable server descriptions and links that persist even if buttons expire or on mobile clients, while Link Buttons offer immediate one-click navigation.
- **Alternatives Considered**:
  - *Link Buttons only*: Users on certain Discord clients or screen readers may miss context about why the servers are suggested.
  - *Text only*: Requires users to copy-paste or click raw markdown links, missing the native Discord server invite button UX.

### Decision 3: Guild Name Resolution and Fallback
- **Choice**: Attempt to resolve the guild's display name from `context.Client.Cache.Guilds.TryGetValue(guildId, out var guild)`:
  - If found in cache, use the guild's actual name (e.g., `Teyvat TCG Lounge`).
  - If not cached or truncated, use a generic clean fallback (e.g., `Community Server`).
  - Cap button labels to Discord's 80-character button limit.
- **Rationale**: The Gateway client maintains a local cache of joined guilds. Utilizing cached guild names provides an appealing and authentic presentation without requiring extra Discord REST API requests.

### Decision 4: Interaction Error Resilience
- **Choice**: Wrap the body of `CardSnapshotSlashCommand.ThreadListAsync` in a `try...catch` block. If an unexpected error occurs during database querying or response building, catch the exception and safely modify the deferred response via `context.Interaction.ModifyResponseAsync` with a user-friendly English error notice.
- **Rationale**: Because `ThreadListAsync` immediately acknowledges the interaction via `SendResponseAsync(DeferredMessage)`, any unhandled exception escaping the command method triggers a secondary NetCord error (`400 Bad Request: Interaction has already been acknowledged`) and leaves the Discord UI in an indefinite loading state. Explicit error catching ensures resilience and clean error messaging.

## Risks / Trade-offs

- **[Risk] Guild invite links may expire or become invalid over time.**
  - *Mitigation*: Only URLs matching valid absolute HTTP/HTTPS schemes are rendered as link buttons. Server administrators can update or remove invite URLs via `/server invite set` and `/server invite remove`.
- **[Risk] Large number of registered servers cluttering the UI.**
  - *Mitigation*: Limit the query to a maximum of 3 top servers (ordered by number of approved report channels or creation date). Discord allows a maximum of 5 buttons per ActionRow, so 3 buttons comfortably fit within a single row.
- **[Risk] Current guild suggested to its own members.**
  - *Mitigation*: The service method accepts `excludeGuildId` to ensure the current server is never suggested to users already in that server.

## Migration Plan

No database schema migrations or data migrations required. Deployment only requires deploying the updated application binary. Rollback can be done simply by redeploying the previous version.
