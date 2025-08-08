using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.EncodeCardCode;

public record Response : HoyolabResponseBase
{
    [JsonPropertyName("data")]
    public Data Data { get; init; } = default!;
}
