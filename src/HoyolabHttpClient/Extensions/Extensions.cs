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
            .ConfigureHttpClient(ConfigureClient)
            .ConfigurePrimaryHttpMessageHandler(ConfigureHandler);
    }

    private static void ConfigureClient(
        IServiceProvider serviceProvider,
        HttpClient client
    )
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<HoyolabHttpClientOptions>>();
        var defaultAuthorize = options.Value.DefaultAuthorize;

        if (defaultAuthorize == null) 
            return;
        
        HoyolabHttpClientService
            .ConfigAuthorizeClient(
                client: client,
                authorize: defaultAuthorize
            );
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