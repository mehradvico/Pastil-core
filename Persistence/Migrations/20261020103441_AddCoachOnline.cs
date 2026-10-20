using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachOnline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CallEndDate",
                table: "CoachReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CallStartDate",
                table: "CoachReserves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CoachReserveMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachReserveId = table.Column<long>(type: "bigint", nullable: false),
                    SenderUserId = table.Column<long>(type: "bigint", nullable: true),
                    CoachReserveMessageTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ReplyToMessageId = table.Column<long>(type: "bigint", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachReserveMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachReserveMessages_CoachReserveMessages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "CoachReserveMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CoachReserveMessages_CoachReserves_CoachReserveId",
                        column: x => x.CoachReserveId,
                        principalTable: "CoachReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserveMessages_Codes_CoachReserveMessageTypeId",
                        column: x => x.CoachReserveMessageTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserveMessages_Users_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CoachReserveMessageAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachReserveMessageId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_CoachReserveMessageAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachReserveMessageAttachments_CoachReserveMessages_CoachReserveMessageId",
                        column: x => x.CoachReserveMessageId,
                        principalTable: "CoachReserveMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachReserveMessageReactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachReserveMessageId = table.Column<long>(type: "bigint", nullable: false),
                    ReactorUserId = table.Column<long>(type: "bigint", nullable: false),
                    Reaction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachReserveMessageReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachReserveMessageReactions_CoachReserveMessages_CoachReserveMessageId",
                        column: x => x.CoachReserveMessageId,
                        principalTable: "CoachReserveMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserveMessageReactions_Users_ReactorUserId",
                        column: x => x.ReactorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserveMessageAttachments_CoachReserveMessageId",
                table: "CoachReserveMessageAttachments",
                column: "CoachReserveMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserveMessageReactions_CoachReserveMessageId",
                table: "CoachReserveMessageReactions",
                column: "CoachReserveMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserveMessageReactions_ReactorUserId",
                table: "CoachReserveMessageReactions",
                column: "ReactorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserveMessages_CoachReserveId",
                table: "CoachReserveMessages",
                column: "CoachReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserveMessages_CoachReserveMessageTypeId",
                table: "CoachReserveMessages",
                column: "CoachReserveMessageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserveMessages_ReplyToMessageId",
                table: "CoachReserveMessages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserveMessages_SenderUserId",
                table: "CoachReserveMessages",
                column: "SenderUserId");

            // داده‌ی اولیه: الگوی پوش‌های ۱۶۵ تا ۱۶۸ (پیام چت، شروع تماس، دو یادآور آنلاین).
            migrationBuilder.Sql("""
                DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
                INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
                    (165, N'پیام جدید چت رزرو مربی', N'PushCoachReserveNewMessage', N'پیام جدید در چت مربی', N'{0}: {1}', N'/companion-chat/{2}', N'coach-reserve-new-message'),
                    (166, N'شروع تماس درون‌برنامه‌ای توسط مربی', N'PushCoachInAppCallStarted', N'تماس درون‌برنامه‌ای', N'{0} با شما تماس می‌گیرد. برای پاسخ ضربه بزنید.', N'/call/{1}', N'coach-in-app-call-started'),
                    (167, N'یادآوری ۱۰ دقیقه مانده به خدمت آنلاین مربی', N'PushCoachOnlineReserveReminderBefore', N'۱۰ دقیقه تا خدمت آنلاین', N'۱۰ دقیقه دیگر باید با {0} درباره‌ی «{1}» از طریق «{2}» ارتباط برقرار کنید.', N'/operator', N'coach-online-reserve-reminder-before'),
                    (168, N'یادآوری سر زمان خدمت آنلاین مربی', N'PushCoachOnlineReserveReminderAtTime', N'زمان خدمت آنلاین رسید', N'همین الان باید با {0} درباره‌ی «{1}» از طریق «{2}» ارتباط برقرار کنید.', N'/operator', N'coach-online-reserve-reminder-attime');

                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
                    THROW 51000, 'A coach push type ID is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
                    THROW 51000, 'A coach push label is already assigned to another ID.', 1;

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
                WHERE pattern.PushTypeId IN (165, 166, 167, 168)
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
                WHERE pattern.PushTypeId IN (165, 166, 167, 168);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (165, 166, 167, 168);

                DELETE FROM PushPatterns WHERE PushTypeId IN (165, 166, 167, 168);
                DELETE FROM PushTypes
                WHERE (Id = 165 AND Label = N'PushCoachReserveNewMessage')
                   OR (Id = 166 AND Label = N'PushCoachInAppCallStarted')
                   OR (Id = 167 AND Label = N'PushCoachOnlineReserveReminderBefore')
                   OR (Id = 168 AND Label = N'PushCoachOnlineReserveReminderAtTime');
                """);

            migrationBuilder.DropTable(
                name: "CoachReserveMessageAttachments");

            migrationBuilder.DropTable(
                name: "CoachReserveMessageReactions");

            migrationBuilder.DropTable(
                name: "CoachReserveMessages");

            migrationBuilder.DropColumn(
                name: "CallEndDate",
                table: "CoachReserves");

            migrationBuilder.DropColumn(
                name: "CallStartDate",
                table: "CoachReserves");
        }
    }
}
