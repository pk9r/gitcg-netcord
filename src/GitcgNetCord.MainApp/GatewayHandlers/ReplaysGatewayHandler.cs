using System.Collections.Immutable;
using System.Drawing;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace GitcgNetCord.MainApp.GatewayHandlers;

public class ReplaysGatewayHandler(
    GatewayClient client,
    HoyolabGcgBasicInfoService gcgBasicInfoService,
    HoyolabCardRoleService cardRoleService
) : IMessageCreateGatewayHandler
{
    public async ValueTask HandleAsync(Message message)
    {
        if (message.Author.IsBot)
            return;

        if (!IsReplaysChannel(message.ChannelId))
            return;

        var content = message.Content;

        var uid = content;
        var server = GetHoyolabServer(content);

        var greenColor = Color.Green.ToNetCordColor();
        var redColor = Color.Red.ToNetCordColor();

        HoyolabHttpClient.Responses.GcgBasicInfo.Data info;

        try
        {
            info = await gcgBasicInfoService
                .GetGcgBasicInfoAsync(server, uid);
        }
        catch
        {
            await message.AddReactionAsync(
                emoji: new ReactionEmojiProperties("❌")
            );
            return;
        }

        var appEmojis = await client.Rest
            .GetApplicationEmojisAsync(client.Id);
        var emojis = appEmojis.ToImmutableDictionary(x => x.Name);

        var cardRoles = await cardRoleService.GetCardRolesAsync();

        await message.ReplyAsync(new ReplyMessageProperties()
            .AddEmbeds(
                info.Replays.Select(x => new EmbedProperties()
                    .WithColor(x.IsWin ? greenColor : redColor)
                    .WithTitle(x.MatchType)
                    .AddFields(
                        new EmbedFieldProperties()
                            .WithName(
                                $"{GetRoleEmojis(x.Self.Linups)} - " +
                                $"{DisplayName(x.Self.Name, x.IsWin)}"),
                        new EmbedFieldProperties()
                            .WithName($"{DisplayResult(x.IsWin)}"),
                        new EmbedFieldProperties()
                            .WithName(
                                $"{GetRoleEmojis(x.Opposite.Linups)} - " +
                                $"{DisplayName(x.Opposite.Name, !x.IsWin)}")
                    )
                    .WithFooter(new EmbedFooterProperties()
                        .WithText(
                            $"{x.MatchTime.Year}/{x.MatchTime.Month:00}/{x.MatchTime.Day:00} " +
                            $"{x.MatchTime.Hour:00}:{x.MatchTime.Minute:00}"
                        )
                    )
                ))
        );
        
        return;

        string GetRoleEmojis(IEnumerable<string> s)
        {
            var roles = s.Select(url => cardRoles.Roles
                .First(x => x.Basic.IconSmall == url)
            );
            var ids = roles.Select(x => x.Basic.ItemId.ToString());
            return string.Join(" ", ids.Select(id => emojis[id]));
        }

        string DisplayName(string name, bool isWin)
        {
            return $"{name}";
        }

        string DisplayResult(bool isWin)
        {
            return isWin ? "Victory" : "Defeat";
        }
    }

    private bool IsReplaysChannel(ulong channelId)
    {
        // if (_options.CardCodeChannels.Contains(channelId))
        //     return true;

        if (channelId == 1402847673865732126)
            return true;

        return false;
    }

    private static string GetHoyolabServer(string uid)
    {
        var serverMap = new Dictionary<char, string>
        {
            ['6'] = "os_usa", //
            ['7'] = "os_euro", //
            ['8'] = "os_asia" //
        };

        return uid.Length switch
        {
            9 => serverMap[uid[0]],
            10 => serverMap[uid[1]],
            _ => throw new ArgumentException(
                message: "Invalid UID length",
                paramName: nameof(uid)
            )
        };
    }
}
