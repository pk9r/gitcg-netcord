using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Models;

public record Cost
{
    [JsonPropertyName("cost_type")]
    public string CostType { get; init; } = default!;

    [JsonPropertyName("icon")]
    public string Icon { get; init; } = default!;

    [JsonPropertyName("cost_num")]
    public string CostNum { get; init; } = default!;
}
