using System.Text.Json.Serialization;

using HoyolabHttpClient.Models.Interfaces;

namespace HoyolabHttpClient.Models;

public record Role : ICardBasic
{
    [JsonPropertyName("basic")]
    public Basic Basic { get; init; } = null!;

    [JsonPropertyName("element")]
    public int Element { get; init; }

    [JsonPropertyName("hp")]
    public int Hp { get; init; }

    [JsonPropertyName("skill_cost")]
    public int SkillCost { get; init; }
}
