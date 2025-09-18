using GitcgNetCord.MainApp.Plugins.DuelAssistant;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace GitcgNetCord.MainApp.GatewayHandlers;

public class DuelAssistantGatewayHandler(
    IServiceScopeFactory serviceScopeFactory,
    GatewayClient client
) : IMessageCreateGatewayHandler
{
    public async ValueTask HandleAsync(Message message)
    {
        if (message.Author.IsBot)
            return;

        if (message.Channel == null)
            return;

        if (!IsDuelAssistantChannel(message.ChannelId))
            return;

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        
        KernelPluginCollection plugins =
        [
            KernelPluginFactory.CreateFromObject(
                scope.ServiceProvider.GetRequiredService<DuelUpdateDeckPlugin>()
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
            Bạn là trợ lý hỗ trợ điều hành trận đấu. Hãy giúp tôi trong các nhiệm này.
            - Xác nhận mã chia sẻ mà người chơi cung cấp. 
            
            Notes:
            - Mã chia sẻ sẽ có độ dài cố định là 68 ký tự.
            - Hãy tự chọn index dựa trên số lượng bộ bài hiện tại.
            - Trong phản hồi dùng "bộ bài thứ index + 1" thay vì "index".
            - Sử dụng ngôn ngữ tiếng Việt để giao tiếp.
            """
        );
        chatHistory.AddSystemMessage(
            """
            requirement:
              number_of_decks: 3
            current_state:
              decks: ["FRHAWhcPAkBh9GMPFlAw9psPFGGBaEgZGGERlrcfDIFgCsYWDJGQackdDUEA3NEdDcAA"]
            """
        );
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

    private static bool IsDuelAssistantChannel(ulong channelId)
        => channelId == 1406496982222241862; // #duel-assistant
}