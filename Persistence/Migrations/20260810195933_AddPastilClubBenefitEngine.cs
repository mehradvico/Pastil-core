using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPastilClubBenefitEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ClubDeliveryDiscount",
                table: "ProductOrders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "ClubFreeDeliveryBenefitId",
                table: "ProductOrders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ClubRewardRedemptionId",
                table: "PastilAiSubscriptions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApplicationMethod",
                table: "ClubRewardTemplates",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<double>(
                name: "ClubDeliveryDiscount",
                table: "Carts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "ClubFreeDeliveryBenefitId",
                table: "Carts",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClubCoupons",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RewardRedemptionId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false),
                    UsedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    OrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReservationId = table.Column<long>(type: "bigint", nullable: true),
                    PaymentId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubCoupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubCoupons_ClubRewardRedemptions_RewardRedemptionId",
                        column: x => x.RewardRedemptionId,
                        principalTable: "ClubRewardRedemptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubCoupons_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubCoupons_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubCoupons_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClubFreeDeliveryBenefits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RewardRedemptionId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    StoreId = table.Column<long>(type: "bigint", nullable: true),
                    CityId = table.Column<long>(type: "bigint", nullable: true),
                    MaximumDeliveryAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    RemainingUsageCount = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubFreeDeliveryBenefits", x => x.Id);
                    table.CheckConstraint("CK_ClubFreeDeliveryBenefit_RemainingUsageCount", "[RemainingUsageCount] >= 0");
                    table.ForeignKey(
                        name: "FK_ClubFreeDeliveryBenefits_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubFreeDeliveryBenefits_ClubRewardRedemptions_RewardRedemptionId",
                        column: x => x.RewardRedemptionId,
                        principalTable: "ClubRewardRedemptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubFreeDeliveryBenefits_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubFreeDeliveryBenefits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClubPromotionalWalletCredits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RewardRedemptionId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RemainingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ServiceScopeType = table.Column<int>(type: "int", nullable: false),
                    ServiceScopeId = table.Column<long>(type: "bigint", nullable: true),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubPromotionalWalletCredits", x => x.Id);
                    table.CheckConstraint("CK_ClubPromotionalWalletCredit_OriginalAmount", "[OriginalAmount] > 0");
                    table.CheckConstraint("CK_ClubPromotionalWalletCredit_RemainingAmount", "[RemainingAmount] >= 0");
                    table.ForeignKey(
                        name: "FK_ClubPromotionalWalletCredits_ClubRewardRedemptions_RewardRedemptionId",
                        column: x => x.RewardRedemptionId,
                        principalTable: "ClubRewardRedemptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubPromotionalWalletCredits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClubRewardCostTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RewardRedemptionId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    BusinessType = table.Column<int>(type: "int", nullable: false),
                    BusinessId = table.Column<long>(type: "bigint", nullable: true),
                    RewardType = table.Column<int>(type: "int", nullable: false),
                    GrossValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PastilFundedValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReservationId = table.Column<long>(type: "bigint", nullable: true),
                    PaymentId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubRewardCostTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubRewardCostTransactions_ClubRewardRedemptions_RewardRedemptionId",
                        column: x => x.RewardRedemptionId,
                        principalTable: "ClubRewardRedemptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubRewardCostTransactions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubRewardCostTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClubRewardPastilAITargets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RewardTemplateId = table.Column<long>(type: "bigint", nullable: false),
                    PlanId = table.Column<long>(type: "bigint", nullable: false),
                    TargetPlanId = table.Column<long>(type: "bigint", nullable: true),
                    FreeDays = table.Column<int>(type: "int", nullable: true),
                    IsUpgrade = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubRewardPastilAITargets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubRewardPastilAITargets_ClubRewardTemplates_RewardTemplateId",
                        column: x => x.RewardTemplateId,
                        principalTable: "ClubRewardTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubRewardPastilAITargets_PastilAiPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "PastilAiPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClubRewardPastilAITargets_PastilAiPlans_TargetPlanId",
                        column: x => x.TargetPlanId,
                        principalTable: "PastilAiPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClubPromotionalCreditUsages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromotionalCreditId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApplicationMethod = table.Column<int>(type: "int", nullable: false),
                    ReferenceKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubPromotionalCreditUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubPromotionalCreditUsages_ClubPromotionalWalletCredits_PromotionalCreditId",
                        column: x => x.PromotionalCreditId,
                        principalTable: "ClubPromotionalWalletCredits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrders_ClubFreeDeliveryBenefitId",
                table: "ProductOrders",
                column: "ClubFreeDeliveryBenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiSubscriptions_ClubRewardRedemptionId",
                table: "PastilAiSubscriptions",
                column: "ClubRewardRedemptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_ClubFreeDeliveryBenefitId",
                table: "Carts",
                column: "ClubFreeDeliveryBenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubCoupons_Code",
                table: "ClubCoupons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubCoupons_PaymentId",
                table: "ClubCoupons",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubCoupons_RebateId",
                table: "ClubCoupons",
                column: "RebateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubCoupons_RewardRedemptionId",
                table: "ClubCoupons",
                column: "RewardRedemptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubCoupons_UserId_Used_ExpiresAt",
                table: "ClubCoupons",
                columns: new[] { "UserId", "Used", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubFreeDeliveryBenefits_CityId",
                table: "ClubFreeDeliveryBenefits",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubFreeDeliveryBenefits_RewardRedemptionId",
                table: "ClubFreeDeliveryBenefits",
                column: "RewardRedemptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubFreeDeliveryBenefits_StoreId",
                table: "ClubFreeDeliveryBenefits",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubFreeDeliveryBenefits_UserId_ExpiresAt_RemainingUsageCount",
                table: "ClubFreeDeliveryBenefits",
                columns: new[] { "UserId", "ExpiresAt", "RemainingUsageCount" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubPromotionalCreditUsages_PromotionalCreditId_ReferenceKey",
                table: "ClubPromotionalCreditUsages",
                columns: new[] { "PromotionalCreditId", "ReferenceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubPromotionalCreditUsages_UserId_CreateDate",
                table: "ClubPromotionalCreditUsages",
                columns: new[] { "UserId", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubPromotionalWalletCredits_RewardRedemptionId",
                table: "ClubPromotionalWalletCredits",
                column: "RewardRedemptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubPromotionalWalletCredits_UserId_Status_ExpiresAt",
                table: "ClubPromotionalWalletCredits",
                columns: new[] { "UserId", "Status", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardCostTransactions_PaymentId",
                table: "ClubRewardCostTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardCostTransactions_RewardRedemptionId_CreateDate",
                table: "ClubRewardCostTransactions",
                columns: new[] { "RewardRedemptionId", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardCostTransactions_UserId_CreateDate",
                table: "ClubRewardCostTransactions",
                columns: new[] { "UserId", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardPastilAITargets_PlanId",
                table: "ClubRewardPastilAITargets",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardPastilAITargets_RewardTemplateId",
                table: "ClubRewardPastilAITargets",
                column: "RewardTemplateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewardPastilAITargets_TargetPlanId",
                table: "ClubRewardPastilAITargets",
                column: "TargetPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_ClubFreeDeliveryBenefits_ClubFreeDeliveryBenefitId",
                table: "Carts",
                column: "ClubFreeDeliveryBenefitId",
                principalTable: "ClubFreeDeliveryBenefits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PastilAiSubscriptions_ClubRewardRedemptions_ClubRewardRedemptionId",
                table: "PastilAiSubscriptions",
                column: "ClubRewardRedemptionId",
                principalTable: "ClubRewardRedemptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductOrders_ClubFreeDeliveryBenefits_ClubFreeDeliveryBenefitId",
                table: "ProductOrders",
                column: "ClubFreeDeliveryBenefitId",
                principalTable: "ClubFreeDeliveryBenefits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_ClubFreeDeliveryBenefits_ClubFreeDeliveryBenefitId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_PastilAiSubscriptions_ClubRewardRedemptions_ClubRewardRedemptionId",
                table: "PastilAiSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductOrders_ClubFreeDeliveryBenefits_ClubFreeDeliveryBenefitId",
                table: "ProductOrders");

            migrationBuilder.DropTable(
                name: "ClubCoupons");

            migrationBuilder.DropTable(
                name: "ClubFreeDeliveryBenefits");

            migrationBuilder.DropTable(
                name: "ClubPromotionalCreditUsages");

            migrationBuilder.DropTable(
                name: "ClubRewardCostTransactions");

            migrationBuilder.DropTable(
                name: "ClubRewardPastilAITargets");

            migrationBuilder.DropTable(
                name: "ClubPromotionalWalletCredits");

            migrationBuilder.DropIndex(
                name: "IX_ProductOrders_ClubFreeDeliveryBenefitId",
                table: "ProductOrders");

            migrationBuilder.DropIndex(
                name: "IX_PastilAiSubscriptions_ClubRewardRedemptionId",
                table: "PastilAiSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Carts_ClubFreeDeliveryBenefitId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "ClubDeliveryDiscount",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "ClubFreeDeliveryBenefitId",
                table: "ProductOrders");

            migrationBuilder.DropColumn(
                name: "ClubRewardRedemptionId",
                table: "PastilAiSubscriptions");

            migrationBuilder.DropColumn(
                name: "ApplicationMethod",
                table: "ClubRewardTemplates");

            migrationBuilder.DropColumn(
                name: "ClubDeliveryDiscount",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "ClubFreeDeliveryBenefitId",
                table: "Carts");
        }
    }
}
