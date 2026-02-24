using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_settlementnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreId = table.Column<long>(type: "bigint", nullable: true),
                    CompanionId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserBankCardId = table.Column<long>(type: "bigint", nullable: false),
                    TrackingCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidPrice = table.Column<double>(type: "float", nullable: false),
                    ItemCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settlements_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Settlements_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Settlements_UserBankCards_UserBankCardId",
                        column: x => x.UserBankCardId,
                        principalTable: "UserBankCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettlementCompanions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionReserveId = table.Column<long>(type: "bigint", nullable: true),
                    PansionReserveId = table.Column<long>(type: "bigint", nullable: true),
                    SettlementId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementCompanions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementCompanions_CompanionReserves_CompanionReserveId",
                        column: x => x.CompanionReserveId,
                        principalTable: "CompanionReserves",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SettlementCompanions_PansionReserves_PansionReserveId",
                        column: x => x.PansionReserveId,
                        principalTable: "PansionReserves",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SettlementCompanions_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettlementStores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductOrderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SettlementId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementStores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementStores_ProductOrders_ProductOrderId",
                        column: x => x.ProductOrderId,
                        principalTable: "ProductOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SettlementStores_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCompanions_CompanionReserveId",
                table: "SettlementCompanions",
                column: "CompanionReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCompanions_PansionReserveId",
                table: "SettlementCompanions",
                column: "PansionReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCompanions_SettlementId",
                table: "SettlementCompanions",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_CompanionId",
                table: "Settlements",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_StoreId",
                table: "Settlements",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_UserBankCardId",
                table: "Settlements",
                column: "UserBankCardId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementStores_ProductOrderId",
                table: "SettlementStores",
                column: "ProductOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementStores_SettlementId",
                table: "SettlementStores",
                column: "SettlementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SettlementCompanions");

            migrationBuilder.DropTable(
                name: "SettlementStores");

            migrationBuilder.DropTable(
                name: "Settlements");
        }
    }
}
