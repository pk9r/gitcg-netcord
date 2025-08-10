using System.Collections.Generic;
using System.Text.Json.Serialization;
using HoyolabHttpClient.Models;

namespace HoyolabHttpClient.Responses.Skills;

public record Data
{
    [JsonPropertyName("skills")]
    public IReadOnlyCollection<Skill> Skills { get; init; } = default!;
}
