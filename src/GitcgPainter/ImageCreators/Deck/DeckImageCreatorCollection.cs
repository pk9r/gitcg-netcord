using GitcgPainter.ImageCreators.Deck.GameBackground;
using GitcgPainter.ImageCreators.Deck.Genshincards;
using GitcgPainter.ImageCreators.Deck.Simplest;

namespace GitcgPainter.ImageCreators.Deck;

public class DeckImageCreatorCollection(
    SimplestDeckImageCreator simplest,
    GenshincardsDeckImageCreator genshincards
)
{
    public SimplestDeckImageCreator Simplest { get; } = simplest;
    public GenshincardsDeckImageCreator Genshincards { get; } = genshincards;
}