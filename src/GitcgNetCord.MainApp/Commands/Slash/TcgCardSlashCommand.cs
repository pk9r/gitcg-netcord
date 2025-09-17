using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using HoyolabHttpClient;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Color = System.Drawing.Color;

namespace GitcgNetCord.MainApp.Commands.Slash;

public partial class TcgCardSlashCommand
{
    public static async Task ExecuteAsync(
        HoyolabHttpClientService hoyolab,
        IServiceProvider serviceProvider,
        ApplicationCommandContext context,
        [
            SlashCommandParameter(
                Description = "Card ID.",
                AutocompleteProviderType = typeof(CardAutocompleteProvider)
            )
        ]
        int cardId,
        [
            SlashCommandParameter(
                Description = "Language. Default: `en-us`.",
                AutocompleteProviderType = typeof(LanguageAutocompleteProvider)
            )
        ]
        string lang = "en-us"
    )
    {
        var cardActionService = serviceProvider
            .GetRequiredService<HoyolabCardActionService>();

        await context.Interaction.SendResponseAsync(
            callback: InteractionCallback.DeferredMessage()
        );

        HoyolabHttpClient.Responses.ActionSkill.Data data;

        try
        {
            data = await hoyolab.GetActionSkillAsync(cardId, lang);
        }
        catch (Exception e)
        {
            await context.Interaction.ModifyResponseAsync(message =>
            {
                //message.WithFlags(MessageFlags.IsComponentsV2);
                message.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription(e.Message)
                    .WithColor(Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        var appEmojis = await context.Client.Rest
            .GetApplicationEmojisAsync(context.Client.Id);
        var emojis = appEmojis.ToImmutableDictionary(x => x.Name);

        var cardActions = await cardActionService
            .GetCardActionsAsync(lang);
        var action = cardActions.Actions
            .First(x => x.Basic.ItemId == cardId);

        await context.Interaction.ModifyResponseAsync(message =>
        {
            message.WithFlags(MessageFlags.IsComponentsV2);
            message.AddComponents([new ComponentContainerProperties()
                .WithAccentColor(Color.Purple.ToNetCordColor())
                .AddComponents(
                    new TextDisplayProperties(
                        $"""
                        ## {GetName(cardId)}
                        """
                    ),
                    new MediaGalleryProperties().AddItems(
                        new MediaGalleryItemProperties(
                            new ComponentMediaProperties(action.Basic.Icon))
                    ),
                    new TextDisplayProperties(
                        $"{GetDesc(data.Desc)}"
                    )
                )
            ]);
        });

        return;

        string GetName(int cardId)
        {
            var builder = new StringBuilder();
            if (data.Cost2Raw > 0)
            {
                builder.Append($"<img src='{data.Cost2TypeIcon}' /> - ");
            }
            builder.Append($"{data.Cost1Raw} ");
            builder.Append($"<img src='{data.Cost1TypeIcon}' /> ");
            builder.Append($"- {action.Basic.Name} ");

            return GetDesc(builder.ToString());
        }

        string GetDesc(string desc)
        {
            var result = desc;

            result = result.Replace("\\n", "\n");

            result = ImgTagRegex().Replace(result, match =>
            {
                var url = match.Groups["url"].Value;

                ApplicationEmoji? emoji = null;

                var availableEmojis = HoyolabSharedUtils.UrlToEmojis
                    .TryGetValue(url, out var emojiKey) 
                    && emojis.TryGetValue(emojiKey, out emoji);

                if (availableEmojis) return emoji!.ToString();

                return match.Value; // leave unchanged if not found
            });

            while (ColorTagRegex().IsMatch(result))
            {
                result = ColorTagRegex().Replace(result, "**$1**");
            }

            return result;
        }
    }

    [
        GeneratedRegex(
            pattern: @"<color\b[^>]*>(.*?)</color>",
            options: RegexOptions.IgnoreCase | RegexOptions.Singleline
        )
    ]
    private static partial Regex ColorTagRegex();

    [
        GeneratedRegex(
            pattern: @"<img\s+src=['""](?<url>[^'""]+)['""][^>]*\/?>",
            options: RegexOptions.IgnoreCase, cultureName: "en-US"
        )
    ]
    private static partial Regex ImgTagRegex();
}
