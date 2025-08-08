using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.GcgBasicInfo;

public record ChallengeSchedule
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("begin")]
    public MatchTime Begin { get; init; } = new();

    [JsonPropertyName("end")]
    public MatchTime End { get; init; } = new();
}
