using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Infrastructure;
using HoyolabHttpClient.Models;
using Xunit;

namespace GitcgNetCord.MainApp.Tests;

public class CardStatsCalculatorTests
{
    private readonly CardStatsCalculator _calculator = new();

    private static Card CreateCard(int id, string name, string type, int useCount, int proficiency)
    {
        return new Card
        {
            Id = id,
            Name = name,
            CardType = type,
            UseCount = useCount,
            Proficiency = proficiency,
            Image = "",
            Desc = "",
            Tags = [],
            CardSkills = [],
            ActionCost = [],
            CardSources = [],
            DeckRecommend = "",
            CardWiki = "",
            Icon = "",
            LargeIcon = "",
            Category = ""
        };
    }

    [Fact]
    public void Calculate_AllTime_ComputesCorrectTotalsAndRankings()
    {
        var cards = new List<Card>
        {
            CreateCard(1, "Char 1", "CardTypeCharacter", 30, 20),
            CreateCard(2, "Char 2", "CardTypeCharacter", 30, 15),
            CreateCard(3, "Char 3", "CardTypeCharacter", 30, 10),
            CreateCard(101, "Action 1", "CardTypeModify", 50, 0),
            CreateCard(102, "Action 2", "CardTypeModify", 20, 0),
        };

        var result = _calculator.Calculate(cards, startSnapshot: null, endSnapshot: null, minUseCount: "1%");

        // Total games = sum(30+30+30) / 3 = 30
        Assert.Equal(30, result.TotalGames);
        // Total wins = sum(20+15+10) / 3 = 15
        Assert.Equal(15, result.TotalWins);
        // Overall win rate = 15 / 30 = 0.5 (50%)
        Assert.Equal(0.5f, result.OverallWinRate);

        Assert.Equal(3, result.BestWinRateCharacters.Count);
        Assert.Equal(1, result.BestWinRateCharacters[0].Id); // 20/30 = 66.7%
        Assert.Equal(2, result.TopUseActions.Count);
        Assert.Equal(101, result.TopUseActions[0].Id);
    }

    [Fact]
    public void Calculate_Periodic_DeltaCalculationCorrect()
    {
        var cards = new List<Card>
        {
            CreateCard(1, "Char 1", "CardTypeCharacter", 100, 60),
            CreateCard(2, "Char 2", "CardTypeCharacter", 80, 50),
            CreateCard(3, "Char 3", "CardTypeCharacter", 60, 40),
            CreateCard(101, "Action 1", "CardTypeModify", 90, 0),
        };

        var startSnapshot = new Dictionary<int, CardStatSummary>
        {
            [1] = new(70, 40),
            [2] = new(50, 30),
            [3] = new(30, 20),
            [101] = new(60, 0)
        };

        // Snapshot to live:
        // Char 1: delta use = 30, delta win = 20 (WR = 66.7%)
        // Char 2: delta use = 30, delta win = 20 (WR = 66.7%)
        // Char 3: delta use = 30, delta win = 20 (WR = 66.7%)
        // Total delta games = 90 / 3 = 30. Total delta wins = 60 / 3 = 20.
        // Action 1: delta use = 30.

        var result = _calculator.Calculate(cards, startSnapshot: startSnapshot, endSnapshot: null);

        Assert.Equal(30, result.TotalGames);
        Assert.Equal(20, result.TotalWins);
        Assert.Equal(20f / 30f, result.OverallWinRate, precision: 4);

        var action1 = Assert.Single(result.TopUseActions);
        Assert.Equal(101, action1.Id);
        Assert.Equal(30, action1.UseCount);
    }

    [Fact]
    public void Calculate_NewCardAddedInLaterVersion_HandledGracefully()
    {
        var cards = new List<Card>
        {
            CreateCard(1, "Old Char", "CardTypeCharacter", 50, 30),
            CreateCard(2, "New Char", "CardTypeCharacter", 20, 15),
            CreateCard(3, "Base Char", "CardTypeCharacter", 50, 25),
        };

        // New Char (id 2) did not exist in startSnapshot
        var startSnapshot = new Dictionary<int, CardStatSummary>
        {
            [1] = new(50, 30),
            [3] = new(30, 10),
        };

        // Delta:
        // Old Char: 50 - 50 = 0
        // New Char: 20 - 0 = 20, win = 15
        // Base Char: 50 - 30 = 20, win = 15
        // Total games = (0 + 20 + 20) / 3 = 13
        // Total wins = (0 + 15 + 15) / 3 = 10

        var result = _calculator.Calculate(cards, startSnapshot: startSnapshot, endSnapshot: null, minUseCount: "0");

        Assert.Equal(13, result.TotalGames);
        Assert.Equal(10, result.TotalWins);

        var newChar = result.TopUseCharacters.FirstOrDefault(x => x.Id == 2);
        Assert.NotNull(newChar);
        Assert.Equal(20, newChar.UseCount);
        Assert.Equal(15, newChar.Proficiency);
    }

    [Fact]
    public void Calculate_ZeroGamesPlayed_ReturnsZeroTotalsWithoutExceptions()
    {
        var cards = new List<Card>
        {
            CreateCard(1, "Char 1", "CardTypeCharacter", 50, 30),
            CreateCard(2, "Char 2", "CardTypeCharacter", 50, 30),
            CreateCard(3, "Char 3", "CardTypeCharacter", 50, 30),
        };

        var startSnapshot = new Dictionary<int, CardStatSummary>
        {
            [1] = new(50, 30),
            [2] = new(50, 30),
            [3] = new(50, 30),
        };

        var result = _calculator.Calculate(cards, startSnapshot: startSnapshot, endSnapshot: null);

        Assert.Equal(0, result.TotalGames);
        Assert.Equal(0, result.TotalWins);
        Assert.Equal(0f, result.OverallWinRate);
        Assert.Empty(result.TopUseCharacters);
        Assert.Empty(result.BestWinRateCharacters);
    }
}
