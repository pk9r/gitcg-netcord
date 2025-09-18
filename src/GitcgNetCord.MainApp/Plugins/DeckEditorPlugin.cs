using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json.Serialization;
using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Models;
using HoyolabHttpClient;
using Microsoft.SemanticKernel;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using Color = System.Drawing.Color;

namespace GitcgNetCord.MainApp.Plugins;

public class DeckEditorPlugin(
    HoyolabHttpClientService hoyolab,
    GatewayClient client,
    DeckImageCreatorCollection deckImageCreatorCollection,
    DeckEditorContext deckEditorContext,
    GitcgGatewayHandlerContext gitcgGatewayHandlerContext
)
{
    [KernelFunction("import_deck")]
    [Description("Imports a deck using the provided sharing code.")]
    // ReSharper disable once UnusedMember.Global
    public async Task<ImportDeckResponse> ImportDeckAsync(string sharingCode)
    {
        var result = await hoyolab.DecodeCardCodeAsync(sharingCode);

        deckEditorContext.DeckModel.RoleCards =
            [.. result.Data.RoleCards.Select(x => x.Basic.ItemId)];
        deckEditorContext.DeckModel.ActionCards =
            [.. result.Data.ActionCards.Select(x => x.Basic.ItemId)];

        return new ImportDeckResponse
        {
            Message = "Deck imported successfully.",
            CurrentContext = deckEditorContext
        };
    }

    [KernelFunction("send_current_deck_image")]
    [Description(
        """
        Sends the current deck image to the user.
        Also includes the sharing code in the message.
        """)]
    // ReSharper disable once UnusedMember.Global
    public async Task<MessageResponse> SendCurrentDeckImageAsync()
    {
        var deckImageCreator = deckImageCreatorCollection.GameBackground;

        var encodeResult = await hoyolab.EncodeCardCodeAsync(
            roleCards: deckEditorContext.DeckModel.RoleCards,
            actionCards: deckEditorContext.DeckModel.ActionCards
        );
        var sharingCode = encodeResult.Code;

        var decodeResult = await hoyolab.DecodeCardCodeAsync(
            code: sharingCode
        );

        var deck = decodeResult.Data;

        var appEmojis = await client.Rest
            .GetApplicationEmojisAsync(client.Id);
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

        await using var image = await deckImageCreator
            .CreateImageAsync(deckData: decodeResult.Data);

        const string deckImageFileName = "deck.png";
        const string deckImageUrl = $"attachment://{deckImageFileName}";

        await gitcgGatewayHandlerContext.Message.Channel!.SendMessageAsync(
            new MessageProperties()
                .WithFlags(MessageFlags.IsComponentsV2)
                .AddAttachments(new AttachmentProperties(
                    fileName: deckImageFileName,
                    stream: image
                ))
                .AddComponents([
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

        return new MessageResponse
        {
            Message = "Deck image sent successfully.",
            CurrentContext = deckEditorContext
        };
    }

    [KernelFunction("update_deck")]
    [Description("Updates the current deck with the provided deck model.")]
    public Task<MessageResponse> UpdateDeck(
        DeckModel deckModel
    )
    {
        deckEditorContext.DeckModel.RoleCards = deckModel.RoleCards;
        deckEditorContext.DeckModel.ActionCards = deckModel.ActionCards;

        return Task.FromResult(new MessageResponse
        {
            Message = "Deck updated successfully.",
            CurrentContext = deckEditorContext
        });
    }

    [KernelFunction("get_action_cards")]
    [Description("Retrieves the collection of action cards in the current deck.")]
    // ReSharper disable once UnusedMember.Global
    public async Task<ActionCollectionModel> GetActionCollectionAsync()
    {
        var result = await hoyolab.GetCardActionsAsync(
            roleIds: deckEditorContext.DeckModel.ActionCards
        );

        var models = result.Actions
            .Select(x => new ActionModel
            {
                ItemId = x.Basic.ItemId,
                Name = x.Basic.Name
            })
            .ToList();

        return new()
        {
            ActionCards = models,
            CurrentContext = deckEditorContext
        };
    }

    //[KernelFunction("encode_sharing_code")]
    //public async Task<string> EncodeSharingCodeAsync()
    //{
    //    var result = await hoyolab.EncodeCardCodeAsync(
    //        roleCards: deckEditorContext.DeckModel.RoleCards,
    //        actionCards: deckEditorContext.DeckModel.ActionCards
    //    );

    //    return result.Code;
    //}
}

public class DeckModel
{
    [JsonPropertyName("role_cards")]
    [Description("List of role card item IDs.")]
    public List<int> RoleCards { get; set; } = [];

    [JsonPropertyName("action_cards")]
    [Description("List of action card item IDs.")]
    public List<int> ActionCards { get; set; } = [];
}

public class ActionModel
{
    [JsonPropertyName("item_id")]
    [Description("The unique identifier for the action card.")]
    public int ItemId { get; set; }

    [JsonPropertyName("name")]
    [Description("The name of the action card.")]
    public string? Name { get; set; }
}

public abstract class PluginOutputBaseModel
{
    [JsonPropertyName("current_context")]
    [Description("The current context of the deck editor.")]
    public DeckEditorContext CurrentContext { get; set; } = new();
}

public class MessageResponse : PluginOutputBaseModel
{
    [JsonPropertyName("message")]
    [Description("A message describing the result of the operation.")]
    public string Message { get; set; } = string.Empty;
}

public class ImportDeckResponse : MessageResponse
{
}

public class ActionCollectionModel : PluginOutputBaseModel
{
    [JsonPropertyName("action_cards")]
    [Description("The collection of action cards in the current deck.")]
    public List<ActionModel> ActionCards { get; set; } = [];
}