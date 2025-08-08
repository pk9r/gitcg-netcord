using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Models;

public record AvatarCard
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
}
