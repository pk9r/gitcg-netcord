using System.Collections.Generic;

namespace HoyolabHttpClient.Models.Interfaces;

public interface IDeckData
{
    public IReadOnlyCollection<Role> RoleCards { get; }

    public IReadOnlyCollection<Action> ActionCards { get; }
}
