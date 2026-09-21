using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ConsultationPurchaseId",
                table: "OnlineSessions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireDate",
                table: "OnlineSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ConsultationPackages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationPackages_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsultationPurchases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ConsultationPackageId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FromWallet = table.Column<bool>(type: "bit", nullable: false),
                    WalletPrice = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: true),
                    RebatePrice = table.Column<double>(type: "float", nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    PaymentId = table.Column<long>(type: "bigint", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompanionShare = table.Column<double>(type: "float", nullable: false),
                    SiteShare = table.Column<double>(type: "float", nullable: false),
                    StartDeadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgentUserId = table.Column<long>(type: "bigint", nullable: true),
                    OnlineSessionId = table.Column<long>(type: "bigint", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RefundDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultationPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsultationPurchases_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationPurchases_ConsultationPackages_ConsultationPackageId",
                        column: x => x.ConsultationPackageId,
                        principalTable: "ConsultationPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationPurchases_OnlineSessions_OnlineSessionId",
                        column: x => x.OnlineSessionId,
                        principalTable: "OnlineSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationPurchases_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationPurchases_Users_AgentUserId",
                        column: x => x.AgentUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsultationPurchases_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPackages_CompanionId_ChannelId_DurationMinutes",
                table: "ConsultationPackages",
                columns: new[] { "CompanionId", "ChannelId", "DurationMinutes" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_AgentUserId",
                table: "ConsultationPurchases",
                column: "AgentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_CompanionId_Status",
                table: "ConsultationPurchases",
                columns: new[] { "CompanionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_ConsultationPackageId",
                table: "ConsultationPurchases",
                column: "ConsultationPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_OnlineSessionId",
                table: "ConsultationPurchases",
                column: "OnlineSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_PurchaseCode",
                table: "ConsultationPurchases",
                column: "PurchaseCode",
                unique: true,
                filter: "[PurchaseCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_RebateId",
                table: "ConsultationPurchases",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_Status_ExpireDate",
                table: "ConsultationPurchases",
                columns: new[] { "Status", "ExpireDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_Status_StartDeadline",
                table: "ConsultationPurchases",
                columns: new[] { "Status", "StartDeadline" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultationPurchases_UserId_Status",
                table: "ConsultationPurchases",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultationPurchases");

            migrationBuilder.DropTable(
                name: "ConsultationPackages");

            migrationBuilder.DropColumn(
                name: "ConsultationPurchaseId",
                table: "OnlineSessions");

            migrationBuilder.DropColumn(
                name: "ExpireDate",
                table: "OnlineSessions");
        }
    }
}
