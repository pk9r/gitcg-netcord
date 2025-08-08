using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.CardActions;

public record Response : HoyolabResponseBase
{
    [JsonPropertyName("data")]
    public Data? Data { get; init; }
}
