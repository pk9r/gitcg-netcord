using System.Collections.Generic;
using HoyolabHttpClient.Models;

namespace HoyolabHttpClient.Extensions;

public static class SkillElements
{
    public static IEnumerable<SkillElement> All { get; }

    public static SkillElement Cyro { get; }
    public static SkillElement Hydro { get; }
    public static SkillElement Pyro { get; }
    public static SkillElement Electro { get; }
    public static SkillElement Anemo { get; }
    public static SkillElement Geo { get; }
    public static SkillElement Dendro { get; }
    public static SkillElement Unaligned { get; }
    public static SkillElement Aligned { get; }
    public static SkillElement Energy { get; }
    public static SkillElement ArcaneLegend { get; }

    static SkillElements()
    {
        Cyro = new SkillElement(nameof(Cyro), "11");
        Hydro = new SkillElement(nameof(Hydro), "12");
        Pyro = new SkillElement(nameof(Pyro), "13");
        Electro = new SkillElement(nameof(Electro), "14");
        Anemo = new SkillElement(nameof(Anemo), "17");
        Geo = new SkillElement(nameof(Geo), "15");
        Dendro = new SkillElement(nameof(Dendro), "16");
        Unaligned = new SkillElement(nameof(Unaligned), "10");
        Aligned = new SkillElement(nameof(Aligned), "3");
        Energy = new SkillElement(nameof(Energy), "1");
        ArcaneLegend = new SkillElement(nameof(ArcaneLegend), "6");

        All =
        [
            Cyro, Hydro, Pyro, Electro, Anemo, Geo, Dendro,
            Unaligned, Aligned,
            Energy,
            ArcaneLegend
        ];
    }
}
