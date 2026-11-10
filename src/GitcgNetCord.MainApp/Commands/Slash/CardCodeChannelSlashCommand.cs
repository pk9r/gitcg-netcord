using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Enums;
using GitcgNetCord.MainApp.Extensions;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;
using Color = System.Drawing.Color;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class CardCodeChannelSlashCommand
{
    public static async Task AddAsync(
        DiscordCardCodeChannelService channelService,
        ApplicationCommandContext context,
        [
            SlashCommandParameter(
                Description = "Channel to register for card code decoding. Defaults to the current channel."
            )
        ]
        Channel? channel = null,
        [
            SlashCommandParameter(
                Description = "Deck visual decode style. Default: ImageGameBackground."
            )
        ]
        SharingDecodeType type = SharingDecodeType.ImageGameBackground
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral)
        );

        if (context.Interaction.GuildId is not { } guildId)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "This command can only be used in a server.";
            });
            return;
        }

        var targetChannelId = channel?.Id ?? context.Channel.Id;

        var result = await channelService.AddAsync(targetChannelId, guildId, type);

        var action = result.IsNew ? "registered" : "updated";
        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = $"Successfully {action} channel <#{targetChannelId}> with decode style `{type}`.";
        });
    }

    public static async Task RemoveAsync(
        DiscordCardCodeChannelService channelService,
        ApplicationCommandContext context,
        [
            SlashCommandParameter(
                Description = "Channel to unregister from card code decoding. Defaults to the current channel."
            )
        ]
        Channel? channel = null
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral)
        );

        if (context.Interaction.GuildId is not { } guildId)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "This command can only be used in a server.";
            });
            return;
        }

        var targetChannelId = channel?.Id ?? context.Channel.Id;

        var removed = await channelService.RemoveAsync(targetChannelId);

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = removed
                ? $"Successfully removed channel <#{targetChannelId}> from card code decoding."
                : $"Channel <#{targetChannelId}> was not registered.";
        });
    }

    public static async Task ListAsync(
        DiscordCardCodeChannelService channelService,
        ApplicationCommandContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(flags: MessageFlags.Ephemeral)
        );

        if (context.Interaction.GuildId is not { } guildId)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "This command can only be used in a server.";
            });
            return;
        }

        var channels = await channelService.GetByGuildAsync(guildId);

        if (channels.Count == 0)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "No card code channels are currently registered for this server.";
            });
            return;
        }

        var description = string.Join("\n", channels.Select(c => $"• <#{c.Id}> (`{c.SharingDecodeType}`)"));

        var embed = new EmbedProperties()
            .WithTitle("Card Code Channels")
            .WithColor(Color.Purple.ToNetCordColor())
            .WithDescription(description);

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.AddEmbeds(embed);
        });
    }
}
