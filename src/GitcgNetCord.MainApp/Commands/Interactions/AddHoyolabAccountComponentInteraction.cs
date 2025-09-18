using GitcgNetCord.MainApp.Commands.Slash;
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
        string hoyolabUserId = "", token = "";

        var textInputs = context.Components.OfType<TextInput>();

        foreach (var textInput in textInputs)
        {
            switch (textInput.CustomId)
            {
                case "hoyolab-user-id":
                    hoyolabUserId = textInput.Value;
                    break;
                case "token":
                    token = textInput.Value;
                    break;
                default:
                    continue;
            }
        }

        await AddHoyolabAccountSlashCommand.ExecuteCoreAsync(
            serviceProvider, context, hoyolabUserId, token
        );
    }
}
