using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Commands.Slash;
using NetCord;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Modules.Feats;

public static class DuelModule
{
    public static void AddDuelModule(this IHost host)
    {
        host.AddSlashCommand(
            name: "duel",
            description: "Start a duel with another player.",
            handler: DuelSlashCommand.ExecuteAsync
        ).AddContexts(
            InteractionContextType.Guild
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: AcceptDuelComponentInteraction.CustomId,
            handler: AcceptDuelComponentInteraction.ExecuteAsync
        );
    }
}