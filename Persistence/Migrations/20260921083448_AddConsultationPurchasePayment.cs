using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationPurchasePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ConsultationPurchaseId",
                table: "Wallets",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_ConsultationPurchaseId",
                table: "Wallets",
                column: "ConsultationPurchaseId",
                unique: true,
                filter: "[ConsultationPurchaseId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_ConsultationPurchases_ConsultationPurchaseId",
                table: "Wallets",
                column: "ConsultationPurchaseId",
                principalTable: "ConsultationPurchases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // کدهای نوع پرداخت و نوع تخفیف «خرید مشاوره آنلاین»؛ همان الگوی SeedSchoolReserve*TypeCode (بدون ID ثابت، با Label)
            migrationBuilder.Sql(@"
DECLARE @PaymentTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'PaymentType_PansionReserve');
IF @PaymentTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'PaymentType_ConsultationPurchase')
BEGIN
    DECLARE @NextPaymentPriority INT = (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @PaymentTypeGroupId);
    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
    VALUES ('PaymentType_ConsultationPurchase', 'consultation-purchase', @PaymentTypeGroupId, @NextPaymentPriority, 1, N'پرداخت خرید مشاوره آنلاین');
END

DECLARE @RebateTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'RebateType_PansionReserve');
IF @RebateTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'RebateType_ConsultationPurchase')
BEGIN
    DECLARE @NextRebatePriority INT = (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @RebateTypeGroupId);
    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
    VALUES ('RebateType_ConsultationPurchase', 'consultation-purchase', @RebateTypeGroupId, @NextRebatePriority, 1, N'تخفیف خرید مشاوره آنلاین');
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM Codes
WHERE Label = 'PaymentType_ConsultationPurchase'
  AND NOT EXISTS (SELECT 1 FROM Payments WHERE TypeId = Codes.Id);
DELETE FROM Codes
WHERE Label = 'RebateType_ConsultationPurchase'
  AND NOT EXISTS (SELECT 1 FROM Rebate WHERE TypeId = Codes.Id);
");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_ConsultationPurchases_ConsultationPurchaseId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_ConsultationPurchaseId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "ConsultationPurchaseId",
                table: "Wallets");
        }
    }
}
