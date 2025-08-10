using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Commands.Slash;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Modules.Feats;

public static class CardCodeModule
{
    public static void AddCardCodeModule(this IHost host)
    {
        host.AddSlashCommand(
            name: "deck",
            description: "View cards from deck sharing code.",
            handler: DeckSlashCommand.ExecuteAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: CopySharingCodeComponentInteraction.BaseCustomId,
            handler: CopySharingCodeComponentInteraction.ExecuteAsync
        );
        
    }
}
