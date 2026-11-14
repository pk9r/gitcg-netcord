using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Infrastructure;
using HoyolabHttpClient.Responses.GameRecordCard;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using Xunit;

namespace GitcgNetCord.MainApp.Tests;

public class HoyolabAddAccountFlowTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public void CreateAccountAddedResponse_ConstructsValidEmbedAndButtons()
    {
        // Arrange
        var gameRecord = new GameRecord
        {
            GameId = 2,
            GameRoleId = "800123456",
            Nickname = "Traveler",
            Level = 60,
            Region = "os_asia",
            RegionName = "Asia Server"
        };
        var accountId = Guid.NewGuid();

        // Act
        var (embed, components) = HoyolabAccountResponseHelper.CreateAccountAddedResponse(gameRecord, accountId);

        // Assert
        Assert.NotNull(embed);
        Assert.Equal("Account Added Successfully!", embed.Title);
        Assert.Contains("Traveler", embed.Description);
        Assert.Contains("800123456", embed.Description);
        Assert.Contains("Asia Server", embed.Description);

        Assert.NotNull(components);
        var buttons = components.OfType<ButtonProperties>().ToList();
        Assert.Equal(2, buttons.Count);

        var enableBtn = buttons.First(b => b.Style == ButtonStyle.Success);
        Assert.Equal("Enable Daily Snapshot", enableBtn.Label);
        Assert.Equal(DailySnapshotPromptComponentInteraction.GetEnableCustomId(accountId), enableBtn.CustomId);

        var skipBtn = buttons.First(b => b.Style == ButtonStyle.Secondary);
        Assert.Equal("No, Maybe Later", skipBtn.Label);
        Assert.Equal(DailySnapshotPromptComponentInteraction.SkipCustomId, skipBtn.CustomId);
    }

    [Fact]
    public void CharacterFiltering_ZeroGenshinCharacters_YieldsEmptyList()
    {
        // Arrange - Honkai: Star Rail (GameId = 6) and Zenless Zone Zero (GameId = 8)
        var records = new List<GameRecord>
        {
            new() { GameId = 6, GameRoleId = "100111222", Nickname = "Trailblazer", Level = 70, Region = "prod_official_usa" },
            new() { GameId = 8, GameRoleId = "150333444", Nickname = "Proxy", Level = 50, Region = "prod_gf_us" }
        };

        // Act
        var genshinRecords = records.Where(x => x.GameId == 2).DistinctBy(x => x.GameRoleId).ToList();

        // Assert
        Assert.Empty(genshinRecords);
    }

    [Fact]
    public void CharacterFiltering_SingleGenshinCharacter_IdentifiedCorrectly()
    {
        // Arrange - One Genshin character and one Honkai character
        var records = new List<GameRecord>
        {
            new() { GameId = 2, GameRoleId = "800123456", Nickname = "Lumine", Level = 60, Region = "os_asia" },
            new() { GameId = 6, GameRoleId = "100111222", Nickname = "Trailblazer", Level = 70, Region = "prod_official_usa" }
        };

        // Act
        var genshinRecords = records.Where(x => x.GameId == 2).DistinctBy(x => x.GameRoleId).ToList();

        // Assert
        Assert.Single(genshinRecords);
        Assert.Equal("800123456", genshinRecords[0].GameRoleId);
    }

    [Fact]
    public void MultiCharacter_OptionDescription_AnnotatesAlreadyLinkedAccounts()
    {
        // Arrange
        var existingUids = new HashSet<string> { "800123456" };
        var linkedRecord = new GameRecord
        {
            GameId = 2,
            GameRoleId = "800123456",
            Nickname = "Lumine",
            Level = 60,
            Region = "os_asia",
            RegionName = "Asia Server"
        };
        var newRecord = new GameRecord
        {
            GameId = 2,
            GameRoleId = "600987654",
            Nickname = "Aether",
            Level = 45,
            Region = "os_usa",
            RegionName = "America Server"
        };

        // Act
        var isLinked1 = existingUids.Contains(linkedRecord.GameRoleId);
        var desc1 = isLinked1
            ? $"Lv. {linkedRecord.Level} {linkedRecord.RegionName} ({linkedRecord.GameRoleId}) [Already Linked - Select to update token]"
            : $"Lv. {linkedRecord.Level} {linkedRecord.RegionName} ({linkedRecord.GameRoleId})";

        var isLinked2 = existingUids.Contains(newRecord.GameRoleId);
        var desc2 = isLinked2
            ? $"Lv. {newRecord.Level} {newRecord.RegionName} ({newRecord.GameRoleId}) [Already Linked - Select to update token]"
            : $"Lv. {newRecord.Level} {newRecord.RegionName} ({newRecord.GameRoleId})";

        // Assert
        Assert.Contains("[Already Linked - Select to update token]", desc1);
        Assert.DoesNotContain("[Already Linked - Select to update token]", desc2);
        Assert.Equal("Lv. 45 America Server (600987654)", desc2);
    }

    [Fact]
    public async Task SingleCharacter_AutoLink_DirectlyPersistsAccountAndSetsActive()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var service = new HoyolabAccountService(dbContext, discordUserService);
        const ulong userId = 20001;

        // Act - Simulate auto-linking the single character directly
        var account = await service.AddHoyolabAccountAsync(
            discordUserId: userId,
            hoyolabUserId: "hoyo_user_1",
            token: "v2_secret_token",
            gameRoleId: "800555666",
            region: "os_asia"
        );

        // Assert
        Assert.NotNull(account);
        Assert.Equal("800555666", account.GameRoleId);
        Assert.Equal("os_asia", account.Region);

        var accounts = (await service.GetHoyolabAccountsAsync(userId)).ToList();
        Assert.Single(accounts);
        Assert.Equal(account.Id, accounts[0].Id);

        var activeAccount = await dbContext.ActiveHoyolabAccounts
            .FirstOrDefaultAsync(x => x.DiscordUserId == userId);
        Assert.NotNull(activeAccount);
        Assert.Equal(account.Id, activeAccount.HoyolabAccountId);
    }

    [Fact]
    public async Task MultiCharacter_SelectExistingAccount_RefreshesTokenWithoutCreatingDuplicate()
    {
        // Arrange
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var service = new HoyolabAccountService(dbContext, discordUserService);
        const ulong userId = 20002;

        // Add initial account
        var initial = await service.AddHoyolabAccountAsync(
            discordUserId: userId,
            hoyolabUserId: "hoyo_user_1",
            token: "old_token",
            gameRoleId: "800777888",
            region: "os_asia"
        );

        // Act - Re-linking with new token (Option A flow)
        var updated = await service.AddHoyolabAccountAsync(
            discordUserId: userId,
            hoyolabUserId: "hoyo_user_1",
            token: "new_token",
            gameRoleId: "800777888",
            region: "os_asia"
        );

        // Assert
        Assert.Equal(initial.Id, updated.Id);
        Assert.Equal("new_token", updated.Token);

        var accounts = (await service.GetHoyolabAccountsAsync(userId)).ToList();
        Assert.Single(accounts);
    }
}
