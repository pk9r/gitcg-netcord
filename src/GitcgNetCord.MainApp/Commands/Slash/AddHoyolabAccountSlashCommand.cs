using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using HoyolabHttpClient;
using HoyolabHttpClient.Responses.GameRecordCard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;
using Npgsql;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class AddHoyolabAccountSlashCommand
{
    public static async Task ExecuteAsync(
        IServiceProvider serviceProvider,
        ApplicationCommandContext context,
        [
            SlashCommandParameter(
                Description = "Your Hoyolab user ID. You can find it in the Hoyolab app or website."
            )
        ]
        string hoyolabUserId,
        [
            SlashCommandParameter(
                Description = "Your Hoyolab token."
            )
        ]
        string token
    )
    {
        await ExecuteCoreAsync(
            serviceProvider: serviceProvider, 
            context: context, 
            hoyolabUserId: hoyolabUserId,
            token: token
        );
    }

    public static async Task ExecuteCoreAsync<TContext>(
        // manual inject services
        // useful for another module calling this command
        IServiceProvider serviceProvider, TContext context,
        string hoyolabUserId, string token
    ) where TContext : IInteractionContext, IUserContext
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral)
        );
        
        var hoyolab = serviceProvider
            .GetRequiredService<HoyolabHttpClientService>();
        var cache = serviceProvider
            .GetRequiredService<HybridCache>();
        var hoyolabAccountService = serviceProvider
            .GetRequiredService<HoyolabAccountService>();

        var authorize = new HoyolabAuthorize(
            HoyolabUserId: hoyolabUserId, Token: token
        );

        Data gameRecordCard;
        try
        {
            gameRecordCard = await hoyolab
                .GetGameRecordCardAsync(authorize: authorize);
        }
        catch (Exception e)
        {
            await context.Interaction.ModifyResponseAsync(message =>
            {
                message.Content = e.Message;
            });
            return;
        }

        var genshinRecords = gameRecordCard.List
            .Where(x => x.GameId == 2) // Filter for Genshin Impact
            .DistinctBy(x => x.GameRoleId)
            .ToList();

        if (genshinRecords.Count == 0)
        {
            await context.Interaction.ModifyResponseAsync(message =>
            {
                message.Content = "No Genshin Impact characters found on this Hoyolab account. Please ensure you have created a Genshin Impact character and linked it to this Hoyolab account.";
            });
            return;
        }

        if (genshinRecords.Count == 1)
        {
            var singleRecord = genshinRecords[0];
            HoyolabAccount addedAccount;
            try
            {
                addedAccount = await hoyolabAccountService.AddHoyolabAccountAsync(
                    discordUserId: context.User.Id,
                    hoyolabUserId: authorize.HoyolabUserId,
                    token: authorize.Token,
                    gameRoleId: singleRecord.GameRoleId,
                    region: singleRecord.Region
                );
            }
            catch (DbUpdateException e) when (
                e.InnerException is PostgresException { SqlState: "23505" } // Unique violation
            )
            {
                await context.Interaction.ModifyResponseAsync(message =>
                {
                    message.Content = "You already have a Hoyolab account added.";
                });
                return;
            }
            catch
            {
                await context.Interaction.ModifyResponseAsync(message =>
                {
                    message.Content = "Failed to add Hoyolab account.";
                });
                return;
            }

            var (embed, components) = HoyolabAccountResponseHelper.CreateAccountAddedResponse(
                gameRecord: singleRecord,
                addedAccountId: addedAccount.Id
            );

            await context.Interaction.ModifyResponseAsync(message =>
            {
                message.Content = null;
                message.AddEmbeds(embed);
                message.AddComponents(components);
            });
            return;
        }

        // Multiple characters (Count > 1)
        var userAccounts = await hoyolabAccountService.GetHoyolabAccountsAsync(context.User.Id);
        var existingUids = userAccounts.Select(x => x.GameRoleId).ToHashSet();

        await cache.SetAsync(
            key: $"game-record-list:{context.User.Id}",
            value: gameRecordCard.List
        );
        await cache.SetAsync(
            key: $"hoyolab-authorize:{context.User.Id}",
            value: authorize
        );

        await context.Interaction.ModifyResponseAsync(message =>
        {
            message.WithComponents([
                new StringMenuProperties(
                    customId: SelectGameRecordComponentInteraction.CustomId
                ).AddOptions(genshinRecords
                    .Select(x =>
                    {
                        var isLinked = existingUids.Contains(x.GameRoleId);
                        var description = isLinked
                            ? $"Lv. {x.Level} {x.RegionName} ({x.GameRoleId}) [Already Linked - Select to update token]"
                            : $"Lv. {x.Level} {x.RegionName} ({x.GameRoleId})";

                        return new StringMenuSelectOptionProperties(
                            label: x.Nickname,
                            value: x.GameRoleId
                        )
                        .WithDescription(description);
                    })
                )
            ]);
        });
    }
}
