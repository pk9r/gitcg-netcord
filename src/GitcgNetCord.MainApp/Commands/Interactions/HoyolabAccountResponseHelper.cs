using GitcgNetCord.MainApp.Extensions;
using HoyolabHttpClient.Responses.GameRecordCard;
using NetCord;
using NetCord.Rest;

namespace GitcgNetCord.MainApp.Commands.Interactions;

public static class HoyolabAccountResponseHelper
{
    public static (EmbedProperties Embed, ActionRowProperties Components) CreateAccountAddedResponse(
        GameRecord gameRecord,
        Guid addedAccountId
    )
    {
        var embed = new EmbedProperties()
            .WithTitle("Account Added Successfully!")
            .WithColor(System.Drawing.Color.Green.ToNetCordColor())
            .WithDescription($"Linked Genshin Impact character **{gameRecord.Nickname}** (`{gameRecord.GameRoleId}`, {gameRecord.RegionName}).\n\nWould you like to enable automatic **Daily Snapshot** capture (at 04:00 AM server reset) and configure daily report delivery?")
            .AddFields(
                new EmbedFieldProperties().WithName("UID").WithValue(gameRecord.GameRoleId).WithInline(),
                new EmbedFieldProperties().WithName("Server").WithValue(gameRecord.Region).WithInline()
            );

        var components = new ActionRowProperties
        {
            new ButtonProperties(
                customId: DailySnapshotPromptComponentInteraction.GetEnableCustomId(addedAccountId),
                label: "Enable Daily Snapshot",
                style: ButtonStyle.Success
            ),
            new ButtonProperties(
                customId: DailySnapshotPromptComponentInteraction.SkipCustomId,
                label: "No, Maybe Later",
                style: ButtonStyle.Secondary
            )
        };

        return (embed, components);
    }
}
