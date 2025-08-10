using System.Text;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using HoyolabHttpClient;
using HoyolabHttpClient.Models;
using HoyolabHttpClient.Responses.DeckList;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using SharedUtils;

namespace GitcgNetCord.MainApp.Commands.Autocompletes;

public class SharingCodeAutocompleteProvider(
    HoyolabHttpClientService hoyolab,
    HoyolabDeckAccountService deckAccountService,
    // ILogger<SharingCodeAutocompleteProvider> logger,
    IServiceScopeFactory scopeFactory
) : IAutocompleteProvider<AutocompleteInteractionContext>
{
    public async ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(
        ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
    {
        List<ApplicationCommandOptionChoiceProperties> suggestions = [];

        await using var scope = scopeFactory.CreateAsyncScope();
        var activeHoyolabAccountService = scope.ServiceProvider
            .GetRequiredService<ActiveHoyolabAccountService>();

        var hoyolabAccount = await activeHoyolabAccountService
            .GetActiveHoyolabAccountAsync(
                discordUserId: context.User.Id
            );

        var keyword = option.Value!;
        keyword = keyword.Trim();

        await AddAccountDecks();
        await AddDecodeMessage();
        AddMessageIfEmpty();
        AddLinkToAccountMessage();

        return suggestions.Take(25);

        void AddMessageIfEmpty()
        {
            if (suggestions.Count != 0)
                return;

            suggestions.Add(
                new ApplicationCommandOptionChoiceProperties(
                    name: "Please enter a valid deck sharing code.",
                    stringValue: keyword
                )
            );
        }

        void AddLinkToAccountMessage()
        {
            // Skip if the user has an active hoyolab account.
            if (hoyolabAccount != null)
                return;

            suggestions.Add(
                new ApplicationCommandOptionChoiceProperties(
                    name: "Link to hoyolab account to see your decks.",
                    stringValue: "hoyolab-accounts"
                )
            );
        }

        async ValueTask AddDecodeMessage()
        {
            if (keyword.Length != SharedConstants.SharingCodeLength)
                return;

            try
            {
                var decoded = await hoyolab.DecodeCardCodeAsync(keyword);

                var rolesDisplay = string.Join(
                    separator: ", ",
                    decoded.RoleCards.Select(x => x.Basic.Name)
                );

                var displayBuilder = new StringBuilder();
                displayBuilder.Append(rolesDisplay);
                displayBuilder.Append(" - ");
                displayBuilder.Append($"{decoded.ActionCards.Count} Cards");

                suggestions.Insert(
                    index: 0,
                    item: new ApplicationCommandOptionChoiceProperties(
                        name: displayBuilder.ToString(),
                        stringValue: keyword
                    )
                );
            }
            catch (Exception e)
            {
                suggestions.Add(
                    new ApplicationCommandOptionChoiceProperties(
                        name: e.Message,
                        stringValue: keyword
                    )
                );
            }
        }

        async ValueTask AddAccountDecks()
        {
            if (hoyolabAccount == null)
                return;

            var authorize = new HoyolabAuthorize(
                HoyolabUserId: hoyolabAccount.HoyolabUserId,
                Token: hoyolabAccount.Token
            );

            Data deckListResult;
            try
            {
                deckListResult = await deckAccountService
                    .GetDeckListAsync(
                        authorize: authorize,
                        hoyolabAccount.GameRoleId,
                        hoyolabAccount.Region
                    );
            }
            catch (Exception e)
            {
                suggestions.Add(
                    new ApplicationCommandOptionChoiceProperties(
                        name: e.Message,
                        stringValue: keyword
                    )
                );
                
                return;
            }

            suggestions.AddRange(
                deckListResult.DeckList.Select(CreateSuggestion)
                    .Where(x => StartsWith(x.Name, keyword) || Contains(x.Name, keyword))
                    .OrderBy(x => StartsWith(x.Name, keyword) ? 0 : 1)
            );
        }
    }

    static ApplicationCommandOptionChoiceProperties
        CreateSuggestion(Deck deck, int index)
    {
        var nameBuilder = new StringBuilder();

        var rolesDisplay = string.Join(
            separator: ", ",
            deck.AvatarCards.Select(y => y.Name)
        );

        nameBuilder.Append($"{index + 1}. ");
        nameBuilder.Append(deck.Name);
        nameBuilder.Append(": ");
        nameBuilder.Append(rolesDisplay);

        LimitNameLength(nameBuilder);

        return new ApplicationCommandOptionChoiceProperties(
            name: nameBuilder.ToString(),
            stringValue: deck.ShareCode
        );
    }

    static bool StartsWith(string x, string k)
    {
        return x.StartsWith(
            value: k,
            comparisonType: StringComparison.OrdinalIgnoreCase
        );
    }

    static bool Contains(string x, string k)
    {
        var ks = k.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        return ks.All(
            predicate: kk => x.Contains(
                value: kk,
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
        );
    }

    static void LimitNameLength(StringBuilder nameBuilder)
    {
        if (nameBuilder.Length <= 100)
            return;

        nameBuilder.Length = 97;
        nameBuilder.Append("...");
    }
}
