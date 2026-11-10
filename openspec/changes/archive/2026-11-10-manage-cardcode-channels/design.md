# Design

## Context

The application runs on .NET 10 using Aspire with PostgreSQL (`AppDbContext`), EF Core, and NetCord for Discord Gateway and Application Commands. Currently, `DiscordCardCodeChannel` only tracks `Id` (Channel ID) and `SharingDecodeType`.

See `proposal.md` for problem motivation and `specs/cardcode-channel-management/spec.md` for behavioral requirements.

## Goals / Non-Goals

**Goals:**
- Associate registered channels with their respective Discord Guilds (`GuildId`) in PostgreSQL.
- Provide clean CRUD operations in `DiscordCardCodeChannelService`.
- Implement `/cardcode-channel` using NetCord's `SlashCommandGroupBuilder` with `add`, `remove`, and `list` subcommands.
- Enforce `Permissions.ManageChannels` natively via Discord application command permissions.
- Make `CardCodeGatewayHandler` dynamically use the configured `SharingDecodeType` instead of hardcoding `GameBackground`.

**Non-Goals:**
- Building a web dashboard UI for channel administration.
- Listening to channel deletion Discord gateway events to prune records automatically.

## Decisions

### 1. Slash Command Architecture: Group Builder vs Separate Root Commands
- **Decision**: Group all channel management actions under the `/cardcode-channel` root command with subcommands: `add`, `remove`, and `list`.
- **Rationale**: Groups all administrative tasks into a single slash command namespace, avoiding cluttering the server's root slash command menu.
- **Alternative considered**: Standalone commands like `/add-cardcode-channel`, `/remove-cardcode-channel`. Rejected due to menu pollution.

### 2. Authorization: Discord Native `DefaultGuildPermissions`
- **Decision**: Set `.WithDefaultGuildPermissions(Permissions.ManageChannels)` and `.AddContexts(InteractionContextType.Guild)`.
- **Rationale**: Discord natively disables or hides the command from members lacking this permission. Server administrators can freely delegate access to custom roles via Discord Server Settings -> Integrations -> Bot permissions without requiring code or DB configuration changes.
- **Alternative considered**: Hardcoded role IDs (similar to `RequireDevRoleAttribute`). Rejected because role IDs are specific to individual servers and not portable across guilds.

### 3. Data Model: Adding `GuildId` to `DiscordCardCodeChannel`
- **Decision**: Add `public ulong GuildId { get; set; }` to `DiscordCardCodeChannel` entity with an EF Core migration.
- **Rationale**: Channel IDs are unique snowflakes, but storing `GuildId` allows filtering by server (`/cardcode-channel list`), validating that the channel being added actually belongs to the caller's guild, and preventing cross-guild tampering.
- **Alternative considered**: Storing only `Id` and looking up guild ID via Discord Rest API. Rejected due to unnecessary Discord API calls and inability to run fast SQL queries for `/cardcode-channel list`.

### 4. Decode Type Resolution in Gateway Handler
- **Decision**: Map `dbChannel.SharingDecodeType` to the appropriate creator in `DeckImageCreatorCollection`:
  - `ImageGameBackground` → `deckImageCreatorCollection.GameBackground`
  - `ImageSimplest` → `deckImageCreatorCollection.Simplest`
  - `ImageGenshincards` → `deckImageCreatorCollection.Genshincards`
  - Default fallback → `GameBackground`
- **Rationale**: Enables per-channel customization of generated deck card images.

## Risks / Trade-offs

- **[Risk] Existing rows in database having default `GuildId = 0`**  
  → **Mitigation**: When updating existing records or on first query, `GuildId` can be updated, or EF Core migration sets column without breaking existing records.
- **[Risk] Interaction timeout on database delay**  
  → **Mitigation**: Defer interaction response with `InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral)` before querying the database.

## Migration Plan

1. Update `DiscordCardCodeChannel` model and `AppDbContext` configuration.
2. Add EF Core migration `AddGuildIdToDiscordCardCodeChannel`.
3. Apply migration to database during startup or via `dotnet ef database update`.

