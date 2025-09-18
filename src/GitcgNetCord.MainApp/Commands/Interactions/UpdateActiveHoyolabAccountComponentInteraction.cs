using GitcgNetCord.MainApp.Entities.Repositories;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Commands.Interactions;

public static class UpdateActiveHoyolabAccountComponentInteraction
{
    public const string CustomId = "update-active-hoyolab-account";
    public const string PostCustomId = "post-active-hoyolab-account";

    public static async Task ExecuteAsync(
        ButtonInteractionContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.Modal(new ModalProperties(
                customId: PostCustomId,
                title: "Update active hoyolab account"
            )
            {
                new LabelProperties(
                    label: "Genshin Impact UID",
                    component: new TextInputProperties(
                        customId: "uid",
                        style: TextInputStyle.Short)
                    {
                        MaxLength = 10
                    }
                )
            })
        );
    }

    public static async Task PostAsync(
        ActiveHoyolabAccountService activeHoyolabAccountService,
        ModalInteractionContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral)
        );

        var uid = "";

        var textInputs = context.Components.OfType<TextInput>();

        foreach (var textInput in textInputs)
        {
            switch (textInput.CustomId)
            {
                case "uid":
                    uid = textInput.Value;
                    break;
                default:
                    continue;
            }
        }

        try
        {
            await activeHoyolabAccountService
                .UpdateActiveHoyolabAccountAsync(
                    discordUserId: context.User.Id,
                    gameRoleId: uid
                );
        }
        catch (Exception e)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"{e.Message}";
            });
            return;
        }

        await context.Interaction.ModifyResponseAsync(message =>
        {
            message.Content = $"Updated active account: {uid}";
        });
    }
}
