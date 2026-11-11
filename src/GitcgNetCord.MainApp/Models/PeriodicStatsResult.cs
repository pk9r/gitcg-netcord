namespace GitcgNetCord.MainApp.Models;

public record CardDeltaStat(
    int Id,
    string Name,
    string CardType,
    int UseCount,
    int Proficiency,
    float WinRate
);

public record PeriodicStatsResult(
    int TotalGames,
    int TotalWins,
    float OverallWinRate,
    int MinUseCountThreshold,
    bool IsPercentThreshold,
    string MinUseCountInput,
    IReadOnlyList<CardDeltaStat> BestWinRateCharacters,
    IReadOnlyList<CardDeltaStat> LowestWinRateCharacters,
    IReadOnlyList<CardDeltaStat> TopUseCharacters,
    IReadOnlyList<CardDeltaStat> TopUseActions
);
