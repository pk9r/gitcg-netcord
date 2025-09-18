using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses;

public record HoyolabResponseBase
{
    [JsonPropertyName("retcode")]
    public int Retcode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

public record HoyolabResponseBase<T> 
    : HoyolabResponseBase
{
    [JsonPropertyName("data")]
    public T? Data { get; init; }
}
