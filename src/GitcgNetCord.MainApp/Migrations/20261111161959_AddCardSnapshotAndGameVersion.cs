using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GitcgNetCord.MainApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCardSnapshotAndGameVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HoyolabAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SnapshotType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    TotalGames = table.Column<int>(type: "integer", nullable: false),
                    TotalWins = table.Column<int>(type: "integer", nullable: false),
                    CardData = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardSnapshots_HoyolabAccounts_HoyolabAccountId",
                        column: x => x.HoyolabAccountId,
                        principalTable: "HoyolabAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VersionNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReleaseDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsProcessed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameVersions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardSnapshots_DiscordUserId",
                table: "CardSnapshots",
                column: "DiscordUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CardSnapshots_HoyolabAccountId_Name",
                table: "CardSnapshots",
                columns: new[] { "HoyolabAccountId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_GameVersions_VersionNumber",
                table: "GameVersions",
                column: "VersionNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardSnapshots");

            migrationBuilder.DropTable(
                name: "GameVersions");
        }
    }
}
