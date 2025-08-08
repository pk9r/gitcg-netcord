using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace HoyolabHttpClient.Responses.GameRecordCard;

public record GameRecord
{
    [JsonPropertyName("background_color")]
    public string BackgroundColor { get; init; } = null!;
    
    [JsonPropertyName("background_image")]
    public string BackgroundImage { get; init; } = null!;
    
    [JsonPropertyName("background_image_v2")]
    public string BackgroundImageV2 { get; init; } = null!;
    
    [JsonPropertyName("data")]
    public JsonArray Data { get; init; } = null!;
    
    [JsonPropertyName("data_switches")]
    public JsonArray DataSwitches { get; init; } = null!;
    
    [JsonPropertyName("game_id")]
    public int GameId { get; init; }
    
    [JsonPropertyName("game_name")]
    public string GameName { get; init; } = null!;

    [JsonPropertyName("game_role_id")]
    public string GameRoleId { get; init; } = null!; // Game UID

    [JsonPropertyName("h5_data_switches")]
    public JsonArray H5DataSwitches { get; init; } = null!;
    
    [JsonPropertyName("has_role")]
    public bool HasRole { get; init; }
    
    [JsonPropertyName("is_public")]
    public bool IsPublic { get; init; }

    [JsonPropertyName("level")]
    public int Level { get; init; }
    
    [JsonPropertyName("logo")]
    public string Logo { get; init; } = null!;
    
    [JsonPropertyName("nickname")]
    public string Nickname { get; init; } = null!;

    [JsonPropertyName("region")]
    public string Region { get; init; } = null!;
    
    [JsonPropertyName("region_name")]
    public string RegionName { get; init; } = null!;
    
    [JsonPropertyName("url")]
    public string Url { get; init; } = null!;
}
