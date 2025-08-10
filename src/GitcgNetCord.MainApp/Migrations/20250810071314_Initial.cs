using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitcgNetCord.MainApp.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiscordUsers",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoyolabAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    HoyolabUserId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    GameRoleId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Region = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoyolabAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoyolabAccounts_DiscordUsers_DiscordUserId",
                        column: x => x.DiscordUserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveHoyolabAccounts",
                columns: table => new
                {
                    DiscordUserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    HoyolabAccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveHoyolabAccounts", x => x.DiscordUserId);
                    table.ForeignKey(
                        name: "FK_ActiveHoyolabAccounts_DiscordUsers_DiscordUserId",
                        column: x => x.DiscordUserId,
                        principalTable: "DiscordUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActiveHoyolabAccounts_HoyolabAccounts_HoyolabAccountId",
                        column: x => x.HoyolabAccountId,
                        principalTable: "HoyolabAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveHoyolabAccounts_HoyolabAccountId",
                table: "ActiveHoyolabAccounts",
                column: "HoyolabAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_HoyolabAccounts_DiscordUserId",
                table: "HoyolabAccounts",
                column: "DiscordUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveHoyolabAccounts");

            migrationBuilder.DropTable(
                name: "HoyolabAccounts");

            migrationBuilder.DropTable(
                name: "DiscordUsers");
        }
    }
}
