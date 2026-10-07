using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanionReservePackageItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanionReservePackageItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionReserveId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionAssistancePackageId = table.Column<long>(type: "bigint", nullable: false),
                    PackageName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PetCount = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PrePaymentPrice = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StatusChangedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatusChangedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    StatusChangedByAdmin = table.Column<bool>(type: "bit", nullable: false),
                    RefundAmount = table.Column<double>(type: "float", nullable: false),
                    RefundDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedAfterPayment = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionReservePackageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionReservePackageItems_CompanionAssistancePackages_CompanionAssistancePackageId",
                        column: x => x.CompanionAssistancePackageId,
                        principalTable: "CompanionAssistancePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReservePackageItems_CompanionReserves_CompanionReserveId",
                        column: x => x.CompanionReserveId,
                        principalTable: "CompanionReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReservePackageItems_Users_StatusChangedByUserId",
                        column: x => x.StatusChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReservePackageItems_CompanionAssistancePackageId",
                table: "CompanionReservePackageItems",
                column: "CompanionAssistancePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReservePackageItems_CompanionReserveId_CompanionAssistancePackageId",
                table: "CompanionReservePackageItems",
                columns: new[] { "CompanionReserveId", "CompanionAssistancePackageId" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReservePackageItems_StatusChangedByUserId",
                table: "CompanionReservePackageItems",
                column: "StatusChangedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanionReservePackageItems");
        }
    }
}
