using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Models;

public record Deck
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    [JsonPropertyName("avatar_cards")]
    public IReadOnlyCollection<AvatarCard> AvatarCards { get; init; } = null!;
    
    [JsonPropertyName("share_code")]
    public string ShareCode { get; init; } = null!;
}
