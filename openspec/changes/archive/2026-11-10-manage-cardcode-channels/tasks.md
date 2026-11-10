# Tasks

## 1. Database Model & Migration

- [x] 1.1 Add `GuildId` property to `DiscordCardCodeChannel` and configure model mapping in `AppDbContext`
- [x] 1.2 Generate EF Core migration `AddGuildIdToDiscordCardCodeChannel` using `dotnet ef migrations add` and verify migration files are generated without compilation errors

## 2. Service Layer Updates

- [x] 2.1 Implement `AddAsync(ulong channelId, ulong guildId, SharingDecodeType decodeType, CancellationToken ct = default)` in `DiscordCardCodeChannelService` supporting insert or update
- [x] 2.2 Implement `RemoveAsync(ulong channelId, CancellationToken ct = default)` and `GetByGuildAsync(ulong guildId, CancellationToken ct = default)` in `DiscordCardCodeChannelService`

## 3. Slash Command Implementation

- [x] 3.1 Create `CardCodeChannelSlashCommand` implementing `AddAsync`, `RemoveAsync`, and `ListAsync` handlers with channel selection defaults and ephemeral responses
- [x] 3.2 Register `/cardcode-channel` slash command group in `CardCodeModule` configured with `Permissions.ManageChannels` and `InteractionContextType.Guild`

## 4. Gateway Handler Integration & Verification

- [x] 4.1 Update `CardCodeGatewayHandler` to dynamically resolve deck image creator from `dbChannel.SharingDecodeType`
- [x] 4.2 Build the entire solution using `dotnet build` and verify that compilation succeeds with zero errors

