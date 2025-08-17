using System;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Commands.Interactions;

public static class SelectDecksComponentInteraction
{
    public const string CustomId = "select-decks";

    public static async Task ExecuteAsync(
        StringMenuInteractionContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage()
        );

    }
}
