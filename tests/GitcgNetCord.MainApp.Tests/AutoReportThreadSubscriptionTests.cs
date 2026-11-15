using GitcgNetCord.MainApp.Commands.Slash;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NetCord.Rest;
using Xunit;

namespace GitcgNetCord.MainApp.Tests;

public class AutoReportThreadSubscriptionTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }



    [Fact]
    public async Task DiscordGuildConfigService_SetAndGetInviteUrl_SavesAndRetrievesCorrectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new DiscordGuildConfigService(dbContext);

        const ulong guildId = 123456789;
        const string inviteUrl = "https://discord.gg/test-guild";

        var config = await service.SetInviteUrlAsync(guildId, inviteUrl);
        Assert.Equal(guildId, config.GuildId);
        Assert.Equal(inviteUrl, config.InviteUrl);

        var retrievedUrl = await service.GetInviteUrlAsync(guildId);
        Assert.Equal(inviteUrl, retrievedUrl);

        var retrievedConfig = await service.GetConfigAsync(guildId);
        Assert.NotNull(retrievedConfig);
        Assert.Equal(inviteUrl, retrievedConfig.InviteUrl);
    }

    [Fact]
    public async Task DiscordGuildConfigService_RemoveInviteUrl_ClearsConfiguredUrl()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new DiscordGuildConfigService(dbContext);

        const ulong guildId = 987654321;
        await service.SetInviteUrlAsync(guildId, "https://discord.gg/remove-test");

        var removed = await service.RemoveInviteUrlAsync(guildId);
        Assert.True(removed);

        var urlAfter = await service.GetInviteUrlAsync(guildId);
        Assert.Null(urlAfter);

        var secondRemove = await service.RemoveInviteUrlAsync(guildId);
        Assert.False(secondRemove);
    }

    [Fact]
    public async Task DiscordGuildConfigService_GetGuildsWithInvites_ReturnsOnlyConfiguredGuilds()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new DiscordGuildConfigService(dbContext);

        await service.SetInviteUrlAsync(111, "https://discord.gg/guild1");
        await service.SetInviteUrlAsync(222, "https://discord.gg/guild2");
        await service.SetInviteUrlAsync(333, "https://discord.gg/guild3");
        await service.RemoveInviteUrlAsync(222);

        var guilds = await service.GetGuildsWithInvitesAsync();
        Assert.Equal(2, guilds.Count);
        Assert.Contains(guilds, g => g.GuildId == 111);
        Assert.Contains(guilds, g => g.GuildId == 333);
        Assert.DoesNotContain(guilds, g => g.GuildId == 222);
    }

    [Fact]
    public async Task DiscordDailyReportService_SubscribeDm_CreatesSubscriptionWithZeroGuildId()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new DiscordDailyReportService(dbContext);

        const ulong dmChannelId = 999111;
        const ulong userId = 555001;
        var accountId = Guid.NewGuid();

        var sub = await service.SubscribeDmAsync(dmChannelId, userId, accountId);
        Assert.NotNull(sub);
        Assert.Equal(dmChannelId, sub.ThreadId);
        Assert.Equal((ulong)0, sub.GuildId);
        Assert.Equal(userId, sub.DiscordUserId);
        Assert.Equal(accountId, sub.HoyolabAccountId);
        Assert.True(sub.IsEnabled);

        var foundDm = await service.FindDmSubscriptionAsync(userId, accountId);
        Assert.NotNull(foundDm);
        Assert.Equal(dmChannelId, foundDm.ThreadId);
    }

    [Fact]
    public async Task DiscordDailyReportService_MultiDestination_AllowsDmAndGuildSimultaneously()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new DiscordDailyReportService(dbContext);

        const ulong dmChannelId = 10001;
        const ulong guildThreadId = 20002;
        const ulong guildId = 99900;
        const ulong userId = 88801;
        var accountId = Guid.NewGuid();

        // Subscribe to DM
        await service.SubscribeDmAsync(dmChannelId, userId, accountId);

        // Subscribe to Guild Thread
        await service.SubscribeThreadAsync(guildThreadId, guildId, userId, accountId);

        var activeSubs = await service.GetActiveSubscriptionsForAccountAsync(accountId);
        Assert.Equal(2, activeSubs.Count);
        Assert.Contains(activeSubs, s => s.ThreadId == dmChannelId && s.GuildId == 0);
        Assert.Contains(activeSubs, s => s.ThreadId == guildThreadId && s.GuildId == guildId);

        var foundGuild = await service.FindSubscriptionInGuildAsync(guildId, accountId);
        Assert.NotNull(foundGuild);
        Assert.Equal(guildThreadId, foundGuild.ThreadId);
    }

    [Fact]
    public async Task DiscordDailyReportService_IsUserSubscriber_ReturnsTrueForOwnerAndFalseForOthers()
    {
        using var dbContext = CreateInMemoryDbContext();
        var service = new DiscordDailyReportService(dbContext);

        const ulong threadId = 333001;
        const ulong ownerUserId = 777001;
        const ulong otherUserId = 777002;
        var accountId = Guid.NewGuid();

        await service.SubscribeThreadAsync(threadId, guildId: 10, ownerUserId, accountId);

        var isOwnerSubscriber = await service.IsUserSubscriberAsync(threadId, ownerUserId);
        var isOtherSubscriber = await service.IsUserSubscriberAsync(threadId, otherUserId);

        Assert.True(isOwnerSubscriber);
        Assert.False(isOtherSubscriber);
    }

    [Fact]
    public void ComponentInteraction_CustomIds_DoNotExceedDiscord100CharacterLimit()
    {
        var accountId = Guid.NewGuid();
        const ulong maxSnowflake = 1301234567890123456; // 19 digits

        var channelCustomId = GitcgNetCord.MainApp.Commands.Interactions.DailyReportDestinationComponentInteraction
            .GetChannelCustomId(maxSnowflake, accountId);
        var dmCustomId = GitcgNetCord.MainApp.Commands.Interactions.DailyReportDestinationComponentInteraction
            .GetDmCustomId(accountId);
        var skipCustomId = GitcgNetCord.MainApp.Commands.Interactions.DailyReportDestinationComponentInteraction
            .SkipCustomId;
        var enableCustomId = GitcgNetCord.MainApp.Commands.Interactions.DailySnapshotPromptComponentInteraction
            .GetEnableCustomId(accountId);

        Assert.True(channelCustomId.Length <= 100, $"channelCustomId '{channelCustomId}' exceeds 100 chars (was {channelCustomId.Length})");
        Assert.True(dmCustomId.Length <= 100, $"dmCustomId '{dmCustomId}' exceeds 100 chars (was {dmCustomId.Length})");
        Assert.True(skipCustomId.Length <= 100, $"skipCustomId '{skipCustomId}' exceeds 100 chars (was {skipCustomId.Length})");
        Assert.True(enableCustomId.Length <= 100, $"enableCustomId '{enableCustomId}' exceeds 100 chars (was {enableCustomId.Length})");
    }

    [Fact]
    public async Task DiscordDailyReportService_GetSuggestedReportGuilds_FiltersAndExcludesCorrectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var reportService = new DiscordDailyReportService(dbContext);
        var configService = new DiscordGuildConfigService(dbContext);

        const ulong guildEligible1 = 1001;
        const ulong guildEligible2 = 1002;
        const ulong guildNoChannels = 1003;
        const ulong guildNoInvite = 1004;
        const ulong guildEmptyInvite = 1005;

        // Setup Guild 1001: 2 report channels, invite
        await configService.SetInviteUrlAsync(guildEligible1, "https://discord.gg/guild1");
        await reportService.AddReportChannelAsync(channelId: 2001, guildId: guildEligible1);
        await reportService.AddReportChannelAsync(channelId: 2002, guildId: guildEligible1);

        // Setup Guild 1002: 1 report channel, invite
        await configService.SetInviteUrlAsync(guildEligible2, "https://discord.gg/guild2");
        await reportService.AddReportChannelAsync(channelId: 2003, guildId: guildEligible2);

        // Setup Guild 1003: invite, but NO report channels
        await configService.SetInviteUrlAsync(guildNoChannels, "https://discord.gg/guild3");

        // Setup Guild 1004: report channel, but NO invite
        await reportService.AddReportChannelAsync(channelId: 2004, guildId: guildNoInvite);

        // Setup Guild 1005: empty invite
        await configService.SetInviteUrlAsync(guildEmptyInvite, "");
        await reportService.AddReportChannelAsync(channelId: 2005, guildId: guildEmptyInvite);

        // Test 1: All suggestions
        var allSuggested = await reportService.GetSuggestedReportGuildsAsync();
        Assert.Equal(2, allSuggested.Count);
        Assert.Equal(guildEligible1, allSuggested[0].GuildId);
        Assert.Equal(2, allSuggested[0].ReportChannelCount);
        Assert.Equal("https://discord.gg/guild1", allSuggested[0].InviteUrl);
        Assert.Equal(guildEligible2, allSuggested[1].GuildId);
        Assert.Equal(1, allSuggested[1].ReportChannelCount);

        // Test 2: Exclude guildEligible1
        var excludedSuggested = await reportService.GetSuggestedReportGuildsAsync(excludeGuildId: guildEligible1);
        Assert.Single(excludedSuggested);
        Assert.Equal(guildEligible2, excludedSuggested[0].GuildId);

        // Test 3: Limit 1
        var limitedSuggested = await reportService.GetSuggestedReportGuildsAsync(limit: 1);
        Assert.Single(limitedSuggested);
        Assert.Equal(guildEligible1, limitedSuggested[0].GuildId);
    }

    [Fact]
    public void BuildServerSuggestions_WhenListIsEmpty_ReturnsNullFieldAndNullActionRow()
    {
        var (field, actionRow) = CardSnapshotSlashCommand.BuildServerSuggestions([], null);

        Assert.Null(field);
        Assert.Null(actionRow);
    }

    [Fact]
    public void BuildServerSuggestions_WithEligibleGuilds_ConstructsFormattedEmbedFieldAndLinkButtons()
    {
        var guilds = new List<SuggestedReportGuild>
        {
            new(1001, "https://discord.gg/inv1", 1),
            new(1002, "https://discord.gg/inv2", 3)
        };

        var (field, actionRow) = CardSnapshotSlashCommand.BuildServerSuggestions(guilds, null);

        Assert.NotNull(field);
        Assert.Equal("Suggested Report Servers", field.Name);
        Assert.Contains("1 report channel", field.Value);
        Assert.Contains("3 report channels", field.Value);
        Assert.Contains("[Join Server](https://discord.gg/inv1)", field.Value);
        Assert.Contains("[Join Server](https://discord.gg/inv2)", field.Value);

        Assert.NotNull(actionRow);
        var buttons = actionRow.ToList();
        Assert.Equal(2, buttons.Count);
        var btn1 = Assert.IsType<LinkButtonProperties>(buttons[0]);
        Assert.Equal("https://discord.gg/inv1", btn1.Url);
        Assert.Equal("Join Community Server", btn1.Label);

        var btn2 = Assert.IsType<LinkButtonProperties>(buttons[1]);
        Assert.Equal("https://discord.gg/inv2", btn2.Url);
        Assert.Equal("Join Community Server", btn2.Label);
    }

    [Fact]
    public void BuildServerSuggestions_WithInvalidUri_ExcludesFromActionRow()
    {
        var guilds = new List<SuggestedReportGuild>
        {
            new(1001, "not-a-valid-url", 2)
        };

        var (field, actionRow) = CardSnapshotSlashCommand.BuildServerSuggestions(guilds, null);

        Assert.NotNull(field);
        Assert.Contains("not-a-valid-url", field.Value);
        Assert.Null(actionRow);
    }
}
