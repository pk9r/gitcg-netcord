using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitcgNetCord.MainApp.Migrations
{
    /// <inheritdoc />
    public partial class MakeHoyolabAccountNavigationsOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActiveHoyolabAccounts_HoyolabAccounts_HoyolabAccountId",
                table: "ActiveHoyolabAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_CardSnapshots_HoyolabAccounts_HoyolabAccountId",
                table: "CardSnapshots");

            migrationBuilder.AlterColumn<Guid>(
                name: "HoyolabAccountId",
                table: "DiscordDailyReportThreads",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "HoyolabAccountId",
                table: "CardSnapshots",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "HoyolabAccountId",
                table: "ActiveHoyolabAccounts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveHoyolabAccounts_HoyolabAccounts_HoyolabAccountId",
                table: "ActiveHoyolabAccounts",
                column: "HoyolabAccountId",
                principalTable: "HoyolabAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CardSnapshots_HoyolabAccounts_HoyolabAccountId",
                table: "CardSnapshots",
                column: "HoyolabAccountId",
                principalTable: "HoyolabAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActiveHoyolabAccounts_HoyolabAccounts_HoyolabAccountId",
                table: "ActiveHoyolabAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_CardSnapshots_HoyolabAccounts_HoyolabAccountId",
                table: "CardSnapshots");

            migrationBuilder.AlterColumn<Guid>(
                name: "HoyolabAccountId",
                table: "DiscordDailyReportThreads",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "HoyolabAccountId",
                table: "CardSnapshots",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "HoyolabAccountId",
                table: "ActiveHoyolabAccounts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveHoyolabAccounts_HoyolabAccounts_HoyolabAccountId",
                table: "ActiveHoyolabAccounts",
                column: "HoyolabAccountId",
                principalTable: "HoyolabAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CardSnapshots_HoyolabAccounts_HoyolabAccountId",
                table: "CardSnapshots",
                column: "HoyolabAccountId",
                principalTable: "HoyolabAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
