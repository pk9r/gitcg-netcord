using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.GcgBasicInfo;

public record PlayerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("linups")]
    public IReadOnlyCollection<string> Linups { get; init; } = null!;

    [JsonPropertyName("is_overflow")]
    public bool IsOverflow { get; init; }
}
