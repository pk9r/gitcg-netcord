using GitcgNetCord.MainApp.Entities.Repositories;
using HoyolabHttpClient;
using HoyolabHttpClient.Responses.GameRecordCard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Npgsql;

namespace GitcgNetCord.MainApp.Commands.Interactions;

public static class SelectGameRecordComponentInteraction
{
    public const string CustomId = "select-game-record";

    public static async Task ExecuteAsync(
        HoyolabAccountService hoyolabAccountService,
        HybridCache cache,
        StringMenuInteractionContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral)
        );

        var gameRoleId = context.SelectedValues[0];

        var hoyolabAuthorize = await cache
            .GetOrCreateAsync<HoyolabAuthorize>(
                key: $"hoyolab-authorize:{context.User.Id}",
                factory: _ => throw new InvalidOperationException(
                    message: "Took too long to authorize Hoyolab account. Please try again."
                )
            );

        var gameRecordList = await cache
            .GetOrCreateAsync<IReadOnlyCollection<GameRecord>>(
                key: $"game-record-list:{context.User.Id}",
                factory: _ => throw new InvalidOperationException(
                    message: "Took too long to fetch game records. Please try again."
                )
            );

        var gameRecord = gameRecordList
            .First(x => x.GameRoleId == gameRoleId);

        try
        {
            await hoyolabAccountService.AddHoyolabAccountAsync(
                discordUserId: context.User.Id,
                hoyolabUserId: hoyolabAuthorize.HoyolabUserId,
                token: hoyolabAuthorize.Token,
                gameRoleId: gameRecord.GameRoleId,
                region: gameRecord.Region
            );
        }
        catch (DbUpdateException e) when (
            e.InnerException is PostgresException { SqlState: "23505" } //Unique violation
        )
        {
            await context.Interaction.ModifyResponseAsync(message =>
            {
                message.Content = $"You already have a Hoyolab account added.";
            });
            return;
        }
        catch
        {
            await context.Interaction.ModifyResponseAsync(message =>
            {
                message.Content = $"Failed to add Hoyolab account.";
            });
            return;
        }

        await context.Interaction.SendFollowupMessageAsync(
            message: "Your Hoyolab account has been added successfully."
        );
    }
}
