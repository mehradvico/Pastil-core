using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScopeCategorySlugUniquenessPerParent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug",
                table: "Categories");

            migrationBuilder.AddColumn<long>(
                name: "SlugScopeParentId",
                table: "Categories",
                type: "bigint",
                nullable: false,
                computedColumnSql: "ISNULL([ParentId], 0)",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SlugScopeParentId_Slug",
                table: "Categories",
                columns: new[] { "SlugScopeParentId", "Slug" },
                unique: true,
                filter: "[Slug] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_SlugScopeParentId_Slug",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "SlugScopeParentId",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");
        }
    }
}
