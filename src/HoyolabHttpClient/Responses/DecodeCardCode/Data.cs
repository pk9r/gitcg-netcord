using System.Collections.Generic;
using System.Text.Json.Serialization;
using HoyolabHttpClient.Models;
using HoyolabHttpClient.Models.Interfaces;

namespace HoyolabHttpClient.Responses.DecodeCardCode;

public record Data : IDeckData
{
    [JsonPropertyName("role_cards")]
    public IReadOnlyList<Role> RoleCards { get; init; } = default!;

    [JsonPropertyName("action_cards")]
    public IReadOnlyList<Models.Action> ActionCards { get; init; } = default!;
}
