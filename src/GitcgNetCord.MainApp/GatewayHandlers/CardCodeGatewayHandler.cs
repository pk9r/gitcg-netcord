using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using GitcgNetCord.MainApp.Commands.Interactions;
using GitcgNetCord.MainApp.Configuration;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using GitcgPainter.ImageCreators.Deck;
using HoyolabHttpClient.Models.Interfaces;
using Microsoft.Extensions.Options;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;
using Color = System.Drawing.Color;

namespace GitcgNetCord.MainApp.GatewayHandlers;

public partial class CardCodeGatewayHandler(
    GatewayClient client,
    HoyolabDecoder decoder,
    IOptions<CardCodeModuleOptions> rawOptions,
    DeckImageCreatorCollection deckImageCreatorCollection,
    IServiceScopeFactory serviceScopeFactory
) : IMessageCreateGatewayHandler
{
    private readonly CardCodeModuleOptions _options = rawOptions.Value;

    public async ValueTask HandleAsync(Message message)
    {
        if (message.Author.IsBot)
            return;

        var dbChannel = await FindDbChannelAsync(message.ChannelId);

        var guildThread = message.Channel as GuildThread;
        
        if (dbChannel == null &&  guildThread == null)
            return;

        if (guildThread != null)
        {
            dbChannel = await FindDbChannelAsync(guildThread.ParentId);
            
            if (dbChannel == null)
                return;
        }

        var channel = message.Channel;
        if (channel == null)
            return;

        var contentBuilder = new StringBuilder();
        contentBuilder.AppendLine(message.Content);
        foreach (var snapshot in message.MessageSnapshots)
            contentBuilder.AppendLine(snapshot.Message.Content);

        var content = contentBuilder.ToString();

        var matches = await MatchesAsync(content);

        var count = matches.Items.Count;

        if (count == 0)
        {
            await message.AddReactionAsync(
                emoji: new ReactionEmojiProperties("❌")
            );
            return;
        }

        await message.AddReactionAsync(
            emoji: new ReactionEmojiProperties("🔍")
        );

        var appEmojis = await client.Rest.GetApplicationEmojisAsync(
            applicationId: client.Id
        );
        var emojis = appEmojis.ToImmutableDictionary(x => x.Name);

        RestMessage? summaryMessage = null;
        var summaryEmbed = new EmbedProperties()
            .WithTitle("Sharing code list")
            .WithColor(new NetCord.Color(Color.Purple.ToArgb()));

        if (count > 1)
        {
            summaryMessage = await message.ReplyAsync(
                replyMessage: new ReplyMessageProperties()
                    .WithEmbeds([summaryEmbed])
            );

            if (guildThread == null)
            {
                var id = Convert.ToBase64String(
                    BitConverter.GetBytes(message.Id)
                );

                channel = await summaryMessage.CreateGuildThreadAsync(
                    new GuildThreadFromMessageProperties(
                        name: $"Sharing code list #{id}"
                    )
                );
            }
        }

        var deckImageCreator = deckImageCreatorCollection.GameBackground;

        for (var i = 0; i < count; i++)
        {
            var deck = matches.Items[i].Deck;
            var sharingCode = matches.Items[i].SharingCode;

            var fileName = $"deck{i + 1}.png";
            var deckImageUrl = $"attachment://{fileName}";

            var deckEmojis = deck.RoleCards.Select(
                selector: x => emojis[x.Basic.ItemId.ToString()]
            );
            var deckEmojisString = string.Join(
                separator: " ",
                values: deckEmojis
            );
            var roleCardString = string.Join(
                separator: ", ",
                values: deck.RoleCards.Select(x => x.Basic.Name)
            );

            var labelBuilder = new StringBuilder();
            if (count > 1) labelBuilder.Append($"{i + 1}/{count} - ");
            labelBuilder.Append($"{deckEmojisString} - {roleCardString}");
            var label = labelBuilder.ToString();

            await using var stream = await deckImageCreator.CreateImageAsync(deck);

            var deckMessage = new MessageProperties();
            if (count == 1)
            {
                deckMessage.MessageReference =
                    MessageReferenceProperties.Reply(
                        message.Id,
                        failIfNotExists: true
                    );
            }

            await channel.SendMessageAsync(
                message: deckMessage
                    .AddAttachments(new AttachmentProperties(
                        fileName: fileName,
                        stream: stream
                    ))
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

            await UpdateSummaryMessageAsync();

            continue;

            async ValueTask UpdateSummaryMessageAsync()
            {
                if (summaryMessage == null)
                    return;

                summaryEmbed.AddFields(new EmbedFieldProperties()
                    .WithName(label)
                    .WithValue(sharingCode)
                );

                await summaryMessage.ModifyAsync(m => { m.WithEmbeds([summaryEmbed]); });
            }
        }

        await message.DeleteCurrentUserReactionAsync(
            emoji: new ReactionEmojiProperties("🔍")
        );

        await message.AddReactionAsync(
            emoji: new ReactionEmojiProperties("✅")
        );

        // await message.ReplyAsync(new ReplyMessageProperties()
        //     .WithEmbeds());
    }

    private async ValueTask<DiscordCardCodeChannel?>
        FindDbChannelAsync(ulong? channelId)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var cardCodeChannelService = scope.ServiceProvider
            .GetRequiredService<DiscordCardCodeChannelService>();

        var dbChannel = await cardCodeChannelService
            .FindAsync(channelId);

        return dbChannel;
    }

    [Obsolete]
    private bool IsCardCodeChannel(ulong channelId)
    {
        return _options.CardCodeChannels.Contains(channelId);
    }

    private async Task<MatchesResult>
        MatchesAsync(string input, int limit = 20)
    {
        var matches = SharingCodeRegex().Matches(input);

        List<ItemResult> items = [];

        var count = 0;
        foreach (Match match in matches)
        {
            var sharingCode = match.Value;

            var result = await decoder.DecodeAsync(sharingCode);

            var added = false;
            if (result.Validate.Succeeded)
            {
                var deck = result.Deck;
                items.Add(new ItemResult(deck, sharingCode));
                added = true;
            }

            if (added && ++count >= limit) break;
        }

        return new MatchesResult(items);
    }

    private record MatchesResult(
        List<ItemResult> Items
    );

    private record ItemResult(
        IDeckData Deck,
        string SharingCode
    );

    [GeneratedRegex(@"[A-Za-z0-9+\/]{68}")]
    private static partial Regex SharingCodeRegex();
}