using GitcgNetCord.MainApp.Enums;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class DuelSlashCommand
{
    public static async Task ExecuteAsync(
        IServiceProvider serviceProvider,
        ApplicationCommandContext context,
        [
            SlashCommandParameter(
                Description = "Discord user to duel."
            )
        ]
        GuildUser opponent,
        [
            SlashCommandParameter(
                Description = "Duel type."
            )
        ]
        DuelType type
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage()
        );

        await context.Interaction.ModifyResponseAsync(message =>
        {
            message.Content = $"{context.User} has challenged {opponent} to a duel of type {type}.";
            message.AddComponents(new ActionRowProperties().AddButtons(
                new ButtonProperties(
                    customId: "accept-duel",
                    label: "Accept",
                    style: ButtonStyle.Success
                ),
                new ButtonProperties(
                    customId: "decline-duel",
                    label: "Decline",
                    style: ButtonStyle.Danger
                )
            ));
        });
    }
}