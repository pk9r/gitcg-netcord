using Gitcg.NetCord.MainApp.Commands.Slash;
using NetCord.Hosting.Services.ApplicationCommands;

namespace Gitcg.NetCord.MainApp.Modules;

public static class RoleEmojisModule
{
    public static void AddRoleEmojisModule(this IHost host)
    {
        host.AddSlashCommand(
            name: "update-role-emojis",
            description: "Fetch emojis for roles from Hoyolab.",
            handler: UpdateRoleEmojisSlashCommand.ExecuteAsync
        );
    }
}
