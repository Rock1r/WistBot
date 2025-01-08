using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WistBot.Migrations
{
    /// <inheritdoc />
    public partial class WishListOwnerTypeChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WishListItems_Users_PerformerNameTelegramId",
                table: "WishListItems");

            migrationBuilder.DropIndex(
                name: "IX_WishListItems_PerformerNameTelegramId",
                table: "WishListItems");

            migrationBuilder.DropColumn(
                name: "PerformerNameTelegramId",
                table: "WishListItems");

            migrationBuilder.AddColumn<string>(
                name: "PerformerName",
                table: "WishListItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerformerName",
                table: "WishListItems");

            migrationBuilder.AddColumn<long>(
                name: "PerformerNameTelegramId",
                table: "WishListItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WishListItems_PerformerNameTelegramId",
                table: "WishListItems",
                column: "PerformerNameTelegramId");

            migrationBuilder.AddForeignKey(
                name: "FK_WishListItems_Users_PerformerNameTelegramId",
                table: "WishListItems",
                column: "PerformerNameTelegramId",
                principalTable: "Users",
                principalColumn: "TelegramId");
        }
    }
}
