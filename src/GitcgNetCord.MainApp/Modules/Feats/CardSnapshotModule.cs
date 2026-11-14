using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Commands.Slash;
using NetCord;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Modules.Feats;

public static class CardSnapshotModule
{
    public static void AddCardSnapshotModule(this IHost host)
    {
        var group = host.AddSlashCommandGroup(
            name: "card-snapshot",
            description: "Manage card statistics snapshots."
        ).AddContexts(
            InteractionContextType.Guild,
            InteractionContextType.BotDMChannel,
            InteractionContextType.DMChannel
        );

        group.AddSubCommand(
            name: "save",
            description: "Save a new snapshot of your current card statistics.",
            handler: CardSnapshotSlashCommand.SaveAsync
        );

        group.AddSubCommand(
            name: "list",
            description: "List all your saved snapshots.",
            handler: CardSnapshotSlashCommand.ListAsync
        );

        group.AddSubCommand(
            name: "daily",
            description: "Enable or disable automatic daily snapshot capture.",
            handler: CardSnapshotSlashCommand.DailyAsync
        );

        group.AddSubCommand(
            name: "delete",
            description: "Delete a custom snapshot.",
            handler: CardSnapshotSlashCommand.DeleteAsync
        );

        group.AddSubCommandGroup(
            name: "channel",
            description: "Manage approved parent channels for daily thread reporting (Admin).",
            builder: channelGroup =>
            {
                channelGroup.AddSubCommand(
                    name: "add",
                    description: "Register an approved parent channel for daily thread reporting.",
                    handler: CardSnapshotSlashCommand.ChannelAddAsync
                );

                channelGroup.AddSubCommand(
                    name: "remove",
                    description: "Unregister an approved parent channel from daily thread reporting.",
                    handler: CardSnapshotSlashCommand.ChannelRemoveAsync
                );

                channelGroup.AddSubCommand(
                    name: "list",
                    description: "List approved parent channels registered for daily thread reporting.",
                    handler: CardSnapshotSlashCommand.ChannelListAsync
                );
            }
        );

        group.AddSubCommandGroup(
            name: "thread",
            description: "Configure daily statistics report delivery in this thread.",
            builder: threadGroup =>
            {
                threadGroup.AddSubCommand(
                    name: "subscribe",
                    description: "Subscribe a Hoyolab account to daily statistics reports in this thread.",
                    handler: CardSnapshotSlashCommand.ThreadSubscribeAsync
                );

                threadGroup.AddSubCommand(
                    name: "unsubscribe",
                    description: "Unsubscribe a Hoyolab account from daily statistics reports in this thread.",
                    handler: CardSnapshotSlashCommand.ThreadUnsubscribeAsync
                );

                threadGroup.AddSubCommand(
                    name: "list",
                    description: "List Hoyolab accounts currently subscribed to daily reports in this thread.",
                    handler: CardSnapshotSlashCommand.ThreadListAsync
                );
            }
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: DailySnapshotPromptComponentInteraction.EnableCustomId,
            handler: DailySnapshotPromptComponentInteraction.EnableAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: DailySnapshotPromptComponentInteraction.SkipCustomId,
            handler: DailySnapshotPromptComponentInteraction.SkipAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: DailyReportDestinationComponentInteraction.DmCustomId,
            handler: DailyReportDestinationComponentInteraction.SelectDmAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: DailyReportDestinationComponentInteraction.ChannelCustomId,
            handler: DailyReportDestinationComponentInteraction.SelectChannelAsync
        );

        host.AddComponentInteraction<ButtonInteractionContext>(
            customId: DailyReportDestinationComponentInteraction.SkipCustomId,
            handler: DailyReportDestinationComponentInteraction.SkipDestinationAsync
        );
    }
}
