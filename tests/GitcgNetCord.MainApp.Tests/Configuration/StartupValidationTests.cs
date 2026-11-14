using GitcgNetCord.MainApp.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GitcgNetCord.MainApp.Tests.Configuration;

public class StartupValidationTests
{
    [Fact]
    public void ValidateDiscordConfiguration_WhenConfigurationNull_ThrowsArgumentNullException()
    {
        IConfiguration configuration = null!;

        Assert.Throws<ArgumentNullException>(() => configuration.ValidateDiscordConfiguration());
    }

    [Fact]
    public void ValidateDiscordConfiguration_WhenTokenMissing_ThrowsInvalidOperationException()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var ex = Assert.Throws<InvalidOperationException>(() => config.ValidateDiscordConfiguration());
        Assert.Contains("Discord bot token is missing", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void ValidateDiscordConfiguration_WhenTokenWhitespace_ThrowsInvalidOperationException(string whitespaceToken)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Discord:Token"] = whitespaceToken
            })
            .Build();

        var ex = Assert.Throws<InvalidOperationException>(() => config.ValidateDiscordConfiguration());
        Assert.Contains("Discord bot token is missing", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateDiscordConfiguration_WhenDiscordTokenProvided_DoesNotThrow()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Discord:Token"] = "valid.bot.token"
            })
            .Build();

        var exception = Record.Exception(() => config.ValidateDiscordConfiguration());
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateDiscordConfiguration_WhenFallbackTokenProvided_DoesNotThrow()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Token"] = "valid.fallback.token"
            })
            .Build();

        var exception = Record.Exception(() => config.ValidateDiscordConfiguration());
        Assert.Null(exception);
    }
}
