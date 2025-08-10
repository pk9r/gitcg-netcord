using System.Collections.Immutable;
using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using HoyolabHttpClient;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Color = System.Drawing.Color;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class TcgSlashCommand
{
    public static async Task ExecuteAsync(
        IServiceProvider serviceProvider,
        ApplicationCommandContext context,
        [
            SlashCommandParameter(
                Description = "Genshin Impact UID. Default will be auto-detected from your Hoyolab account."
            )
        ]
        string uid = "",
        [
            SlashCommandParameter(
                Description = "Server. Default will be auto-detected.",
                AutocompleteProviderType = typeof(ServerAutocompleteProvider)
            )
        ]
        string server = ""
    )
    {
        var gcgBasicInfoService = serviceProvider
            .GetRequiredService<HoyolabGcgBasicInfoService>();
        var activeHoyolabAccountService = serviceProvider
            .GetRequiredService<ActiveHoyolabAccountService>();
        var cardRoleService = serviceProvider
            .GetRequiredService<HoyolabCardRoleService>();

        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage()
        );

        var hoyolabAccount = await activeHoyolabAccountService
            .GetActiveHoyolabAccountAsync(context.User.Id);

        HoyolabAuthorize? authorize = null;
        if (hoyolabAccount != null)
        {
            authorize = new HoyolabAuthorize(
                HoyolabUserId: hoyolabAccount.HoyolabUserId,
                Token: hoyolabAccount.Token
            );

            if (string.IsNullOrWhiteSpace(uid))
                uid = hoyolabAccount.GameRoleId;

            if (string.IsNullOrWhiteSpace(server))
                server = hoyolabAccount.Region;
        }

        server = GetServerValue(server, uid);

        var greenColor = new NetCord.Color(Color.Green.ToArgb());
        var redColor = new NetCord.Color(Color.Red.ToArgb());

        HoyolabHttpClient.Responses.GcgBasicInfo.Data info;
        
        try
        {
            info = await gcgBasicInfoService
                .GetGcgBasicInfoAsync(server, uid, authorize);
        }
        catch (Exception e)
        {
            await context.Interaction.ModifyResponseAsync(message =>
            {
                message.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription(e.Message)
                    .WithColor(redColor)
                );
            });
            return;
        }

        var appEmojis = await context.Client.Rest
            .GetApplicationEmojisAsync(context.Client.Id);
        var emojis = appEmojis.ToImmutableDictionary(x => x.Name);

        var cardRoles = await cardRoleService.GetCardRolesAsync();

        await context.Interaction.ModifyResponseAsync(message =>
        {
            message.AddEmbeds(
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
                ));
        });

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

    private static string GetServerValue(
        string server, string uid
    )
    {
        if (!string.IsNullOrWhiteSpace(server))
            return server;

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
            _ => server
        };
    }
}
