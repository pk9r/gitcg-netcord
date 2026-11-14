# Tasks

## 1. Domain Models and Database Migrations

- [x] 1.1 Create `DiscordGuildConfig` entity in `src/GitcgNetCord.MainApp/Entities/DiscordGuildConfig.cs` and configure `DbSet<DiscordGuildConfig>` in `AppDbContext.cs` with primary key and indexes. Verify compilation with `dotnet build src/GitcgNetCord.MainApp`.
- [x] 1.2 Generate EF Core migration `AddDiscordGuildConfig` for PostgreSQL and apply via `dotnet ef database update`, adhering to `standardize-ef-migrations-and-dotnet-skills` policy. Verify migration files exist in `src/GitcgNetCord.MainApp/Migrations`.
- [x] 1.3 Create `DiscordGuildConfigService` in `src/GitcgNetCord.MainApp/Entities/Repositories/` providing `SetInviteUrlAsync`, `GetInviteUrlAsync`, and `RemoveInviteUrlAsync`, and register in `Extensions.cs`. Verify service registration via build.

## 2. Server Configuration Module and Slash Commands

- [x] 2.1 Implement `ServerSlashCommand.cs` in `src/GitcgNetCord.MainApp/Commands/Slash/` with `InviteSetAsync`, `InviteRemoveAsync`, and `InviteViewAsync` subcommands enforcing `ManageGuild`/`Administrator` permissions and English responses. Verify with `dotnet build`.
- [x] 2.2 Implement `ServerModule.cs` in `src/GitcgNetCord.MainApp/Modules/` registering the `/server invite` slash command group and wire it in `Program.cs`. Verify module registration via build.

## 3. Core Report Service and Permission Enhancements

- [x] 3.1 Update `DiscordDailyReportService` to support Direct Message subscriptions (`GuildId = 0`), multi-destination querying, and `IsUserSubscriberAsync(threadId, userId)`. Verify with unit tests.
- [x] 3.2 Update `DailyReportDeliveryService` to verify delivery to both thread IDs and DM channel IDs, maintaining auto-disabling on 403 Forbidden. Verify delivery error-handling logic.
- [x] 3.3 Update `CardSnapshotSlashCommand.cs` (`ThreadUnsubscribeAsync` and `ThreadListAsync`) to check `thread.OwnerId == user.Id || isSubscriber`, enabling users to manage bot-created threads. Verify command compilation.

## 4. Interactive Onboarding & Destination Selection

- [x] 4.1 Update `SelectGameRecordComponentInteraction.cs` to reply with interactive buttons `[ Enable Daily Snapshot ]` (`daily-snapshot:enable:{accountId}`) and `[ No, Maybe Later ]` (`daily-snapshot:skip`) upon successfully adding an account. Verify component generation.
- [x] 4.2 Implement `DailySnapshotPromptComponentInteraction.cs` in `src/GitcgNetCord.MainApp/Commands/Interactions/` using compact custom IDs (`drpt-dm`, `drpt-ch`, `drpt-skip`), verified URL links, and resilient `RestException` handling. Verify interaction routing.
- [x] 4.3 Implement `DailyReportDestinationComponentInteraction.cs` to process selected destinations: calling `CreateGuildThreadAsync` with user mention for server channels or `GetDMChannelAsync` for DMs, and saving the subscription. Verify subscription persistence.

## 5. Verification and Integration Tests

- [x] 5.1 Add unit tests in `tests/GitcgNetCord.MainApp.Tests/` covering `DiscordGuildConfigService`, DM report subscription, automated thread creation logic, bot-created thread permission checks, and verification that component custom IDs do not exceed Discord's 100-character limit. Verify tests compile.
- [x] 5.2 Execute solution unit tests via `dotnet test tests/GitcgNetCord.MainApp.Tests/GitcgNetCord.MainApp.Tests.csproj` and verify all tests pass with zero errors.
