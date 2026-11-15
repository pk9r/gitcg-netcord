using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Extensions;
using HoyolabHttpClient;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Color = System.Drawing.Color;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class HoyolabAccountsSlashCommand
{
    public static async Task ListAsync(
        IServiceProvider serviceProvider,
        ApplicationCommandContext context
    )
    {
        await ExecuteAsync(serviceProvider, context);
    }

    public static async Task AddAsync(
        ApplicationCommandContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.Modal(AddHoyolabAccountComponentInteraction.CreateModal())
        );
    }

    public static async Task SwitchAsync(
        ActiveHoyolabAccountService activeHoyolabAccountService,
        HoyolabAccountService hoyolabAccountService,
        ApplicationCommandContext context,
        [SlashCommandParameter(
            Description = "Genshin Impact UID of the account to set as active.",
            AutocompleteProviderType = typeof(HoyolabAccountAutocompleteProvider)
        )]
        string account
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral)
        );

        var uid = account.Trim();
        var accounts = await hoyolabAccountService.GetHoyolabAccountsAsync(context.User.Id);

        if (!accounts.Any())
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "You do not have any linked HoYoLAB accounts. Please link an account first.";
            });
            return;
        }

        var target = accounts.FirstOrDefault(x => x.GameRoleId == uid);
        if (target == null)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"HoYoLAB account with UID '{uid}' was not found or does not belong to you.";
            });
            return;
        }

        await activeHoyolabAccountService.UpdateActiveHoyolabAccountAsync(context.User.Id, uid);

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = $"Successfully switched active HoYoLAB account to: `{target.GameRoleId}` ({target.Region}).";
        });
    }

    public static async Task ExecuteAsync(
        // manual inject services
        // useful for another module calling this command
        IServiceProvider serviceProvider,
        ApplicationCommandContext context
    )
    {
        var activeHoyolabAccountService =
            serviceProvider.GetRequiredService<ActiveHoyolabAccountService>();
        var hoyolabAccountService = serviceProvider
            .GetRequiredService<HoyolabAccountService>();
        var hoyolab = serviceProvider
            .GetRequiredService<HoyolabHttpClientService>();

        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral)
        );

        var activeHoyolabAccount = await activeHoyolabAccountService
            .GetActiveHoyolabAccountAsync(
                discordUserId: context.User.Id
            );

        var accounts = await hoyolabAccountService
            .GetHoyolabAccountsAsync(context.User.Id);

        var gameRecordList = await Task.WhenAll(
            accounts.Select(async account =>
            {
                var gameRecordCard = await hoyolab.GetGameRecordCardAsync(
                    new HoyolabAuthorize(account.HoyolabUserId, account.Token)
                );

                var gameRecord = gameRecordCard.List
                    .First(x => x.GameRoleId == account.GameRoleId);

                return gameRecord;
            })
        );

        await context.Interaction.ModifyResponseAsync(message =>
        {
            message
                .AddEmbeds(new EmbedProperties()
                    .WithTitle("Accounts")
                    .AddFields(gameRecordList
                        .Select(x =>
                        {
                            var embedField = new EmbedFieldProperties()
                                .WithValue($"Lv. {x.Level} {x.RegionName} ({x.GameRoleId})");

                            embedField.WithName(
                                activeHoyolabAccount?.GameRoleId == x.GameRoleId
                                    ? $"{x.Nickname} [Active]"
                                    : $"{x.Nickname}"
                            );

                            return embedField;
                        })
                    )
                    .AddMessageIfAccountsEmpty(gameRecordList)
                    .WithColor(Color.Purple.ToNetCordColor())
                )
                .AddComponents(
                    new ActionRowProperties()
                    {
                        new ButtonProperties(
                            customId: AddHoyolabAccountComponentInteraction.CustomId,
                            label: "Add account",
                            style: ButtonStyle.Success),
                        new ButtonProperties(
                            customId: UpdateActiveHoyolabAccountComponentInteraction.CustomId,
                            label: "Update active account",
                            style: ButtonStyle.Secondary
                        ),
                        new ButtonProperties(
                            customId: RemoveHoyolabAccountComponentInteraction.CustomId,
                            label: "Remove account",
                            style: ButtonStyle.Danger)
                    }
                );
        });
    }
}

static class EmbedPropertiesExtensions
{
    public static EmbedProperties AddMessageIfAccountsEmpty<T>(
        this EmbedProperties embedProperties,
        IEnumerable<T> gameRecordList
    )
    {
        if (gameRecordList.Any())
            return embedProperties;
        

        embedProperties.WithDescription(
            "No accounts found. Please add an account."
        );

        return embedProperties;
    }
}
