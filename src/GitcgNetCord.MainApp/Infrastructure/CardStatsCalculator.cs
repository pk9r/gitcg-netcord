using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Models;
using HoyolabHttpClient.Models;

namespace GitcgNetCord.MainApp.Infrastructure;

public interface ICardStatsCalculator
{
    PeriodicStatsResult Calculate(
        IReadOnlyCollection<Card> allCardsMetadata,
        IReadOnlyDictionary<int, CardStatSummary>? startSnapshot,
        IReadOnlyDictionary<int, CardStatSummary>? endSnapshot,
        string minUseCount = "1%",
        int topWinRateCount = 5,
        int topUseCount = 10
    );
}

public class CardStatsCalculator : ICardStatsCalculator
{
    public PeriodicStatsResult Calculate(
        IReadOnlyCollection<Card> allCardsMetadata,
        IReadOnlyDictionary<int, CardStatSummary>? startSnapshot,
        IReadOnlyDictionary<int, CardStatSummary>? endSnapshot,
        string minUseCount = "1%",
        int topWinRateCount = 5,
        int topUseCount = 10
    )
    {
        var deltaList = new List<CardDeltaStat>(allCardsMetadata.Count);

        foreach (var card in allCardsMetadata)
        {
            var endUse = card.UseCount;
            var endProf = card.Proficiency;
            if (endSnapshot != null)
            {
                if (endSnapshot.TryGetValue(card.Id, out var endSummary))
                {
                    endUse = endSummary.UseCount;
                    endProf = endSummary.Proficiency;
                }
                else
                {
                    endUse = 0;
                    endProf = 0;
                }
            }

            var startUse = 0;
            var startProf = 0;
            if (startSnapshot != null && startSnapshot.TryGetValue(card.Id, out var startSummary))
            {
                startUse = startSummary.UseCount;
                startProf = startSummary.Proficiency;
            }

            var deltaUse = Math.Max(0, endUse - startUse);
            var deltaProf = Math.Max(0, endProf - startProf);
            var winRate = deltaUse > 0 ? (float)deltaProf / deltaUse : 0f;

            deltaList.Add(new CardDeltaStat(
                Id: card.Id,
                Name: card.Name,
                CardType: card.CardType,
                UseCount: deltaUse,
                Proficiency: deltaProf,
                WinRate: winRate
            ));
        }

        var characters = deltaList
            .Where(x => x.CardType == "CardTypeCharacter")
            .ToList();

        var actions = deltaList
            .Where(x => x.CardType != "CardTypeCharacter")
            .ToList();

        var totalGames = characters.Sum(x => x.UseCount) / 3;
        var totalWins = characters.Sum(x => x.Proficiency) / 3;
        var overallWinRate = totalGames > 0 ? (float)totalWins / totalGames : 0f;

        var minUseCountValue = 0;
        var isPercent = minUseCount.EndsWith('%');
        if (isPercent)
        {
            if (float.TryParse(minUseCount.AsSpan(0, minUseCount.Length - 1), out var percent))
            {
                minUseCountValue = (int)(totalGames * percent / 100);
            }
        }
        else
        {
            int.TryParse(minUseCount, out minUseCountValue);
        }

        var winRateList = characters
            .Where(x => x.UseCount > minUseCountValue)
            .ToList();

        var bestWinRate = winRateList
            .OrderByDescending(x => x.WinRate)
            .ThenByDescending(x => x.UseCount)
            .Take(topWinRateCount)
            .ToList();

        var lowestWinRate = winRateList
            .OrderBy(x => x.WinRate)
            .ThenByDescending(x => x.UseCount)
            .Take(topWinRateCount)
            .ToList();

        var usedCharacters = characters
            .Where(x => x.UseCount > 0)
            .OrderByDescending(x => x.UseCount)
            .ThenByDescending(x => x.WinRate)
            .ToList();

        var topUseCharacters = usedCharacters
            .Take(topUseCount)
            .ToList();

        var topUseActions = actions
            .Where(x => x.UseCount > 0)
            .OrderByDescending(x => x.UseCount)
            .Take(topUseCount)
            .ToList();

        return new PeriodicStatsResult(
            TotalGames: totalGames,
            TotalWins: totalWins,
            OverallWinRate: overallWinRate,
            MinUseCountThreshold: minUseCountValue,
            IsPercentThreshold: isPercent,
            MinUseCountInput: minUseCount,
            BestWinRateCharacters: bestWinRate,
            LowestWinRateCharacters: lowestWinRate,
            TopUseCharacters: topUseCharacters,
            TopUseActions: topUseActions,
            UsedCharacters: usedCharacters
        );
    }
}
