using System.Collections.Generic;
using System.Text.Json.Serialization;
using HoyolabHttpClient.Models;
using HoyolabHttpClient.Models.Interfaces;

namespace HoyolabHttpClient.Responses.DecodeCardCode;

public record Data : IDeckData
{
    [JsonPropertyName("role_cards")]
    public IReadOnlyList<Role> RoleCards { get; init; } = null!;

    [JsonPropertyName("action_cards")]
    public IReadOnlyList<Action> ActionCards { get; init; } = null!;
}
