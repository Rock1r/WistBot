using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WistBot.Migrations
{
    /// <inheritdoc />
    public partial class AddedMediaTypesToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MediaType",
                table: "Items",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaType",
                table: "Items");
        }
    }
}
