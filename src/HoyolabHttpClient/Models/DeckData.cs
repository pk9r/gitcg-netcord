using System.Collections.Generic;
using HoyolabHttpClient.Models.Interfaces;

namespace HoyolabHttpClient.Models;

public record DeckData(
    IReadOnlyCollection<Role> RoleCards,
    IReadOnlyCollection<Action> ActionCards
) : IDeckData;