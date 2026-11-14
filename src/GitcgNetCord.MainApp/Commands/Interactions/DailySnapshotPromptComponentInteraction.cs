using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Extensions;
using GitcgNetCord.MainApp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace GitcgNetCord.MainApp.Commands.Interactions;

public static class DailySnapshotPromptComponentInteraction
{
    public const string EnableCustomId = "daily-snapshot-enable";
    public const string SkipCustomId = "daily-snapshot-skip";

    public static string GetEnableCustomId(Guid accountId) => $"{EnableCustomId}:{accountId}";

    public static async Task EnableAsync(
        AppDbContext dbContext,
        DiscordDailyReportService reportService,
        DiscordGuildConfigService guildConfigService,
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

            account.IsDailySnapshotEnabled = true;
            await dbContext.SaveChangesAsync();

            var embed = new EmbedProperties()
                .WithTitle("Daily Snapshot Enabled!")
                .WithColor(System.Drawing.Color.Green.ToNetCordColor())
                .WithDescription($"Automatic daily snapshot capture is now **enabled** for UID `{account.GameRoleId}`.\n\n" +
                                 "Where would you like to receive your daily card statistics reports?");

            var actionRows = new List<ActionRowProperties>();
            var primaryRow = new ActionRowProperties();

            // Option 1: Direct Message
            primaryRow.Add(new ButtonProperties(
                customId: DailyReportDestinationComponentInteraction.GetDmCustomId(account.Id),
                label: "Receive in DM",
                style: ButtonStyle.Success
            ));

            // Option 2: Current Guild Thread (if in guild with approved channels)
            if (context.Interaction.GuildId is { } guildId)
            {
                var approvedChannels = await reportService.GetReportChannelsByGuildAsync(guildId);
                if (approvedChannels.Count == 1)
                {
                    var ch = approvedChannels[0];
                    primaryRow.Add(new ButtonProperties(
                        customId: DailyReportDestinationComponentInteraction.GetChannelCustomId(ch.Id, account.Id),
                        label: "Create Server Thread",
                        style: ButtonStyle.Primary
                    ));
                }
                else if (approvedChannels.Count > 1)
                {
                    for (var i = 0; i < Math.Min(approvedChannels.Count, 3); i++)
                    {
                        var ch = approvedChannels[i];
                        var chName = context.Guild?.Channels.TryGetValue(ch.Id, out var gChan) == true
                            ? $"#{gChan.Name}"
                            : $"Channel {i + 1}";
                        if (chName.Length > 20) chName = chName[..20];

                        primaryRow.Add(new ButtonProperties(
                            customId: DailyReportDestinationComponentInteraction.GetChannelCustomId(ch.Id, account.Id),
                            label: $"Thread in {chName}",
                            style: ButtonStyle.Primary
                        ));
                    }
                }
            }

            // Option 3: Skip Discord Report (Data only)
            primaryRow.Add(new ButtonProperties(
                customId: DailyReportDestinationComponentInteraction.SkipCustomId,
                label: "Skip (Data Only)",
                style: ButtonStyle.Secondary
            ));

            actionRows.Add(primaryRow);

            // Community servers with public invites (only suggest if invite exists)
            var guildsWithInvites = await guildConfigService.GetGuildsWithInvitesAsync();
            var externalInvites = guildsWithInvites
                .Where(x => context.Interaction.GuildId == null || x.GuildId != context.Interaction.GuildId)
                .Take(2)
                .ToList();

            if (externalInvites.Count > 0)
            {
                var communityRow = new ActionRowProperties();
                foreach (var ext in externalInvites)
                {
                    if (!string.IsNullOrWhiteSpace(ext.InviteUrl) &&
                        Uri.TryCreate(ext.InviteUrl.Trim(), UriKind.Absolute, out var uri) &&
                        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                    {
                        communityRow.Add(new LinkButtonProperties(
                            url: ext.InviteUrl.Trim(),
                            label: "Join Community Server"
                        ));
                    }
                }

                if (communityRow.Any())
                {
                    actionRows.Add(communityRow);
                }
            }

            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = null;
                m.Embeds = [embed];
                m.Components = actionRows;
            });
        }
        catch (RestException ex)
        {
            var detail = ex.Error != null ? $"{ex.Error.Code} ({ex.Error.Message})" : ex.Message;
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"An error occurred while enabling daily snapshot: {detail}";
                m.Components = [];
                m.Embeds = [];
            });
        }
        catch (Exception ex)
        {
            await context.Interaction.ModifyResponseAsync(m =>
            {
                m.Content = $"An error occurred while enabling daily snapshot: {ex.Message}";
                m.Components = [];
                m.Embeds = [];
            });
        }
    }

    public static async Task SkipAsync(
        ButtonInteractionContext context
    )
    {
        await context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredModifyMessage
        );

        await context.Interaction.ModifyResponseAsync(m =>
        {
            m.Content = "Daily snapshot was not enabled. You can enable it anytime using `/card-snapshot daily enabled:True`.";
            m.Components = [];
            m.Embeds = [];
        });
    }
}
