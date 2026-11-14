# Design

## Context

In `gitcg-netcord`, daily card statistics snapshots and thread report delivery are managed through EF Core (PostgreSQL), NetCord (v1.0.0-beta.29), and a background worker. Currently:
- Daily snapshots are toggled solely as a boolean flag `IsDailySnapshotEnabled` on `HoyolabAccount`.
- Thread report delivery is tracked in `DiscordDailyReportThread`, but requires the user to already be inside a pre-created thread whose parent channel is in `DiscordDailyReportChannel`.
- The bot creates interactions using ephemeral responses, modal dialogs, and component select menus.
- See `proposal.md` for problem motivation and capability scope.

## Goals / Non-Goals

**Goals:**
- Provide a clean domain boundary for general server settings by introducing `DiscordGuildConfig` and `/server invite` commands.
- Provide a guided, interactive onboarding flow right after adding a Hoyolab account (`SelectGameRecordComponentInteraction` -> Daily Snapshot Prompt -> Destination Selection).
- Enable Direct Messages (DM) as a first-class report destination alongside Server Threads without requiring server membership.
- Automatically create public threads (`client.Rest.CreateGuildThreadAsync`) in approved parent channels with user auto-join mentions and register subscriptions.
- Support multi-destination report delivery per Hoyolab account (e.g. DM + Server Threads).
- Adjust permission checks on `/card-snapshot thread` commands to support bot-created threads (`thread.OwnerId == bot.Id`).

**Non-Goals:**
- Automated channel/category creation wizards for server administrators.
- Modifying the underlying daily snapshot calculation or SkiaSharp deck rendering pipeline.
- Web UI or custom user delivery scheduling.

## Decisions

### Decision 1: Dedicated Server Configuration Entity (`DiscordGuildConfig`)
- **Rationale**: Server metadata (such as a public invite URL) belongs to the guild as a whole and may be utilized across multiple bot features (matchmaking, community discovery, deck sharing). Attaching `InviteUrl` to `DiscordDailyReportChannel` would violate database normalization (1 guild has N report channels) and force admins to re-enter invite URLs for each channel.
- **Implementation**:
  ```csharp
  public class DiscordGuildConfig
  {
      public ulong GuildId { get; set; } // Primary Key
      public string? InviteUrl { get; set; }
      public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
      public DateTimeOffset? UpdatedAt { get; set; }
  }
  ```
  Exposed via `DiscordGuildConfigService` and registered in `ServiceCollectionExtensions`.
- **Alternatives Considered**: Storing invite URLs in `DiscordDailyReportChannel` (rejected: data duplication and awkward UX).

### Decision 2: Direct Message Destination via `ThreadId = dmChannel.Id` and `GuildId = 0`
- **Rationale**: In Discord API, a DM channel is a `TextChannel` with its own unique channel ID. `DailyReportDeliveryService` delivers reports using:
  ```csharp
  await client.Rest.SendMessageAsync(subscription.ThreadId, messageProperties, cancellationToken: cancellationToken);
  ```
  By saving `subscription.ThreadId = dmChannel.Id` and `subscription.GuildId = 0`, the existing delivery pipeline immediately supports DM delivery without any architectural rewrites or new tables. The unique constraint `(ThreadId, HoyolabAccountId)` naturally accommodates both DM and multiple server thread subscriptions.
- **Alternatives Considered**: Creating a dedicated `DiscordDailyReportDm` table and duplicate delivery loop (rejected: unnecessary complexity, code duplication).

### Decision 3: Interactive Onboarding Flow with Action Buttons & Select Menus
- **Rationale**: Immediately upon successfully adding an account in `SelectGameRecordComponentInteraction`, the bot replies with an ephemeral message containing:
  1. Confirmation embed showing linked character nickname and UID.
  2. Action Row with buttons:
     - `daily-snapshot-enable:{accountId}` (Style: Success) -> "Enable Daily Snapshot"
     - `daily-snapshot-skip` (Style: Secondary) -> "No, Maybe Later"
  3. When "Enable Daily Snapshot" is clicked:
     - The bot sets `account.IsDailySnapshotEnabled = true`.
     - The message updates to display destination choices with compact identifiers:
       - `drpt-dm:{accountId}` -> "Receive in DM"
       - `drpt-ch:{channelId}:{accountId}` -> "Create Server Thread" / "Thread in #{channel}" (resolved contextually via `context.Interaction.GuildId`, ensuring the string never exceeds Discord's 100-character limit)
       - Link buttons for community servers with verified HTTP/HTTPS invite URLs
       - `drpt-skip` -> "Skip (Data Only)"
- **Alternatives Considered**: Showing static slash command hints (rejected: high friction, users frequently abandon setup).

### Decision 4: Automated Thread Creation & Auto-Join Mention
- **Rationale**: When the user selects a server report channel, the bot calls NetCord's `RestClient.CreateGuildThreadAsync`:
  ```csharp
  var threadProps = new GuildThreadProperties($"📊 TCG Daily - {nickname} ({account.GameRoleId})")
  {
      AutoArchiveDuration = ThreadArchiveDuration.OneWeek
  };
  var thread = await client.Rest.CreateGuildThreadAsync(parentChannelId, threadProps, cancellationToken: cancellationToken);
  ```
  Immediately after creation, the bot sends an initial message mentioning `<@{userId}>`:
  ```csharp
  await thread.SendMessageAsync($"Welcome <@{context.User.Id}>! Daily TCG statistics reports for UID `{account.GameRoleId}` will be delivered here.");
  ```
  Mentioning the user automatically adds them to the Discord thread and notifies them without requiring extra invite API calls.
- **Alternatives Considered**: Creating threads from existing messages (rejected: requires an existing message in the channel).

### Decision 5: Expanded Thread Ownership Check
- **Rationale**: When the bot creates a thread, Discord assigns `thread.OwnerId = bot.Id`. In `/card-snapshot thread unsubscribe` and `/card-snapshot thread list`, the check:
  ```csharp
  if (thread.OwnerId != context.User.Id)
  ```
  would lock out the user who actually subscribed the account.
  The check is updated to:
  ```csharp
  var isSubscriber = await reportService.IsUserSubscriberAsync(thread.Id, context.User.Id);
  if (thread.OwnerId != context.User.Id && !isSubscriber)
  {
      // reject with error
  }
  ```
- **Alternatives Considered**: Transferring thread ownership (Discord does not support transferring thread ownership to regular users).

## Risks / Trade-offs

- **[Missing Bot Permissions in Parent Channel]** → Creating threads requires `CreatePublicThreads` and `SendMessagesInThreads`. If the bot lacks permissions, REST calls fail with 403 Forbidden.
  - *Mitigation*: Catch `RestException` (403 Forbidden) during thread creation and display an informative ephemeral message advising the user/admin of the missing bot permissions.
- **[User Blocks Bot DMs]** → If a user selects DM reports but blocks DMs from server members/bots, message delivery fails with 403 Forbidden.
  - *Mitigation*: `DailyReportDeliveryService` already catches 403 Forbidden and marks `subscription.IsEnabled = false` without crashing.
- **[Duplicate Threads]** → If a user runs setup multiple times, multiple threads could be spawned for the same account in the same guild.
  - *Mitigation*: Before creating a thread, query `DiscordDailyReportThreads` for existing subscriptions matching `(GuildId, HoyolabAccountId)`. If found, reuse the existing thread and return a link to `<#{threadId}>`.
- **[Discord Component custom_id 100-Character Limit & Form Validation]** → Discord enforces a hard 100-character limit on button `custom_id` values and requires valid URI schemes on link buttons. Exceeding the length results in a `400 Bad Request (Invalid Form Body)` exception.
  - *Mitigation*: Shortened identifier prefixes (`drpt-ch`, `drpt-dm`, `drpt-skip`), omit redundant guild IDs from payload strings (resolved from interaction context), and enforce `Uri.TryCreate` validation on all invite URLs.

## Migration Plan

1. Generate EF Core migration `AddDiscordGuildConfig` to create `DiscordGuildConfigs` table in PostgreSQL.
2. Apply migration out-of-band using `dotnet ef database update --project src/GitcgNetCord.MainApp --startup-project src/GitcgNetCord.MainApp`, strictly adhering to the `standardize-ef-migrations-and-dotnet-skills` policy (zero runtime migrations in `Program.cs`).
3. No data backfill is required for existing accounts or channels.
