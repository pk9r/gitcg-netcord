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
        Assert.Contains(embed.Fields, f => f.Name == "Most Used Characters" && f.Value != null && f.Value.Contains("Raiden Shogun"));
        Assert.Contains(embed.Fields, f => f.Name == "Highest Win Rate Characters" && f.Value != null && f.Value.Contains("Raiden Shogun"));
        Assert.Contains(embed.Fields, f => f.Name == "Most Used Actions" && f.Value != null && f.Value.Contains("Liben"));
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
        var mostUsedField = Assert.Single(embed.Fields, f => f.Name == "Most Used Characters");
        Assert.Contains("<:1101:999888777> **Raiden Shogun**", mostUsedField.Value);
        Assert.Contains("2. **Nahida**", mostUsedField.Value);

        var bestWinRateField = Assert.Single(embed.Fields, f => f.Name == "Highest Win Rate Characters");
        Assert.Contains("<:1101:999888777> **Raiden Shogun**", bestWinRateField.Value);
    }
}
