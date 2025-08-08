using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.GcgBasicInfo;

public record HornorCharacter
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("card_face")]
    public string CardFace { get; init; } = string.Empty;

    [JsonPropertyName("card_icon")]
    public string CardIcon { get; init; } = string.Empty;

    [JsonPropertyName("kill_cnt")]
    public int KillCnt { get; init; }

    [JsonPropertyName("max_damage")]
    public int MaxDamage { get; init; }

    [JsonPropertyName("damage_value")]
    public int DamageValue { get; init; }

    [JsonPropertyName("hurt_value")]
    public int HurtValue { get; init; }

    [JsonPropertyName("hornor_cnt")]
    public int HornorCnt { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
}
