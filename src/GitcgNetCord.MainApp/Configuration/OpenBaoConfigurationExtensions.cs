using System.Text.Json;
using Microsoft.Extensions.Configuration;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace GitcgNetCord.MainApp.Configuration;

/// <summary>
/// Options for configuring the OpenBao / Vault KV v2 secret configuration provider.
/// </summary>
public class OpenBaoConfigurationOptions
{
    public string? Endpoint { get; set; }
    public string? Token { get; set; }
    public string? Namespace { get; set; }
    public string MountPoint { get; set; } = "secret";
    public string SecretPath { get; set; } = "bot";
    public bool Optional { get; set; } = true;
}

/// <summary>
/// Configuration source that creates an <see cref="OpenBaoConfigurationProvider"/>.
/// </summary>
public class OpenBaoConfigurationSource(OpenBaoConfigurationOptions options) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new OpenBaoConfigurationProvider(options);
    }
}

/// <summary>
/// Custom configuration provider that loads secrets from OpenBao KV v2 using VaultSharp.
/// </summary>
public class OpenBaoConfigurationProvider(OpenBaoConfigurationOptions options) : ConfigurationProvider
{
    public override void Load()
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint) || string.IsNullOrWhiteSpace(options.Token))
        {
            if (options.Optional)
            {
                return;
            }

            throw new InvalidOperationException(
                "OpenBao is configured as required, but Endpoint or Token is missing."
            );
        }

        try
        {
            LoadAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to retrieve secrets from OpenBao at '{options.Endpoint}' (Mount: '{options.MountPoint}', Path: '{options.SecretPath}', Namespace: '{options.Namespace}'). {ex.Message}",
                ex
            );
        }
    }

    private async Task LoadAsync()
    {
        var authMethod = new TokenAuthMethodInfo(options.Token);
        var settings = new VaultClientSettings(options.Endpoint, authMethod);

        if (!string.IsNullOrWhiteSpace(options.Namespace))
        {
            settings.Namespace = options.Namespace;
        }

        var client = new VaultClient(settings);

        var secret = await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(
            path: options.SecretPath,
            mountPoint: options.MountPoint
        ).ConfigureAwait(false);

        if (secret?.Data?.Data is { } secretDictionary)
        {
            foreach (var (key, value) in secretDictionary)
            {
                if (value is not null)
                {
                    Data[key] = ExtractValue(value);
                }
            }
        }
    }

    private static string? ExtractValue(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Null => null,
                _ => jsonElement.GetRawText()
            };
        }

        return value.ToString();
    }
}

/// <summary>
/// Extension methods for registering the OpenBao configuration provider.
/// </summary>
public static class OpenBaoConfigurationExtensions
{
    public static IConfigurationBuilder AddOpenBaoConfiguration(
        this IConfigurationBuilder builder
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        var tempConfig = builder.Build();

        var endpoint = tempConfig["OPENBAO_ADDR"]
                       ?? tempConfig["VAULT_ADDR"]
                       ?? tempConfig["OpenBao:Endpoint"];

        var token = tempConfig["OPENBAO_TOKEN"]
                    ?? tempConfig["VAULT_TOKEN"]
                    ?? tempConfig["OpenBao:Token"];

        var ns = tempConfig["OPENBAO_NAMESPACE"]
                 ?? tempConfig["VAULT_NAMESPACE"]
                 ?? tempConfig["OpenBao:Namespace"];

        var mountPoint = tempConfig["OPENBAO_MOUNT"]
                         ?? tempConfig["VAULT_MOUNT"]
                         ?? tempConfig["OpenBao:MountPoint"]
                         ?? "secret";

        var secretPath = tempConfig["OPENBAO_PATH"]
                         ?? tempConfig["VAULT_PATH"]
                         ?? tempConfig["OpenBao:SecretPath"]
                         ?? "bot";

        var isConfigured = !string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(token);

        var options = new OpenBaoConfigurationOptions
        {
            Endpoint = endpoint,
            Token = token,
            Namespace = ns,
            MountPoint = mountPoint,
            SecretPath = secretPath,
            Optional = !isConfigured
        };

        return builder.Add(new OpenBaoConfigurationSource(options));
    }
}
