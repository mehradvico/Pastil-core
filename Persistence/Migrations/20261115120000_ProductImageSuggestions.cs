using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    // تصویر پیشنهادیِ فروشگاه‌ها برای محصولات کاتالوگ (جدول ProductImageSuggestions) + ردیف اعلان ادمین
    // «ProductImageSuggestion.Submitted». ادمین تأیید کند، Product.PictureId همان تصویر می‌شود.
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261115120000_ProductImageSuggestions")]
    public partial class ProductImageSuggestions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductImageSuggestions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    StoreId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    ReplacedPictureId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImageSuggestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImageSuggestions_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductImageSuggestions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductImageSuggestions_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_ProductImageSuggestions_PictureId", table: "ProductImageSuggestions", column: "PictureId");
            migrationBuilder.CreateIndex(name: "IX_ProductImageSuggestions_ProductId", table: "ProductImageSuggestions", column: "ProductId");
            migrationBuilder.CreateIndex(name: "IX_ProductImageSuggestions_Status_Id", table: "ProductImageSuggestions", columns: new[] { "Status", "Id" });
            migrationBuilder.CreateIndex(name: "IX_ProductImageSuggestions_StoreId_ProductId_Status", table: "ProductImageSuggestions", columns: new[] { "StoreId", "ProductId", "Status" });

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM NoticeTypes WHERE Label = N'ProductImageSuggestion.Submitted')
                    INSERT INTO NoticeTypes (Label, Title, Name, NavigationTemplate, Importance, IsActive)
                    VALUES (N'ProductImageSuggestion.Submitted', N'تصویر پیشنهادی جدید برای محصول',
                            N'فروشگاه «{storeName}» برای محصول «{productName}» تصویر پیشنهاد داد.',
                            N'/admin/productimagesuggestion', 2, 1);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProductImageSuggestions");

            // اعلان‌های قبلاً ساخته‌شده به این نوع FK دارند؛ حذف نمی‌کنیم، فقط غیرفعالش می‌کنیم.
            migrationBuilder.Sql("UPDATE NoticeTypes SET IsActive = 0 WHERE Label = N'ProductImageSuggestion.Submitted';");
        }
    }
}
