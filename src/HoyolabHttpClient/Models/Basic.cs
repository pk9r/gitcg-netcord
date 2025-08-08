using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Models;

public record Basic
{
    [JsonPropertyName("item_id")]
    public int ItemId { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("icon")]
    public string Icon { get; init; } = default!;

    [JsonPropertyName("icon_small")]
    public string IconSmall { get; init; } = default!;

    [JsonPropertyName("wiki_url")]
    public string WikiUrl { get; init; } = default!;

    [JsonPropertyName("max_cnt")]
    public int MaxCnt { get; init; }

}
