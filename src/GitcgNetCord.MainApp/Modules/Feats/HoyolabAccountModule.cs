using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Commands.Slash;
using NetCord;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Modules.Feats;

public static class HoyolabAccountModule
{
    public static void AddHoyolabAccountModule(this IHost host)
    {
        var hoyolabGroup = host.AddSlashCommandGroup(
            name: "hoyolab",
            description: "Manage linked HoYoLAB accounts."
        ).AddContexts(
            InteractionContextType.Guild,
            InteractionContextType.BotDMChannel,
            InteractionContextType.DMChannel
        );

        hoyolabGroup.AddSubCommand(
            name: "list",
            description: "List all linked HoYoLAB accounts and view the current active account.",
            handler: HoyolabAccountsSlashCommand.ListAsync
        );

        hoyolabGroup.AddSubCommand(
            name: "add",
            description: "Link a new HoYoLAB account via secure modal dialogue.",
            handler: HoyolabAccountsSlashCommand.AddAsync
        );

        hoyolabGroup.AddSubCommand(
            name: "remove",
            description: "Remove a linked HoYoLAB account.",
            handler: RemoveHoyolabAccountSlashCommand.ExecuteAsync
        );

        hoyolabGroup.AddSubCommand(
            name: "switch",
            description: "Switch your active HoYoLAB account.",
            handler: HoyolabAccountsSlashCommand.SwitchAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: AddHoyolabAccountComponentInteraction.CustomId,
            handler: AddHoyolabAccountComponentInteraction.ExecuteAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: UpdateActiveHoyolabAccountComponentInteraction.CustomId,
            handler: UpdateActiveHoyolabAccountComponentInteraction.ExecuteAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: RemoveHoyolabAccountComponentInteraction.CustomId,
            handler: RemoveHoyolabAccountComponentInteraction.ExecuteAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: RemoveHoyolabAccountComponentInteraction.ConfirmCustomId,
            handler: RemoveHoyolabAccountComponentInteraction.ConfirmRemovalAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: RemoveHoyolabAccountComponentInteraction.CancelCustomId,
            handler: RemoveHoyolabAccountComponentInteraction.CancelRemovalAsync
        );

        host.AddComponentInteraction<StringMenuInteractionContext>(
            customId: SelectGameRecordComponentInteraction.CustomId,
            handler: SelectGameRecordComponentInteraction.ExecuteAsync
        );

        host.AddComponentInteraction<StringMenuInteractionContext>(
            customId: RemoveHoyolabAccountComponentInteraction.SelectMenuCustomId,
            handler: RemoveHoyolabAccountComponentInteraction.SelectAsync
        );

        host.AddComponentInteraction<StringMenuInteractionContext>(
            customId: RemoveHoyolabAccountComponentInteraction.FallbackActiveSelectCustomId,
            handler: RemoveHoyolabAccountComponentInteraction.SelectFallbackActiveAsync
        );

        host.AddComponentInteraction<ModalInteractionContext>(
            customId: AddHoyolabAccountComponentInteraction.PostCustomId,
            handler: AddHoyolabAccountComponentInteraction.PostAsync
        );

        host.AddComponentInteraction<ModalInteractionContext>(
            customId: UpdateActiveHoyolabAccountComponentInteraction.PostCustomId,
            handler: UpdateActiveHoyolabAccountComponentInteraction.PostAsync
        );
    }
}
