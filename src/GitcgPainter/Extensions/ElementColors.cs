using SixLabors.ImageSharp;

namespace GitcgPainter.Extensions;

public static class ElementColors
{
    public static Color Cyro { get; } = Color.ParseHex("#7af2f2");

    public static Color Hydro { get; } = Color.ParseHex("#00c0ff");

    public static Color Pyro { get; } = Color.ParseHex("#ff6640");

    public static Color Electro { get; } = Color.ParseHex("#cc80ff");

    public static Color Anemo { get; } = Color.ParseHex("#33d7a0");

    public static Color Geo { get; } = Color.ParseHex("#ffb00d");

    public static Color Dendro { get; } = Color.ParseHex("#9be53d");

    public static Color Unaligned { get; } = Color.Gray;

    public static Color Aligned { get; } = Color.White;

    public static Color Energy { get; } = Color.ParseHex("#ffffdf");

    public static Color ArcaneLegend { get; } = Color.Purple;
}
