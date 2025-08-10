using System.Collections.Generic;
using System.Text.Json.Serialization;

using HoyolabHttpClient.Models.Interfaces;

namespace HoyolabHttpClient.Models;

public record Action : ICardBasic
{
    [JsonPropertyName("basic")]
    public Basic Basic { get; init; } = null!;

    [JsonPropertyName("card_tag")]
    public IEnumerable<string> CardTags { get; init; } = null!;

    [JsonPropertyName("skill_element")]
    public string SkillElement { get; init; } = null!;

    [JsonPropertyName("skill_value")]
    public int SkillValue { get; init; }

    [JsonPropertyName("energy_cost")]
    public int EnergyCost { get; init; }

    [JsonPropertyName("card_type")]
    public int CardType { get; init; }

    [JsonPropertyName("skill_element2")]
    public string SkillElement2 { get; init; } = null!;

    [JsonPropertyName("skill_value2")]
    public int SkillValue2 { get; init; }
}
