using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Infrastructure;
using GitcgNetCord.MainApp.Models;
using Xunit;

namespace GitcgNetCord.MainApp.Tests;

public class DailyReportDeliveryTests
{
    private readonly HoyolabAccount _testAccount = new()
    {
        Id = Guid.NewGuid(),
        GameRoleId = "800123456",
        Region = "os_asia",
        IsDailySnapshotEnabled = true
    };

    [Fact]
    public void BuildDailyReportEmbed_ZeroMatches_ProducesNoticeEmbed()
    {
        var currentSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261112",
            TotalGames = 50,
            TotalWins = 35,
            CardData = []
        };

        var previousSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261111",
            TotalGames = 50,
            TotalWins = 35,
            CardData = []
        };

        var result = new PeriodicStatsResult(
            TotalGames: 0,
            TotalWins: 0,
            OverallWinRate: 0f,
            MinUseCountThreshold: 1,
            IsPercentThreshold: true,
            MinUseCountInput: "1%",
            BestWinRateCharacters: [],
            LowestWinRateCharacters: [],
            TopUseCharacters: [],
            TopUseActions: []
        );

        var localDate = new DateTime(2026, 11, 12);
        var embed = DailyReportDeliveryService.BuildDailyReportEmbed(
            _testAccount,
            currentSnapshot,
            previousSnapshot,
            result,
            localDate
        );

        Assert.Equal("Daily TCG Statistics Report - 2026-11-11", embed.Title);
        Assert.NotNull(embed.Fields);
        Assert.Contains(embed.Fields, f => f.Name == "UID" && f.Value == "800123456");
        Assert.Contains(embed.Fields, f => f.Name == "Period" && f.Value == "2026-11-11");
        Assert.Contains(embed.Fields, f => f.Name == "Notice" && f.Value != null && f.Value.Contains("No matches were recorded"));
        Assert.NotNull(embed.Footer);
        Assert.NotNull(embed.Footer.Text);
        Assert.Contains("50 games", embed.Footer.Text);
    }

    [Fact]
    public void BuildDailyReportEmbed_NoPreviousSnapshot_ProducesNoticeEmbed()
    {
        var currentSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261112",
            TotalGames = 10,
            TotalWins = 7,
            CardData = []
        };

        var result = new PeriodicStatsResult(
            TotalGames: 0,
            TotalWins: 0,
            OverallWinRate: 0f,
            MinUseCountThreshold: 1,
            IsPercentThreshold: true,
            MinUseCountInput: "1%",
            BestWinRateCharacters: [],
            LowestWinRateCharacters: [],
            TopUseCharacters: [],
            TopUseActions: []
        );

        var localDate = new DateTime(2026, 11, 12);
        var embed = DailyReportDeliveryService.BuildDailyReportEmbed(
            _testAccount,
            currentSnapshot,
            null,
            result,
            localDate
        );

        Assert.NotNull(embed.Fields);
        Assert.Contains(embed.Fields, f => f.Name == "Period" && f.Value == "2026-11-12");
        Assert.Contains(embed.Fields, f => f.Name == "Notice" && f.Value != null && f.Value.Contains("No matches were recorded"));
    }

    [Fact]
    public void BuildDailyReportEmbed_WithMatchesPlayed_ProducesDetailedStats()
    {
        var currentSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261112",
            TotalGames = 54,
            TotalWins = 38,
            CardData = []
        };

        var previousSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261111",
            TotalGames = 50,
            TotalWins = 35,
            CardData = []
        };

        var result = new PeriodicStatsResult(
            TotalGames: 4,
            TotalWins: 3,
            OverallWinRate: 0.75f,
            MinUseCountThreshold: 1,
            IsPercentThreshold: true,
            MinUseCountInput: "1%",
            BestWinRateCharacters:
            [
                new CardDeltaStat(1, "Raiden Shogun", "CardTypeCharacter", 4, 3, 0.75f)
            ],
            LowestWinRateCharacters: [],
            TopUseCharacters:
            [
                new CardDeltaStat(1, "Raiden Shogun", "CardTypeCharacter", 4, 3, 0.75f),
                new CardDeltaStat(2, "Nahida", "CardTypeCharacter", 4, 3, 0.75f)
            ],
            TopUseActions:
            [
                new CardDeltaStat(101, "Liben", "CardTypeModify", 6, 0, 0f)
            ]
        );

        var localDate = new DateTime(2026, 11, 12);
        var embed = DailyReportDeliveryService.BuildDailyReportEmbed(
            _testAccount,
            currentSnapshot,
            previousSnapshot,
            result,
            localDate
        );

        Assert.Equal("Daily TCG Statistics Report - 2026-11-11", embed.Title);
        Assert.NotNull(embed.Fields);
        Assert.Contains(embed.Fields, f => f.Name == "Period" && f.Value == "2026-11-11");
        Assert.Contains(embed.Fields, f => f.Name == "Games Played" && f.Value == "4");
        Assert.Contains(embed.Fields, f => f.Name == "Wins" && f.Value != null && f.Value.Contains("3"));
        Assert.Contains(embed.Fields, f => f.Name == "Losses" && f.Value == "1");
        Assert.Contains(embed.Fields, f => f.Name == "Characters Used" && f.Value != null && f.Value.Contains("Raiden Shogun"));
        Assert.Contains(embed.Fields, f => f.Name == "Top Actions" && f.Value != null && f.Value.Contains("Liben"));
        Assert.NotNull(embed.Footer);
        Assert.NotNull(embed.Footer.Text);
        Assert.Contains("54 games", embed.Footer.Text);
    }

    [Fact]
    public void BuildDailyReportEmbed_MultiDayDifference_ProducesMultiDayPeriod()
    {
        var currentSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261113",
            TotalGames = 60,
            TotalWins = 40,
            CardData = []
        };

        var previousSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261110",
            TotalGames = 50,
            TotalWins = 35,
            CardData = []
        };

        var result = new PeriodicStatsResult(
            TotalGames: 10,
            TotalWins: 5,
            OverallWinRate: 0.5f,
            MinUseCountThreshold: 1,
            IsPercentThreshold: true,
            MinUseCountInput: "1%",
            BestWinRateCharacters: [],
            LowestWinRateCharacters: [],
            TopUseCharacters: [],
            TopUseActions: []
        );

        var embed = DailyReportDeliveryService.BuildDailyReportEmbed(
            _testAccount,
            currentSnapshot,
            previousSnapshot,
            result,
            new DateTime(2026, 11, 13)
        );

        Assert.Equal("Daily TCG Statistics Report - 2026-11-10 ~ 2026-11-13 (3 days)", embed.Title);
        Assert.NotNull(embed.Fields);
        Assert.Contains(embed.Fields, f => f.Name == "Period" && f.Value == "2026-11-10 ~ 2026-11-13 (3 days)");
    }

    [Fact]
    public void BuildDailyReportEmbed_SameDayDifference_ProducesTodayPeriod()
    {
        var currentSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261112",
            TotalGames = 55,
            TotalWins = 38,
            CardData = []
        };

        var previousSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261112",
            TotalGames = 50,
            TotalWins = 35,
            CardData = []
        };

        var result = new PeriodicStatsResult(
            TotalGames: 5,
            TotalWins: 3,
            OverallWinRate: 0.6f,
            MinUseCountThreshold: 1,
            IsPercentThreshold: true,
            MinUseCountInput: "1%",
            BestWinRateCharacters: [],
            LowestWinRateCharacters: [],
            TopUseCharacters: [],
            TopUseActions: []
        );

        var embed = DailyReportDeliveryService.BuildDailyReportEmbed(
            _testAccount,
            currentSnapshot,
            previousSnapshot,
            result,
            new DateTime(2026, 11, 12)
        );

        Assert.Equal("Daily TCG Statistics Report - 2026-11-12 (Today)", embed.Title);
        Assert.NotNull(embed.Fields);
        Assert.Contains(embed.Fields, f => f.Name == "Period" && f.Value == "2026-11-12 (Today)");
    }

    [Fact]
    public void BuildDailyReportEmbed_WithEmojis_PrefixesCharacterNames()
    {
        var currentSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261112",
            TotalGames = 54,
            TotalWins = 38,
            CardData = []
        };

        var previousSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261111",
            TotalGames = 50,
            TotalWins = 35,
            CardData = []
        };

        var result = new PeriodicStatsResult(
            TotalGames: 4,
            TotalWins: 3,
            OverallWinRate: 0.75f,
            MinUseCountThreshold: 1,
            IsPercentThreshold: true,
            MinUseCountInput: "1%",
            BestWinRateCharacters:
            [
                new CardDeltaStat(1, "Raiden Shogun", "CardTypeCharacter", 4, 3, 0.75f)
            ],
            LowestWinRateCharacters: [],
            TopUseCharacters:
            [
                new CardDeltaStat(1, "Raiden Shogun", "CardTypeCharacter", 4, 3, 0.75f),
                new CardDeltaStat(2, "Nahida", "CardTypeCharacter", 4, 3, 0.75f)
            ],
            TopUseActions: []
        );

        var emojis = new Dictionary<string, string>
        {
            ["1"] = "<:1101:999888777>"
        };

        var embed = DailyReportDeliveryService.BuildDailyReportEmbed(
            _testAccount,
            currentSnapshot,
            previousSnapshot,
            result,
            new DateTime(2026, 11, 12),
            emojis
        );

        Assert.NotNull(embed.Fields);
        var charField = Assert.Single(embed.Fields, f => f.Name == "Characters Used");
        Assert.Contains("<:1101:999888777> **Raiden Shogun**", charField.Value);
        Assert.Contains("2. **Nahida**", charField.Value);
        Assert.Contains("4 games (75.0% - 3W/1L)", charField.Value);
    }

    [Fact]
    public void BuildDailyReportEmbed_DisplaysAllCharactersAndTop10Actions()
    {
        var currentSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261112",
            TotalGames = 100,
            TotalWins = 60,
            CardData = []
        };

        var previousSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261111",
            TotalGames = 70,
            TotalWins = 40,
            CardData = []
        };

        var usedChars = Enumerable.Range(1, 8)
            .Select(i => new CardDeltaStat(i, $"Character {i}", "CardTypeCharacter", 10 - i, 8 - i, (float)(8 - i) / (10 - i)))
            .ToList();

        var topActions = Enumerable.Range(1, 15)
            .Select(i => new CardDeltaStat(100 + i, $"Action Card {i}", "CardTypeModify", 20 - i, 0, 0f))
            .ToList();

        var result = new PeriodicStatsResult(
            TotalGames: 10,
            TotalWins: 6,
            OverallWinRate: 0.6f,
            MinUseCountThreshold: 1,
            IsPercentThreshold: true,
            MinUseCountInput: "1%",
            BestWinRateCharacters: [],
            LowestWinRateCharacters: [],
            TopUseCharacters: usedChars.Take(5).ToList(),
            TopUseActions: topActions,
            UsedCharacters: usedChars
        );

        var embed = DailyReportDeliveryService.BuildDailyReportEmbed(
            _testAccount,
            currentSnapshot,
            previousSnapshot,
            result,
            new DateTime(2026, 11, 12)
        );

        Assert.NotNull(embed.Fields);
        var charField = Assert.Single(embed.Fields, f => f.Name == "Characters Used");
        // Verify all 8 characters are included in Characters Used field
        for (var i = 1; i <= 8; i++)
        {
            Assert.Contains($"Character {i}", charField.Value);
        }

        var actionsField = Assert.Single(embed.Fields, f => f.Name == "Top Actions");
        // Verify top 10 actions are included (Action Card 1 through 10)
        for (var i = 1; i <= 10; i++)
        {
            Assert.Contains($"Action Card {i}", actionsField.Value);
        }
        // Action Card 11 should not be present
        Assert.DoesNotContain("Action Card 11", actionsField.Value);
    }

    [Fact]
    public void BuildDailyReportEmbed_ManyCharacters_SplitsAcrossContinuationField()
    {
        var currentSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261112",
            TotalGames = 100,
            TotalWins = 60,
            CardData = []
        };

        var previousSnapshot = new CardSnapshot
        {
            Id = Guid.NewGuid(),
            Name = "daily-20261111",
            TotalGames = 70,
            TotalWins = 40,
            CardData = []
        };

        // Create 25 characters with long names so combined length exceeds 1024 chars
        var usedChars = Enumerable.Range(1, 25)
            .Select(i => new CardDeltaStat(i, $"Extremely Long Character Name Designation {i}", "CardTypeCharacter", 5, 4, 0.8f))
            .ToList();

        var result = new PeriodicStatsResult(
            TotalGames: 10,
            TotalWins: 6,
            OverallWinRate: 0.6f,
            MinUseCountThreshold: 1,
            IsPercentThreshold: true,
            MinUseCountInput: "1%",
            BestWinRateCharacters: [],
            LowestWinRateCharacters: [],
            TopUseCharacters: usedChars.Take(5).ToList(),
            TopUseActions: [],
            UsedCharacters: usedChars
        );

        var embed = DailyReportDeliveryService.BuildDailyReportEmbed(
            _testAccount,
            currentSnapshot,
            previousSnapshot,
            result,
            new DateTime(2026, 11, 12)
        );

        Assert.NotNull(embed.Fields);
        Assert.Contains(embed.Fields, f => f.Name == "Characters Used");
        Assert.Contains(embed.Fields, f => f.Name == "Characters Used (Cont.)");
        // Every field value must be <= 1024 chars
        foreach (var field in embed.Fields)
        {
            Assert.True(field.Value?.Length <= 1024, $"Field '{field.Name}' exceeded 1024 characters.");
        }
    }
}
