using GitcgNetCord.MainApp.Commands.Slash;
using NetCord;
using NetCord.Hosting.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Modules;

public static class ServerModule
{
    public static void AddServerModule(this IHost host)
    {
        var serverGroup = host.AddSlashCommandGroup(
            name: "server",
            description: "Manage server-level bot configuration."
        )
        .WithDefaultGuildPermissions(Permissions.ManageGuild)
        .AddContexts([InteractionContextType.Guild]);

        serverGroup.AddSubCommandGroup(
            name: "overview",
            description: "View overall server bot configuration.",
            builder: overviewGroup =>
            {
                overviewGroup.AddSubCommand(
                    name: "show",
                    description: "Show a dashboard summarizing all bot configurations for this server.",
                    handler: ServerSlashCommand.OverviewShowAsync
                );
            }
        );

        serverGroup.AddSubCommandGroup(
            name: "invite",
            description: "Configure the server's public invite URL.",
            builder: inviteGroup =>
            {
                inviteGroup.AddSubCommand(
                    name: "set",
                    description: "Set the public Discord invite URL for this server.",
                    handler: ServerSlashCommand.InviteSetAsync
                );

                inviteGroup.AddSubCommand(
                    name: "view",
                    description: "View the currently configured Discord invite URL.",
                    handler: ServerSlashCommand.InviteViewAsync
                );

                inviteGroup.AddSubCommand(
                    name: "remove",
                    description: "Remove the configured Discord invite URL.",
                    handler: ServerSlashCommand.InviteRemoveAsync
                );
            }
        );

        serverGroup.AddSubCommandGroup(
            name: "cardcode",
            description: "Manage channels for card code recognition.",
            builder: cardCodeGroup =>
            {
                cardCodeGroup.AddSubCommand(
                    name: "add",
                    description: "Register a channel for card code recognition.",
                    handler: CardCodeChannelSlashCommand.AddAsync
                );

                cardCodeGroup.AddSubCommand(
                    name: "remove",
                    description: "Unregister a channel from card code recognition.",
                    handler: CardCodeChannelSlashCommand.RemoveAsync
                );

                cardCodeGroup.AddSubCommand(
                    name: "list",
                    description: "List registered card code channels in this server.",
                    handler: CardCodeChannelSlashCommand.ListAsync
                );
            }
        );

        serverGroup.AddSubCommandGroup(
            name: "report-channel",
            description: "Manage approved parent channels for daily thread reporting.",
            builder: reportGroup =>
            {
                reportGroup.AddSubCommand(
                    name: "add",
                    description: "Register an approved parent channel for daily thread reporting.",
                    handler: CardSnapshotSlashCommand.ChannelAddAsync
                );

                reportGroup.AddSubCommand(
                    name: "remove",
                    description: "Unregister an approved parent channel from daily thread reporting.",
                    handler: CardSnapshotSlashCommand.ChannelRemoveAsync
                );

                reportGroup.AddSubCommand(
                    name: "list",
                    description: "List approved parent channels registered for daily thread reporting.",
                    handler: CardSnapshotSlashCommand.ChannelListAsync
                );
            }
        );
    }
}
