using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Photos_AlbumId",
                table: "Photos");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsPublished_IsFeatured_DisplayOrder",
                table: "Projects",
                columns: new[] { "IsPublished", "IsFeatured", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Photos_AlbumId_IsPublished_DisplayOrder",
                table: "Photos",
                columns: new[] { "AlbumId", "IsPublished", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Photos_IsPublished_DisplayOrder",
                table: "Photos",
                columns: new[] { "IsPublished", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Photos_UploadedAtUtc",
                table: "Photos",
                column: "UploadedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_IsPublished_DisplayOrder",
                table: "Albums",
                columns: new[] { "IsPublished", "DisplayOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_IsPublished_IsFeatured_DisplayOrder",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Photos_AlbumId_IsPublished_DisplayOrder",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_IsPublished_DisplayOrder",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_UploadedAtUtc",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Albums_IsPublished_DisplayOrder",
                table: "Albums");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_AlbumId",
                table: "Photos",
                column: "AlbumId");
        }
    }
}
