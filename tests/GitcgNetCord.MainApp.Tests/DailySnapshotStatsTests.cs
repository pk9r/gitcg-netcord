using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Infrastructure;
using HoyolabHttpClient.Models;
using Xunit;

namespace GitcgNetCord.MainApp.Tests;

public class DailySnapshotStatsTests
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
    public void Calculate_BetweenTwoDailySnapshots_ComputesAccurateDailyDeltas()
    {
        // Day 1 (daily-20261111) stats:
        var day1Snapshot = new Dictionary<int, CardStatSummary>
        {
            [1] = new(UseCount: 10, Proficiency: 8),
            [2] = new(UseCount: 10, Proficiency: 6),
            [3] = new(UseCount: 10, Proficiency: 4),
            [101] = new(UseCount: 15, Proficiency: 0)
        };

        // Day 2 (daily-20261112) stats:
        var day2Snapshot = new Dictionary<int, CardStatSummary>
        {
            [1] = new(UseCount: 16, Proficiency: 12), // +6 games, +4 wins
            [2] = new(UseCount: 13, Proficiency: 8),  // +3 games, +2 wins
            [3] = new(UseCount: 13, Proficiency: 6),  // +3 games, +2 wins
            [101] = new(UseCount: 20, Proficiency: 0) // +5 uses
        };

        var allCards = new List<Card>
        {
            CreateCard(1, "Char 1", "CardTypeCharacter", 16, 12),
            CreateCard(2, "Char 2", "CardTypeCharacter", 13, 8),
            CreateCard(3, "Char 3", "CardTypeCharacter", 13, 6),
            CreateCard(101, "Action 1", "CardTypeModify", 20, 0),
        };

        var result = _calculator.Calculate(
            allCardsMetadata: allCards,
            startSnapshot: day1Snapshot,
            endSnapshot: day2Snapshot,
            minUseCount: "1%"
        );

        // Daily delta total games = (6 + 3 + 3) / 3 = 4 games played on Day 2
        Assert.Equal(4, result.TotalGames);
        // Daily delta total wins = (4 + 2 + 2) / 3 = 2.66 -> approx rounded to 2 or 3
        Assert.Equal((4 + 2 + 2) / 3, result.TotalWins);
        // Action 1 delta uses = 20 - 15 = 5
        Assert.Single(result.TopUseActions);
        Assert.Equal(5, result.TopUseActions[0].UseCount);
    }

    [Fact]
    public void SnapshotType_Daily_IsDistinctFromCustomAndVersion()
    {
        Assert.NotEqual(SnapshotType.GameVersion, SnapshotType.Daily);
        Assert.NotEqual(SnapshotType.UserCustom, SnapshotType.Daily);
        Assert.Equal("Daily", SnapshotType.Daily.ToString());
    }
}
