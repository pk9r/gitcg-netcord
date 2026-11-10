using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitcgNetCord.MainApp.Migrations
{
    /// <inheritdoc />
    public partial class AddSharingDecodeTypeToDiscordCardCodeChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SharingDecodeType",
                table: "DiscordCardCodeChannels",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SharingDecodeType",
                table: "DiscordCardCodeChannels");
        }
    }
}
