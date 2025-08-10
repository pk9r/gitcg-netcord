namespace GitcgNetCord.MainApp.Configuration;

public class CardCodeModuleOptions
{
    public const string ConfigurationSectionName = nameof(CardCodeModuleOptions);

    public ulong[] CardCodeChannels { get; set; } = [];
}
