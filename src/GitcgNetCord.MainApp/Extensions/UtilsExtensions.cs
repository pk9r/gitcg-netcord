using NetCord;

namespace GitcgNetCord.MainApp.Extensions;

public static class UtilsExtensions
{
    public static Color ToNetCordColor(
        this System.Drawing.Color color
    )
    {
        return new Color(
            red: color.R,
            green: color.G,
            blue: color.B
        );
    }
}