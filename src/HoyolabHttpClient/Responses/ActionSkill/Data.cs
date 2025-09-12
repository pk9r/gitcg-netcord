using System.Collections.Generic;
using System.Text.Json.Serialization;
using HoyolabHttpClient.Models;

namespace HoyolabHttpClient.Responses.ActionSkill;

public record Data
{
    [JsonPropertyName("cost1_raw")]
    public int Cost1Raw { get; init; }

    [JsonPropertyName("cost1_type_icon")]
    public string Cost1TypeIcon { get; init; } = string.Empty;

    [JsonPropertyName("cost1_type_raw")]
    public string Cost1TypeRaw { get; init; } = string.Empty;

    [JsonPropertyName("cost2_raw")]
    public int Cost2Raw { get; init; }

    [JsonPropertyName("cost2_type_icon")]
    public string Cost2TypeIcon { get; init; } = string.Empty;

    [JsonPropertyName("cost2_type_raw")]
    public string Cost2TypeRaw { get; init; } = string.Empty;

    [JsonPropertyName("desc")]
    public string Desc { get; init; } = string.Empty;

    [JsonPropertyName("has_good_id")]
    public bool HasGoodId { get; init; }
}
