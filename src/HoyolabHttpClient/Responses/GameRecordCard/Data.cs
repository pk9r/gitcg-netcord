using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.GameRecordCard;

public record Data
{
    [JsonPropertyName("list")]
    public IReadOnlyCollection<GameRecord> List { get; init; } = null!;
}
