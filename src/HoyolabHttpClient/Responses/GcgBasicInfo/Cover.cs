using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.GcgBasicInfo;

public record Cover
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("image")]
    public string Image { get; init; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; init; } = string.Empty;

    [JsonPropertyName("action_cost")]
    public List<ActionCost> ActionCost { get; init; } = new();

    [JsonPropertyName("has_data")]
    public bool HasData { get; init; }

    [JsonPropertyName("image_v2")]
    public string ImageV2 { get; init; } = string.Empty;
}
