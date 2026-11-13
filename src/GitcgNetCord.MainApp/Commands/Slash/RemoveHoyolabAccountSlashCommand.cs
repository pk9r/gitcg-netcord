using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Entities.Repositories;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class RemoveHoyolabAccountSlashCommand
{
    public static async Task ExecuteAsync(
        HoyolabAccountService hoyolabAccountService,
        ApplicationCommandContext context,
        [SlashCommandParameter(
            Description = "Genshin Impact UID of the Hoyolab account to remove.",
            AutocompleteProviderType = typeof(HoyolabAccountAutocompleteProvider)
        )]
        string account
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral)
        );

        var accounts = await hoyolabAccountService.GetHoyolabAccountsAsync(context.User.Id);
        var targetAccount = accounts.FirstOrDefault(x => x.GameRoleId == account.Trim());

        if (targetAccount == null)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "Account not found or does not belong to you.";
            });
            return;
        }

        await context.Interaction.ModifyResponseAsync(m =>
            RemoveHoyolabAccountComponentInteraction.PopulateConfirmationPrompt(
                targetAccount.GameRoleId,
                targetAccount.Region,
                m
            )
        );
    }
}
