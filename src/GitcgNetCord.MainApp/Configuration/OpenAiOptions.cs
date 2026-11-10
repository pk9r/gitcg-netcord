namespace GitcgNetCord.MainApp.Configuration;

public class OpenAiOptions
{
    public const string ConfigurationSectionName = nameof(OpenAiOptions);

    public bool Enabled { get; set; } = false;
    public string ModelId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
}

