using GitcgNetCord.MainApp.Commands.Interactions;
using HoyolabHttpClient;
using HoyolabHttpClient.Responses.GameRecordCard;
using Microsoft.Extensions.Caching.Hybrid;
using NetCord;
using NetCord.Rest;
using NetCord.Services;
using NetCord.Services.ApplicationCommands;

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
                ).AddOptions(gameRecordCard.List
                    .Where(x => x.GameId == 2) // Filter for Genshin Impact
                    .Select(x => new StringMenuSelectOptionProperties(
                            label: x.Nickname,
                            value: x.GameRoleId
                        )
                        .WithDescription($"Lv. {x.Level} {x.RegionName} ({x.GameRoleId})")
                    )
                )
            ]);
        });
    }
}
