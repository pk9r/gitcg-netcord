using System.Collections.Generic;
using System.Text.Json.Serialization;
using HoyolabHttpClient.Models;

namespace HoyolabHttpClient.Responses.CardRoles;

public record Data
{
    [JsonPropertyName("roles")]
    public IReadOnlyCollection<Role> Roles { get; init; } = default!;
}
