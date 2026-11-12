using GitcgNetCord.MainApp.Commands.Slash;
using NetCord;
using NetCord.Hosting.Services.ApplicationCommands;

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
            description: "Bật hoặc tắt tự động chụp Daily Snapshot theo ngày.",
            handler: CardSnapshotSlashCommand.DailyAsync
        );

        group.AddSubCommand(
            name: "delete",
            description: "Delete a custom snapshot.",
            handler: CardSnapshotSlashCommand.DeleteAsync
        );
    }
}
