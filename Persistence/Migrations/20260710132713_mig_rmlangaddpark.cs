using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_rmlangaddpark : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrandSeoFieldLang");

            migrationBuilder.DropTable(
                name: "CategorySeoFieldLang");

            migrationBuilder.DropTable(
                name: "CityNameFieldLang");

            migrationBuilder.DropTable(
                name: "CodeGroupNameFieldLang");

            migrationBuilder.DropTable(
                name: "CodeNameFieldLang");

            migrationBuilder.DropTable(
                name: "FeatureItemNameFieldLang");

            migrationBuilder.DropTable(
                name: "FeatureNameFieldLang");

            migrationBuilder.DropTable(
                name: "FullNameFieldLangQuestion");

            migrationBuilder.DropTable(
                name: "GalleryItemFullNameFieldLangs");

            migrationBuilder.DropTable(
                name: "GallerySeoFieldLang");

            migrationBuilder.DropTable(
                name: "NameFieldLangProductFeatureValue");

            migrationBuilder.DropTable(
                name: "NameFieldLangState");

            migrationBuilder.DropTable(
                name: "PostSeoFieldLang");

            migrationBuilder.DropTable(
                name: "ProductSeoFieldLang");

            migrationBuilder.DropTable(
                name: "VarietyNameFieldLangs");

            migrationBuilder.DropTable(
                name: "FullNameFieldLangs");

            migrationBuilder.DropTable(
                name: "SeoFieldLangs");

            migrationBuilder.DropTable(
                name: "NameFieldLangs");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "PansionPictures",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Parks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: false),
                    Suggested = table.Column<bool>(type: "bit", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parks_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parks_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ParkPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParkId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkPictures_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParkPictures_ParkId",
                table: "ParkPictures",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkPictures_PictureId",
                table: "ParkPictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Parks_NeighborhoodId",
                table: "Parks",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Parks_PictureId",
                table: "Parks",
                column: "PictureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParkPictures");

            migrationBuilder.DropTable(
                name: "Parks");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "PansionPictures");

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FullNameFieldLangs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FullNameFieldLangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FullNameFieldLangs_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NameFieldLangs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NameFieldLangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NameFieldLangs_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NameFieldLangs_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SeoFieldLangs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoH1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoMinDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoPictureAlt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaticPageId = table.Column<long>(type: "bigint", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeoFieldLangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeoFieldLangs_Language_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Language",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SeoFieldLangs_StaticPages_StaticPageId",
                        column: x => x.StaticPageId,
                        principalTable: "StaticPages",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FullNameFieldLangQuestion",
                columns: table => new
                {
                    FullNameFieldLangsId = table.Column<long>(type: "bigint", nullable: false),
                    QuestionsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FullNameFieldLangQuestion", x => new { x.FullNameFieldLangsId, x.QuestionsId });
                    table.ForeignKey(
                        name: "FK_FullNameFieldLangQuestion_FullNameFieldLangs_FullNameFieldLangsId",
                        column: x => x.FullNameFieldLangsId,
                        principalTable: "FullNameFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FullNameFieldLangQuestion_Questions_QuestionsId",
                        column: x => x.QuestionsId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GalleryItemFullNameFieldLangs",
                columns: table => new
                {
                    FullNameFieldLangsId = table.Column<long>(type: "bigint", nullable: false),
                    GalleryItemsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryItemFullNameFieldLangs", x => new { x.FullNameFieldLangsId, x.GalleryItemsId });
                    table.ForeignKey(
                        name: "FK_GalleryItemFullNameFieldLangs_FullNameFieldLangs_FullNameFieldLangsId",
                        column: x => x.FullNameFieldLangsId,
                        principalTable: "FullNameFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GalleryItemFullNameFieldLangs_GalleryItems_GalleryItemsId",
                        column: x => x.GalleryItemsId,
                        principalTable: "GalleryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CityNameFieldLang",
                columns: table => new
                {
                    CitiesId = table.Column<long>(type: "bigint", nullable: false),
                    NameFieldLangsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityNameFieldLang", x => new { x.CitiesId, x.NameFieldLangsId });
                    table.ForeignKey(
                        name: "FK_CityNameFieldLang_Cities_CitiesId",
                        column: x => x.CitiesId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CityNameFieldLang_NameFieldLangs_NameFieldLangsId",
                        column: x => x.NameFieldLangsId,
                        principalTable: "NameFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CodeGroupNameFieldLang",
                columns: table => new
                {
                    CodeGroupsId = table.Column<long>(type: "bigint", nullable: false),
                    NameFieldLangsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeGroupNameFieldLang", x => new { x.CodeGroupsId, x.NameFieldLangsId });
                    table.ForeignKey(
                        name: "FK_CodeGroupNameFieldLang_CodeGroups_CodeGroupsId",
                        column: x => x.CodeGroupsId,
                        principalTable: "CodeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CodeGroupNameFieldLang_NameFieldLangs_NameFieldLangsId",
                        column: x => x.NameFieldLangsId,
                        principalTable: "NameFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CodeNameFieldLang",
                columns: table => new
                {
                    CodesId = table.Column<long>(type: "bigint", nullable: false),
                    NameFieldLangsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeNameFieldLang", x => new { x.CodesId, x.NameFieldLangsId });
                    table.ForeignKey(
                        name: "FK_CodeNameFieldLang_Codes_CodesId",
                        column: x => x.CodesId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CodeNameFieldLang_NameFieldLangs_NameFieldLangsId",
                        column: x => x.NameFieldLangsId,
                        principalTable: "NameFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeatureItemNameFieldLang",
                columns: table => new
                {
                    FeatureItemsId = table.Column<long>(type: "bigint", nullable: false),
                    NameFieldLangsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureItemNameFieldLang", x => new { x.FeatureItemsId, x.NameFieldLangsId });
                    table.ForeignKey(
                        name: "FK_FeatureItemNameFieldLang_FeatureItems_FeatureItemsId",
                        column: x => x.FeatureItemsId,
                        principalTable: "FeatureItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeatureItemNameFieldLang_NameFieldLangs_NameFieldLangsId",
                        column: x => x.NameFieldLangsId,
                        principalTable: "NameFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeatureNameFieldLang",
                columns: table => new
                {
                    FeaturesId = table.Column<long>(type: "bigint", nullable: false),
                    NameFieldLangsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureNameFieldLang", x => new { x.FeaturesId, x.NameFieldLangsId });
                    table.ForeignKey(
                        name: "FK_FeatureNameFieldLang_Features_FeaturesId",
                        column: x => x.FeaturesId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FeatureNameFieldLang_NameFieldLangs_NameFieldLangsId",
                        column: x => x.NameFieldLangsId,
                        principalTable: "NameFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NameFieldLangProductFeatureValue",
                columns: table => new
                {
                    NameFieldLangsId = table.Column<long>(type: "bigint", nullable: false),
                    ProductFeatureValuesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NameFieldLangProductFeatureValue", x => new { x.NameFieldLangsId, x.ProductFeatureValuesId });
                    table.ForeignKey(
                        name: "FK_NameFieldLangProductFeatureValue_NameFieldLangs_NameFieldLangsId",
                        column: x => x.NameFieldLangsId,
                        principalTable: "NameFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NameFieldLangProductFeatureValue_ProductFeatureValues_ProductFeatureValuesId",
                        column: x => x.ProductFeatureValuesId,
                        principalTable: "ProductFeatureValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NameFieldLangState",
                columns: table => new
                {
                    NameFieldLangsId = table.Column<long>(type: "bigint", nullable: false),
                    StatesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NameFieldLangState", x => new { x.NameFieldLangsId, x.StatesId });
                    table.ForeignKey(
                        name: "FK_NameFieldLangState_NameFieldLangs_NameFieldLangsId",
                        column: x => x.NameFieldLangsId,
                        principalTable: "NameFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NameFieldLangState_States_StatesId",
                        column: x => x.StatesId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VarietyNameFieldLangs",
                columns: table => new
                {
                    NameFieldLangsId = table.Column<long>(type: "bigint", nullable: false),
                    VarietiesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VarietyNameFieldLangs", x => new { x.NameFieldLangsId, x.VarietiesId });
                    table.ForeignKey(
                        name: "FK_VarietyNameFieldLangs_NameFieldLangs_NameFieldLangsId",
                        column: x => x.NameFieldLangsId,
                        principalTable: "NameFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VarietyNameFieldLangs_Varieties_VarietiesId",
                        column: x => x.VarietiesId,
                        principalTable: "Varieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BrandSeoFieldLang",
                columns: table => new
                {
                    SeoFieldLangsId = table.Column<long>(type: "bigint", nullable: false),
                    brandsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandSeoFieldLang", x => new { x.SeoFieldLangsId, x.brandsId });
                    table.ForeignKey(
                        name: "FK_BrandSeoFieldLang_Brands_brandsId",
                        column: x => x.brandsId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BrandSeoFieldLang_SeoFieldLangs_SeoFieldLangsId",
                        column: x => x.SeoFieldLangsId,
                        principalTable: "SeoFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategorySeoFieldLang",
                columns: table => new
                {
                    CategoriesId = table.Column<long>(type: "bigint", nullable: false),
                    SeoFieldLangsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategorySeoFieldLang", x => new { x.CategoriesId, x.SeoFieldLangsId });
                    table.ForeignKey(
                        name: "FK_CategorySeoFieldLang_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategorySeoFieldLang_SeoFieldLangs_SeoFieldLangsId",
                        column: x => x.SeoFieldLangsId,
                        principalTable: "SeoFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GallerySeoFieldLang",
                columns: table => new
                {
                    GalleriesId = table.Column<long>(type: "bigint", nullable: false),
                    SeoFieldLangsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GallerySeoFieldLang", x => new { x.GalleriesId, x.SeoFieldLangsId });
                    table.ForeignKey(
                        name: "FK_GallerySeoFieldLang_Galleries_GalleriesId",
                        column: x => x.GalleriesId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GallerySeoFieldLang_SeoFieldLangs_SeoFieldLangsId",
                        column: x => x.SeoFieldLangsId,
                        principalTable: "SeoFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostSeoFieldLang",
                columns: table => new
                {
                    PostsId = table.Column<long>(type: "bigint", nullable: false),
                    SeoFieldLangsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostSeoFieldLang", x => new { x.PostsId, x.SeoFieldLangsId });
                    table.ForeignKey(
                        name: "FK_PostSeoFieldLang_Posts_PostsId",
                        column: x => x.PostsId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostSeoFieldLang_SeoFieldLangs_SeoFieldLangsId",
                        column: x => x.SeoFieldLangsId,
                        principalTable: "SeoFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductSeoFieldLang",
                columns: table => new
                {
                    ProductsId = table.Column<long>(type: "bigint", nullable: false),
                    SeoFieldLangsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSeoFieldLang", x => new { x.ProductsId, x.SeoFieldLangsId });
                    table.ForeignKey(
                        name: "FK_ProductSeoFieldLang_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductSeoFieldLang_SeoFieldLangs_SeoFieldLangsId",
                        column: x => x.SeoFieldLangsId,
                        principalTable: "SeoFieldLangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrandSeoFieldLang_brandsId",
                table: "BrandSeoFieldLang",
                column: "brandsId");

            migrationBuilder.CreateIndex(
                name: "IX_CategorySeoFieldLang_SeoFieldLangsId",
                table: "CategorySeoFieldLang",
                column: "SeoFieldLangsId");

            migrationBuilder.CreateIndex(
                name: "IX_CityNameFieldLang_NameFieldLangsId",
                table: "CityNameFieldLang",
                column: "NameFieldLangsId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeGroupNameFieldLang_NameFieldLangsId",
                table: "CodeGroupNameFieldLang",
                column: "NameFieldLangsId");

            migrationBuilder.CreateIndex(
                name: "IX_CodeNameFieldLang_NameFieldLangsId",
                table: "CodeNameFieldLang",
                column: "NameFieldLangsId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureItemNameFieldLang_NameFieldLangsId",
                table: "FeatureItemNameFieldLang",
                column: "NameFieldLangsId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureNameFieldLang_NameFieldLangsId",
                table: "FeatureNameFieldLang",
                column: "NameFieldLangsId");

            migrationBuilder.CreateIndex(
                name: "IX_FullNameFieldLangQuestion_QuestionsId",
                table: "FullNameFieldLangQuestion",
                column: "QuestionsId");

            migrationBuilder.CreateIndex(
                name: "IX_FullNameFieldLangs_LanguageId",
                table: "FullNameFieldLangs",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryItemFullNameFieldLangs_GalleryItemsId",
                table: "GalleryItemFullNameFieldLangs",
                column: "GalleryItemsId");

            migrationBuilder.CreateIndex(
                name: "IX_GallerySeoFieldLang_SeoFieldLangsId",
                table: "GallerySeoFieldLang",
                column: "SeoFieldLangsId");

            migrationBuilder.CreateIndex(
                name: "IX_NameFieldLangProductFeatureValue_ProductFeatureValuesId",
                table: "NameFieldLangProductFeatureValue",
                column: "ProductFeatureValuesId");

            migrationBuilder.CreateIndex(
                name: "IX_NameFieldLangs_LanguageId",
                table: "NameFieldLangs",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_NameFieldLangs_NeighborhoodId",
                table: "NameFieldLangs",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_NameFieldLangState_StatesId",
                table: "NameFieldLangState",
                column: "StatesId");

            migrationBuilder.CreateIndex(
                name: "IX_PostSeoFieldLang_SeoFieldLangsId",
                table: "PostSeoFieldLang",
                column: "SeoFieldLangsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSeoFieldLang_SeoFieldLangsId",
                table: "ProductSeoFieldLang",
                column: "SeoFieldLangsId");

            migrationBuilder.CreateIndex(
                name: "IX_SeoFieldLangs_LanguageId",
                table: "SeoFieldLangs",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SeoFieldLangs_StaticPageId",
                table: "SeoFieldLangs",
                column: "StaticPageId");

            migrationBuilder.CreateIndex(
                name: "IX_VarietyNameFieldLangs_VarietiesId",
                table: "VarietyNameFieldLangs",
                column: "VarietiesId");
        }
    }
}
