using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SecurePaymentWalletRebateAndFinancialIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ClubRewards
                SET RebateId = RebateId1
                WHERE RebateId1 IS NOT NULL;

                ;WITH UsageTotals AS
                (
                    SELECT UserId, RebateId, MIN(Id) AS KeepId, SUM(UsageCount) AS TotalUsage
                    FROM UserRebates
                    GROUP BY UserId, RebateId
                )
                UPDATE target
                SET UsageCount = totals.TotalUsage
                FROM UserRebates AS target
                INNER JOIN UsageTotals AS totals ON totals.KeepId = target.Id;

                ;WITH Duplicates AS
                (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY UserId, RebateId ORDER BY Id) AS RowNo
                    FROM UserRebates
                )
                DELETE FROM Duplicates WHERE RowNo > 1;

                UPDATE Rebate
                SET CodeValue = LEFT(LOWER(REPLACE(LTRIM(RTRIM(ISNULL(CodeValue, ''))), ' ', '')), 100);

                UPDATE Rebate
                SET CodeValue = CONCAT('legacy-', Id, '-', REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))
                WHERE CodeValue = '';

                ;WITH DuplicateCodes AS
                (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY CodeValue ORDER BY Id) AS RowNo
                    FROM Rebate
                    WHERE Deleted = 0
                )
                UPDATE rebate
                SET CodeValue = CONCAT(LEFT(rebate.CodeValue, 50), '-', rebate.Id, '-', REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))
                FROM Rebate AS rebate
                INNER JOIN DuplicateCodes AS duplicate ON duplicate.Id = rebate.Id
                WHERE duplicate.RowNo > 1;

                ;WITH DuplicateReferences AS
                (
                    SELECT Id, ROW_NUMBER() OVER (PARTITION BY RefNumber ORDER BY Id) AS RowNo
                    FROM Payments
                    WHERE RefNumber IS NOT NULL AND IsOnline = 0 AND IsSuccess = 1
                )
                UPDATE payment
                SET RefNumber = CONCAT('legacy-manual-', payment.Id, '-', REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))
                FROM Payments AS payment
                INNER JOIN DuplicateReferences AS duplicate ON duplicate.Id = payment.Id
                WHERE duplicate.RowNo > 1;

                UPDATE Payments SET Amount = 0 WHERE Amount < 0;
                UPDATE Wallets SET Amount = ABS(Amount) WHERE Amount < 0;
                UPDATE Stores SET CommissionPercent = CASE WHEN CommissionPercent < 0 THEN 0 WHEN CommissionPercent > 100 THEN 100 ELSE CommissionPercent END;
                UPDATE Pansions SET DailyCommissionPercent = CASE WHEN DailyCommissionPercent < 0 THEN 0 WHEN DailyCommissionPercent > 100 THEN 100 ELSE DailyCommissionPercent END;
                UPDATE Pansions SET HourlyCommissionPercent = CASE WHEN HourlyCommissionPercent < 0 THEN 0 WHEN HourlyCommissionPercent > 100 THEN 100 ELSE HourlyCommissionPercent END;
                UPDATE CompanionAssistances SET CommissionPercent = CASE WHEN CommissionPercent < 0 THEN 0 WHEN CommissionPercent > 100 THEN 100 ELSE CommissionPercent END;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_ClubRewards_Rebate_RebateId1",
                table: "ClubRewards");

            migrationBuilder.DropIndex(
                name: "IX_UserRebates_UserId",
                table: "UserRebates");

            migrationBuilder.DropIndex(
                name: "IX_Payments_UserId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_ClubRewards_RebateId1",
                table: "ClubRewards");

            migrationBuilder.DropColumn(
                name: "RebateId1",
                table: "ClubRewards");

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionPercent",
                table: "Stores",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "CodeValue",
                table: "Rebate",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RefNumber",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CallBackTypeLabel",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CallBackId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppliedDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ApprovedByUserId",
                table: "Payments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedIp",
                table: "Payments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CallbackToken",
                table: "Payments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GrossAmount",
                table: "Payments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RebateAmount",
                table: "Payments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<long>(
                name: "RebateId",
                table: "Payments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "WalletAmount",
                table: "Payments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.Sql(@"
                UPDATE Payments
                SET GrossAmount = Amount,
                    AppliedDate = CASE WHEN IsSuccess = 1 THEN CreateDate ELSE NULL END,
                    GatewayStatus = CASE
                        WHEN IsSuccess = 1 AND GatewayStatus IS NULL THEN 'LEGACY_APPLIED'
                        WHEN IsSuccess IS NULL THEN 'LEGACY_EXPIRED'
                        ELSE GatewayStatus
                    END,
                    IsSuccess = CASE WHEN IsSuccess IS NULL THEN 0 ELSE IsSuccess END;
            ");

            migrationBuilder.AlterColumn<decimal>(
                name: "HourlyCommissionPercent",
                table: "Pansions",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DailyCommissionPercent",
                table: "Pansions",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionPercent",
                table: "CompanionAssistances",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Wallet_Amount",
                table: "Wallets",
                sql: "[Amount] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserRebates_UserId_RebateId",
                table: "UserRebates",
                columns: new[] { "UserId", "RebateId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserRebate_UsageCount",
                table: "UserRebates",
                sql: "[UsageCount] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Rebate_CodeValue",
                table: "Rebate",
                column: "CodeValue",
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CallbackToken",
                table: "Payments",
                column: "CallbackToken",
                unique: true,
                filter: "[CallbackToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RebateId_UserId_CreateDate",
                table: "Payments",
                columns: new[] { "RebateId", "UserId", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_RefNumber",
                table: "Payments",
                column: "RefNumber",
                unique: true,
                filter: "[RefNumber] IS NOT NULL AND [IsOnline] = 0 AND [IsSuccess] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId_CallBackTypeLabel_CallBackId",
                table: "Payments",
                columns: new[] { "UserId", "CallBackTypeLabel", "CallBackId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payment_Amount",
                table: "Payments",
                sql: "[Amount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payment_GrossAmount",
                table: "Payments",
                sql: "[GrossAmount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payment_RebateAmount",
                table: "Payments",
                sql: "[RebateAmount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Payment_WalletAmount",
                table: "Payments",
                sql: "[WalletAmount] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewards_RebateId",
                table: "ClubRewards",
                column: "RebateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClubRewards_Rebate_RebateId",
                table: "ClubRewards",
                column: "RebateId",
                principalTable: "Rebate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Rebate_RebateId",
                table: "Payments",
                column: "RebateId",
                principalTable: "Rebate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClubRewards_Rebate_RebateId",
                table: "ClubRewards");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Rebate_RebateId",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Wallet_Amount",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_UserRebates_UserId_RebateId",
                table: "UserRebates");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserRebate_UsageCount",
                table: "UserRebates");

            migrationBuilder.DropIndex(
                name: "IX_Rebate_CodeValue",
                table: "Rebate");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CallbackToken",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_RebateId_UserId_CreateDate",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_RefNumber",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_UserId_CallBackTypeLabel_CallBackId",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payment_Amount",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payment_GrossAmount",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payment_RebateAmount",
                table: "Payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Payment_WalletAmount",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_ClubRewards_RebateId",
                table: "ClubRewards");

            migrationBuilder.DropColumn(
                name: "AppliedDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ApprovedIp",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CallbackToken",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "GrossAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RebateAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RebateId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "WalletAmount",
                table: "Payments");

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionPercent",
                table: "Stores",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "CodeValue",
                table: "Rebate",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "RefNumber",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CallBackTypeLabel",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CallBackId",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "HourlyCommissionPercent",
                table: "Pansions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DailyCommissionPercent",
                table: "Pansions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "CommissionPercent",
                table: "CompanionAssistances",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AddColumn<long>(
                name: "RebateId1",
                table: "ClubRewards",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRebates_UserId",
                table: "UserRebates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewards_RebateId1",
                table: "ClubRewards",
                column: "RebateId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ClubRewards_Rebate_RebateId1",
                table: "ClubRewards",
                column: "RebateId1",
                principalTable: "Rebate",
                principalColumn: "Id");
        }
    }
}
