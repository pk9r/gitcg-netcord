using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Enums;

public enum SharingCodeValidationRuleType
{
    [SlashCommandChoice(Name = "Decodable")] Decodable,
    [SlashCommandChoice(Name = "Playable")] Playable
}
