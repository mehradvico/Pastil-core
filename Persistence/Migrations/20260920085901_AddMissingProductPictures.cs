using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingProductPictures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MissingProductPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissingProductId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissingProductPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissingProductPictures_MissingProducts_MissingProductId",
                        column: x => x.MissingProductId,
                        principalTable: "MissingProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MissingProductPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MissingProductPictures_MissingProductId_SortOrder",
                table: "MissingProductPictures",
                columns: new[] { "MissingProductId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MissingProductPictures_PictureId",
                table: "MissingProductPictures",
                column: "PictureId");

            // رکوردهای موجود یک تصویر دارند (ستون PictureId)؛ همان می‌شود اولین عضو فهرست تا
            // درخواست‌های قبلی هم در اپ/پنل تصویرشان را داشته باشند و قابل حذف/افزودن باشند.
            migrationBuilder.Sql(@"
INSERT INTO MissingProductPictures (MissingProductId, PictureId, SortOrder)
SELECT Id, PictureId, 0 FROM MissingProducts WHERE PictureId IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MissingProductPictures");
        }
    }
}
