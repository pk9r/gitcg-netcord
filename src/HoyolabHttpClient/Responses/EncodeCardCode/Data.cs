using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.EncodeCardCode;

public record Data
{
    [JsonPropertyName("code")]
    public string Code { get; init; } = default!;
}
