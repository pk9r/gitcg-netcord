using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitcgNetCord.MainApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscordCardCodeChannelDataSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordCardCodeChannels",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordCardCodeChannels", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordCardCodeChannels");
        }
    }
}
