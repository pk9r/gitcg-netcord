using System.Collections.Immutable;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using Color = System.Drawing.Color;
using DeckImageCreatorCollection = GitcgNetCord.MainApp.Models.DeckImageCreatorCollection;

namespace GitcgNetCord.MainApp.Commands.Interactions;

public static class SelectDecksComponentInteraction
{
    public const string CustomId = "select-decks";

    public static async Task ExecuteAsync(
        IServiceProvider serviceProvider,
        StringMenuInteractionContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage()
        );
        
        var decoder = serviceProvider
            .GetRequiredService<HoyolabDecoder>();
        var deckImageCreatorCollection = serviceProvider
            .GetRequiredService<DeckImageCreatorCollection>();

        var deckImageCreator = deckImageCreatorCollection.GameBackground;

        var appEmojis = await context.Client.Rest
            .GetApplicationEmojisAsync(context.Client.Id);
        var emojis = appEmojis.ToImmutableDictionary(x => x.Name);

        var decks = await Task.WhenAll(
            context.SelectedValues.Select(async (s, i) =>
            {
                var result = await decoder.DecodeAsync(s);

                var deckEmoji = result.Deck.RoleCards
                    .Select(x => emojis[x.Basic.ItemId.ToString()]);

                var image = await deckImageCreator.CreateImageAsync(result.Deck);

                return new
                {
                    RoleEmojis = string.Join(" ", deckEmoji),
                    RoleNames = string.Join(
                        separator: ", ",
                        values: result.Deck.RoleCards.Select(x => x.Basic.Name)
                    ),
                    SharingCode = s,
                    FileName = $"deck{i + 1}.png",
                    Image = image
                };
            })
        );

        await context.Interaction.ModifyResponseAsync(message => message
            .WithFlags(MessageFlags.IsComponentsV2)
            .AddAttachments(
                decks.Select(x => new AttachmentProperties(x.FileName, x.Image))
            )
            .AddComponents(
                decks.Select(s => new ComponentContainerProperties()
                    .WithAccentColor(new NetCord.Color(Color.Purple.ToArgb()))
                    .AddComponents(
                        new TextDisplayProperties(
                            $"""
                             ## {s.RoleEmojis} - {s.RoleNames}
                             """
                        ),
                        new MediaGalleryProperties().AddItems(
                            new MediaGalleryItemProperties(
                                new ComponentMediaProperties($"attachment://{s.FileName}"))
                        ),
                        new TextDisplayProperties(
                            $"`{s.SharingCode}`"
                        ),
                        new ActionRowProperties().AddButtons(
                            CopySharingCodeComponentInteraction
                                .CreateCopySharingCodeButton(s.SharingCode)
                        )
                    )
                )
            )
        );

        foreach (var deck in decks) 
            await deck.Image.DisposeAsync();
    }
}