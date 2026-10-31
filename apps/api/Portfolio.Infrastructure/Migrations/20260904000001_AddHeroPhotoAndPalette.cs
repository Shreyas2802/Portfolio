using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Migrations
{
    /// <summary>
    /// Adds the hero-photo flag and accent palette to Photos.
    ///
    /// These columns were first added to production by a hand-run SQL script
    /// that also inserted this exact MigrationId into __EFMigrationsHistory,
    /// so production skips this migration as already applied. It exists so
    /// that fresh databases (Docker, new environments) get the same schema,
    /// and so EF Core's model snapshot matches the entity model again.
    /// </summary>
    public partial class AddHeroPhotoAndPalette : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHeroPhoto",
                table: "Photos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AccentPalette",
                table: "Photos",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_IsHeroPhoto",
                table: "Photos",
                column: "IsHeroPhoto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Photos_IsHeroPhoto",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "AccentPalette",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "IsHeroPhoto",
                table: "Photos");
        }
    }
}
