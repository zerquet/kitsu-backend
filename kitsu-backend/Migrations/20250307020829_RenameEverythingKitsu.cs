using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace kitsu_backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameEverythingKitsu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteAnime_AspNetUsers_KitsuUserId",
                table: "FavoriteAnime");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryEntries_AspNetUsers_KitsuUserId",
                table: "LibraryEntries");

            migrationBuilder.RenameColumn(
                name: "KitsuUserId",
                table: "LibraryEntries",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_LibraryEntries_KitsuUserId",
                table: "LibraryEntries",
                newName: "IX_LibraryEntries_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "KitsuUserId",
                table: "FavoriteAnime",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteAnime_KitsuUserId",
                table: "FavoriteAnime",
                newName: "IX_FavoriteAnime_ApplicationUserId");


            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteAnime_AspNetUsers_ApplicationUserId",
                table: "FavoriteAnime",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryEntries_AspNetUsers_ApplicationUserId",
                table: "LibraryEntries",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteAnime_AspNetUsers_ApplicationUserId",
                table: "FavoriteAnime");

            migrationBuilder.DropForeignKey(
                name: "FK_LibraryEntries_AspNetUsers_ApplicationUserId",
                table: "LibraryEntries");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "LibraryEntries",
                newName: "KitsuUserId");

            migrationBuilder.RenameIndex(
                name: "IX_LibraryEntries_ApplicationUserId",
                table: "LibraryEntries",
                newName: "IX_LibraryEntries_KitsuUserId");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "FavoriteAnime",
                newName: "KitsuUserId");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteAnime_ApplicationUserId",
                table: "FavoriteAnime",
                newName: "IX_FavoriteAnime_KitsuUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteAnime_AspNetUsers_KitsuUserId",
                table: "FavoriteAnime",
                column: "KitsuUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryEntries_AspNetUsers_KitsuUserId",
                table: "LibraryEntries",
                column: "KitsuUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
