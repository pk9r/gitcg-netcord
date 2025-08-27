using System;
using System.IO;
using System.Threading.Tasks;
using HoyolabHttpClient.Models.Interfaces;

namespace GitcgPainter.ImageCreators.Deck.Abstractions;

[Obsolete(
    "This interface is deprecated. " +
    "Use IDeckImageCreator<TOptions> from GitcgSharp.Shared library instead."
)]
public interface IDeckImageCreator<in TOptions>
    : IDeckImageCreationService where TOptions : class
{
    Task<Stream> CreateImageAsync(
        IDeckData deckData,
        TOptions? options = null
    );
}

[Obsolete(
    "This interface is deprecated. " +
    "Use IDeckImageCreationService from GitcgSharp.Shared library instead."
)]
public interface IDeckImageCreationService
{
    Task<Stream> CreateImageAsync(IDeckData deckData);
}