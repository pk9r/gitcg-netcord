using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.WebLoginByPassword;
public record UserInfo
{
    [JsonPropertyName("aid")]
    public string AccountId { get; init; } = default!;
}
