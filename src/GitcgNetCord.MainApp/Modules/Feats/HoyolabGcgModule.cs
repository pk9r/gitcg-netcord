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
            handler: TcgSlashCommand.ExecuteAsync
        ).AddContexts(
            InteractionContextType.Guild,
            InteractionContextType.BotDMChannel,
            InteractionContextType.DMChannel
        ); ;
        
        host.AddSlashCommand(
            name: "card",
            description: "Show Genshin Impact TCG card information.",
            handler: CardSlashCommand.ExecuteAsync
        ).AddContexts(
            InteractionContextType.Guild,
            InteractionContextType.BotDMChannel,
            InteractionContextType.DMChannel
        ); ;

        host.AddSlashCommand(
            name: "tcg-card",
            description: "Information about a specific Genius Invokation card.",
            handler: TcgCardSlashCommand.ExecuteAsync
        ).AddContexts(
            InteractionContextType.Guild,
            InteractionContextType.BotDMChannel,
            InteractionContextType.DMChannel
        );
    }
}
