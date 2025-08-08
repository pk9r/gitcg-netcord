using Gitcg.NetCord.MainApp.Modules;

namespace Gitcg.NetCord.MainApp.Extensions;

public static class Extensions
{
    public static void AddDiscordBotModules(
        this IHost host
    )
    {
        // Feature modules
        // host.AddCardCodeModule();
        // host.AddHoyolabAccountModule();
        // host.AddHoyolabGcgModule();
        
        // Utility modules
        host.AddRoleEmojisModule();
    }
}
