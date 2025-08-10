using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.WebLoginByPassword;

public record Data
{
    [JsonPropertyName("user_info")]
    public UserInfo UserInfo { get; init; } = default!;
}
