using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GitcgNetCord.MainApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDeletedToHoyolabAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "HoyolabAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "HoyolabAccounts");
        }
    }
}
