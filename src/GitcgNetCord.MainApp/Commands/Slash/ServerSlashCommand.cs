using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Extensions;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class ServerSlashCommand
{
    public static async Task InviteSetAsync(
        DiscordGuildConfigService configService,
        ApplicationCommandContext context,
        [SlashCommandParameter(Description = "Public Discord invite URL (e.g. https://discord.gg/xyz).")]
        string url
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        if (context.Interaction.GuildId is not { } guildId)
        {
            await context.Interaction.ModifyResponseAsync(m => m.Content = "This command can only be used in a server.");
            return;
        }

        if (context.User is GuildInteractionUser guildUser &&
            !guildUser.Permissions.HasFlag(Permissions.ManageGuild) &&
            !guildUser.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.Interaction.ModifyResponseAsync(m => m.Content = "You do not have permission to manage server settings (ManageGuild).");
            return;
        }

        var cleanUrl = url.Trim();
        if (!Uri.TryCreate(cleanUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            await context.Interaction.ModifyResponseAsync(m =>
                m.Content = "Invalid invite URL. Please provide a valid HTTP or HTTPS Discord invite link (e.g. https://discord.gg/xyz).");
            return;
        }

        await configService.SetInviteUrlAsync(guildId, cleanUrl);

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = $"Successfully configured server invite URL to: {cleanUrl}";
        });
    }

    public static async Task InviteViewAsync(
        DiscordGuildConfigService configService,
        ApplicationCommandContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        if (context.Interaction.GuildId is not { } guildId)
        {
            await context.Interaction.ModifyResponseAsync(m => m.Content = "This command can only be used in a server.");
            return;
        }

        if (context.User is GuildInteractionUser guildUser &&
            !guildUser.Permissions.HasFlag(Permissions.ManageGuild) &&
            !guildUser.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.Interaction.ModifyResponseAsync(m => m.Content = "You do not have permission to manage server settings (ManageGuild).");
            return;
        }

        var config = await configService.GetConfigAsync(guildId);
        if (config == null || string.IsNullOrWhiteSpace(config.InviteUrl))
        {
            await context.Interaction.ModifyResponseAsync(m => m.Content = "No invite URL is currently configured for this server.");
            return;
        }

        var timestamp = (config.UpdatedAt ?? config.CreatedAt).ToUnixTimeSeconds();
        var embed = new EmbedProperties()
            .WithTitle("Server Invite Configuration")
            .WithColor(System.Drawing.Color.Green.ToNetCordColor())
            .AddFields(
                new EmbedFieldProperties().WithName("Invite URL").WithValue(config.InviteUrl),
                new EmbedFieldProperties().WithName("Last Updated").WithValue($"<t:{timestamp}:R>")
            );

        await context.Interaction.ModifyResponseAsync(m => m.AddEmbeds(embed));
    }

    public static async Task InviteRemoveAsync(
        DiscordGuildConfigService configService,
        ApplicationCommandContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        if (context.Interaction.GuildId is not { } guildId)
        {
            await context.Interaction.ModifyResponseAsync(m => m.Content = "This command can only be used in a server.");
            return;
        }

        if (context.User is GuildInteractionUser guildUser &&
            !guildUser.Permissions.HasFlag(Permissions.ManageGuild) &&
            !guildUser.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.Interaction.ModifyResponseAsync(m => m.Content = "You do not have permission to manage server settings (ManageGuild).");
            return;
        }

        var removed = await configService.RemoveInviteUrlAsync(guildId);
        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = removed
                ? "Successfully removed the server invite URL."
                : "No invite URL was configured for this server.";
        });
    }
}
