using System.Collections.Concurrent;
using System.Collections.Immutable;
using GitcgNetCord.MainApp.Commands.Preconditions;
using GitcgPainter;
using GitcgPainter.Extensions;
using HoyolabHttpClient;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using SixLabors.ImageSharp;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class UpdateRoleEmojisSlashCommand
{
    private static ImmutableDictionary<string, ApplicationEmoji> _emojis = null!;

    [RequireDevRole<ApplicationCommandContext>]
    public static async Task ExecuteAsync(
        HoyolabHttpClientService hoyolab,
        ImageCacheService imageCacheService,
        ApplicationCommandContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            callback: InteractionCallback.DeferredMessage()
        );

        var applicationId = context.Client.Id;

        var appEmojis = await context.Client.Rest
            .GetApplicationEmojisAsync(applicationId);
        _emojis = appEmojis.ToImmutableDictionary(x => x.Name);

        var response = await hoyolab.GetCardRolesAsync();

        ConcurrentBag<ApplicationEmoji> newEmotes = [];

        await Task.WhenAll(
            response.Roles.Select(CreateApplicationEmoteAsync)
        );

        var newEmotesString = string.Join(" ", newEmotes);

        await context.Interaction.ModifyResponseAsync(message =>
        {
            message.Content =
                $"""
                 Emojis updated!
                 {newEmotesString}
                 """;
        });

        return;

        async Task CreateApplicationEmoteAsync(
            HoyolabHttpClient.Models.Role role
        )
        {
            var emojiName = role.Basic.ItemId.ToString();

            // Skip if the emote already exists
            if (_emojis.ContainsKey(emojiName))
                return;

            using var iconSmall = await imageCacheService
                .LoadIconSmallAsync(role: role);
            using var memoryStream = new MemoryStream();
            await iconSmall.SaveAsPngAsync(memoryStream);

            var emoji = await context.Client.Rest.CreateApplicationEmojiAsync(
                applicationId: applicationId,
                new ApplicationEmojiProperties(
                    name: emojiName,
                    image: new ImageProperties()
                        .WithData(memoryStream.GetBuffer())
                )
            );

            newEmotes.Add(emoji);
        }
    }
}
