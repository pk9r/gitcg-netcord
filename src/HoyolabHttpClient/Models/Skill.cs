using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Models;

public record Skill
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = default!;

    [JsonPropertyName("icon")]
    public string Icon { get; init; } = default!;

    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("type")]
    public IEnumerable<string> Type { get; init; } = default!;

    [JsonPropertyName("rich_desc")]
    public string RichDesc { get; init; } = default!;

    [JsonPropertyName("cost_types")]
    public IEnumerable<Cost> CostTypes { get; init; } = default!;
}
