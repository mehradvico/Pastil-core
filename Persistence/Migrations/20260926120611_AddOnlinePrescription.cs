using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOnlinePrescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OnlinePrescriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionReserveId = table.Column<long>(type: "bigint", nullable: true),
                    ConsultationPurchaseId = table.Column<long>(type: "bigint", nullable: true),
                    AuthorUserId = table.Column<long>(type: "bigint", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlinePrescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlinePrescriptions_CompanionReserves_CompanionReserveId",
                        column: x => x.CompanionReserveId,
                        principalTable: "CompanionReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlinePrescriptions_ConsultationPurchases_ConsultationPurchaseId",
                        column: x => x.ConsultationPurchaseId,
                        principalTable: "ConsultationPurchases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlinePrescriptions_Users_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OnlinePrescriptionPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OnlinePrescriptionId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnlinePrescriptionPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnlinePrescriptionPictures_OnlinePrescriptions_OnlinePrescriptionId",
                        column: x => x.OnlinePrescriptionId,
                        principalTable: "OnlinePrescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OnlinePrescriptionPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OnlinePrescriptionPictures_OnlinePrescriptionId",
                table: "OnlinePrescriptionPictures",
                column: "OnlinePrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlinePrescriptionPictures_PictureId",
                table: "OnlinePrescriptionPictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlinePrescriptions_AuthorUserId",
                table: "OnlinePrescriptions",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlinePrescriptions_CompanionReserveId",
                table: "OnlinePrescriptions",
                column: "CompanionReserveId",
                unique: true,
                filter: "[CompanionReserveId] IS NOT NULL AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_OnlinePrescriptions_ConsultationPurchaseId",
                table: "OnlinePrescriptions",
                column: "ConsultationPurchaseId",
                unique: true,
                filter: "[ConsultationPurchaseId] IS NOT NULL AND [Deleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OnlinePrescriptionPictures");

            migrationBuilder.DropTable(
                name: "OnlinePrescriptions");
        }
    }
}
