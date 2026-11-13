using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Models;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Color = System.Drawing.Color;

namespace GitcgNetCord.MainApp.Commands.Interactions;

public static class RemoveHoyolabAccountComponentInteraction
{
    public const string CustomId = "remove-hoyolab-account";
    public const string SelectMenuCustomId = "select-remove-hoyolab-account";
    public const string FallbackActiveSelectCustomId = "select-fallback-active-account";
    public const string ConfirmCustomId = "confirm-remove-hoyolab-account";
    public const string CancelCustomId = "cancel-remove-hoyolab-account";

    public static string GetConfirmCustomId(string uid) => $"{ConfirmCustomId}:{uid}";

    public static async Task ExecuteAsync(
        HoyolabAccountService hoyolabAccountService,
        ButtonInteractionContext context
    )
    {
        var accounts = (await hoyolabAccountService.GetHoyolabAccountsAsync(context.User.Id)).ToList();

        if (accounts.Count == 0)
        {
            await context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent("You have no Hoyolab accounts to remove.")
                    .WithFlags(MessageFlags.Ephemeral)
                )
            );
            return;
        }

        var options = accounts.Select(acc => new StringMenuSelectOptionProperties(
            label: $"{acc.GameRoleId} ({acc.Region})",
            value: acc.GameRoleId
        )).ToList();

        await context.Interaction.SendResponseAsync(
            InteractionCallback.Message(new InteractionMessageProperties()
                .WithContent("Select the Hoyolab account you want to remove:")
                .WithFlags(MessageFlags.Ephemeral)
                .AddComponents(
                    new StringMenuProperties(SelectMenuCustomId)
                        .AddOptions(options)
                        .WithPlaceholder("Select account to remove")
                )
            )
        );
    }

    public static async Task SelectAsync(
        HoyolabAccountService hoyolabAccountService,
        StringMenuInteractionContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredModifyMessage
        );

        var selectedUid = context.SelectedValues[0];
        var accounts = await hoyolabAccountService.GetHoyolabAccountsAsync(context.User.Id);
        var account = accounts.FirstOrDefault(acc => acc.GameRoleId == selectedUid);

        if (account == null)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "Account not found or does not belong to you.";
                m.Components = [];
                m.Embeds = [];
            });
            return;
        }

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Components = [];
            m.Embeds = [];
            PopulateConfirmationPrompt(account.GameRoleId, account.Region, m);
        });
    }

    public static async Task ConfirmRemovalAsync(
        HoyolabAccountService hoyolabAccountService,
        ButtonInteractionContext context,
        string uid
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredModifyMessage
        );

        var result = await hoyolabAccountService.RemoveHoyolabAccountAsync(context.User.Id, uid);

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Components = [];
            m.Embeds = [];
            PopulateRemovalResponse(result, m);
        });
    }

    public static async Task CancelRemovalAsync(
        ButtonInteractionContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredModifyMessage
        );

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = "Account removal cancelled.";
            m.Components = [];
            m.Embeds = [];
        });
    }

    public static void PopulateConfirmationPrompt(string uid, string region, MessageOptions message)
    {
        var embed = new EmbedProperties()
            .WithTitle("Confirm Account Removal")
            .WithColor(Color.Orange.ToNetCordColor())
            .WithDescription($"Are you sure you want to remove Hoyolab account **{uid}** ({region})?\n\n" +
                             "⚠️ **Warning**: This action will unlink your account and stop automated daily snapshot captures and report deliveries. Historical card statistics snapshots are preserved.")
            .AddFields(
                new EmbedFieldProperties().WithName("UID").WithValue(uid).WithInline(),
                new EmbedFieldProperties().WithName("Region").WithValue(region).WithInline()
            );

        message.Content = null;
        message.AddEmbeds(embed);
        message.AddComponents(
            new ActionRowProperties
            {
                new ButtonProperties(
                    customId: GetConfirmCustomId(uid),
                    label: "Confirm Remove",
                    style: ButtonStyle.Danger
                ),
                new ButtonProperties(
                    customId: CancelCustomId,
                    label: "Cancel",
                    style: ButtonStyle.Secondary
                )
            }
        );
    }

    public static async Task SelectFallbackActiveAsync(
        ActiveHoyolabAccountService activeHoyolabAccountService,
        StringMenuInteractionContext context
    )
    {
        var newActiveUid = context.SelectedValues[0];

        try
        {
            await activeHoyolabAccountService.UpdateActiveHoyolabAccountAsync(
                discordUserId: context.User.Id,
                gameRoleId: newActiveUid
            );

            await context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent($"Active Hoyolab account has been updated to **{newActiveUid}**.")
                    .WithFlags(MessageFlags.Ephemeral)
                )
            );
        }
        catch (Exception ex)
        {
            await context.Interaction.SendResponseAsync(
                InteractionCallback.Message(new InteractionMessageProperties()
                    .WithContent($"Failed to update active account: {ex.Message}")
                    .WithFlags(MessageFlags.Ephemeral)
                )
            );
        }
    }

    public static void PopulateRemovalResponse(RemoveHoyolabAccountResult result, MessageOptions message)
    {
        if (!result.Success)
        {
            message.Content = result.Message;
            return;
        }

        var removed = result.RemovedAccount!;
        var embed = new EmbedProperties()
            .WithTitle("Hoyolab Account Removed")
            .WithColor(Color.Orange.ToNetCordColor())
            .AddFields(
                new EmbedFieldProperties().WithName("UID").WithValue(removed.GameRoleId).WithInline(),
                new EmbedFieldProperties().WithName("Region").WithValue(removed.Region).WithInline(),
                new EmbedFieldProperties()
                    .WithName("Status")
                    .WithValue("Account unlinked. Daily snapshot captures and report deliveries stopped. Historical statistics snapshots are preserved.")
            );

        string activeStatusNote;
        if (!result.WasActive)
        {
            activeStatusNote = "The removed account was not active. Your active account remains unchanged.";
        }
        else
        {
            if (result.RemainingAccounts.Count == 0)
            {
                activeStatusNote = "This was your active account. You currently have no linked Hoyolab accounts.";
            }
            else if (result.RemainingAccounts.Count == 1)
            {
                var newActive = result.NewActiveAccount!;
                activeStatusNote = $"This was your active account. Active account has been automatically switched to **{newActive.GameRoleId}** ({newActive.Region}).";
            }
            else
            {
                var newActive = result.NewActiveAccount!;
                activeStatusNote = $"This was your active account. Active account has been automatically switched to **{newActive.GameRoleId}** ({newActive.Region}). You can select a different active account below:";

                var fallbackOptions = result.RemainingAccounts.Select(acc => new StringMenuSelectOptionProperties(
                    label: $"{acc.GameRoleId} ({acc.Region})",
                    value: acc.GameRoleId
                )).ToList();

                message.AddComponents(
                    new StringMenuProperties(FallbackActiveSelectCustomId)
                        .AddOptions(fallbackOptions)
                        .WithPlaceholder("Switch active account")
                );
            }
        }

        embed.AddFields(new EmbedFieldProperties().WithName("Active Account").WithValue(activeStatusNote));
        message.AddEmbeds(embed);
    }
}
