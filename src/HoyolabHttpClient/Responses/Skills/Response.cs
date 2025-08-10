using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.Skills;

public record Response : HoyolabResponseBase
{
    [JsonPropertyName("data")]
    public Data? Data { get; init; }
}
