using System.Collections.Generic;
using System.Text.Json.Serialization;
using HoyolabHttpClient.Models;

namespace HoyolabHttpClient.Responses.GcgCardList;

public record Data
{
    [JsonPropertyName("card_list")]
    public IReadOnlyCollection<Card> CardList { get; init; } = null!;
    
    [JsonPropertyName("is_last")]
    public bool IsLast { get; init; }
    
    [JsonPropertyName("next_offset")]
    public int NextOffset { get; init; }
    
    [JsonPropertyName("stats")]
    public Stats Stats { get; init; } = null!;
}

public record Stats
{
    [JsonPropertyName("level")]
    public int Level { get; init; }
    
    [JsonPropertyName("nickname")]
    public string Nickname { get; init; } = null!;
    
    [JsonPropertyName("avatar_card_num_gained")]
    public int AvatarCardNumGained { get; init; }
    
    [JsonPropertyName("avatar_card_num_total")]
    public int AvatarCardNumTotal { get; init; }
    
    [JsonPropertyName("action_card_num_gained")]
    public int ActionCardNumGained { get; init; }
    
    [JsonPropertyName("action_card_num_total")]
    public int ActionCardNumTotal { get; init; }
}