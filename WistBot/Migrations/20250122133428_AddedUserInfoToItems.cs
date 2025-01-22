using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WistBot.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserInfoToItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OwnerId",
                table: "Items",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Items_OwnerId",
                table: "Items",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Users_OwnerId",
                table: "Items",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "TelegramId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Users_OwnerId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_OwnerId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Items");
        }
    }
}
