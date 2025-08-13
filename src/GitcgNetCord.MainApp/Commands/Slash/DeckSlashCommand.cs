using System.Collections.Immutable;
using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Enums;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using GitcgPainter.ImageCreators.Deck;
using GitcgPainter.ImageCreators.Deck.Abstractions;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Color = System.Drawing.Color;

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
        if (sharingCode == "hoyolab-accounts")
        {
            await HoyolabAccountsSlashCommand
                .ExecuteAsync(serviceProvider, context);

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
            await context.Interaction.ModifyResponseAsync(message => message
                .AddEmbeds(new EmbedProperties()
                    .WithTitle("Invalid sharing code")
                    .WithDescription(string.Join('\n', decodeResult.Validate.Failures))
                    .WithColor(new NetCord.Color(Color.Red.ToArgb()))
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

        var label = $"{deckEmojisString} - {roleCardsString}";

        if (type.IsCreateImageType())
        {
            IDeckImageCreationService deckImageCreator = type switch
            {
                SharingDecodeType.ImageGameBackground
                    => deckImageCreatorCollection.GameBackground,
                SharingDecodeType.ImageSimplest
                    => deckImageCreatorCollection.Simplest,
                SharingDecodeType.ImageGenshincards
                    => deckImageCreatorCollection.Genshincards,
                _ => throw new NotImplementedException()
            };

            var pngBytes = await deckImageCreator.CreateImageAsync(deck);

            const string deckImageFileName = "deck.png";
            const string deckImageUrl = $"attachment://{deckImageFileName}";

            using var memoryStream = new MemoryStream(pngBytes);

            await context.Interaction.ModifyResponseAsync(message => message
                .AddAttachments(
                    new AttachmentProperties(
                        fileName: deckImageFileName,
                        stream: memoryStream)
                )
                .AddEmbeds(new EmbedProperties()
                    .WithTitle(label)
                    .AddFields(
                        new EmbedFieldProperties()
                            .WithName("Sharing code")
                            .WithValue(sharingCode)
                    )
                    .WithImage(new EmbedImageProperties(deckImageUrl))
                    .WithColor(new NetCord.Color(Color.Purple.ToArgb()))
                )
                .AddComponents(new ActionRowProperties().AddButtons(
                    CopySharingCodeComponentInteraction
                        .CreateCopySharingCodeButton(sharingCode)
                ))
            );

            return;
        }

        if (type == SharingDecodeType.Text)
        {
            throw new NotImplementedException();
        }
    }
}
