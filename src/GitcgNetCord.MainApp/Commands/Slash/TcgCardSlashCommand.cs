using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using HoyolabHttpClient;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Collections.Immutable;
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
            return GetDesc(
                $"{data.Cost1Raw}" +
                $"<img src='{data.Cost2TypeIcon}' /> - " +
                $"{action.Basic.Name}"
            );
        }

        string GetDesc(string desc)
        {
            var result = desc;

            result = result.Replace("\\n", "\n");
            result = result.Replace(
                "<img src='https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/e25f04745615df9e779831a1c1354e38.png' />",
                emojis["food"].ToString()
            );
            result = result.Replace(
                "<img src='https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/d92127a21b942663e6ed0717cef1086e.png' />",
                emojis["location"].ToString()
            );
            result = result.Replace(
                "<img src='https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/377c4198e3072e9c68066736be5b790c.png' />",
                emojis["item"].ToString()
            );
            result = result.Replace(
                "<img src='https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/1643452964b58e2e69e64e2b5d3b5878.png' />",
                emojis["catalyst"].ToString()
            );
            result = result.Replace(
                "<img src='https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f71f/49acb071b0d634ded17cb788d9f520ed.png' />",
                emojis["weapon"].ToString()
            );
            result = result.Replace(
                "<img src='https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/e1430a8775eb864ed0617b7ef516e608.png' />",
                emojis["physical_dmg"].ToString()
            );
            result = result.Replace(
                "<img src='https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/8401f5013a7c381edb6cd088b207bb9f.png' />",
                emojis["omni"].ToString()
            );
            result = result.Replace(
                "<img src='https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/' />",
                emojis["aligned"].ToString()
            );
            result = result.Replace(
                "<img src='https://act-webstatic.hoyoverse.com/hk4e/e20200928calculate/item_icon/67c7f719/39de35b178b080a090ac95622bbbf0df.png' />",
                emojis["unaligned"].ToString()
            );

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
}
