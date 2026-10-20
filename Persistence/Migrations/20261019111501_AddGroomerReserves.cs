using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGroomerReserves : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // داده‌ی اولیه: نوع پرداخت و نوع تخفیف رزرو آرایشگاه (Codes) + الگوی پوش‌های رزرو (PushTypes/PushPatterns/PushSettings).
            migrationBuilder.Sql("""
                DECLARE @PaymentTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'PaymentType_PansionReserve');
                IF @PaymentTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'PaymentType_GroomerReserve')
                BEGIN
                    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
                    VALUES ('PaymentType_GroomerReserve', 'groomer-reserve', @PaymentTypeGroupId,
                            (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @PaymentTypeGroupId), 1, N'پرداخت رزرو آرایشگاه');
                END

                DECLARE @RebateTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'RebateType_PansionReserve');
                IF @RebateTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'RebateType_GroomerReserve')
                BEGIN
                    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
                    VALUES ('RebateType_GroomerReserve', 'groomer-reserve', @RebateTypeGroupId,
                            (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @RebateTypeGroupId), 1, N'تخفیف رزرو آرایشگاه');
                END
                """);

            migrationBuilder.Sql("""
                DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
                INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
                    (126, N'رزرو آرایشگاه منتظر تأیید (به آرایشگاه)', N'PushGroomerReserveApprovalRequired', N'رزرو جدید منتظر تأیید', N'رزرو جدید {1} در {0} منتظر تأیید شماست.', N'/companionProfile/groomerReserve', N'groomer-reserve-approval'),
                    (127, N'رزرو آرایشگاه تأیید شد (به کاربر)', N'PushGroomerReserveApproved', N'رزرو شما تأیید شد', N'رزرو {1} در {0} توسط آرایشگاه تأیید شد.', N'/reserve', N'groomer-reserve-approved'),
                    (128, N'رزرو آرایشگاه رد شد (به کاربر)', N'PushGroomerReserveRejected', N'رزرو شما رد شد', N'رزرو شما در {0} رد شد و مبلغ به کیف پول شما برگشت. دلیل: {1}', N'/reserve', N'groomer-reserve-rejected'),
                    (129, N'آرایشگاه به رزرو پاسخ نداد، رزرو لغو شد (به کاربر)', N'PushGroomerReserveExpired', N'رزرو شما لغو شد', N'آرایشگاه {0} در مهلت مقرر به رزرو شما پاسخ نداد؛ رزرو لغو و مبلغ به کیف پول شما برگشت.', N'/reserve', N'groomer-reserve-expired'),
                    (130, N'رزرو آرایشگاه توسط کاربر لغو شد (به آرایشگاه)', N'PushGroomerReserveCancelledByBooker', N'رزرو لغو شد', N'رزرو {0} در تاریخ {1} توسط مشتری لغو شد.', N'/companionProfile/groomerReserve', N'groomer-reserve-cancelled');

                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
                    THROW 51000, 'A groomer reserve push type ID is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
                    THROW 51000, 'A groomer reserve push label is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;
                INSERT INTO PushTypes (Id, Name, Label)
                SELECT s.Id, s.Name, s.Label FROM @Types s
                WHERE NOT EXISTS (SELECT 1 FROM PushTypes t WHERE t.Id = s.Id);
                SET IDENTITY_INSERT PushTypes OFF;

                INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                SELECT s.Id, s.Title, s.Body, s.Url, NULL, s.Tag, 1 FROM @Types s
                WHERE NOT EXISTS (SELECT 1 FROM PushPatterns p WHERE p.PushTypeId = s.Id);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (126, 127, 128, 129, 130)
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
                """);

            migrationBuilder.AddColumn<long>(
                name: "GroomerReserveId",
                table: "Wallets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GroomerReserveId",
                table: "GroomerComments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReserved",
                table: "GroomerComments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GroomerReserves",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReserveCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    GroomerId = table.Column<long>(type: "bigint", nullable: false),
                    GroomerServiceId = table.Column<long>(type: "bigint", nullable: false),
                    GroomerServicePackageId = table.Column<long>(type: "bigint", nullable: false),
                    BookerId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    AssigneeUserId = table.Column<long>(type: "bigint", nullable: true),
                    GroomerTimeId = table.Column<long>(type: "bigint", nullable: true),
                    DoDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsHomeService = table.Column<bool>(type: "bit", nullable: false),
                    AddressId = table.Column<long>(type: "bigint", nullable: true),
                    PickupRequested = table.Column<bool>(type: "bit", nullable: false),
                    BookerNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PackagePrice = table.Column<double>(type: "float", nullable: false),
                    PickupFee = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    OnlineAmount = table.Column<double>(type: "float", nullable: false),
                    FromWallet = table.Column<bool>(type: "bit", nullable: false),
                    WalletPrice = table.Column<double>(type: "float", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: true),
                    RebatePrice = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProviderShare = table.Column<double>(type: "float", nullable: false),
                    SiteShare = table.Column<double>(type: "float", nullable: false),
                    Permitted = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsReserved = table.Column<bool>(type: "bit", nullable: false),
                    IsCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByBooker = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoneDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerDecision = table.Column<int>(type: "int", nullable: false),
                    OwnerDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerDecisionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerApprovalDeadline = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerReserves", x => x.Id);
                    table.CheckConstraint("CK_GroomerReserve_Dates", "[EndDate] >= [DoDate]");
                    table.CheckConstraint("CK_GroomerReserve_Prices", "[Price] >= 0 AND [OnlineAmount] >= 0 AND [PaymentPrice] >= 0 AND [WalletPrice] >= 0 AND [RebatePrice] >= 0");
                    table.ForeignKey(
                        name: "FK_GroomerReserves_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerReserves_GroomerServicePackages_GroomerServicePackageId",
                        column: x => x.GroomerServicePackageId,
                        principalTable: "GroomerServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerReserves_GroomerServices_GroomerServiceId",
                        column: x => x.GroomerServiceId,
                        principalTable: "GroomerServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerReserves_GroomerTimes_GroomerTimeId",
                        column: x => x.GroomerTimeId,
                        principalTable: "GroomerTimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerReserves_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerReserves_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerReserves_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerReserves_Users_AssigneeUserId",
                        column: x => x.AssigneeUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerReserves_Users_BookerId",
                        column: x => x.BookerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettlementGroomers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettlementId = table.Column<long>(type: "bigint", nullable: false),
                    GroomerReserveId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementGroomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementGroomers_GroomerReserves_GroomerReserveId",
                        column: x => x.GroomerReserveId,
                        principalTable: "GroomerReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementGroomers_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_GroomerReserveId",
                table: "Wallets",
                column: "GroomerReserveId",
                unique: true,
                filter: "[GroomerReserveId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerComments_GroomerReserveId",
                table: "GroomerComments",
                column: "GroomerReserveId",
                unique: true,
                filter: "[GroomerReserveId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserves_AddressId",
                table: "GroomerReserves",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserves_AssigneeUserId",
                table: "GroomerReserves",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserves_BookerId_CreateDate",
                table: "GroomerReserves",
                columns: new[] { "BookerId", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserves_GroomerId_DoDate_EndDate",
                table: "GroomerReserves",
                columns: new[] { "GroomerId", "DoDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserves_GroomerServiceId",
                table: "GroomerReserves",
                column: "GroomerServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserves_GroomerServicePackageId",
                table: "GroomerReserves",
                column: "GroomerServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserves_GroomerTimeId",
                table: "GroomerReserves",
                column: "GroomerTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserves_OwnerDecision_OwnerApprovalDeadline",
                table: "GroomerReserves",
                columns: new[] { "OwnerDecision", "OwnerApprovalDeadline" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserves_RebateId",
                table: "GroomerReserves",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserves_ReserveCode",
                table: "GroomerReserves",
                column: "ReserveCode",
                unique: true,
                filter: "[ReserveCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserves_UserPetId",
                table: "GroomerReserves",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementGroomers_GroomerReserveId",
                table: "SettlementGroomers",
                column: "GroomerReserveId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementGroomers_SettlementId",
                table: "SettlementGroomers",
                column: "SettlementId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroomerComments_GroomerReserves_GroomerReserveId",
                table: "GroomerComments",
                column: "GroomerReserveId",
                principalTable: "GroomerReserves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_GroomerReserves_GroomerReserveId",
                table: "Wallets",
                column: "GroomerReserveId",
                principalTable: "GroomerReserves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE notification
                FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId IN (126, 127, 128, 129, 130);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (126, 127, 128, 129, 130);

                DELETE FROM PushPatterns WHERE PushTypeId IN (126, 127, 128, 129, 130);
                DELETE FROM PushTypes
                WHERE (Id = 126 AND Label = N'PushGroomerReserveApprovalRequired')
                   OR (Id = 127 AND Label = N'PushGroomerReserveApproved')
                   OR (Id = 128 AND Label = N'PushGroomerReserveRejected')
                   OR (Id = 129 AND Label = N'PushGroomerReserveExpired')
                   OR (Id = 130 AND Label = N'PushGroomerReserveCancelledByBooker');

                DELETE FROM Codes
                WHERE Label = 'RebateType_GroomerReserve'
                  AND NOT EXISTS (SELECT 1 FROM Rebate WHERE TypeId = Codes.Id);
                DELETE FROM Codes
                WHERE Label = 'PaymentType_GroomerReserve'
                  AND NOT EXISTS (SELECT 1 FROM Payments WHERE TypeId = Codes.Id);
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_GroomerComments_GroomerReserves_GroomerReserveId",
                table: "GroomerComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_GroomerReserves_GroomerReserveId",
                table: "Wallets");

            migrationBuilder.DropTable(
                name: "SettlementGroomers");

            migrationBuilder.DropTable(
                name: "GroomerReserves");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_GroomerReserveId",
                table: "Wallets");

            migrationBuilder.DropIndex(
                name: "IX_GroomerComments_GroomerReserveId",
                table: "GroomerComments");

            migrationBuilder.DropColumn(
                name: "GroomerReserveId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "GroomerReserveId",
                table: "GroomerComments");

            migrationBuilder.DropColumn(
                name: "IsReserved",
                table: "GroomerComments");
        }
    }
}
