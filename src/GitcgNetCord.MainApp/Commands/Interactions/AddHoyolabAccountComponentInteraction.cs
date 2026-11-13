using GitcgNetCord.MainApp.Commands.Slash;
using GitcgNetCord.MainApp.Extensions;
using HoyolabHttpClient;
using Microsoft.Extensions.Caching.Hybrid;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Commands.Interactions;

public static class AddHoyolabAccountComponentInteraction
{
    public const string CustomId = "add-hoyolab-account";
    public const string PostCustomId = "post-hoyolab-account";

    public static async Task ExecuteAsync(
        ButtonInteractionContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.Modal(
                new ModalProperties(
                    customId: PostCustomId,
                    title: "Hoyolab account"
                )
                {
                    new LabelProperties(
                        label: "Hoyolab user ID (ltuid_v2)",
                        component: new TextInputProperties(
                            customId: "hoyolab-user-id",
                            style: TextInputStyle.Short
                        )
                        {
                            MaxLength = 10,
                            Placeholder = "ltuid_v2"
                        }
                    ),
                    new LabelProperties(
                        label: "Hoyolab token (ltoken_v2)",
                        component: new TextInputProperties(
                            customId: "token",
                            style: TextInputStyle.Paragraph)
                        {
                            MaxLength = 255,
                            Placeholder = "v2_..."
                        }
                    )
                }
            )
        );
    }

    public static async Task PostAsync(
        IServiceProvider serviceProvider,
        ModalInteractionContext context
    )
    {
        var hoyolabUserId = context.GetTextInputValue("hoyolab-user-id")?.Trim();
        var token = context.GetTextInputValue("token")?.Trim();

        if (string.IsNullOrWhiteSpace(hoyolabUserId) || string.IsNullOrWhiteSpace(token))
        {
            await context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new InteractionMessageProperties
                {
                    Content = "Please provide both Hoyolab user ID and token.",
                    Flags = MessageFlags.Ephemeral
                })
            );
            return;
        }

        await AddHoyolabAccountSlashCommand.ExecuteCoreAsync(
            serviceProvider, context, hoyolabUserId, token
        );
    }
}
