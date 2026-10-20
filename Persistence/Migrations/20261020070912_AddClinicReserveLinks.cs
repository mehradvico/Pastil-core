using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicReserveLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClinicReserveId",
                table: "Trips",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ClinicReserveId",
                table: "OnlinePrescriptions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallEndDate",
                table: "ClinicReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallStartDate",
                table: "ClinicReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AddedAfterPayment",
                table: "ClinicReserveItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "ClinicReserveItems",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PackageName",
                table: "ClinicReserveItems",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RefundAmount",
                table: "ClinicReserveItems",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundDate",
                table: "ClinicReserveItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ClinicReserveItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "StatusChangedByAdmin",
                table: "ClinicReserveItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "StatusChangedByUserId",
                table: "ClinicReserveItems",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusChangedDate",
                table: "ClinicReserveItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusReason",
                table: "ClinicReserveItems",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClinicReserveMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicReserveId = table.Column<long>(type: "bigint", nullable: false),
                    SenderUserId = table.Column<long>(type: "bigint", nullable: true),
                    ClinicReserveMessageTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ReplyToMessageId = table.Column<long>(type: "bigint", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicReserveMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicReserveMessages_ClinicReserveMessages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "ClinicReserveMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ClinicReserveMessages_ClinicReserves_ClinicReserveId",
                        column: x => x.ClinicReserveId,
                        principalTable: "ClinicReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserveMessages_Codes_ClinicReserveMessageTypeId",
                        column: x => x.ClinicReserveMessageTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserveMessages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClinicReserveMessageAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicReserveMessageId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicReserveMessageAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicReserveMessageAttachments_ClinicReserveMessages_ClinicReserveMessageId",
                        column: x => x.ClinicReserveMessageId,
                        principalTable: "ClinicReserveMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicReserveMessageReactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicReserveMessageId = table.Column<long>(type: "bigint", nullable: false),
                    ReactorUserId = table.Column<long>(type: "bigint", nullable: false),
                    Reaction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicReserveMessageReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicReserveMessageReactions_ClinicReserveMessages_ClinicReserveMessageId",
                        column: x => x.ClinicReserveMessageId,
                        principalTable: "ClinicReserveMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserveMessageReactions_Users_ReactorUserId",
                        column: x => x.ReactorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Trips_ClinicReserveId",
                table: "Trips",
                column: "ClinicReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_OnlinePrescriptions_ClinicReserveId",
                table: "OnlinePrescriptions",
                column: "ClinicReserveId",
                unique: true,
                filter: "[ClinicReserveId] IS NOT NULL AND [Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserveMessageAttachments_ClinicReserveMessageId",
                table: "ClinicReserveMessageAttachments",
                column: "ClinicReserveMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserveMessageReactions_ClinicReserveMessageId",
                table: "ClinicReserveMessageReactions",
                column: "ClinicReserveMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserveMessageReactions_ReactorUserId",
                table: "ClinicReserveMessageReactions",
                column: "ReactorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserveMessages_ClinicReserveId",
                table: "ClinicReserveMessages",
                column: "ClinicReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserveMessages_ClinicReserveMessageTypeId",
                table: "ClinicReserveMessages",
                column: "ClinicReserveMessageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserveMessages_ReplyToMessageId",
                table: "ClinicReserveMessages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserveMessages_SenderUserId",
                table: "ClinicReserveMessages",
                column: "SenderUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OnlinePrescriptions_ClinicReserves_ClinicReserveId",
                table: "OnlinePrescriptions",
                column: "ClinicReserveId",
                principalTable: "ClinicReserves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_ClinicReserves_ClinicReserveId",
                table: "Trips",
                column: "ClinicReserveId",
                principalTable: "ClinicReserves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // داده‌ی اولیه: الگوی پوش‌های ۱۴۸ تا ۱۵۳ (آیتم بسته تأیید/لغو، پیام چت، شروع تماس، دو یادآور آنلاین).
            migrationBuilder.Sql("""
                DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
                INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
                    (148, N'تأیید بسته‌ی رزرو کلینیک (به کاربر)', N'PushClinicReserveItemApproved', N'بسته‌ی شما تأیید شد', N'کلینیک بسته «{0}» رزرو شما را تأیید کرد.', N'/reserve', N'clinic-reserve-item-approved'),
                    (149, N'لغو بسته‌ی رزرو کلینیک (به کاربر)', N'PushClinicReserveItemCancelled', N'بسته‌ی رزرو لغو شد', N'بسته «{0}» رزرو شما لغو شد و {3} تومان به کیف پول شما برگشت داده شد.', N'/reserve', N'clinic-reserve-item-cancelled'),
                    (150, N'پیام جدید چت رزرو کلینیک', N'PushClinicReserveNewMessage', N'پیام جدید در چت کلینیک', N'{0}: {1}', N'/companion-chat/{2}', N'clinic-reserve-new-message'),
                    (151, N'شروع تماس درون‌برنامه‌ای توسط کلینیک', N'PushClinicInAppCallStarted', N'تماس درون‌برنامه‌ای', N'{0} با شما تماس می‌گیرد. برای پاسخ ضربه بزنید.', N'/call/{1}', N'clinic-in-app-call-started'),
                    (152, N'یادآوری ۱۰ دقیقه مانده به خدمت آنلاین کلینیک', N'PushClinicOnlineReserveReminderBefore', N'۱۰ دقیقه تا خدمت آنلاین', N'۱۰ دقیقه دیگر باید با {0} درباره‌ی «{1}» از طریق «{2}» ارتباط برقرار کنید.', N'/operator', N'clinic-online-reserve-reminder-before'),
                    (153, N'یادآوری سر زمان خدمت آنلاین کلینیک', N'PushClinicOnlineReserveReminderAtTime', N'زمان خدمت آنلاین رسید', N'همین الان باید با {0} درباره‌ی «{1}» از طریق «{2}» ارتباط برقرار کنید.', N'/operator', N'clinic-online-reserve-reminder-attime');

                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
                    THROW 51000, 'A clinic push type ID is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
                    THROW 51000, 'A clinic push label is already assigned to another ID.', 1;

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
                WHERE pattern.PushTypeId IN (148, 149, 150, 151, 152, 153)
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
                WHERE pattern.PushTypeId IN (148, 149, 150, 151, 152, 153);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (148, 149, 150, 151, 152, 153);

                DELETE FROM PushPatterns WHERE PushTypeId IN (148, 149, 150, 151, 152, 153);
                DELETE FROM PushTypes
                WHERE (Id = 148 AND Label = N'PushClinicReserveItemApproved')
                   OR (Id = 149 AND Label = N'PushClinicReserveItemCancelled')
                   OR (Id = 150 AND Label = N'PushClinicReserveNewMessage')
                   OR (Id = 151 AND Label = N'PushClinicInAppCallStarted')
                   OR (Id = 152 AND Label = N'PushClinicOnlineReserveReminderBefore')
                   OR (Id = 153 AND Label = N'PushClinicOnlineReserveReminderAtTime');
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_OnlinePrescriptions_ClinicReserves_ClinicReserveId",
                table: "OnlinePrescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_ClinicReserves_ClinicReserveId",
                table: "Trips");

            migrationBuilder.DropTable(
                name: "ClinicReserveMessageAttachments");

            migrationBuilder.DropTable(
                name: "ClinicReserveMessageReactions");

            migrationBuilder.DropTable(
                name: "ClinicReserveMessages");

            migrationBuilder.DropIndex(
                name: "IX_Trips_ClinicReserveId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_OnlinePrescriptions_ClinicReserveId",
                table: "OnlinePrescriptions");

            migrationBuilder.DropColumn(
                name: "ClinicReserveId",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "ClinicReserveId",
                table: "OnlinePrescriptions");

            migrationBuilder.DropColumn(
                name: "CallEndDate",
                table: "ClinicReserves");

            migrationBuilder.DropColumn(
                name: "CallStartDate",
                table: "ClinicReserves");

            migrationBuilder.DropColumn(
                name: "AddedAfterPayment",
                table: "ClinicReserveItems");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "ClinicReserveItems");

            migrationBuilder.DropColumn(
                name: "PackageName",
                table: "ClinicReserveItems");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "ClinicReserveItems");

            migrationBuilder.DropColumn(
                name: "RefundDate",
                table: "ClinicReserveItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ClinicReserveItems");

            migrationBuilder.DropColumn(
                name: "StatusChangedByAdmin",
                table: "ClinicReserveItems");

            migrationBuilder.DropColumn(
                name: "StatusChangedByUserId",
                table: "ClinicReserveItems");

            migrationBuilder.DropColumn(
                name: "StatusChangedDate",
                table: "ClinicReserveItems");

            migrationBuilder.DropColumn(
                name: "StatusReason",
                table: "ClinicReserveItems");
        }
    }
}
