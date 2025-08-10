using System.Collections.Generic;

namespace HoyolabHttpClient.Models.Interfaces;

public interface IDeckData
{
    public IReadOnlyList<Role> RoleCards { get; }

    public IReadOnlyList<Action> ActionCards { get; }
}
