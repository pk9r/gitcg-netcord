using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Commands.Interactions;

public static class DailyReportDestinationComponentInteraction
{
    public const string DmCustomId = "drpt-dm";
    public const string ChannelCustomId = "drpt-ch";
    public const string SkipCustomId = "drpt-skip";

    public static string GetDmCustomId(Guid accountId) => $"{DmCustomId}:{accountId}";

    public static string GetChannelCustomId(ulong channelId, Guid accountId) =>
        $"{ChannelCustomId}:{channelId}:{accountId}";

    public static async Task SelectDmAsync(
        GatewayClient client,
        DiscordDailyReportService reportService,
        AppDbContext dbContext,
        ButtonInteractionContext context,
        Guid accountId
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredModifyMessage
        );

        try
        {
            var account = await dbContext.HoyolabAccounts
                .FirstOrDefaultAsync(x => x.Id == accountId && x.DiscordUserId == context.User.Id);

            if (account == null)
            {
                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Content = "Hoyolab account not found.";
                    m.Components = [];
                    m.Embeds = [];
                });
                return;
            }

            var dmChannel = await client.Rest.GetDMChannelAsync(context.User.Id);

            await reportService.SubscribeDmAsync(
                dmChannelId: dmChannel.Id,
                discordUserId: context.User.Id,
                hoyolabAccountId: account.Id
            );

            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"✅ Successfully subscribed! Daily TCG statistics reports for UID `{account.GameRoleId}` will be delivered directly to your Direct Messages (DMs) at 04:00 AM server reset.";
                m.Components = [];
                m.Embeds = [];
            });
        }
        catch (RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "Cannot send Direct Messages to you. Please enable 'Allow direct messages from server members' in your Discord privacy settings and try again.";
                m.Components = [];
                m.Embeds = [];
            });
        }
        catch (Exception ex)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"An error occurred while subscribing to DM reports: {ex.Message}";
                m.Components = [];
                m.Embeds = [];
            });
        }
    }

    public static async Task SelectChannelAsync(
        GatewayClient client,
        DiscordDailyReportService reportService,
        AppDbContext dbContext,
        ButtonInteractionContext context,
        ulong channelId,
        Guid accountId
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredModifyMessage
        );

        if (context.Interaction.GuildId is not { } guildId)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "This option can only be used within a server.";
                m.Components = [];
                m.Embeds = [];
            });
            return;
        }

        try
        {
            var account = await dbContext.HoyolabAccounts
                .FirstOrDefaultAsync(x => x.Id == accountId && x.DiscordUserId == context.User.Id);

            if (account == null)
            {
                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Content = "Hoyolab account not found.";
                    m.Components = [];
                    m.Embeds = [];
                });
                return;
            }

            var isApproved = await reportService.IsApprovedParentChannelAsync(channelId);
            if (!isApproved)
            {
                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Content = "The selected channel is no longer registered as an approved report channel.";
                    m.Components = [];
                    m.Embeds = [];
                });
                return;
            }

            // Check for existing subscription in this guild for this account
            var existingSub = await reportService.FindSubscriptionInGuildAsync(guildId, account.Id);
            if (existingSub != null)
            {
                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Content = $"Your account (UID: `{account.GameRoleId}`) is already subscribed to reports in this server: <#{existingSub.ThreadId}>.";
                    m.Components = [];
                    m.Embeds = [];
                });
                return;
            }

            var threadName = $"📊 TCG Daily - {context.User.Username} ({account.GameRoleId})";
            if (threadName.Length > 100) threadName = threadName[..100];

            GuildThread thread;
            try
            {
                thread = await client.Rest.CreateGuildThreadAsync(
                    channelId: channelId,
                    threadProperties: new GuildThreadProperties(threadName)
                    {
                        AutoArchiveDuration = ThreadArchiveDuration.OneWeek
                    }
                );
            }
            catch (RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Content = "The bot lacks permissions to create threads in that channel (`CreatePublicThreads` and `SendMessagesInThreads` are required). Please ask a server administrator to check bot permissions.";
                    m.Components = [];
                    m.Embeds = [];
                });
                return;
            }
            catch (Exception ex)
            {
                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Content = $"Failed to create thread: {ex.Message}";
                    m.Components = [];
                    m.Embeds = [];
                });
                return;
            }

            try
            {
                await thread.SendMessageAsync(
                    $"Welcome <@{context.User.Id}>! Daily TCG statistics reports for account `{account.GameRoleId}` ({account.Region}) will be posted here each day at 04:00 AM server reset."
                );
            }
            catch
            {
                // Non-critical if initial message fails, continue to subscribe
            }

            await reportService.SubscribeThreadAsync(
                threadId: thread.Id,
                guildId: guildId,
                discordUserId: context.User.Id,
                hoyolabAccountId: account.Id
            );

            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"🎉 Successfully created your report thread <#{thread.Id}> and subscribed account UID `{account.GameRoleId}`!";
                m.Components = [];
                m.Embeds = [];
            });
        }
        catch (Exception ex)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"An error occurred while creating the report thread: {ex.Message}";
                m.Components = [];
                m.Embeds = [];
            });
        }
    }

    public static async Task SkipDestinationAsync(
        ButtonInteractionContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredModifyMessage
        );

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = "Daily statistics capture is active. Reports will not be posted to Discord, but your data is being recorded. You can configure a destination anytime using `/card-snapshot thread subscribe`.";
            m.Components = [];
            m.Embeds = [];
        });
    }
}
