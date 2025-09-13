using GitcgNetCord.MainApp.Commands.Preconditions;
using GitcgSkia;
using GitcgSkia.Extensions;
using HoyolabHttpClient;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using SixLabors.ImageSharp;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class UpdateEmojisSlashCommand
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

        await Task.WhenAll([
            ..response.Roles.Select(CreateApplicationEmoteByRoleAsync),
            ..HoyolabSharedUtils.UrlToEmojis.Select(
                x => CreateApplicationEmoteByUrlAsync(
                    emojiName: x.Value, url: x.Key
                )
            )
        ]);

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

        async Task CreateApplicationEmoteByRoleAsync(
            HoyolabHttpClient.Models.Role role
        )
        {
            var emojiName = role.Basic.ItemId.ToString();
            var url = role.Basic.IconSmall;

            await CreateApplicationEmoteByUrlAsync(emojiName, url);
        }

        async Task CreateApplicationEmoteByUrlAsync(
            string emojiName, string url
        )
        {
            // Skip if the emote already exists
            if (_emojis.ContainsKey(emojiName))
                return;

            if (url == "https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/")
            {
                url += "item_icon/67c7f719/8b295c3fed21771dcced6055cdf2f2ce.png";
            }

            using var icon = await imageCacheService.LoadHttpImageAsync(url);

            await EncodeThenUploadAsync(emojiName, icon);
        }

        async Task EncodeThenUploadAsync(
            string emojiName, SKBitmap icon
        )
        {
            using var encoded = icon.Encode(
                format: SKEncodedImageFormat.Webp,
                quality: 100
            );

            var emoji = await context.Client.Rest.CreateApplicationEmojiAsync(
                applicationId: applicationId,
                new ApplicationEmojiProperties(
                    name: emojiName,
                    image: new ImageProperties()
                        .WithData(encoded.ToArray())
                )
            );

            newEmotes.Add(emoji);
        }
    }
}
