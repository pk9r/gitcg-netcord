using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.GcgBasicInfo;

public record Data
{
    [JsonPropertyName("level")]
    public int Level { get; init; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; init; } = string.Empty;

    [JsonPropertyName("avatar_card_num_gained")]
    public int AvatarCardNumGained { get; init; }

    [JsonPropertyName("avatar_card_num_total")]
    public int AvatarCardNumTotal { get; init; }

    [JsonPropertyName("action_card_num_gained")]
    public int ActionCardNumGained { get; init; }

    [JsonPropertyName("action_card_num_total")]
    public int ActionCardNumTotal { get; init; }

    [JsonPropertyName("covers")]
    public IReadOnlyCollection<Cover> Covers { get; init; } = null!;

    [JsonPropertyName("replays")]
    public IReadOnlyCollection<Replay> Replays { get; init; } = null!;

    [JsonPropertyName("hornor_character")]
    public HornorCharacter? HornorCharacter { get; init; }

    [JsonPropertyName("challenge_basic")]
    public ChallengeBasic? ChallengeBasic { get; init; }

    [JsonPropertyName("is_hide_covers")]
    public bool IsHideCovers { get; init; }

    [JsonPropertyName("is_hide_replays")]
    public bool IsHideReplays { get; init; }
}
