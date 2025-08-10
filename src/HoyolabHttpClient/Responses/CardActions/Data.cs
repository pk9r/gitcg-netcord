using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.CardActions;

public record Data
{
    [JsonPropertyName("actions")]
    public IReadOnlyCollection<Models.Action> Actions { get; init; } = default!;
}
