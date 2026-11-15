using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Enums;
using GitcgNetCord.MainApp.Infrastructure;
using GitcgNetCord.MainApp.Modules;
using GitcgNetCord.MainApp.Modules.Feats;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCord;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;
using Xunit;

namespace GitcgNetCord.MainApp.Tests;

public class ServerAdminCommandsTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public void AddDiscordBotModules_RegistersServerAndFeatureModulesWithoutErrors()
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddApplicationCommands();
            services.AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>();
            services.AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>();
        });

        var host = builder.Build();

        // Verifies ServerModule, CardCodeModule, and CardSnapshotModule all register their slash command groups cleanly
        host.AddServerModule();
        host.AddCardCodeModule();
        host.AddCardSnapshotModule();

        var commandService = host.Services.GetService<IApplicationCommandService>();
        Assert.NotNull(commandService);
    }

    [Fact]
    public async Task ServerOverview_WithConfiguredSettings_RetrievesAllGuildMetadataCorrectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var configService = new DiscordGuildConfigService(dbContext);
        var cardCodeService = new DiscordCardCodeChannelService(dbContext);
        var reportService = new DiscordDailyReportService(dbContext);

        const ulong guildId = 123456789012345678;
        const string inviteUrl = "https://discord.gg/gcg-test";
        const ulong cardCodeChannelId1 = 111111;
        const ulong cardCodeChannelId2 = 222222;
        const ulong reportChannelId = 333333;

        // Configure invite
        await configService.SetInviteUrlAsync(guildId, inviteUrl);

        // Configure card code channels
        await cardCodeService.AddAsync(cardCodeChannelId1, guildId, SharingDecodeType.ImageGameBackground);
        await cardCodeService.AddAsync(cardCodeChannelId2, guildId, SharingDecodeType.Text);

        // Configure daily report parent channel
        await reportService.AddReportChannelAsync(reportChannelId, guildId);

        // Fetch overview data
        var config = await configService.GetConfigAsync(guildId);
        var cardCodeChannels = await cardCodeService.GetByGuildAsync(guildId);
        var reportChannels = await reportService.GetReportChannelsByGuildAsync(guildId);

        Assert.NotNull(config);
        Assert.Equal(inviteUrl, config.InviteUrl);

        Assert.Equal(2, cardCodeChannels.Count);
        Assert.Contains(cardCodeChannels, c => c.Id == cardCodeChannelId1 && c.SharingDecodeType == SharingDecodeType.ImageGameBackground);
        Assert.Contains(cardCodeChannels, c => c.Id == cardCodeChannelId2 && c.SharingDecodeType == SharingDecodeType.Text);

        Assert.Single(reportChannels);
        Assert.Equal(reportChannelId, reportChannels[0].Id);
        Assert.Equal(guildId, reportChannels[0].GuildId);
    }

    [Fact]
    public async Task ServerOverview_WithEmptyConfiguration_ReturnsNullConfigAndEmptyCollections()
    {
        using var dbContext = CreateInMemoryDbContext();
        var configService = new DiscordGuildConfigService(dbContext);
        var cardCodeService = new DiscordCardCodeChannelService(dbContext);
        var reportService = new DiscordDailyReportService(dbContext);

        const ulong guildId = 999999999999999999;

        var config = await configService.GetConfigAsync(guildId);
        var cardCodeChannels = await cardCodeService.GetByGuildAsync(guildId);
        var reportChannels = await reportService.GetReportChannelsByGuildAsync(guildId);

        Assert.Null(config);
        Assert.Empty(cardCodeChannels);
        Assert.Empty(reportChannels);
    }

    [Fact]
    public async Task CardCodeChannelService_AddUpdateRemove_OperatesCorrectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var cardCodeService = new DiscordCardCodeChannelService(dbContext);

        const ulong channelId = 444555;
        const ulong guildId = 888999;

        // Add channel
        var (addedChannel, isNew) = await cardCodeService.AddAsync(channelId, guildId, SharingDecodeType.ImageGameBackground);
        Assert.True(isNew);
        Assert.Equal(SharingDecodeType.ImageGameBackground, addedChannel.SharingDecodeType);

        // Update channel style
        var (updatedChannel, isNewSecond) = await cardCodeService.AddAsync(channelId, guildId, SharingDecodeType.ImageSimplest);
        Assert.False(isNewSecond);
        Assert.Equal(SharingDecodeType.ImageSimplest, updatedChannel.SharingDecodeType);

        // Remove channel
        var removed = await cardCodeService.RemoveAsync(channelId);
        Assert.True(removed);

        // Verify gone
        var channels = await cardCodeService.GetByGuildAsync(guildId);
        Assert.Empty(channels);
    }

    [Fact]
    public async Task DailyReportChannelService_AddAndRemove_OperatesCorrectly()
    {
        using var dbContext = CreateInMemoryDbContext();
        var reportService = new DiscordDailyReportService(dbContext);

        const ulong channelId = 777888;
        const ulong guildId = 555666;

        // Add
        var (channel, isNew) = await reportService.AddReportChannelAsync(channelId, guildId);
        Assert.True(isNew);
        Assert.Equal(channelId, channel.Id);

        // List
        var list = await reportService.GetReportChannelsByGuildAsync(guildId);
        Assert.Single(list);

        // Remove
        var removed = await reportService.RemoveReportChannelAsync(channelId);
        Assert.True(removed);

        // Verify gone
        var emptyList = await reportService.GetReportChannelsByGuildAsync(guildId);
        Assert.Empty(emptyList);
    }
}
