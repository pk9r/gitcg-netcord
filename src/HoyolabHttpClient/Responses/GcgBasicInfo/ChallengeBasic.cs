using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.GcgBasicInfo;

public record ChallengeBasic
{
    [JsonPropertyName("schedule")]
    public ChallengeSchedule Schedule { get; init; } = new();

    [JsonPropertyName("nickname")]
    public string Nickname { get; init; } = string.Empty;

    [JsonPropertyName("uid")]
    public string Uid { get; init; } = string.Empty;

    [JsonPropertyName("win_cnt")]
    public int WinCnt { get; init; }

    [JsonPropertyName("medal")]
    public string Medal { get; init; } = string.Empty;

    [JsonPropertyName("has_data")]
    public bool HasData { get; init; }
}
