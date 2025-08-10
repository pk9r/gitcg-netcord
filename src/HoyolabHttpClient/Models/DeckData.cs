using System.Collections.Generic;
using HoyolabHttpClient.Models.Interfaces;

namespace HoyolabHttpClient.Models;

public record DeckData(
    IReadOnlyList<Role> RoleCards,
    IReadOnlyList<Action> ActionCards
) : IDeckData;