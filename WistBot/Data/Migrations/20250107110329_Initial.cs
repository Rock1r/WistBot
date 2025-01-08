using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WistBot.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhotoSizeEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileId = table.Column<string>(type: "TEXT", nullable: false),
                    FileUniqueId = table.Column<string>(type: "TEXT", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    FileSize = table.Column<int>(type: "INTEGER", nullable: false),
                    WishListItemId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotoSizeEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    TelegramId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.TelegramId);
                });

            migrationBuilder.CreateTable(
                name: "VideoEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileId = table.Column<string>(type: "TEXT", nullable: false),
                    FileUniqueId = table.Column<string>(type: "TEXT", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false),
                    ThumbId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<int>(type: "INTEGER", nullable: false),
                    WishListItemId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoEntity_PhotoSizeEntity_ThumbId",
                        column: x => x.ThumbId,
                        principalTable: "PhotoSizeEntity",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WishLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    OwnerId = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishLists_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "TelegramId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WishListItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Link = table.Column<string>(type: "TEXT", nullable: false),
                    PhotoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    VideoId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PerformerNameTelegramId = table.Column<long>(type: "INTEGER", nullable: true),
                    CurrentState = table.Column<int>(type: "INTEGER", nullable: false),
                    ListId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishListItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishListItems_PhotoSizeEntity_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "PhotoSizeEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WishListItems_Users_PerformerNameTelegramId",
                        column: x => x.PerformerNameTelegramId,
                        principalTable: "Users",
                        principalColumn: "TelegramId");
                    table.ForeignKey(
                        name: "FK_WishListItems_VideoEntity_VideoId",
                        column: x => x.VideoId,
                        principalTable: "VideoEntity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WishListItems_WishLists_ListId",
                        column: x => x.ListId,
                        principalTable: "WishLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoEntity_ThumbId",
                table: "VideoEntity",
                column: "ThumbId");

            migrationBuilder.CreateIndex(
                name: "IX_WishListItems_ListId",
                table: "WishListItems",
                column: "ListId");

            migrationBuilder.CreateIndex(
                name: "IX_WishListItems_PerformerNameTelegramId",
                table: "WishListItems",
                column: "PerformerNameTelegramId");

            migrationBuilder.CreateIndex(
                name: "IX_WishListItems_PhotoId",
                table: "WishListItems",
                column: "PhotoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WishListItems_VideoId",
                table: "WishListItems",
                column: "VideoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WishLists_OwnerId",
                table: "WishLists",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WishListItems");

            migrationBuilder.DropTable(
                name: "VideoEntity");

            migrationBuilder.DropTable(
                name: "WishLists");

            migrationBuilder.DropTable(
                name: "PhotoSizeEntity");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
