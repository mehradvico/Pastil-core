using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentSlugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Roles",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Pets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Pets",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "PetBreeds",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Galleries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Features",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Brands",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Banners",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Banners",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE [Roles]
                SET [Slug] = LOWER(REPLACE(LTRIM(RTRIM([Label])), N' ', N'-'))
                WHERE NULLIF(LTRIM(RTRIM([Label])), N'') IS NOT NULL;

                UPDATE [Products]
                SET [Slug] = LOWER(REPLACE(LTRIM(RTRIM([ProductLabel])), N' ', N'-'))
                WHERE NULLIF(LTRIM(RTRIM([ProductLabel])), N'') IS NOT NULL;

                UPDATE [Pets]
                SET [Slug] = LOWER(REPLACE(LTRIM(RTRIM([Label])), N' ', N'-'))
                WHERE NULLIF(LTRIM(RTRIM([Label])), N'') IS NOT NULL;

                UPDATE [PetBreeds]
                SET [Slug] = LOWER(REPLACE(LTRIM(RTRIM([Label])), N' ', N'-'))
                WHERE NULLIF(LTRIM(RTRIM([Label])), N'') IS NOT NULL;

                UPDATE [Galleries]
                SET [Slug] = LOWER(REPLACE(LTRIM(RTRIM([Label])), N' ', N'-'))
                WHERE NULLIF(LTRIM(RTRIM([Label])), N'') IS NOT NULL;

                UPDATE [Features]
                SET [Slug] = LOWER(REPLACE(LTRIM(RTRIM([Label])), N' ', N'-'))
                WHERE NULLIF(LTRIM(RTRIM([Label])), N'') IS NOT NULL;

                UPDATE [Categories]
                SET [Slug] = LOWER(REPLACE(LTRIM(RTRIM([Label])), N' ', N'-'))
                WHERE NULLIF(LTRIM(RTRIM([Label])), N'') IS NOT NULL;

                UPDATE [Brands]
                SET [Slug] = LOWER(REPLACE(LTRIM(RTRIM([SecondName])), N' ', N'-'))
                WHERE NULLIF(LTRIM(RTRIM([SecondName])), N'') IS NOT NULL;

                UPDATE [Banners]
                SET [Slug] = LOWER(REPLACE(LTRIM(RTRIM([Label])), N' ', N'-'))
                WHERE NULLIF(LTRIM(RTRIM([Label])), N'') IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Slug",
                table: "Roles",
                column: "Slug",
                unique: true,
                filter: "[Slug] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
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
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roles_Slug",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Products_Slug",
                table: "Products");

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
                name: "IX_Categories_Slug",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Brands_Slug",
                table: "Brands");

            migrationBuilder.DropIndex(
                name: "IX_Banners_Slug",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "PetBreeds");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Galleries");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Brands");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "Banners");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Banners");
        }
    }
}
