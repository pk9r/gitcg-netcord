using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GitcgNetCord.MainApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscordDailyReportTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordDailyReportChannels",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordDailyReportChannels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscordDailyReportThreads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ThreadId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    DiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    HoyolabAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordDailyReportThreads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscordDailyReportThreads_HoyolabAccounts_HoyolabAccountId",
                        column: x => x.HoyolabAccountId,
                        principalTable: "HoyolabAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordDailyReportChannels_GuildId",
                table: "DiscordDailyReportChannels",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordDailyReportThreads_DiscordUserId",
                table: "DiscordDailyReportThreads",
                column: "DiscordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordDailyReportThreads_GuildId",
                table: "DiscordDailyReportThreads",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordDailyReportThreads_HoyolabAccountId",
                table: "DiscordDailyReportThreads",
                column: "HoyolabAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscordDailyReportThreads_ThreadId_HoyolabAccountId",
                table: "DiscordDailyReportThreads",
                columns: new[] { "ThreadId", "HoyolabAccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordDailyReportChannels");

            migrationBuilder.DropTable(
                name: "DiscordDailyReportThreads");
        }
    }
}
