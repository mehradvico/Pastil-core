using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGroomerPhase3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InviteDate",
                table: "GroomerUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRescheduleDate",
                table: "GroomerReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OriginalDoDate",
                table: "GroomerReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RescheduleCount",
                table: "GroomerReserves",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GroomerReserveItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerReserveId = table.Column<long>(type: "bigint", nullable: false),
                    GroomerServiceId = table.Column<long>(type: "bigint", nullable: false),
                    GroomerServicePackageId = table.Column<long>(type: "bigint", nullable: false),
                    PackagePrice = table.Column<double>(type: "float", nullable: false),
                    OnlineAmount = table.Column<double>(type: "float", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProviderShare = table.Column<double>(type: "float", nullable: false),
                    SiteShare = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerReserveItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerReserveItems_GroomerReserves_GroomerReserveId",
                        column: x => x.GroomerReserveId,
                        principalTable: "GroomerReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerReserveItems_GroomerServicePackages_GroomerServicePackageId",
                        column: x => x.GroomerServicePackageId,
                        principalTable: "GroomerServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerReserveItems_GroomerServices_GroomerServiceId",
                        column: x => x.GroomerServiceId,
                        principalTable: "GroomerServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserveItems_GroomerReserveId",
                table: "GroomerReserveItems",
                column: "GroomerReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserveItems_GroomerServiceId",
                table: "GroomerReserveItems",
                column: "GroomerServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerReserveItems_GroomerServicePackageId",
                table: "GroomerReserveItems",
                column: "GroomerServicePackageId");
            // هر رزروِ قبلی (فاز ۲) یک آیتم می‌گیرد تا تسویه و گزارش آیتمی یکدست باشد.
            migrationBuilder.Sql("""
                INSERT INTO GroomerReserveItems (GroomerReserveId, GroomerServiceId, GroomerServicePackageId, PackagePrice, OnlineAmount, DurationMinutes, CommissionPercent, ProviderShare, SiteShare)
                SELECT r.Id, r.GroomerServiceId, r.GroomerServicePackageId, r.PackagePrice, r.OnlineAmount, r.DurationMinutes, r.CommissionPercent, r.ProviderShare, r.SiteShare
                FROM GroomerReserves r
                WHERE NOT EXISTS (SELECT 1 FROM GroomerReserveItems i WHERE i.GroomerReserveId = r.Id);
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'ScoreTransactionType_GroomerReserve')
                BEGIN
                    DECLARE @ScoreGroupId bigint = (SELECT TOP 1 CodeGroupId FROM Codes WHERE Label = 'ScoreTransactionType_PansionReserve');
                    IF @ScoreGroupId IS NOT NULL
                        INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
                        VALUES ('ScoreTransactionType_GroomerReserve', 'groomer-reserve', @ScoreGroupId,
                                (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @ScoreGroupId), 1, N'رزرو آرایشگاه');
                END
                """);

            migrationBuilder.Sql("""
                DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
                INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
                    (131, N'آرایشگاه تأیید شد (به مالک)', N'PushGroomerApproved', N'آرایشگاه شما تأیید شد', N'آرایشگاه {0} تأیید شد و اکنون در پاستیل نمایش داده می‌شود.', N'/companionProfile', N'groomer-approved'),
                    (132, N'آرایشگاه رد شد (به مالک)', N'PushGroomerRejected', N'آرایشگاه شما رد شد', N'آرایشگاه {0} رد شد. دلیل: {1}', N'/companionProfile', N'groomer-rejected'),
                    (133, N'تغییر زمان رزرو آرایشگاه (به کاربر)', N'PushGroomerReserveRescheduled', N'زمان رزرو شما تغییر کرد', N'زمان رزرو شما در {0} به {1} ساعت {2} تغییر کرد.', N'/reserve', N'groomer-reserve-rescheduled'),
                    (134, N'تغییر زمان رزرو آرایشگاه (به آرایشگاه)', N'PushGroomerReserveRescheduledForGroomer', N'زمان رزرو تغییر کرد', N'زمان رزرو {0} به {1} ساعت {2} تغییر کرد.', N'/companionProfile/groomerReserve', N'groomer-reserve-rescheduled-groomer'),
                    (135, N'دعوت به پرسنل آرایشگاه (به کاربر)', N'PushGroomerStaffInvite', N'دعوت به همکاری', N'شما به پرسنل {0} دعوت شده‌اید. برای پذیرش وارد اپ شوید.', N'/', N'groomer-staff-invite'),
                    (136, N'لغو رزرو آرایشگاه توسط پشتیبانی (به کاربر)', N'PushGroomerReserveCancelledByAdmin', N'رزرو شما لغو شد', N'رزرو شما در {0} توسط پشتیبانی لغو شد. دلیل: {1}', N'/reserve', N'groomer-reserve-cancelled-admin');

                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
                    THROW 51000, 'A groomer push type ID is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
                    THROW 51000, 'A groomer push label is already assigned to another ID.', 1;

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
                WHERE pattern.PushTypeId IN (131, 132, 133, 134, 135, 136)
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE notification
                FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId IN (131, 132, 133, 134, 135, 136);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (131, 132, 133, 134, 135, 136);

                DELETE FROM PushPatterns WHERE PushTypeId IN (131, 132, 133, 134, 135, 136);
                DELETE FROM PushTypes
                WHERE (Id = 131 AND Label = N'PushGroomerApproved')
                   OR (Id = 132 AND Label = N'PushGroomerRejected')
                   OR (Id = 133 AND Label = N'PushGroomerReserveRescheduled')
                   OR (Id = 134 AND Label = N'PushGroomerReserveRescheduledForGroomer')
                   OR (Id = 135 AND Label = N'PushGroomerStaffInvite')
                   OR (Id = 136 AND Label = N'PushGroomerReserveCancelledByAdmin');

                DELETE FROM Codes
                WHERE Label = 'ScoreTransactionType_GroomerReserve'
                  AND NOT EXISTS (SELECT 1 FROM ScoreTransactions WHERE TransactionTypeId = Codes.Id);
                """);

            migrationBuilder.DropTable(
                name: "GroomerReserveItems");

            migrationBuilder.DropColumn(
                name: "InviteDate",
                table: "GroomerUsers");

            migrationBuilder.DropColumn(
                name: "LastRescheduleDate",
                table: "GroomerReserves");

            migrationBuilder.DropColumn(
                name: "OriginalDoDate",
                table: "GroomerReserves");

            migrationBuilder.DropColumn(
                name: "RescheduleCount",
                table: "GroomerReserves");
        }
    }
}
