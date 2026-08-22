using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductStockAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductStockAlerts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotifiedStoreId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStockAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductStockAlerts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductStockAlerts_Stores_NotifiedStoreId",
                        column: x => x.NotifiedStoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ProductStockAlerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockAlerts_ProductId",
                table: "ProductStockAlerts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockAlerts_NotifiedStoreId",
                table: "ProductStockAlerts",
                column: "NotifiedStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductStockAlerts_UserId_ProductId_Active",
                table: "ProductStockAlerts",
                columns: new[] { "UserId", "ProductId" },
                unique: true,
                filter: "[IsActive] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ProductStockAlerts");
        }
    }
}
