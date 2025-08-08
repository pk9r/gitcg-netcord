using System.Linq;
using HoyolabHttpClient.Extensions;

namespace HoyolabHttpClient.Models;

public record SkillElement(string Name, string Value)
{
    public static SkillElement? FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return SkillElements.All.Single(x => x.Value == value);
    }

    public override string ToString()
    {
        return Name;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
