using System;
using System.Net.Http;
using HoyolabHttpClient.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HoyolabHttpClient.Extensions;

public static class Extensions
{
    public static void AddHoyolabHttpClient(
        this IServiceCollection services
    )
    {
        services
            .AddOptionsWithValidateOnStart<HoyolabHttpClientOptions>()
            .BindConfiguration(HoyolabHttpClientOptions.ConfigurationSectionName);

        services.AddHttpClient("HoyolabHttpClient")
            .ConfigurePrimaryHttpMessageHandler(ConfigureHandler);

        services.AddHttpClient<HoyolabHttpClientService>()
            .ConfigurePrimaryHttpMessageHandler(ConfigureHandler);
    }

    private static HttpMessageHandler ConfigureHandler(
        IServiceProvider serviceProvider
    )
    {
        var httpMessageHandler = new HttpClientHandler
        {
            UseCookies = false //
        };
        
        return httpMessageHandler;
    }
}