using System.Drawing;
using System.Text;
using GitcgNetCord.MainApp.Commands.Autocompletes;
using GitcgNetCord.MainApp.Entities;
using GitcgNetCord.MainApp.Entities.Repositories;
using GitcgNetCord.MainApp.Extensions;
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
        [SlashCommandParameter(Description = "Tên cho bản lưu (ví dụ: truoc-giai-dau)")]
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
                    .WithTitle("Lỗi")
                    .WithDescription("Bạn chưa thiết lập tài khoản Hoyolab hoạt động. Vui lòng dùng lệnh `/hoyolab-accounts` để thêm.")
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
                    .WithTitle("Lỗi")
                    .WithDescription("Tên bản lưu phải từ 1 đến 50 ký tự.")
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
                    .WithTitle("Lỗi")
                    .WithDescription($"Không thể lấy dữ liệu từ Hoyolab: {ex.Message}")
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
                .WithTitle("Đã lưu Snapshot thành công!")
                .WithColor(System.Drawing.Color.Green.ToNetCordColor())
                .AddFields(
                    new EmbedFieldProperties().WithName("Tên bản lưu").WithValue(cleanName).WithInline(),
                    new EmbedFieldProperties().WithName("Tổng số trận").WithValue(totalGames.ToString()).WithInline(),
                    new EmbedFieldProperties().WithName("Số trận thắng").WithValue(totalWins.ToString()).WithInline()
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
                    .WithTitle("Lỗi")
                    .WithDescription("Bạn chưa thiết lập tài khoản Hoyolab hoạt động. Vui lòng dùng lệnh `/hoyolab-accounts` để thêm.")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        var snapshots = await cardSnapshotService.GetSnapshotsForAccountAsync(hoyolabAccount.Id);

        if (snapshots.Count == 0)
        {
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Danh sách Snapshot")
                    .WithDescription("Bạn chưa có bản lưu snapshot nào. Dùng `/card-snapshot save [name]` để lưu mốc hiện tại.")
                    .WithColor(System.Drawing.Color.Purple.ToNetCordColor())
                );
            });
            return;
        }

        var sb = new StringBuilder();
        foreach (var s in snapshots.Take(25))
        {
            var typeTag = s.SnapshotType == SnapshotType.GameVersion ? "[Version]" : "[Custom]";
            sb.AppendLine($"• **{typeTag} {s.Name}** ({s.CreatedAt:dd/MM/yyyy HH:mm}) - {s.TotalGames} trận ({s.TotalWins} thắng)");
        }

        await context.Interaction.ModifyResponseAsync(msg =>
        {
            msg.AddEmbeds(new EmbedProperties()
                .WithTitle("Danh sách Snapshot của bạn")
                .WithDescription(sb.ToString())
                .WithColor(System.Drawing.Color.Purple.ToNetCordColor())
                .WithFooter(new EmbedFooterProperties().WithText($"Tổng cộng: {snapshots.Count} bản lưu"))
            );
        });
    }

    public static async Task DeleteAsync(
        ActiveHoyolabAccountService activeHoyolabAccountService,
        CardSnapshotService cardSnapshotService,
        ApplicationCommandContext context,
        [SlashCommandParameter(
            Description = "Tên bản lưu cần xóa",
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
                    .WithTitle("Lỗi")
                    .WithDescription("Bạn chưa thiết lập tài khoản Hoyolab hoạt động. Vui lòng dùng lệnh `/hoyolab-accounts` để thêm.")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        var cleanName = name.Trim();
        var deleted = await cardSnapshotService.DeleteSnapshotAsync(hoyolabAccount.Id, cleanName);

        if (!deleted)
        {
            await context.Interaction.ModifyResponseAsync(msg =>
            {
                msg.AddEmbeds(new EmbedProperties()
                    .WithTitle("Lỗi")
                    .WithDescription($"Không tìm thấy bản lưu nào mang tên '{cleanName}'.")
                    .WithColor(System.Drawing.Color.Red.ToNetCordColor())
                );
            });
            return;
        }

        await context.Interaction.ModifyResponseAsync(msg =>
        {
            msg.AddEmbeds(new EmbedProperties()
                .WithTitle("Thành công")
                .WithDescription($"Đã xóa bản lưu snapshot '{cleanName}'.")
                .WithColor(System.Drawing.Color.Green.ToNetCordColor())
            );
        });
    }
}
