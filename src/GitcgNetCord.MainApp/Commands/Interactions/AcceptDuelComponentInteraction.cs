using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Commands.Interactions;

public static class AcceptDuelComponentInteraction
{
    public const string CustomId = "accept-duel";

    public static async Task ExecuteAsync(
        ButtonInteractionContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage()
        );
        
        GuildThread? duelThread;
        try
        {
            duelThread = await context.Message.CreateGuildThreadAsync(
                new GuildThreadFromMessageProperties(
                    $"Duel #{Random.Shared.Next(1000, 9999)} - {context.User.Username}"
                )
            );
        }
        catch
        {
            duelThread = context.Message.StartedThread;
            if (duelThread == null) throw;
        }
        
        await duelThread.SendMessageAsync(
            $"{context.User} has accepted the duel with {context.Message.Author}."
        );

        await context.Interaction.DeleteResponseAsync();
    }
}