using System.Linq;
using HoyolabHttpClient.Models;

namespace HoyolabHttpClient.Extensions;

public static class ActionExtension
{
    public static bool HasTag(this Models.Action action, CardTag tag)
    {
        return action.CardTags.Contains(tag.Value);
    }

    public static bool IsArcaneLegend(this Models.Action action)
    {
        return action.HasTag(CardTag.ArcaneLegend);
    }

    public static SkillElement? GetSkillElement(this Models.Action action)
    {
        return SkillElement.FromValue(action.SkillElement);
    }

    public static SkillElement? GetSkillElement2(this Models.Action action)
    {
        return SkillElement.FromValue(action.SkillElement2);
    }
}
