using System.Collections.Generic;
using System.Text.Json.Serialization;
using HoyolabHttpClient.Models;
using HoyolabHttpClient.Models.Interfaces;

namespace HoyolabHttpClient.Responses.DecodeCardCode;

public record Data : IDeckData
{
    [JsonPropertyName("role_cards")]
    public IReadOnlyCollection<Role> RoleCards { get; init; } = null!;

    [JsonPropertyName("action_cards")]
    public IReadOnlyCollection<Action> ActionCards { get; init; } = null!;
}
