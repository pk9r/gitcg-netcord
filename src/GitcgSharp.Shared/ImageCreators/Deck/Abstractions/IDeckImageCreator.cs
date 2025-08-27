using HoyolabHttpClient.Models.Interfaces;

namespace GitcgSharp.Shared.ImageCreators.Deck.Abstractions;

public interface IDeckImageCreator<in TOptions>
    : IDeckImageCreationService where TOptions : class
{
    Task<Stream> CreateImageAsync(
        IDeckData deckData, 
        TOptions? options = null
    );
}

public interface IDeckImageCreationService
{
    Task<Stream> CreateImageAsync(IDeckData deckData);
}
