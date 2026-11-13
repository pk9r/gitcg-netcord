using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Extensions;
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
        var uid = context.GetTextInputValue("uid")?.Trim();

        if (string.IsNullOrWhiteSpace(uid))
        {
            await context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new InteractionMessageProperties
                {
                    Content = "Please provide a valid Genshin Impact UID.",
                    Flags = MessageFlags.Ephemeral
                })
            );
            return;
        }

        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral)
        );

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
