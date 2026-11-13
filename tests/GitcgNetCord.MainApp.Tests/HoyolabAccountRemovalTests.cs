using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using Xunit;

namespace GitcgNetCord.MainApp.Tests;

public class HoyolabAccountRemovalTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task RemoveHoyolabAccountAsync_AccountNotFound_ReturnsFailure()
    {
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var service = new HoyolabAccountService(dbContext, discordUserService);

        var result = await service.RemoveHoyolabAccountAsync(discordUserId: 12345, gameRoleId: "999999999");

        Assert.False(result.Success);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.RemovedAccount);
    }

    [Fact]
    public async Task RemoveHoyolabAccountAsync_SingleActiveAccount_ClearsActiveAccountAndWipesCredentials()
    {
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var service = new HoyolabAccountService(dbContext, discordUserService);

        const ulong userId = 10001;
        await service.AddHoyolabAccountAsync(
            discordUserId: userId,
            hoyolabUserId: "hoyo1",
            token: "secret_token",
            gameRoleId: "800111222",
            region: "os_asia"
        );

        var result = await service.RemoveHoyolabAccountAsync(discordUserId: userId, gameRoleId: "800111222");

        Assert.True(result.Success);
        Assert.True(result.WasActive);
        Assert.Null(result.NewActiveAccount);
        Assert.Empty(result.RemainingAccounts);

        // Verify account is soft-deleted and credentials wiped
        var account = await dbContext.HoyolabAccounts
            .IgnoreQueryFilters()
            .FirstAsync(x => x.GameRoleId == "800111222");

        Assert.True(account.IsDeleted);
        Assert.Equal(string.Empty, account.Token);
        Assert.Equal(string.Empty, account.HoyolabUserId);
        Assert.False(account.IsDailySnapshotEnabled);

        // Active account entity removed
        var active = await dbContext.ActiveHoyolabAccounts
            .FirstOrDefaultAsync(x => x.DiscordUserId == userId);
        Assert.Null(active);

        // Filtered queries exclude the account
        var activeAccounts = await service.GetHoyolabAccountsAsync(userId);
        Assert.Empty(activeAccounts);
    }

    [Fact]
    public async Task RemoveHoyolabAccountAsync_TwoAccounts_ActiveAccountRemoved_FallsBackToRemaining()
    {
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var service = new HoyolabAccountService(dbContext, discordUserService);

        const ulong userId = 10002;
        await service.AddHoyolabAccountAsync(userId, "hoyo1", "token1", "800000001", "os_asia");
        await service.AddHoyolabAccountAsync(userId, "hoyo2", "token2", "800000002", "os_euro");

        // First added account is active by default
        var activeBefore = await dbContext.ActiveHoyolabAccounts
            .Include(x => x.HoyolabAccount)
            .FirstAsync(x => x.DiscordUserId == userId);
        Assert.Equal("800000001", activeBefore.HoyolabAccount!.GameRoleId);

        // Remove active account
        var result = await service.RemoveHoyolabAccountAsync(userId, "800000001");

        Assert.True(result.Success);
        Assert.True(result.WasActive);
        Assert.NotNull(result.NewActiveAccount);
        Assert.Equal("800000002", result.NewActiveAccount.GameRoleId);
        Assert.Single(result.RemainingAccounts);

        // DB active account points to the second account
        var activeAfter = await dbContext.ActiveHoyolabAccounts
            .Include(x => x.HoyolabAccount)
            .FirstAsync(x => x.DiscordUserId == userId);
        Assert.Equal("800000002", activeAfter.HoyolabAccount!.GameRoleId);
    }

    [Fact]
    public async Task RemoveHoyolabAccountAsync_ThreeAccounts_ActiveRemoved_ReturnsAllRemainingForDropdown()
    {
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var service = new HoyolabAccountService(dbContext, discordUserService);

        const ulong userId = 10003;
        await service.AddHoyolabAccountAsync(userId, "h1", "t1", "100", "os_asia");
        await service.AddHoyolabAccountAsync(userId, "h2", "t2", "200", "os_euro");
        await service.AddHoyolabAccountAsync(userId, "h3", "t3", "300", "os_usa");

        var result = await service.RemoveHoyolabAccountAsync(userId, "100");

        Assert.True(result.Success);
        Assert.True(result.WasActive);
        Assert.NotNull(result.NewActiveAccount);
        Assert.Equal("200", result.NewActiveAccount.GameRoleId);
        Assert.Equal(2, result.RemainingAccounts.Count);
        Assert.Contains(result.RemainingAccounts, x => x.GameRoleId == "200");
        Assert.Contains(result.RemainingAccounts, x => x.GameRoleId == "300");
    }

    [Fact]
    public async Task RemoveHoyolabAccountAsync_InactiveAccountRemoved_KeepsActiveAccountUnchanged()
    {
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var service = new HoyolabAccountService(dbContext, discordUserService);

        const ulong userId = 10004;
        await service.AddHoyolabAccountAsync(userId, "h1", "t1", "UID_MAIN", "os_asia");
        await service.AddHoyolabAccountAsync(userId, "h2", "t2", "UID_ALT", "os_euro");

        // Remove inactive alt account
        var result = await service.RemoveHoyolabAccountAsync(userId, "UID_ALT");

        Assert.True(result.Success);
        Assert.False(result.WasActive);
        Assert.NotNull(result.NewActiveAccount);
        Assert.Equal("UID_MAIN", result.NewActiveAccount.GameRoleId);
        Assert.Single(result.RemainingAccounts);
        Assert.Equal("UID_MAIN", result.RemainingAccounts[0].GameRoleId);

        var active = await dbContext.ActiveHoyolabAccounts
            .Include(x => x.HoyolabAccount)
            .FirstAsync(x => x.DiscordUserId == userId);
        Assert.Equal("UID_MAIN", active.HoyolabAccount!.GameRoleId);
    }

    [Fact]
    public async Task RemoveHoyolabAccountAsync_CleansUpSubscriptionsAndPreservesSnapshots()
    {
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var service = new HoyolabAccountService(dbContext, discordUserService);

        const ulong userId = 10005;
        await service.AddHoyolabAccountAsync(userId, "h1", "t1", "888888888", "os_asia");

        var account = await dbContext.HoyolabAccounts.FirstAsync(x => x.GameRoleId == "888888888");

        // Add daily subscription
        dbContext.DiscordDailyReportThreads.Add(new DiscordDailyReportThread
        {
            Id = 1,
            ThreadId = 99999,
            GuildId = 11111,
            DiscordUserId = userId,
            HoyolabAccountId = account.Id,
            IsEnabled = true,
            CreatedAt = DateTimeOffset.UtcNow
        });

        // Add historical snapshot
        dbContext.CardSnapshots.Add(new CardSnapshot
        {
            Id = Guid.NewGuid(),
            HoyolabAccountId = account.Id,
            DiscordUserId = userId,
            Name = "v5.2-release",
            SnapshotType = SnapshotType.GameVersion,
            TotalGames = 120,
            TotalWins = 85,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync();

        // Act
        var result = await service.RemoveHoyolabAccountAsync(userId, "888888888");

        Assert.True(result.Success);

        // Verify daily report subscriptions deleted
        var subscriptions = await dbContext.DiscordDailyReportThreads
            .Where(x => x.HoyolabAccountId == account.Id)
            .ToListAsync();
        Assert.Empty(subscriptions);

        // Verify snapshots are preserved
        var snapshots = await dbContext.CardSnapshots
            .Where(x => x.HoyolabAccountId == account.Id)
            .ToListAsync();
        Assert.Single(snapshots);
        Assert.Equal("v5.2-release", snapshots[0].Name);
    }

    [Fact]
    public async Task AddHoyolabAccountAsync_ReactivatesSoftDeletedAccount_PreservesSnapshots()
    {
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var service = new HoyolabAccountService(dbContext, discordUserService);

        const ulong userId = 10006;
        await service.AddHoyolabAccountAsync(userId, "h1", "token_old", "777777777", "os_asia");
        var originalAccount = await dbContext.HoyolabAccounts.FirstAsync(x => x.GameRoleId == "777777777");
        var originalId = originalAccount.Id;

        // Add snapshot
        dbContext.CardSnapshots.Add(new CardSnapshot
        {
            Id = Guid.NewGuid(),
            HoyolabAccountId = originalId,
            DiscordUserId = userId,
            Name = "custom-milestone",
            SnapshotType = SnapshotType.UserCustom,
            TotalGames = 40,
            TotalWins = 30,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        // Remove account
        await service.RemoveHoyolabAccountAsync(userId, "777777777");

        // Re-add same account with refreshed token
        await service.AddHoyolabAccountAsync(userId, "h1", "token_new", "777777777", "os_asia");

        var reactivated = await dbContext.HoyolabAccounts.FirstAsync(x => x.GameRoleId == "777777777");
        Assert.Equal(originalId, reactivated.Id);
        Assert.False(reactivated.IsDeleted);
        Assert.Equal("token_new", reactivated.Token);

        // Verify snapshot still exists and belongs to this account
        var snapshots = await dbContext.CardSnapshots
            .Where(x => x.HoyolabAccountId == reactivated.Id)
            .ToListAsync();
        Assert.Single(snapshots);
        Assert.Equal("custom-milestone", snapshots[0].Name);
    }

    private static MessageOptions CreateMessageOptions()
    {
        return (MessageOptions)Activator.CreateInstance(typeof(MessageOptions), nonPublic: true)!;
    }

    [Fact]
    public void PopulateConfirmationPrompt_SetsEmbedAndConfirmCancelButtons()
    {
        var message = CreateMessageOptions();
        const string uid = "800123456";
        const string region = "os_asia";

        RemoveHoyolabAccountComponentInteraction.PopulateConfirmationPrompt(uid, region, message);

        Assert.Null(message.Content);
        Assert.NotNull(message.Embeds);
        var embed = message.Embeds.First();
        Assert.Equal("Confirm Account Removal", embed.Title);
        Assert.Contains(uid, embed.Description!);
        Assert.Contains(region, embed.Description!);

        Assert.NotNull(message.Components);
        var row = Assert.IsType<ActionRowProperties>(message.Components.First());
        var buttons = row.OfType<ButtonProperties>().ToList();
        Assert.Equal(2, buttons.Count);

        var confirmButton = buttons.First(b => b.Style == ButtonStyle.Danger);
        Assert.Equal(RemoveHoyolabAccountComponentInteraction.GetConfirmCustomId(uid), confirmButton.CustomId);
        Assert.Equal("Confirm Remove", confirmButton.Label);

        var cancelButton = buttons.First(b => b.Style == ButtonStyle.Secondary);
        Assert.Equal(RemoveHoyolabAccountComponentInteraction.CancelCustomId, cancelButton.CustomId);
        Assert.Equal("Cancel", cancelButton.Label);
    }

    [Fact]
    public async Task RemoveHoyolabAccountAsync_ThenPopulateRemovalResponse_SetsSuccessEmbed()
    {
        using var dbContext = CreateInMemoryDbContext();
        var discordUserService = new DiscordUserService(dbContext);
        var service = new HoyolabAccountService(dbContext, discordUserService);

        const ulong userId = 10007;
        await service.AddHoyolabAccountAsync(userId, "h1", "t1", "800999888", "os_asia");

        var result = await service.RemoveHoyolabAccountAsync(userId, "800999888");
        Assert.True(result.Success);

        var message = CreateMessageOptions();
        RemoveHoyolabAccountComponentInteraction.PopulateRemovalResponse(result, message);

        Assert.NotNull(message.Embeds);
        var embed = message.Embeds.First();
        Assert.Equal("Hoyolab Account Removed", embed.Title);
        Assert.NotNull(embed.Fields);
        Assert.Contains(embed.Fields, f => f.Name == "UID" && f.Value == "800999888");
        Assert.Contains(embed.Fields, f => f.Name == "Region" && f.Value == "os_asia");
    }
}
