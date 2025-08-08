using System.Threading.Tasks;
using HoyolabHttpClient.Models.Interfaces;

namespace GitcgPainter.ImageCreators.Deck.Abstractions;

public interface IDeckImageCreator<TOptions>
    : IDeckImageCreationService where TOptions : class
{
    Task<byte[]> CreateImageAsync(IDeckData deckData, TOptions? options = null);
}

public interface IDeckImageCreationService
{
    Task<byte[]> CreateImageAsync(IDeckData deckData);
}
