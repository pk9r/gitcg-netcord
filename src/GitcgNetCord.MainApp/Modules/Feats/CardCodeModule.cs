using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Commands.Slash;
using NetCord;
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
        ).AddContexts(
            InteractionContextType.Guild,
            InteractionContextType.BotDMChannel,
            InteractionContextType.DMChannel
        );

        var cardCodeChannelGroup = host.AddSlashCommandGroup(
            name: "cardcode-channel",
            description: "Manage channels for card code recognition."
        )
        .WithDefaultGuildPermissions(Permissions.ManageChannels)
        .AddContexts([InteractionContextType.Guild]);

        cardCodeChannelGroup.AddSubCommand(
            name: "add",
            description: "Register a channel for card code recognition.",
            handler: CardCodeChannelSlashCommand.AddAsync
        );

        cardCodeChannelGroup.AddSubCommand(
            name: "remove",
            description: "Unregister a channel from card code recognition.",
            handler: CardCodeChannelSlashCommand.RemoveAsync
        );

        cardCodeChannelGroup.AddSubCommand(
            name: "list",
            description: "List registered card code channels in this server.",
            handler: CardCodeChannelSlashCommand.ListAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: CopySharingCodeComponentInteraction.BaseCustomId,
            handler: CopySharingCodeComponentInteraction.ExecuteAsync
        );
        
        host.AddComponentInteraction<StringMenuInteractionContext>(
            customId: SelectDecksComponentInteraction.CustomId,
            handler: SelectDecksComponentInteraction.ExecuteAsync
        );
    }
}