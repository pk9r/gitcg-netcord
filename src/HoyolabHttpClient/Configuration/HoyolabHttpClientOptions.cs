namespace HoyolabHttpClient.Configuration;

public class HoyolabHttpClientOptions
{
    public const string ConfigurationSectionName = nameof(HoyolabHttpClientOptions);

    public HoyolabAuthorize? DefaultAuthorize { get; set; }
}
