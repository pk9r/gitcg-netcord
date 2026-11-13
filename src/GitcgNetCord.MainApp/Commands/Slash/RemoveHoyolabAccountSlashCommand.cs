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

        var result = await hoyolabAccountService.RemoveHoyolabAccountAsync(context.User.Id, account.Trim());

        await context.Interaction.ModifyResponseAsync(m =>
            RemoveHoyolabAccountComponentInteraction.PopulateRemovalResponse(result, m)
        );
    }
}
