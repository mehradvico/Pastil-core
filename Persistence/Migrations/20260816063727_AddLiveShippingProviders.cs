using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveShippingProviders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ShippingHeightCm",
                table: "Products",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingLengthCm",
                table: "Products",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingWeightGrams",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingWidthCm",
                table: "Products",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingPaymentMode",
                table: "ProductOrderStores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingProvider",
                table: "ProductOrderStores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ShippingQuoteId",
                table: "ProductOrderStores",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ShippingQuotedPrice",
                table: "ProductOrderStores",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "AllowPrepaid",
                table: "Deliveries",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowReceiverPay",
                table: "Deliveries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LivePricing",
                table: "Deliveries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ShippingProvider",
                table: "Deliveries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShippingPaymentMode",
                table: "CartStores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShippingProvider",
                table: "CartStores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ShippingQuoteId",
                table: "CartStores",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ShippingQuotedPrice",
                table: "CartStores",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "ShippingQuotes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CartStoreId = table.Column<long>(type: "bigint", nullable: true),
                    AddressId = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    PaymentMode = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ExternalQuoteId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    RequestFingerprint = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SelectedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingQuotes_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ShippingQuotes_CartStores_CartStoreId",
                        column: x => x.CartStoreId,
                        principalTable: "CartStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ShippingQuotes_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ShippingQuotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductOrderStoreId = table.Column<long>(type: "bigint", nullable: false),
                    ShippingQuoteId = table.Column<long>(type: "bigint", nullable: true),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    PaymentMode = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    QuotedPrice = table.Column<double>(type: "float", nullable: false),
                    ChargedPrice = table.Column<double>(type: "float", nullable: false),
                    ExternalShipmentId = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TrackingCode = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_ProductOrderStores_ProductOrderStoreId",
                        column: x => x.ProductOrderStoreId,
                        principalTable: "ProductOrderStores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Shipments_ShippingQuotes_ShippingQuoteId",
                        column: x => x.ShippingQuoteId,
                        principalTable: "ShippingQuotes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrderStores_ShippingQuoteId",
                table: "ProductOrderStores",
                column: "ShippingQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CartStores_ShippingQuoteId",
                table: "CartStores",
                column: "ShippingQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ProductOrderStoreId",
                table: "Shipments",
                column: "ProductOrderStoreId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_Provider_Status",
                table: "Shipments",
                columns: new[] { "Provider", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShippingQuoteId",
                table: "Shipments",
                column: "ShippingQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingQuotes_AddressId",
                table: "ShippingQuotes",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingQuotes_CartStoreId",
                table: "ShippingQuotes",
                column: "CartStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingQuotes_DeliveryId",
                table: "ShippingQuotes",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingQuotes_ExpiresAtUtc",
                table: "ShippingQuotes",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingQuotes_Token",
                table: "ShippingQuotes",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingQuotes_UserId_CartStoreId_Status",
                table: "ShippingQuotes",
                columns: new[] { "UserId", "CartStoreId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_CartStores_ShippingQuotes_ShippingQuoteId",
                table: "CartStores",
                column: "ShippingQuoteId",
                principalTable: "ShippingQuotes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductOrderStores_ShippingQuotes_ShippingQuoteId",
                table: "ProductOrderStores",
                column: "ShippingQuoteId",
                principalTable: "ShippingQuotes",
                principalColumn: "Id");

            migrationBuilder.Sql(@"
DECLARE @DeliveryTypeGroupId BIGINT =
(
    SELECT TOP (1) CodeGroupId
    FROM Codes
    WHERE Label IN ('DeliveryType_Tipax', 'DeliveryType_Post', 'DeliveryType_Courier', 'DeliveryType_InStore')
    ORDER BY CASE WHEN Label = 'DeliveryType_Tipax' THEN 0 ELSE 1 END, Id
);

IF @DeliveryTypeGroupId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'DeliveryType_AloPeyk')
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('DeliveryType_AloPeyk', 'alopeyk', @DeliveryTypeGroupId, 5, 1, N'الوپیک');

    IF NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'DeliveryType_SnappBox')
        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
        VALUES ('DeliveryType_SnappBox', 'snapp-box', @DeliveryTypeGroupId, 6, 1, N'اسنپ باکس');
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM Codes
WHERE Label IN ('DeliveryType_AloPeyk', 'DeliveryType_SnappBox')
  AND NOT EXISTS (SELECT 1 FROM Deliveries WHERE DeliveryTypeId = Codes.Id);
");
            migrationBuilder.DropForeignKey(
                name: "FK_CartStores_ShippingQuotes_ShippingQuoteId",
                table: "CartStores");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductOrderStores_ShippingQuotes_ShippingQuoteId",
                table: "ProductOrderStores");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "ShippingQuotes");

            migrationBuilder.DropIndex(
                name: "IX_ProductOrderStores_ShippingQuoteId",
                table: "ProductOrderStores");

            migrationBuilder.DropIndex(
                name: "IX_CartStores_ShippingQuoteId",
                table: "CartStores");

            migrationBuilder.DropColumn(
                name: "ShippingHeightCm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShippingLengthCm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShippingWeightGrams",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShippingWidthCm",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ShippingPaymentMode",
                table: "ProductOrderStores");

            migrationBuilder.DropColumn(
                name: "ShippingProvider",
                table: "ProductOrderStores");

            migrationBuilder.DropColumn(
                name: "ShippingQuoteId",
                table: "ProductOrderStores");

            migrationBuilder.DropColumn(
                name: "ShippingQuotedPrice",
                table: "ProductOrderStores");

            migrationBuilder.DropColumn(
                name: "AllowPrepaid",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "AllowReceiverPay",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "LivePricing",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "ShippingProvider",
                table: "Deliveries");

            migrationBuilder.DropColumn(
                name: "ShippingPaymentMode",
                table: "CartStores");

            migrationBuilder.DropColumn(
                name: "ShippingProvider",
                table: "CartStores");

            migrationBuilder.DropColumn(
                name: "ShippingQuoteId",
                table: "CartStores");

            migrationBuilder.DropColumn(
                name: "ShippingQuotedPrice",
                table: "CartStores");
        }
    }
}
