using System.Globalization;
using GitcgNetCord.MainApp.Commands.Slash;
using Xunit;

namespace GitcgNetCord.MainApp.Tests;

public class CardSlashCommandTests
{
    [Theory]
    [InlineData("")]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    [InlineData("vi-VN")]
    public void FormatWinRate_AcrossCultures_FormatsTwoDecimalsWithoutSpace(string cultureName)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = string.IsNullOrEmpty(cultureName)
                ? CultureInfo.InvariantCulture
                : CultureInfo.GetCultureInfo(cultureName);

            Assert.Equal("75.00%", CardSlashCommand.FormatWinRate(0.75f));
            Assert.Equal("0.00%", CardSlashCommand.FormatWinRate(0f));
            Assert.Equal("100.00%", CardSlashCommand.FormatWinRate(1f));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
