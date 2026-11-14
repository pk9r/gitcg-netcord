using System.Drawing;
using System.Text;
using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Infrastructure;
using HoyolabHttpClient;
using NetCord;
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
                    .WithDescription("You don't have an active Hoyolab account. Please use the `/hoyolab-accounts` command to set one up.")
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
                    .WithDescription("You don't have an active Hoyolab account. Please use the `/hoyolab-accounts` command to set one up.")
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
                    .WithDescription("You don't have an active Hoyolab account. Please use the `/hoyolab-accounts` command to set one up.")
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
                    .WithDescription("You don't have an active Hoyolab account. Please use the `/hoyolab-accounts` command to set one up.")
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
            !guildUser.Permissions.HasFlag(Permissions.ManageChannels) &&
            !guildUser.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.Interaction.ModifyResponseAsync(m => m.Content = "You do not have permission to manage channels (ManageChannels).");
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
            !guildUser.Permissions.HasFlag(Permissions.ManageChannels) &&
            !guildUser.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.Interaction.ModifyResponseAsync(m => m.Content = "You do not have permission to manage channels (ManageChannels).");
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
            !guildUser.Permissions.HasFlag(Permissions.ManageChannels) &&
            !guildUser.Permissions.HasFlag(Permissions.Administrator))
        {
            await context.Interaction.ModifyResponseAsync(m => m.Content = "You do not have permission to manage channels (ManageChannels).");
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

    // --- Thread User Subcommands ---

    public static async Task ThreadSubscribeAsync(
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

        if (context.Channel is not GuildThread thread)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "This command can only be used inside a Thread.";
            });
            return;
        }

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
                m.Content = "This thread's parent channel is not registered as an approved daily report channel. Please ask a server admin to run `/card-snapshot channel add`.";
            });
            return;
        }

        HoyolabAccount? targetAccount = null;
        if (string.IsNullOrWhiteSpace(account))
        {
            targetAccount = await activeHoyolabAccountService.GetActiveHoyolabAccountAsync(context.User.Id);
            if (targetAccount == null)
            {
                await context.Interaction.ModifyResponseAsync(m =>
                {
                    m.Content = "You don't have an active Hoyolab account. Please use `/hoyolab-accounts` to set one up.";
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
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"Account (UID: {targetAccount.GameRoleId}) does not have Daily Snapshot enabled. Please run `/card-snapshot daily enabled:True` before subscribing to thread reports.";
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
    }

    public static async Task ThreadUnsubscribeAsync(
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

        if (context.Channel is not GuildThread thread)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "This command can only be used inside a Thread.";
            });
            return;
        }

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

        var removed = await reportService.UnsubscribeThreadAsync(thread.Id, targetAccount.Id);
        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = removed
                ? $"Successfully unsubscribed daily reports for account (UID: {targetAccount.GameRoleId}) in this thread."
                : $"Account (UID: {targetAccount.GameRoleId}) is not subscribed to daily reports in this thread.";
        });
    }

    public static async Task ThreadListAsync(
        DiscordDailyReportService reportService,
        ApplicationCommandContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral)
        );

        if (context.Channel is not GuildThread thread)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "This command can only be used inside a Thread.";
            });
            return;
        }

        var subscriptions = await reportService.GetThreadSubscriptionsAsync(thread.Id);
        if (subscriptions.Count == 0)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = "No Hoyolab accounts are currently subscribed to daily reports in this thread.";
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

        var embed = new EmbedProperties()
            .WithTitle("Thread Daily Report Subscriptions")
            .WithColor(System.Drawing.Color.Purple.ToNetCordColor())
            .WithDescription(sb.ToString());

        await context.Interaction.ModifyResponseAsync(m => m.AddEmbeds(embed));
    }
}
