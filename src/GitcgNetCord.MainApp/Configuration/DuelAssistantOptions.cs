namespace GitcgNetCord.MainApp.Configuration;

public class DuelAssistantOptions
{
    public const string ConfigurationSectionName = nameof(DuelAssistantOptions);
    
    public string ModelId { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}