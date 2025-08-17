using GitcgNetCord.MainApp.Commands.Slash;
using NetCord;
using NetCord.Hosting.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Modules.Feats;

public static class HoyolabGcgModule
{
    public static void AddHoyolabGcgModule(this IHost host)
    {
        host.AddSlashCommand(
            name: "tcg",
            description: "Show Genshin Impact TCG information.",
            handler: TcgSlashCommand.ExecuteAsync,
            contexts:
            [
                InteractionContextType.Guild,
                InteractionContextType.BotDMChannel,
                InteractionContextType.DMChannel
            ]
        );
        
        host.AddSlashCommand(
            name: "card",
            description: "Show Genshin Impact TCG card information.",
            handler: CardSlashCommand.ExecuteAsync,
            contexts:
            [
                InteractionContextType.Guild,
                InteractionContextType.BotDMChannel,
                InteractionContextType.DMChannel
            ]
        );
    }
}
