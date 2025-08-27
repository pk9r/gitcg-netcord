using GitcgNetCord.MainApp.Configuration;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.GatewayHandlers;
using GitcgNetCord.MainApp.Infrastructure.HoyolabServices;
using GitcgNetCord.MainApp.Models;
using GitcgNetCord.MainApp.Modules;
using GitcgNetCord.MainApp.Modules.Feats;
using GitcgPainter.Extensions;
using GitcgSkia.Extensions;
using HoyolabHttpClient.Extensions;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Extensions;

public static class Extensions
{
    public static void AddDiscordBotModules(
        this IHost host
    )
    {
        // Feature modules
        host.AddCardCodeModule();
        host.AddHoyolabAccountModule();
        host.AddHoyolabGcgModule();

        // Utility modules
        host.AddRoleEmojisModule();
    }

    public static void AddNetCordServices(
        this IServiceCollection services
    )
    {
        // builder.AddDiscordBotLoginOptions();

        services
            .AddDiscordGateway(options => { options.Intents = GatewayIntents.All; })
            .AddApplicationCommands()
            .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
            .AddComponentInteractions<StringMenuInteraction, StringMenuInteractionContext>()
            .AddComponentInteractions<UserMenuInteraction, UserMenuInteractionContext>()
            .AddComponentInteractions<RoleMenuInteraction, RoleMenuInteractionContext>()
            .AddComponentInteractions<MentionableMenuInteraction, MentionableMenuInteractionContext>()
            .AddComponentInteractions<ChannelMenuInteraction, ChannelMenuInteractionContext>()
            .AddComponentInteractions<ModalInteraction, ModalInteractionContext>();

        services
            .AddGatewayHandler<CardCodeGatewayHandler>()
            .AddGatewayHandler<ReplaysGatewayHandler>();

        services
            .AddOptionsWithValidateOnStart<CardCodeModuleOptions>()
            .BindConfiguration(CardCodeModuleOptions.ConfigurationSectionName);
    }

    public static void AddHoyolabServices(
        this IServiceCollection services
    )
    {
        services.AddHoyolabHttpClient();
        services.AddSingleton<HoyolabCardRoleService>();
        services.AddSingleton<HoyolabDecoder>();
        services.AddSingleton<HoyolabDeckAccountService>();
        services.AddSingleton<HoyolabGcgBasicInfoService>();
    }

    public static void AddAppServices(
        this IServiceCollection services
    )
    {
        services.AddScoped<ActiveHoyolabAccountService>();
        services.AddScoped<DiscordCardCodeChannelService>();
        services.AddScoped<DiscordUserService>();
        services.AddScoped<HoyolabAccountService>();

        services.AddGitcgPainter();
        services.AddGitcgSkia();
        services.AddTransient<DeckImageCreatorCollection>();
    }
}