using SkiaSharp;

namespace GitcgSkia.Extensions;

public static class SkExtension
{
    public static SKColor ToSkColor(this System.Drawing.Color color)
    {
        return new SKColor(
            red: color.R,
            green: color.G,
            blue: color.B,
            alpha: color.A
        );
    }
}