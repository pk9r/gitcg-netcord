using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.GcgBasicInfo;

public record ActionCost
{
    [JsonPropertyName("cost_type")]
    public string CostType { get; init; } = string.Empty;

    [JsonPropertyName("cost_value")]
    public int CostValue { get; init; }
}
