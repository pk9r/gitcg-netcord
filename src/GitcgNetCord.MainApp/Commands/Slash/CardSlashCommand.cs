using System.Collections.Immutable;
using System.Drawing;
using System.Text;
using GitcgNetCord.MainApp.Entities.Repositories;
using HoyolabHttpClient;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class CardSlashCommand
{
    public static async Task ExecuteAsync(
        HoyolabHttpClientService hoyolab,
        IServiceProvider serviceProvider,
        ApplicationCommandContext context,
        [
            SlashCommandParameter(
                Description = "Minimum use count for a card to be considered. Default: 1%."
            )
        ]
        string minUseCount = "1%",
        [
            SlashCommandParameter(
                Description = "Number of top win rates to display. Default: 5."
            )
        ]
        int topWinRateCount = 5,
        [
            SlashCommandParameter(
                Description = "Number of top use counts to display. Default: 10."
            )
        ]
        int topUseCount = 10
    )
    {
        var activeHoyolabAccountService = serviceProvider
            .GetRequiredService<ActiveHoyolabAccountService>();

        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage()
        );

        var hoyolabAccount = await activeHoyolabAccountService
            .GetActiveHoyolabAccountAsync(context.User.Id);

        if (hoyolabAccount == null)
        {
            var commands = await context.Client.Rest
                .GetGlobalApplicationCommandsAsync(context.Client.Id);

            var accountCommand = commands
                .First(x => x.Name == "hoyolab-accounts");

            await context.Interaction.ModifyResponseAsync(message =>
            {
                message.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription(
                        $"""
                         You don't have an active Hoyolab account.
                         Please use the command {accountCommand} to set one up.
                         """
                    )
                    .WithColor(new NetCord.Color(Color.Red.ToArgb()))
                );
            });

            return;
        }

        var authorize = new HoyolabAuthorize(
            HoyolabUserId: hoyolabAccount.HoyolabUserId,
            Token: hoyolabAccount.Token
        );

        var uid = hoyolabAccount.GameRoleId;
        var server = hoyolabAccount.Region;

        HoyolabHttpClient.Responses.GcgCardList.Data data;
        try
        {
            data = await hoyolab.GetGcgCardListAsync(
                server: server,
                roleId: uid,
                authorize: authorize
            );
        }
        catch (Exception e)
        {
            await context.Interaction.ModifyResponseAsync(message =>
            {
                message.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription(e.Message)
                    .WithColor(new NetCord.Color(Color.Red.ToArgb()))
                );
            });
            return;
        }

        var appEmojis = await context.Client.Rest
            .GetApplicationEmojisAsync(context.Client.Id);
        var emojis = appEmojis.ToImmutableDictionary(x => x.Name);

        var characters = data.CardList
            .Where(x => x is { CardType: "CardTypeCharacter" })
            .ToImmutableArray();

        var gameCount = characters.Sum(x => x.UseCount) / 3;
        var winCount = characters.Sum(x => x.Proficiency) / 3;
        var winRate = (float)winCount / gameCount;

        var minUseCountValue = 0;
        var isPercent = minUseCount.EndsWith('%');
        if (isPercent)
        {
            if (float.TryParse(
                    minUseCount.AsSpan(0, minUseCount.Length - 1),
                    out var percent
                ))
            {
                minUseCountValue = (int)(gameCount * percent / 100);
            }
        }
        else
        {
            if (!int.TryParse(minUseCount, out minUseCountValue))
            {
            }
        }

        var winRateList = characters
            .Select(x => new
            {
                x.Id, x.Name, x.Proficiency, x.UseCount,
                WinRate = (float)x.Proficiency / x.UseCount
            })
            .Where(x => x.UseCount > minUseCountValue)
            .ToImmutableArray();

        var bestWinRate = winRateList
            .OrderByDescending(x => x.WinRate)
            .ThenByDescending(x => x.UseCount)
            .Take(topWinRateCount)
            .ToImmutableArray();

        var lowestWinRate = winRateList
            .OrderBy(x => x.WinRate)
            .ThenByDescending(x => x.UseCount)
            .Take(topWinRateCount)
            .ToImmutableArray();

        var topCharacterUseCounts = characters
            .OrderByDescending(x => x.UseCount)
            .Select(x => new
            {
                x.Id, x.Name, x.UseCount
            })
            .Take(topUseCount)
            .ToImmutableArray();

        var topActionUseCounts = data.CardList
            .Where(x => x.CardType != "CardTypeCharacter")
            .Select(x => new
            {
                x.Id, x.Name, x.UseCount
            })
            .OrderByDescending(x => x.UseCount)
            .Take(topUseCount)
            .ToImmutableArray();

        var bestWinRateStringBuilder = new StringBuilder();

        for (var i = 0; i < bestWinRate.Length; i++)
        {
            var x = bestWinRate[i];

            bestWinRateStringBuilder.AppendLine(
                $"{i + 1}. {emojis[x.Id.ToString()]} {x.Name} - " +
                $"{x.WinRate:P2} " +
                $"({x.Proficiency}/{x.UseCount})"
            );
        }

        var lowestWinRateStringBuilder = new StringBuilder();
        for (var i = 0; i < lowestWinRate.Length; i++)
        {
            var x = lowestWinRate[i];

            lowestWinRateStringBuilder.AppendLine(
                $"{i + 1}. {emojis[x.Id.ToString()]} {x.Name} - " +
                $"{x.WinRate:P2} " +
                $"({x.Proficiency}/{x.UseCount})"
            );
        }

        var topUseCountsStringBuilder = new StringBuilder();
        for (var i = 0; i < topCharacterUseCounts.Length; i++)
        {
            var x = topCharacterUseCounts[i];

            topUseCountsStringBuilder.AppendLine(
                $"{i + 1}. {emojis[x.Id.ToString()]} {x.Name} - {x.UseCount}"
            );
        }

        var topActionUseCountsStringBuilder = new StringBuilder();
        for (var i = 0; i < topActionUseCounts.Length; i++)
        {
            var card = topActionUseCounts[i];

            topActionUseCountsStringBuilder.AppendLine(
                $"{i + 1}. {card.Name} - {card.UseCount}"
            );
        }

        await context.Interaction.ModifyResponseAsync(message =>
        {
            message.AddEmbeds(
                new EmbedProperties()
                    .WithColor(new NetCord.Color(Color.Purple.ToArgb()))
                    .WithTitle($"{data.Stats.Nickname}")
                    .AddFields(
                        new EmbedFieldProperties()
                            .WithName("UID")
                            .WithValue($"{uid}")
                            .WithInline(),
                        new EmbedFieldProperties()
                            .WithName("Total games")
                            .WithValue($"{gameCount}")
                            .WithInline(),
                        new EmbedFieldProperties()
                            .WithName("Total wins")
                            .WithValue($"{winCount} ({winRate:P2})")
                            .WithInline()
                    )
                ,
                new EmbedProperties()
                    .WithColor(new NetCord.Color(Color.Purple.ToArgb()))
                    .AddFields(
                        new EmbedFieldProperties()
                            .WithName("Best win rate")
                            .WithValue(bestWinRateStringBuilder.ToString())
                            .WithInline(),
                        new EmbedFieldProperties()
                            .WithName("Lowest win rate")
                            .WithValue(lowestWinRateStringBuilder.ToString())
                            .WithInline()
                    )
                    .WithFooter(new EmbedFooterProperties()
                        .WithText(
                            "Win rate stats only include cards " +
                            (isPercent
                                ? $"with use count > {minUseCountValue} ({minUseCount} total games)."
                                : $"with use count > {minUseCountValue}.")
                        )
                    )
                ,
                new EmbedProperties()
                    .WithColor(new NetCord.Color(Color.Purple.ToArgb()))
                    .AddFields(
                        new EmbedFieldProperties()
                            .WithName("Top use count characters")
                            .WithValue(topUseCountsStringBuilder.ToString())
                            .WithInline(),
                        new EmbedFieldProperties()
                            .WithName("Top use count actions")
                            .WithValue(topActionUseCountsStringBuilder.ToString())
                            .WithInline()
                    )
            );
        });
    }
}