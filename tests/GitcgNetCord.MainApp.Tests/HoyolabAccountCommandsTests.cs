using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Infrastructure;
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

public class HoyolabAccountCommandsTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public void AddHoyolabAccountModule_RegistersCommandGroupAndSubcommandsCleanly()
    {
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddApplicationCommands();
            services.AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>();
            services.AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>();
            services.AddComponentInteractions<ModalInteraction, ModalInteractionContext>();
        });

        var host = builder.Build();

        host.AddHoyolabAccountModule();

        var commandService = host.Services.GetService<IApplicationCommandService>();
        Assert.NotNull(commandService);
    }

    [Fact]
    public async Task SwitchActiveAccount_WhenUserHasMultipleAccounts_SwitchesSuccessfully()
    {
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var hoyoService = new HoyolabAccountService(dbContext, discordUserService);
        var activeService = new ActiveHoyolabAccountService(hoyoService, dbContext);

        const ulong userId = 10001;
        await hoyoService.AddHoyolabAccountAsync(
            discordUserId: userId,
            hoyolabUserId: "hoyo1",
            token: "token1",
            gameRoleId: "800111222",
            region: "os_asia"
        );

        await hoyoService.AddHoyolabAccountAsync(
            discordUserId: userId,
            hoyolabUserId: "hoyo2",
            token: "token2",
            gameRoleId: "800333444",
            region: "os_usa"
        );

        // Initially first account was added and set as active
        var initialActive = await activeService.GetActiveHoyolabAccountAsync(userId);
        Assert.NotNull(initialActive);
        Assert.Equal("800111222", initialActive.GameRoleId);

        // Switch to second account
        await activeService.UpdateActiveHoyolabAccountAsync(userId, "800333444");

        var updatedActive = await activeService.GetActiveHoyolabAccountAsync(userId);
        Assert.NotNull(updatedActive);
        Assert.Equal("800333444", updatedActive.GameRoleId);
        Assert.Equal("os_usa", updatedActive.Region);
    }

    [Fact]
    public async Task SwitchActiveAccount_WhenAccountDoesNotExistForUser_ThrowsInvalidOperationException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var hoyoService = new HoyolabAccountService(dbContext, discordUserService);
        var activeService = new ActiveHoyolabAccountService(hoyoService, dbContext);

        const ulong userId = 10002;
        await hoyoService.AddHoyolabAccountAsync(
            discordUserId: userId,
            hoyolabUserId: "hoyo1",
            token: "token1",
            gameRoleId: "800111222",
            region: "os_asia"
        );

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            activeService.UpdateActiveHoyolabAccountAsync(userId, "999999999")
        );
    }

    [Fact]
    public async Task SwitchActiveAccount_WhenAccountBelongsToAnotherUser_ThrowsInvalidOperationException()
    {
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var hoyoService = new HoyolabAccountService(dbContext, discordUserService);
        var activeService = new ActiveHoyolabAccountService(hoyoService, dbContext);

        const ulong userA = 10003;
        const ulong userB = 10004;

        await hoyoService.AddHoyolabAccountAsync(
            discordUserId: userA,
            hoyolabUserId: "hoyoA",
            token: "tokenA",
            gameRoleId: "800111222",
            region: "os_asia"
        );

        await hoyoService.AddHoyolabAccountAsync(
            discordUserId: userB,
            hoyolabUserId: "hoyoB",
            token: "tokenB",
            gameRoleId: "800333444",
            region: "os_usa"
        );

        // User A tries to switch to User B's UID
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            activeService.UpdateActiveHoyolabAccountAsync(userA, "800333444")
        );
    }

    [Fact]
    public void HoyolabAccountAutocompleteProvider_BuildChoices_FormatsLabelsAndLimitsTo25()
    {
        var accounts = Enumerable.Range(1, 30).Select(i => new HoyolabAccount
        {
            Id = Guid.NewGuid(),
            GameRoleId = $"8000000{i:D2}",
            Region = "os_asia",
            IsDailySnapshotEnabled = i % 2 == 0,
            DiscordUserId = 12345
        }).ToList();

        var choices = HoyolabAccountAutocompleteProvider.BuildChoices(accounts, keyword: null).ToList();

        Assert.Equal(25, choices.Count);

        var firstChoice = choices[0];
        Assert.Equal("800000001 (os_asia) - [Daily: OFF]", firstChoice.Name);

        var secondChoice = choices[1];
        Assert.Equal("800000002 (os_asia) - [Daily: ON]", secondChoice.Name);
    }

    [Fact]
    public void HoyolabAccountAutocompleteProvider_BuildChoices_FiltersByKeyword()
    {
        var accounts = new List<HoyolabAccount>
        {
            new() { Id = Guid.NewGuid(), GameRoleId = "800111222", Region = "os_asia", DiscordUserId = 12345 },
            new() { Id = Guid.NewGuid(), GameRoleId = "900333444", Region = "os_usa", DiscordUserId = 12345 },
            new() { Id = Guid.NewGuid(), GameRoleId = "700555666", Region = "os_euro", DiscordUserId = 12345 }
        };

        var filteredByUid = HoyolabAccountAutocompleteProvider.BuildChoices(accounts, "900").ToList();
        Assert.Single(filteredByUid);
        Assert.Contains("900333444", filteredByUid[0].Name);

        var filteredByRegion = HoyolabAccountAutocompleteProvider.BuildChoices(accounts, "euro").ToList();
        Assert.Single(filteredByRegion);
        Assert.Contains("700555666", filteredByRegion[0].Name);

        var filteredEmpty = HoyolabAccountAutocompleteProvider.BuildChoices(accounts, "nonexistent").ToList();
        Assert.Empty(filteredEmpty);
    }
}
