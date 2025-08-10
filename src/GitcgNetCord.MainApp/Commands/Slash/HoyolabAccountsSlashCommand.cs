using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Entities.Repositories;
using HoyolabHttpClient;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Color = System.Drawing.Color;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class HoyolabAccountsSlashCommand
{
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
                    .WithColor(new NetCord.Color(Color.Purple.ToArgb()))
                )
                .AddComponents(new ActionRowProperties()
                    .AddButtons(
                        new ButtonProperties(
                            customId: AddHoyolabAccountComponentInteraction.CustomId,
                            label: "Add account",
                            style: ButtonStyle.Success),
                        new ButtonProperties(
                            customId: UpdateActiveHoyolabAccountComponentInteraction.CustomId,
                            label: "Update active account",
                            style: ButtonStyle.Secondary
                        )
                        // new ButtonProperties(
                        //     customId: "remove-hoyolab-account",
                        //     label: "Remove account",
                        //     style: ButtonStyle.Danger)
                    ));
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
