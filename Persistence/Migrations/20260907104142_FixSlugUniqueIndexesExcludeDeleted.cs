using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixSlugUniqueIndexesExcludeDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Slug",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Posts_Slug",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Pets_Slug",
                table: "Pets");

            migrationBuilder.DropIndex(
                name: "IX_PetBreeds_Slug",
                table: "PetBreeds");

            migrationBuilder.DropIndex(
                name: "IX_Galleries_Slug",
                table: "Galleries");

            migrationBuilder.DropIndex(
                name: "IX_Features_Slug",
                table: "Features");

            migrationBuilder.DropIndex(
                name: "IX_Categories_SlugScopeParentId_Slug",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Brands_Slug",
                table: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Banners_Slug",
                table: "Banners");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Slug",
                table: "Posts",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_Slug",
                table: "Pets",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PetBreeds_Slug",
                table: "PetBreeds",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_Slug",
                table: "Galleries",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Features_Slug",
                table: "Features",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SlugScopeParentId_Slug",
                table: "Categories",
                columns: new[] { "SlugScopeParentId", "Slug" },
                unique: true,
                filter: "[Slug] IS NOT NULL AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Slug",
                table: "Brands",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_Slug",
                table: "Banners",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL AND [Deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Slug",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Posts_Slug",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Pets_Slug",
                table: "Pets");

            migrationBuilder.DropIndex(
                name: "IX_PetBreeds_Slug",
                table: "PetBreeds");

            migrationBuilder.DropIndex(
                name: "IX_Galleries_Slug",
                table: "Galleries");

            migrationBuilder.DropIndex(
                name: "IX_Features_Slug",
                table: "Features");

            migrationBuilder.DropIndex(
                name: "IX_Categories_SlugScopeParentId_Slug",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Brands_Slug",
                table: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Banners_Slug",
                table: "Banners");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Slug",
                table: "Posts",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_Slug",
                table: "Pets",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PetBreeds_Slug",
                table: "PetBreeds",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_Slug",
                table: "Galleries",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Features_Slug",
                table: "Features",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_SlugScopeParentId_Slug",
                table: "Categories",
                columns: new[] { "SlugScopeParentId", "Slug" },
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Slug",
                table: "Brands",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_Slug",
                table: "Banners",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");
        }
    }
}
