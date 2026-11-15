using System.Drawing;
using System.Text;
using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Infrastructure;
using HoyolabHttpClient;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace GitcgNetCord.MainApp.Commands.Slash;

public static class CardSnapshotSlashCommand
{
    public static async Task SaveAsync(
        HoyolabHttpClientService hoyolab,
        ActiveHoyolabAccountService activeHoyolabAccountService,
        CardSnapshotService cardSnapshotService,
        ApplicationCommandContext context,
        [SlashCommandParameter(Description = "Name for the snapshot milestone (e.g. pre-tournament)")]
        string name
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        var hoyolabAccount = await activeHoyolabAccountService
            .GetActiveHoyolabAccountAsync(context.User.Id);

        if (hoyolabAccount == null)
        {
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription("You don't have an active Hoyolab account. Please use the `/hoyolab list` command to set one up.")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        var cleanName = name.Trim();
        if (cleanName.Length is < 1 or > 50)
        {
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription("Snapshot name must be between 1 and 50 characters.")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        var authorize = new HoyolabAuthorize(
            HoyolabUserId: hoyolabAccount.HoyolabUserId,
            Token: hoyolabAccount.Token
        );

        HoyolabHttpClient.Responses.GcgCardList.Data data;
        try
        {
            data = await hoyolab.GetGcgCardListAsync(
                server: hoyolabAccount.Region,
                roleId: hoyolabAccount.GameRoleId,
                authorize: authorize
            );
        }
        catch (Exception ex)
        {
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription($"Failed to retrieve Hoyolab data: {ex.Message}")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        var cardData = data.CardList.ToDictionary(
            x => x.Id,
            x => new CardStatSummary(x.UseCount, x.Proficiency)
        );

        var characters = data.CardList
            .Where(x => x.CardType == "CardTypeCharacter")
            .ToList();

        var totalGames = characters.Sum(x => x.UseCount) / 3;
        var totalWins = characters.Sum(x => x.Proficiency) / 3;

        await cardSnapshotService.SaveSnapshotAsync(
            hoyolabAccountId: hoyolabAccount.Id,
            discordUserId: context.User.Id,
            name: cleanName,
            type: SnapshotType.UserCustom,
            totalGames: totalGames,
            totalWins: totalWins,
            cardData: cardData
        );

        await context.Interaction.ModifyResponseAsync(msg =>
        {
            msg.AddEmbeds(new EmbedProperties()
                .WithTitle("Snapshot Saved Successfully!")
                .WithColor(System.Drawing.Color.Green.ToNetCordColor())
                .AddFields(
                    new EmbedFieldProperties().WithName("Snapshot Name").WithValue(cleanName).WithInline(),
                    new EmbedFieldProperties().WithName("Total Games").WithValue(totalGames.ToString()).WithInline(),
                    new EmbedFieldProperties().WithName("Total Wins").WithValue(totalWins.ToString()).WithInline()
                )
            );
        });
    }

    public static async Task DailyAsync(
        ActiveHoyolabAccountService activeHoyolabAccountService,
        AppDbContext dbContext,
        ApplicationCommandContext context,
        [SlashCommandParameter(Description = "Enable or disable automatic daily snapshot capture.")]
        bool enabled
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        var hoyolabAccount = await activeHoyolabAccountService
            .GetActiveHoyolabAccountAsync(context.User.Id);

        if (hoyolabAccount == null)
        {
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription("You don't have an active Hoyolab account. Please use the `/hoyolab list` command to set one up.")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        hoyolabAccount.IsDailySnapshotEnabled = enabled;
        await dbContext.SaveChangesAsync();

        var statusText = enabled
            ? "Automatic daily snapshot capture at 04:00 AM (game server reset time) is now **enabled**."
            : "Automatic daily snapshot capture is now **disabled**.";

        await context.Interaction.ModifyResponseAsync(msg =>
        {
            msg.AddEmbeds(new EmbedProperties()
                .WithTitle("Daily Snapshot Configuration")
                .WithDescription(statusText)
                .WithColor(enabled ? System.Drawing.Color.Green.ToNetCordColor() : System.Drawing.Color.Gray.ToNetCordColor())
                .AddFields(
                    new EmbedFieldProperties().WithName("UID").WithValue(hoyolabAccount.GameRoleId).WithInline(),
                    new EmbedFieldProperties().WithName("Server").WithValue(hoyolabAccount.Region).WithInline()
                )
            );
        });
    }

    public static async Task ListAsync(
        ActiveHoyolabAccountService activeHoyolabAccountService,
        CardSnapshotService cardSnapshotService,
        ApplicationCommandContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        var hoyolabAccount = await activeHoyolabAccountService
            .GetActiveHoyolabAccountAsync(context.User.Id);

        if (hoyolabAccount == null)
        {
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription("You don't have an active Hoyolab account. Please use the `/hoyolab list` command to set one up.")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        var snapshots = await cardSnapshotService.GetSnapshotsForAccountAsync(hoyolabAccount.Id);

        var displaySnapshots = snapshots
            .Where(x => x.SnapshotType != SnapshotType.Daily)
            .ToList();
        var dailyCount = snapshots.Count(x => x.SnapshotType == SnapshotType.Daily);

        if (displaySnapshots.Count == 0 && dailyCount == 0)
        {
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Snapshot List")
                    .WithDescription("You don't have any saved snapshots yet. Use `/card-snapshot save [name]` to save your current milestone.")
                    .WithColor(System.Drawing.Color.Purple.ToNetCordColor())
                );
            });
            return;
        }

        var sb = new StringBuilder();
        if (displaySnapshots.Count == 0)
        {
            sb.AppendLine("You do not have any Version or Custom snapshots.");
        }
        else
        {
            foreach (var s in displaySnapshots.Take(25))
            {
                var typeTag = s.SnapshotType == SnapshotType.GameVersion ? "[Version]" : "[Custom]";
                sb.AppendLine($"• **{typeTag} {s.Name}** ({s.CreatedAt:yyyy-MM-dd HH:mm}) - {s.TotalGames} games ({s.TotalWins} wins)");
            }
        }

        var footerText = dailyCount > 0
            ? $"Total: {displaySnapshots.Count} snapshots (+ {dailyCount} daily snapshots)"
            : $"Total: {displaySnapshots.Count} snapshots";

        await context.Interaction.ModifyResponseAsync(msg =>
        {
            msg.AddEmbeds(new EmbedProperties()
                .WithTitle("Your Saved Snapshots")
                .WithDescription(sb.ToString())
                .WithColor(System.Drawing.Color.Purple.ToNetCordColor())
                .WithFooter(new EmbedFooterProperties().WithText(footerText))
            );
        });
    }

    public static async Task DeleteAsync(
        ActiveHoyolabAccountService activeHoyolabAccountService,
        CardSnapshotService cardSnapshotService,
        ApplicationCommandContext context,
        [SlashCommandParameter(
            Description = "Name of the custom snapshot to delete",
            AutocompleteProviderType = typeof(CardSnapshotAutocompleteProvider)
        )]
        string name
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        var hoyolabAccount = await activeHoyolabAccountService
            .GetActiveHoyolabAccountAsync(context.User.Id);

        if (hoyolabAccount == null)
        {
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription("You don't have an active Hoyolab account. Please use the `/hoyolab list` command to set one up.")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        var cleanName = name.Trim();
        var snapshot = await cardSnapshotService.GetSnapshotByNameAsync(hoyolabAccount.Id, cleanName);

        if (snapshot == null)
        {
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription($"No snapshot found with name '{cleanName}'.")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        if (snapshot.SnapshotType != SnapshotType.UserCustom)
        {
            var typeLabel = snapshot.SnapshotType == SnapshotType.Daily ? "Daily Snapshot" : "Game Version";
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription($"Only custom snapshots can be deleted. You cannot delete system snapshots ({typeLabel}).")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        var deleted = await cardSnapshotService.DeleteSnapshotAsync(hoyolabAccount.Id, cleanName);

        if (!deleted)
        {
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Error")
                    .WithDescription($"Failed to delete snapshot '{cleanName}'.")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        await context.Interaction.ModifyResponseAsync(msg =>
        {
            msg.AddEmbeds(new EmbedProperties()
                .WithTitle("Success")
                .WithDescription($"Successfully deleted snapshot '{cleanName}'.")
                .WithColor(System.Drawing.Color.Green.ToNetCordColor())
            );
        });
    }

    // --- Channel Admin Subcommands ---

    public static async Task ChannelAddAsync(
        DiscordDailyReportService reportService,
        ApplicationCommandContext context,
        [SlashCommandParameter(Description = "Parent channel to approve for daily thread reporting. Defaults to current channel.")]
        Channel? channel = null
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

        var targetChannelId = channel?.Id ?? context.Channel.Id;
        var (reportChannel, isNew) = await reportService.AddReportChannelAsync(targetChannelId, guildId);
        var action = isNew ? "registered" : "updated";

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = $"Successfully {action} channel <#{targetChannelId}> as an approved daily report channel.";
        });
    }

    public static async Task ChannelRemoveAsync(
        DiscordDailyReportService reportService,
        ApplicationCommandContext context,
        [SlashCommandParameter(Description = "Parent channel to unregister from daily thread reporting. Defaults to current channel.")]
        Channel? channel = null
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

        var targetChannelId = channel?.Id ?? context.Channel.Id;
        var removed = await reportService.RemoveReportChannelAsync(targetChannelId);

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = removed
                ? $"Successfully removed channel <#{targetChannelId}> from approved daily report channels."
                : $"Channel <#{targetChannelId}> was not registered.";
        });
    }

    public static async Task ChannelListAsync(
        DiscordDailyReportService reportService,
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

        var channels = await reportService.GetReportChannelsByGuildAsync(guildId);
        if (channels.Count == 0)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "No approved daily report channels are currently registered for this server.";
            });
            return;
        }

        var description = string.Join("\n", channels.Select(c => $"• <#{c.Id}> (Added: <t:{c.CreatedAt.ToUnixTimeSeconds()}:d>)"));

        var embed = new EmbedProperties()
            .WithTitle("Approved Daily Report Channels")
            .WithColor(System.Drawing.Color.Green.ToNetCordColor())
            .WithDescription(description);

        await context.Interaction.ModifyResponseAsync(m => m.AddEmbeds(embed));
    }

    // --- Report User Subcommands ---

    public static async Task ReportSubscribeAsync(
        DiscordDailyReportService reportService,
        ActiveHoyolabAccountService activeHoyolabAccountService,
        HoyolabAccountService hoyolabAccountService,
        ApplicationCommandContext context,
        [SlashCommandParameter(
            Description = "Hoyolab account UID (defaults to active account).",
            AutocompleteProviderType = typeof(HoyolabAccountAutocompleteProvider)
        )]
        string? account = null
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        HoyolabAccount? targetAccount = null;
        if (string.IsNullOrWhiteSpace(account))
        {
            targetAccount = await activeHoyolabAccountService.GetActiveHoyolabAccountAsync(context.User.Id);
            if (targetAccount == null)
            {
                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Content = "You don't have an active Hoyolab account. Please use `/hoyolab list` to set one up.";
                });
                return;
            }
        }
        else
        {
            var userAccounts = await hoyolabAccountService.GetHoyolabAccountsAsync(context.User.Id);
            targetAccount = userAccounts.FirstOrDefault(x => x.GameRoleId == account.Trim());
            if (targetAccount == null)
            {
                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Content = $"No Hoyolab account with UID '{account}' found belonging to you.";
                });
                return;
            }
        }

        if (!targetAccount.IsDailySnapshotEnabled)
        {
            var destinationLabel = context.Channel is GuildThread ? "thread reports" : "daily reports";
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"Account (UID: {targetAccount.GameRoleId}) does not have Daily Snapshot enabled. Please run `/card-snapshot daily enabled:True` before subscribing to {destinationLabel}.";
            });
            return;
        }

        if (context.Channel is GuildThread thread)
        {
            if (thread.OwnerId != context.User.Id)
            {
                var isSubscriber = await reportService.IsUserSubscriberAsync(thread.Id, context.User.Id);
                if (!isSubscriber)
                {
                    await context.Interaction.ModifyResponseAsync(m =>
                    {
                        m.Content = "Only the creator, owner, or subscriber of this thread can configure daily reports.";
                    });
                    return;
                }
            }

            var isApproved = await reportService.IsApprovedParentChannelAsync(thread.ParentId);
            if (!isApproved)
            {
                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Content = "This thread's parent channel is not registered as an approved daily report channel. Please ask a server admin to run `/server report-channel add`.";
                });
                return;
            }

            await reportService.SubscribeThreadAsync(
                threadId: thread.Id,
                guildId: thread.GuildId,
                discordUserId: context.User.Id,
                hoyolabAccountId: targetAccount.Id
            );

            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"Successfully subscribed daily statistics reports for account (UID: {targetAccount.GameRoleId}, {targetAccount.Region}) to this thread.";
            });
            return;
        }

        if (context.Channel is DMChannel || context.Interaction.GuildId == null)
        {
            ulong dmChannelId;
            try
            {
                var dm = await context.Client.Rest.GetDMChannelAsync(context.User.Id);
                dmChannelId = dm.Id;
            }
            catch (RestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Content = "Cannot send Direct Messages to you. Please enable 'Allow direct messages from server members' in your Discord privacy settings and try again.";
                });
                return;
            }

            await reportService.SubscribeDmAsync(
                dmChannelId: dmChannelId,
                discordUserId: context.User.Id,
                hoyolabAccountId: targetAccount.Id
            );

            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"✅ Successfully subscribed! Daily TCG statistics reports for UID `{targetAccount.GameRoleId}` will be delivered directly to your Direct Messages (DMs) at 04:00 AM server reset.";
            });
            return;
        }

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = "To receive daily reports in this server, run this command inside a Thread under an approved report channel. Or run this command in Direct Messages (DM) to receive private reports.";
        });
    }

    public static async Task ReportUnsubscribeAsync(
        DiscordDailyReportService reportService,
        ActiveHoyolabAccountService activeHoyolabAccountService,
        HoyolabAccountService hoyolabAccountService,
        ApplicationCommandContext context,
        [SlashCommandParameter(
            Description = "Hoyolab account UID (defaults to active account).",
            AutocompleteProviderType = typeof(HoyolabAccountAutocompleteProvider)
        )]
        string? account = null
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        HoyolabAccount? targetAccount = null;
        if (string.IsNullOrWhiteSpace(account))
        {
            targetAccount = await activeHoyolabAccountService.GetActiveHoyolabAccountAsync(context.User.Id);
        }
        else
        {
            var userAccounts = await hoyolabAccountService.GetHoyolabAccountsAsync(context.User.Id);
            targetAccount = userAccounts.FirstOrDefault(x => x.GameRoleId == account.Trim());
        }

        if (targetAccount == null)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "Could not find a matching Hoyolab account to unsubscribe.";
            });
            return;
        }

        if (context.Channel is GuildThread thread)
        {
            if (thread.OwnerId != context.User.Id)
            {
                var isSubscriber = await reportService.IsUserSubscriberAsync(thread.Id, context.User.Id);
                if (!isSubscriber)
                {
                    await context.Interaction.ModifyResponseAsync(m =>
                    {
                        m.Content = "Only the creator, owner, or subscriber of this thread can manage daily report subscriptions.";
                    });
                    return;
                }
            }

            var removed = await reportService.UnsubscribeThreadAsync(thread.Id, targetAccount.Id);
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = removed
                    ? $"Successfully unsubscribed daily reports for account (UID: {targetAccount.GameRoleId}) in this thread."
                    : $"Account (UID: {targetAccount.GameRoleId}) is not subscribed to daily reports in this thread.";
            });
            return;
        }

        if (context.Channel is DMChannel || context.Interaction.GuildId == null)
        {
            var dmChannelId = context.Channel.Id;
            try
            {
                var dm = await context.Client.Rest.GetDMChannelAsync(context.User.Id);
                dmChannelId = dm.Id;
            }
            catch
            {
                // Fallback to current context channel ID
            }

            var removed = await reportService.UnsubscribeThreadAsync(dmChannelId, targetAccount.Id);
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = removed
                    ? $"Successfully unsubscribed daily reports for account (UID: {targetAccount.GameRoleId}) from your Direct Messages."
                    : $"Account (UID: {targetAccount.GameRoleId}) is not currently subscribed to daily reports in Direct Messages.";
            });
            return;
        }

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = "This command must be executed inside a Discord Thread (for server reports) or in Direct Messages (for private reports).";
        });
    }

    public static async Task ReportListAsync(
        DiscordDailyReportService reportService,
        ApplicationCommandContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        try
        {
            var gatewayClient = context.Client as GatewayClient;

            if (context.Channel is GuildThread thread)
            {
                var subscriptions = await reportService.GetThreadSubscriptionsAsync(thread.Id);
                var threadSuggested = await reportService.GetSuggestedReportGuildsAsync(excludeGuildId: thread.GuildId);
                var (field, actionRow) = BuildServerSuggestions(threadSuggested, gatewayClient);

                if (subscriptions.Count == 0)
                {
                    var embed = new EmbedProperties()
                        .WithTitle("Thread Daily Report Subscriptions")
                        .WithColor(System.Drawing.Color.Purple.ToNetCordColor())
                        .WithDescription("No Hoyolab accounts are currently subscribed to daily reports in this thread.");

                    if (field != null)
                    {
                        embed.AddFields(field);
                    }

                    await context.Interaction.ModifyResponseAsync(m =>
                    {
                        m.Embeds = [embed];
                        if (actionRow != null)
                        {
                            m.Components = [actionRow];
                        }
                    });
                    return;
                }

                var sb = new StringBuilder();
                foreach (var sub in subscriptions)
                {
                    var acc = sub.HoyolabAccount;
                    var info = acc != null ? $"UID: **{acc.GameRoleId}** ({acc.Region})" : "Unknown Account";
                    sb.AppendLine($"• {info} - Subscribed by: <@{sub.DiscordUserId}> (<t:{sub.CreatedAt.ToUnixTimeSeconds()}:d>)");
                }

                var subsEmbed = new EmbedProperties()
                    .WithTitle("Thread Daily Report Subscriptions")
                    .WithColor(System.Drawing.Color.Purple.ToNetCordColor())
                    .WithDescription(sb.ToString());

                if (field != null)
                {
                    subsEmbed.AddFields(field);
                }

                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Embeds = [subsEmbed];
                    if (actionRow != null)
                    {
                        m.Components = [actionRow];
                    }
                });
                return;
            }

            if (context.Channel is DMChannel || context.Interaction.GuildId == null)
            {
                var userSubs = await reportService.GetSubscriptionsForUserAsync(context.User.Id);
                var suggested = await reportService.GetSuggestedReportGuildsAsync();
                var (suggestField, suggestActionRow) = BuildServerSuggestions(suggested, gatewayClient);

                if (userSubs.Count == 0)
                {
                    var embed = new EmbedProperties()
                        .WithTitle("Your Daily Report Subscriptions")
                        .WithColor(System.Drawing.Color.Purple.ToNetCordColor())
                        .WithDescription(
                            "You are not currently subscribed to daily reports in Direct Messages or server threads.\n\n" +
                            "• To receive reports in DMs, run `/card-snapshot report subscribe` here.\n" +
                            "• To receive reports in a server, run `/card-snapshot report subscribe` inside an approved report thread."
                        );

                    if (suggestField != null)
                    {
                        embed.AddFields(suggestField);
                    }

                    await context.Interaction.ModifyResponseAsync(m =>
                    {
                        m.Embeds = [embed];
                        if (suggestActionRow != null)
                        {
                            m.Components = [suggestActionRow];
                        }
                    });
                    return;
                }

                var dmSubs = userSubs.Where(s => s.GuildId == 0).ToList();
                var threadSubs = userSubs.Where(s => s.GuildId != 0).ToList();

                var sb = new StringBuilder();
                if (dmSubs.Count > 0)
                {
                    sb.AppendLine("📩 **Direct Messages**");
                    foreach (var s in dmSubs)
                    {
                        var acc = s.HoyolabAccount;
                        var info = acc != null ? $"UID: `{acc.GameRoleId}` ({acc.Region})" : "Unknown Account";
                        sb.AppendLine($"• {info} (Subscribed: <t:{s.CreatedAt.ToUnixTimeSeconds()}:d>)");
                    }
                    sb.AppendLine();
                }

                if (threadSubs.Count > 0)
                {
                    sb.AppendLine("🧵 **Server Threads**");
                    foreach (var s in threadSubs)
                    {
                        var acc = s.HoyolabAccount;
                        var info = acc != null ? $"UID: `{acc.GameRoleId}`" : "Unknown Account";
                        var guildName = gatewayClient != null && gatewayClient.Cache.Guilds.TryGetValue(s.GuildId, out var g) && !string.IsNullOrWhiteSpace(g.Name)
                            ? g.Name
                            : $"Server {s.GuildId}";
                        sb.AppendLine($"• {info} in **{guildName}**: <#{s.ThreadId}> (<t:{s.CreatedAt.ToUnixTimeSeconds()}:d>)");
                    }
                }

                var subsEmbed = new EmbedProperties()
                    .WithTitle("Your Daily Report Subscriptions")
                    .WithColor(System.Drawing.Color.Purple.ToNetCordColor())
                    .WithDescription(sb.ToString().TrimEnd());

                if (suggestField != null)
                {
                    subsEmbed.AddFields(suggestField);
                }

                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Embeds = [subsEmbed];
                    if (suggestActionRow != null)
                    {
                        m.Components = [suggestActionRow];
                    }
                    else
                    {
                        m.Components = [];
                    }
                });
                return;
            }

            var guildSuggested = await reportService.GetSuggestedReportGuildsAsync(excludeGuildId: context.Interaction.GuildId);
            var (guildSuggestField, guildSuggestActionRow) = BuildServerSuggestions(guildSuggested, gatewayClient);

            var infoEmbed = new EmbedProperties()
                .WithTitle("Daily Report Information")
                .WithColor(System.Drawing.Color.Purple.ToNetCordColor())
                .WithDescription(
                    "This command is designed to be run inside an active Discord Thread (to view thread subscriptions) or in Direct Messages (to view your personal subscriptions).\n\n" +
                    "To configure or view daily statistics reports, run `/card-snapshot report` inside a report thread or in DMs."
                );

            if (guildSuggestField != null)
            {
                infoEmbed.AddFields(guildSuggestField);
            }

            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Embeds = [infoEmbed];
                if (guildSuggestActionRow != null)
                {
                    m.Components = [guildSuggestActionRow];
                }
            });
        }
        catch (Exception ex)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"An error occurred while retrieving report subscriptions: {ex.Message}";
                m.Embeds = [];
                m.Components = [];
            });
        }
    }

    public static (EmbedFieldProperties? Field, ActionRowProperties? ActionRow) BuildServerSuggestions(
        List<SuggestedReportGuild> suggestedGuilds,
        GatewayClient? gatewayClient
    )
    {
        if (suggestedGuilds.Count == 0)
            return (null, null);

        var lines = new List<string>();
        var actionRow = new ActionRowProperties();

        foreach (var g in suggestedGuilds)
        {
            var guildName = gatewayClient != null &&
                            gatewayClient.Cache.Guilds.TryGetValue(g.GuildId, out var guild) &&
                            !string.IsNullOrWhiteSpace(guild.Name)
                ? guild.Name
                : "Community Server";

            var channelText = g.ReportChannelCount == 1 ? "channel" : "channels";
            lines.Add($"• **{guildName}** ({g.ReportChannelCount} report {channelText}) - [Join Server]({g.InviteUrl})");

            if (!string.IsNullOrWhiteSpace(g.InviteUrl) &&
                Uri.TryCreate(g.InviteUrl.Trim(), UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                var label = $"Join {guildName}";
                if (label.Length > 80)
                    label = label[..77] + "...";

                actionRow.Add(new LinkButtonProperties(
                    url: uri.ToString(),
                    label: label
                ));
            }
        }

        var field = new EmbedFieldProperties
        {
            Name = "Suggested Report Servers",
            Value = string.Join("\n", lines)
        };

        return (field, actionRow.Any() ? actionRow : null);
    }
}
