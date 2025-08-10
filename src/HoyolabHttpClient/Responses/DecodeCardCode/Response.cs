using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.DecodeCardCode;

public record Response : HoyolabResponseBase
{
    [JsonPropertyName("data")]
    public Data? Data { get; init; }
}
