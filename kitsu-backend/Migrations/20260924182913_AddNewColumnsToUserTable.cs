using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace kitsu_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumnsToUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaReactions_LibraryEntries_LibraryEntryId",
                table: "MediaReactions");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaReactions_LibraryEntries_LibraryEntryId",
                table: "MediaReactions",
                column: "LibraryEntryId",
                principalTable: "LibraryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaReactions_LibraryEntries_LibraryEntryId",
                table: "MediaReactions");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaReactions_LibraryEntries_LibraryEntryId",
                table: "MediaReactions",
                column: "LibraryEntryId",
                principalTable: "LibraryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
