using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Enums;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using GitcgNetCord.MainApp.Models;
using HoyolabHttpClient;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using System.Collections.Immutable;
using Color = System.Drawing.Color;
using IDeckImageCreationService = GitcgSharp.Shared.ImageCreators.Deck.Abstractions.IDeckImageCreationService;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class DeckSlashCommand
{
    public static async Task ExecuteAsync(
        HoyolabDecoder decoder,
        DeckImageCreatorCollection deckImageCreatorCollection,
        IServiceProvider serviceProvider,
        ApplicationCommandContext context,
        [
            SlashCommandParameter(
                Description = "Deck sharing code. The sharing code can get from the game/hoyolab.",
                AutocompleteProviderType = typeof(SharingCodeAutocompleteProvider)
            )
        ]
        string sharingCode,
        [
            SlashCommandParameter(
                Description = "Decode deck code type. Default: `ImageGameBackground`."
            )
        ]
        SharingDecodeType type = SharingDecodeType.ImageGameBackground,
        [
            SlashCommandParameter(
                Description = "Validation rule. Default: `Playable`."
            )
        ]
        SharingCodeValidationRuleType validationRule = SharingCodeValidationRuleType.Playable,
        [
            SlashCommandParameter(
                Description = "Language. Default: `en-us`.",
                AutocompleteProviderType = typeof(LanguageAutocompleteProvider)
            )
        ]
        string lang = "en-us"
    )
    {
        switch (sharingCode)
        {
            case "hoyolab-accounts":
                await HoyolabAccountsSlashCommand
                    .ExecuteAsync(serviceProvider, context);
                return;
            case "account-decks":
                await ExecuteAccountDecksAsync(serviceProvider, context);
                return;
        }

        await context.Interaction.SendResponseAsync(
            callback: InteractionCallback.DeferredMessage()
        );

        var decodeResult = await decoder.DecodeAsync(
            sharingCode: sharingCode,
            validationRule: validationRule,
            lang: lang
        );

        if (decodeResult.Validate.Failed)
        {
            var failures = string.Join(
                separator: '\n',
                values: decodeResult.Validate.Failures
            );

            await context.Interaction.ModifyResponseAsync(message => message
                .AddEmbeds(new EmbedProperties()
                    .WithTitle("Invalid sharing code")
                    .WithDescription(failures)
                    .WithColor(Color.Red.ToNetCordColor())
                )
            );
            return;
        }

        var deck = decodeResult.Deck;

        var appEmojis = await context.Client.Rest
            .GetApplicationEmojisAsync(context.Client.Id);
        var emojis = appEmojis.ToImmutableDictionary(x => x.Name);

        var deckEmojis = deck.RoleCards.Select(
            selector: x => emojis[x.Basic.ItemId.ToString()]
        );
        var deckEmojisString = string.Join(
            separator: " ",
            values: deckEmojis
        );
        var roleCardsString = string.Join(
            separator: ", ",
            values: deck.RoleCards.Select(x => x.Basic.Name)
        );

        if (type.IsCreateImageType())
        {
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            IDeckImageCreationService deckImageCreator = type switch
            {
                SharingDecodeType.ImageGameBackground
                    => deckImageCreatorCollection.GameBackground,
                SharingDecodeType.ImageSimplest
                    => deckImageCreatorCollection.Simplest,
                SharingDecodeType.ImageGenshincards
                    => deckImageCreatorCollection.Genshincards,
                // This case should never be hit because of the IsCreateImageType check above.
                _ => throw new NotImplementedException()
            };

            await using var stream = await deckImageCreator.CreateImageAsync(deck);

            const string deckImageFileName = "deck.png";
            const string deckImageUrl = $"attachment://{deckImageFileName}";

            // ReSharper disable once RawStringCanBeSimplified
            await context.Interaction.ModifyResponseAsync(message => message
                .WithFlags(MessageFlags.IsComponentsV2)
                .AddAttachments(new AttachmentProperties(
                    fileName: deckImageFileName,
                    stream: stream
                ))
                .AddComponents(
                    [
                        new ComponentContainerProperties()
                            .WithAccentColor(Color.Purple.ToNetCordColor())
                            .AddComponents(
                                new TextDisplayProperties(
                                    $"""
                                     ## {deckEmojisString} - {roleCardsString}
                                     """
                                ),
                                new MediaGalleryProperties().AddItems(
                                    new MediaGalleryItemProperties(
                                        new ComponentMediaProperties(deckImageUrl))
                                ),
                                new TextDisplayProperties(
                                    $"`{sharingCode}`"
                                ),
                                new ActionRowProperties([
                                    CopySharingCodeComponentInteraction
                                        .CreateCopySharingCodeButton(sharingCode)
                                ])
                            )
                    ]
                )
            );

            return;
        }

        if (type == SharingDecodeType.Text)
        {
            throw new NotImplementedException();
        }
    }

    private static async Task ExecuteAccountDecksAsync(
        IServiceProvider serviceProvider,
        ApplicationCommandContext context
    )
    {
        var hoyolab = serviceProvider
            .GetRequiredService<HoyolabHttpClientService>();
        var deckAccountService = serviceProvider
            .GetRequiredService<HoyolabDeckAccountService>();
        var activeHoyolabAccountService = serviceProvider
            .GetRequiredService<ActiveHoyolabAccountService>();

        await context.Interaction.SendResponseAsync(
            callback: InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        var hoyolabAccount = await activeHoyolabAccountService
            .GetActiveHoyolabAccountAsync(
                discordUserId: context.User.Id
            );

        if (hoyolabAccount == null)
            return;

        var authorize = new HoyolabAuthorize(
            HoyolabUserId: hoyolabAccount.HoyolabUserId,
            Token: hoyolabAccount.Token
        );

        var deckListResult = await deckAccountService
            .GetDeckListAsync(
                authorize: authorize,
                hoyolabAccount.GameRoleId,
                hoyolabAccount.Region
            );

        var appEmojis = await context.Client.Rest
            .GetApplicationEmojisAsync(context.Client.Id);
        var emojis = appEmojis.ToImmutableDictionary(x => x.Name);

        await context.Interaction.ModifyResponseAsync(message => message
            .AddComponents(
                new StringMenuProperties(
                        customId: SelectDecksComponentInteraction.CustomId
                    ).AddOptions(
                        deckListResult.DeckList.Select((deck, index) =>
                        {
                            var rolesDisplay = string.Join(
                                separator: ", ",
                                deck.AvatarCards.Select(y => y.Name)
                            );

                            var emoji = EmojiProperties.Custom(
                                emojis[deck.AvatarCards.First().Id.ToString()].Id);

                            return new StringMenuSelectOptionProperties(
                                    label: LimitLength($"{index + 1}. {deck.Name}"),
                                    value: deck.ShareCode
                                )
                                .WithDescription(LimitLength(rolesDisplay))
                                .WithEmoji(emoji);
                        })
                    )
                    .WithMaxValues(Math.Min(deckListResult.DeckList.Count, 5))
            )
        );
    }

    private static string LimitLength(ReadOnlySpan<char> name)
    {
        return name.Length <= 100 ? name.ToString() : $"{name[..97]}...";
    }
}