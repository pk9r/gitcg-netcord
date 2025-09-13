using GitcgNetCord.MainApp.Commands.Slash;
using NetCord.Hosting.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Modules;

public static class UpdateEmojisModule
{
    public static void AddUpdateEmojisModule(this IHost host)
    {
        host.AddSlashCommand(
            name: "update-emojis",
            description: "Fetch emojis and update the bot's emojis.",
            handler: UpdateEmojisSlashCommand.ExecuteAsync
        );
    }
}
