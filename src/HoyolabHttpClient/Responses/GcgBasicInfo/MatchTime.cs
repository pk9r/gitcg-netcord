using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.GcgBasicInfo;

public record MatchTime
{
    [JsonPropertyName("year")]
    public int Year { get; init; }

    [JsonPropertyName("month")]
    public int Month { get; init; }

    [JsonPropertyName("day")]
    public int Day { get; init; }

    [JsonPropertyName("hour")]
    public int Hour { get; init; }

    [JsonPropertyName("minute")]
    public int Minute { get; init; }

    [JsonPropertyName("second")]
    public int Second { get; init; }
}
