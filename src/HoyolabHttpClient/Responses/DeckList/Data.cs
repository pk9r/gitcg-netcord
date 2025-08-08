using System.Collections.Generic;
using System.Text.Json.Serialization;
using HoyolabHttpClient.Models;

namespace HoyolabHttpClient.Responses.DeckList;
public record Data
{
    [JsonPropertyName("deck_list")]
    public IReadOnlyCollection<Deck> DeckList { get; init; } = null!;

    [JsonPropertyName("role_id")]
    public string RoleId { get; init; } = null!;

    [JsonPropertyName("level")]
    public int Level { get; init; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; init; } = null!;
}
