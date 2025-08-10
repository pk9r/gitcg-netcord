using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.GcgBasicInfo;

public record Replay
{
    [JsonPropertyName("game_id")]
    public string GameId { get; init; } = string.Empty;

    [JsonPropertyName("self")]
    public PlayerInfo Self { get; init; } = new();

    [JsonPropertyName("opposite")]
    public PlayerInfo Opposite { get; init; } = new();

    [JsonPropertyName("match_type")]
    public string MatchType { get; init; } = string.Empty;

    [JsonPropertyName("match_time")]
    public MatchTime MatchTime { get; init; } = new();

    [JsonPropertyName("is_win")]
    public bool IsWin { get; init; }
}
