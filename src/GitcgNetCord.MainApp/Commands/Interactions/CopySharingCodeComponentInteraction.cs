using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Commands.Interactions;

public static class CopySharingCodeComponentInteraction
{
    public const string BaseCustomId = "copy-sharing-code";

    public static string GetCustomId(string sharingCode)
        => $"{BaseCustomId}:{sharingCode}";

    public static async Task ExecuteAsync(
        ButtonInteractionContext context,
        string sharingCode
    )
    {
        await context.Interaction.SendResponseAsync(
            callback: InteractionCallback.Message(
                new InteractionMessageProperties()
                    .WithContent(sharingCode)
                    .WithFlags(MessageFlags.Ephemeral)
            )
        );
    }

    public static IButtonProperties
        CreateCopySharingCodeButton(
            string sharingCode
        )
    {
        return new ButtonProperties(
            customId: GetCustomId(sharingCode),
            emoji: EmojiProperties.Standard("📋"),
            label: "Copy sharing code",
            style: ButtonStyle.Secondary);
    }
}