using Microsoft.Extensions.Configuration;

namespace GitcgNetCord.MainApp.Configuration;

/// <summary>
/// Extension methods for early startup and bootstrapping validation.
/// </summary>
public static class StartupValidationExtensions
{
    /// <summary>
    /// Validates that the Discord bot token is configured.
    /// Throws an <see cref="InvalidOperationException"/> if the token is missing or empty.
    /// </summary>
    public static void ValidateDiscordConfiguration(this IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var token = configuration["Discord:Token"] ?? configuration["Token"];
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Discord bot token is missing or empty. Please configure 'Discord:Token' (or 'Token') via environment variables, user secrets (dotnet user-secrets set \"Discord:Token\" \"<token>\"), appsettings.json, or Azure Key Vault before starting the application."
            );
        }
    }
}
