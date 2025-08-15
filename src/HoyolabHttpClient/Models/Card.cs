using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Models;

public record Card
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    
    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;
    
    [JsonPropertyName("image")]
    public string Image { get; init; } = null!;
    
    [JsonPropertyName("desc")]
    public string Desc { get; init; } = null!;
    
    [JsonPropertyName("card_type")]
    public string CardType { get; init; } = null!;
    
    [JsonPropertyName("num")]
    public int Num { get; init; }
    
    [JsonPropertyName("tags")]
    public IReadOnlyCollection<string> Tags { get; init; } = null!;
    
    [JsonPropertyName("proficiency")]
    public int Proficiency { get; init; }
    
    [JsonPropertyName("use_count")]
    public int UseCount { get; init; }
    
    [JsonPropertyName("hp")]
    public int Hp { get; init; }
    
    [JsonPropertyName("card_skills")]
    public IReadOnlyCollection<CardSkill> CardSkills { get; init; } = null!;
    
    [JsonPropertyName("action_cost")]
    public IReadOnlyCollection<ActionCost> ActionCost { get; init; } = null!;
    
    [JsonPropertyName("card_sources")]
    public IReadOnlyCollection<string> CardSources { get; init; } = null!;
    
    [JsonPropertyName("rank_id")]
    public int RankId { get; init; }
    
    [JsonPropertyName("deck_recommend")]
    public string DeckRecommend { get; init; } = null!;
    
    [JsonPropertyName("card_wiki")]
    public string CardWiki { get; init; } = null!;
    
    [JsonPropertyName("icon")]
    public string Icon { get; init; } = null!;
    
    [JsonPropertyName("large_icon")]
    public string LargeIcon { get; init; } = null!;
    
    [JsonPropertyName("category")]
    public string Category { get; init; } = null!;
}

public record CardSkill
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = null!;

    [JsonPropertyName("desc")]
    public string Desc { get; init; } = null!;

    [JsonPropertyName("tag")]
    public string Tag { get; init; } = null!;
}