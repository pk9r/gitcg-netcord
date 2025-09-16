using GitcgPainter.ImageCreators.Deck.Simplest;
using GitcgSkia.ImageCreators.Deck.GameBackground;
using GitcgSkia.ImageCreators.Deck.Genshincards;

namespace GitcgNetCord.MainApp.Models;

public class DeckImageCreatorCollection(
    SimplestDeckImageCreator simplest,
    GameBackgroundDeckImageCreator gameBackground,
    GenshincardsDeckImageCreator genshincards
)
{
    public SimplestDeckImageCreator Simplest { get; } = simplest;
    public GameBackgroundDeckImageCreator GameBackground { get; } = gameBackground;
    public GenshincardsDeckImageCreator Genshincards { get; } = genshincards;
}