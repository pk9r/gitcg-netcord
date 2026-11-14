using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
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
}
