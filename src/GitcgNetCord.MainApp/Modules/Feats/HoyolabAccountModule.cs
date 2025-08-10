using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Commands.Slash;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Modules.Feats;

public static class HoyolabAccountModule
{
    public static void AddHoyolabAccountModule(this IHost host)
    {
        if (host is WebApplication app && app.Environment.IsDevelopment())
        {
            host.AddSlashCommand(
                name: "add-hoyolab-account",
                description: "Add your Hoyolab account.",
                handler: AddHoyolabAccountSlashCommand.ExecuteAsync
            );
        }

        host.AddSlashCommand(
            name: "hoyolab-accounts",
            description: "Manage your Hoyolab account.",
            handler: HoyolabAccountsSlashCommand.ExecuteAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: AddHoyolabAccountComponentInteraction.CustomId,
            handler: AddHoyolabAccountComponentInteraction.ExecuteAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: UpdateActiveHoyolabAccountComponentInteraction.CustomId,
            handler: UpdateActiveHoyolabAccountComponentInteraction.ExecuteAsync
        );

        host.AddComponentInteraction<StringMenuInteractionContext>(
            customId: SelectGameRecordComponentInteraction.CustomId,
            handler: SelectGameRecordComponentInteraction.ExecuteAsync
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
