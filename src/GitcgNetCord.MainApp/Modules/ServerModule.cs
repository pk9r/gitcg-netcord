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
    }
}
