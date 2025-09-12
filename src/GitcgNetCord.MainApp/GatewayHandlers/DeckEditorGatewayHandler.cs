using GitcgNetCord.MainApp.Models;
using GitcgNetCord.MainApp.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace GitcgNetCord.MainApp.GatewayHandlers;

public class DeckEditorGatewayHandler(
    IServiceScopeFactory serviceScopeFactory,
    GitcgGatewayHandlerContext gitcgGatewayHandlerContext,
    GatewayClient client
) : IMessageCreateGatewayHandler
{
    public async ValueTask HandleAsync(Message message)
    {
        if (message.Author.IsBot)
            return;

        if (message.Channel == null)
            return;

        if (!IsDeckEditorChannel(message.ChannelId))
            return;

        gitcgGatewayHandlerContext.Message = message;

        await using var scope = serviceScopeFactory.CreateAsyncScope();

        KernelPluginCollection plugins =
        [
            KernelPluginFactory.CreateFromObject(
                scope.ServiceProvider.GetRequiredService<DeckEditorPlugin>()
            )
        ];

        var kernel = new Kernel(scope.ServiceProvider, plugins);
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        OpenAIPromptExecutionSettings executionSettings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(
            """
            You are an assistant that helps to edit decks. Please assist me in these tasks.
            - Importing a deck using a sharing code.
            - Sending the current deck image.
            - Updating the deck by adding or removing cards.

            Instructions:
            - Sharing code length is fixed at 68 characters.
            - After importing the deck successfully using the sharing code, you can send the deck image to the player.
            - When receiving a request to update the deck, if you don't have the list of cards yet, get the list of cards first.
            - After updating the deck, send the new deck image to the player.
            """
        );
        // chatHistory.AddSystemMessage(
        //     """
        //     Additional instructions:
        //     - Use Vietnamese to communicate.
        //     """
        // );
        chatHistory.AddUserMessage(message.Content);

        var reaction = new ReactionEmojiProperties("⏳");
        await message.AddReactionAsync(reaction);

        var result = await chatCompletionService
            .GetChatMessageContentAsync(
                chatHistory: chatHistory,
                executionSettings: executionSettings,
                kernel: kernel
            );

        await message.DeleteCurrentUserReactionAsync(reaction);

        await message.Channel.SendMessageAsync(
            new MessageProperties()
                .WithContent(result.Content)
        );
    }

    private static bool IsDeckEditorChannel(ulong channelId)
        => channelId == 1410989256092156065; // #deck-editor
}