using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitcgNetCord.MainApp.Migrations
{
    /// <inheritdoc />
    public partial class AddGuildIdToDiscordCardCodeChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GuildId",
                table: "DiscordCardCodeChannels",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_DiscordCardCodeChannels_GuildId",
                table: "DiscordCardCodeChannels",
                column: "GuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiscordCardCodeChannels_GuildId",
                table: "DiscordCardCodeChannels");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "DiscordCardCodeChannels");
        }
    }
}
