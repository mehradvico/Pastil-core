using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsultationPurchasePets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsultationPurchasePets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultationPurchaseId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationPurchasePets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationPurchasePets_ConsultationPurchases_ConsultationPurchaseId",
                        column: x => x.ConsultationPurchaseId,
                        principalTable: "ConsultationPurchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationPurchasePets_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchasePets_ConsultationPurchaseId_UserPetId",
                table: "ConsultationPurchasePets",
                columns: new[] { "ConsultationPurchaseId", "UserPetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchasePets_UserPetId",
                table: "ConsultationPurchasePets",
                column: "UserPetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultationPurchasePets");
        }
    }
}
