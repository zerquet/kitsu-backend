using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace kitsu_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMediaReaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LibraryEntryId",
                table: "MediaReactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MediaReactions_LibraryEntryId",
                table: "MediaReactions",
                column: "LibraryEntryId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MediaReactions_LibraryEntries_LibraryEntryId",
                table: "MediaReactions",
                column: "LibraryEntryId",
                principalTable: "LibraryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaReactions_LibraryEntries_LibraryEntryId",
                table: "MediaReactions");

            migrationBuilder.DropIndex(
                name: "IX_MediaReactions_LibraryEntryId",
                table: "MediaReactions");

            migrationBuilder.DropColumn(
                name: "LibraryEntryId",
                table: "MediaReactions");
        }
    }
}
