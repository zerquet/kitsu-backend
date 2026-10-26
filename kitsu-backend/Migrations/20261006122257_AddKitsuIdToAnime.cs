using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace kitsu_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddKitsuIdToAnime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KitsuId",
                table: "Animes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Animes_KitsuId",
                table: "Animes",
                column: "KitsuId",
                unique: true,
                filter: "[KitsuId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Animes_KitsuId",
                table: "Animes");

            migrationBuilder.DropColumn(
                name: "KitsuId",
                table: "Animes");
        }
    }
}
