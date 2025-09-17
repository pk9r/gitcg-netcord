using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using HoyolabHttpClient.Models.Interfaces;
using Microsoft.SemanticKernel;

namespace GitcgNetCord.MainApp.Plugins.DuelAssistant;

// ReSharper disable once ClassNeverInstantiated.Global
public class DuelUpdateDeckPlugin(
    HoyolabDecoder hoyolabDecoder
)
{
    [KernelFunction("update_decks")]
    [Description("Update the decks of current player")]
    // ReSharper disable once UnusedMember.Global
    public async Task<UpdateDecksResponse> UpdateDecksAsync(
        ICollection<UpdateDeckModel> decks
    )
    {
        var success = true;
        var dbDecks = new IDeckData?[3];
        var messageBuilder = new StringBuilder();

        await Task.WhenAll(
            decks.Select(async x =>
            {
                var decodedResult = await hoyolabDecoder.DecodeAsync(x.SharingCode);

                if (decodedResult.Validate.Failed)
                {
                    success = false;
                    messageBuilder.AppendLine(
                        $"""
                         Deck {x.Index + 1}:
                         {decodedResult.Validate.FailureMessage}
                         """
                    );
                }

                dbDecks[x.Index] = decodedResult.Deck;

                return Task.CompletedTask;
            })
        );

        var occurrences = new Dictionary<
            int,
            (string Name, List<int> Indexes)
        >();
        for (var i = 0; i < dbDecks.Length; i++)
        {
            var deck = dbDecks[i];
            if (deck == null) continue;

            foreach (var card in deck.RoleCards)
            {
                if (!occurrences.ContainsKey(card.Basic.ItemId))
                    occurrences[card.Basic.ItemId] = (card.Basic.Name, []);

                occurrences[card.Basic.ItemId].Indexes.Add(i + 1);
            }
        }

        foreach (var occurrence in occurrences)
        {
            if (occurrence.Value.Indexes.Count <= 1)
                continue;

            var indexes = string.Join(", ", occurrence.Value.Indexes);
            
            success = false;
            messageBuilder.AppendLine(
                $"Deck {indexes} contains the same card: {occurrence.Value.Name}."
            );
        }

        return new UpdateDecksResponse
        {
            Success = success,
            Message = messageBuilder.ToString()
        };
    }
}

[Description("Deck update model")]
public class UpdateDeckModel
{
    [JsonPropertyName("index")]
    [Description("Index of the deck to update")]
    public int Index { get; set; }

    [JsonPropertyName("sharing_code")]
    [Description("Sharing code of the deck")]
    public string SharingCode { get; set; } = string.Empty;
}

public class UpdateDecksResponse
{
    [JsonPropertyName("success")]
    [Description("Indicates whether the update was successful.")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    [Description(
        "Message providing additional information about the update."
    )]
    public string Message { get; set; } = string.Empty;
}