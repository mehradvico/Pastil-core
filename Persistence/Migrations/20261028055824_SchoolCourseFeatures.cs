using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchoolCourseFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DayReminderPushSentDate",
                table: "SchoolCourseSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CertificateEnabled",
                table: "SchoolCourses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FullRefundHoursBeforeStart",
                table: "SchoolCourses",
                type: "int",
                nullable: false,
                defaultValue: 24);

            migrationBuilder.AddColumn<int>(
                name: "LateCancelRefundPercent",
                table: "SchoolCourses",
                type: "int",
                nullable: false,
                defaultValue: 50);

            migrationBuilder.AddColumn<int>(
                name: "LateEnrollmentMode",
                table: "SchoolCourses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "SchoolCourses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "PrerequisiteCourseId",
                table: "SchoolCourses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RateAvg",
                table: "SchoolCourses",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "RateCount",
                table: "SchoolCourses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "SchoolCourseId",
                table: "SchoolComments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SchoolAttendances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolCourseSessionId = table.Column<long>(type: "bigint", nullable: false),
                    SchoolReserveId = table.Column<long>(type: "bigint", nullable: false),
                    Present = table.Column<bool>(type: "bit", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MissedPushSentDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolAttendances_SchoolCourseSessions_SchoolCourseSessionId",
                        column: x => x.SchoolCourseSessionId,
                        principalTable: "SchoolCourseSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolAttendances_SchoolReserves_SchoolReserveId",
                        column: x => x.SchoolReserveId,
                        principalTable: "SchoolReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolCertificates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolReserveId = table.Column<long>(type: "bigint", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuedByUserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolCertificates_SchoolReserves_SchoolReserveId",
                        column: x => x.SchoolReserveId,
                        principalTable: "SchoolReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolCourseInstructors",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolCourseId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsLead = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolCourseInstructors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolCourseInstructors_SchoolCourses_SchoolCourseId",
                        column: x => x.SchoolCourseId,
                        principalTable: "SchoolCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolCourseInstructors_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolCourseSkills",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolCourseId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolCourseSkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolCourseSkills_SchoolCourses_SchoolCourseId",
                        column: x => x.SchoolCourseId,
                        principalTable: "SchoolCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolCourseWaitlists",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolCourseId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolCourseWaitlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolCourseWaitlists_SchoolCourses_SchoolCourseId",
                        column: x => x.SchoolCourseId,
                        principalTable: "SchoolCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolCourseWaitlists_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolCourseWaitlists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SchoolProgresses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolReserveId = table.Column<long>(type: "bigint", nullable: false),
                    SchoolCourseSkillId = table.Column<long>(type: "bigint", nullable: false),
                    SchoolCourseSessionId = table.Column<long>(type: "bigint", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordedByUserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolProgresses_SchoolCourseSessions_SchoolCourseSessionId",
                        column: x => x.SchoolCourseSessionId,
                        principalTable: "SchoolCourseSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolProgresses_SchoolCourseSkills_SchoolCourseSkillId",
                        column: x => x.SchoolCourseSkillId,
                        principalTable: "SchoolCourseSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SchoolProgresses_SchoolReserves_SchoolReserveId",
                        column: x => x.SchoolReserveId,
                        principalTable: "SchoolReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCourses_PrerequisiteCourseId",
                table: "SchoolCourses",
                column: "PrerequisiteCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolComments_SchoolCourseId",
                table: "SchoolComments",
                column: "SchoolCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolAttendances_SchoolCourseSessionId_SchoolReserveId",
                table: "SchoolAttendances",
                columns: new[] { "SchoolCourseSessionId", "SchoolReserveId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolAttendances_SchoolReserveId",
                table: "SchoolAttendances",
                column: "SchoolReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCertificates_Code",
                table: "SchoolCertificates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCertificates_SchoolReserveId",
                table: "SchoolCertificates",
                column: "SchoolReserveId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCourseInstructors_SchoolCourseId_UserId",
                table: "SchoolCourseInstructors",
                columns: new[] { "SchoolCourseId", "UserId" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCourseInstructors_UserId",
                table: "SchoolCourseInstructors",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCourseSkills_SchoolCourseId",
                table: "SchoolCourseSkills",
                column: "SchoolCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCourseWaitlists_SchoolCourseId_UserPetId",
                table: "SchoolCourseWaitlists",
                columns: new[] { "SchoolCourseId", "UserPetId" },
                unique: true,
                filter: "[Deleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCourseWaitlists_UserId",
                table: "SchoolCourseWaitlists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolCourseWaitlists_UserPetId",
                table: "SchoolCourseWaitlists",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolProgresses_SchoolCourseSessionId",
                table: "SchoolProgresses",
                column: "SchoolCourseSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolProgresses_SchoolCourseSkillId",
                table: "SchoolProgresses",
                column: "SchoolCourseSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolProgresses_SchoolReserveId",
                table: "SchoolProgresses",
                column: "SchoolReserveId");

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolComments_SchoolCourses_SchoolCourseId",
                table: "SchoolComments",
                column: "SchoolCourseId",
                principalTable: "SchoolCourses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SchoolCourses_SchoolCourses_PrerequisiteCourseId",
                table: "SchoolCourses",
                column: "PrerequisiteCourseId",
                principalTable: "SchoolCourses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // پوش‌های جدید مدرسه (۱۶۹ تا ۱۷۲)؛ الگوی همان SeedSchoolClassReminderPushType
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 169 AND Label <> N'PushSchoolWaitlistSeatOpen')
                    THROW 51000, 'PushType ID 169 is already assigned to another label.', 1;
                SET IDENTITY_INSERT PushTypes ON;
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 169)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (169, N'جا در دوره‌ی لیست انتظار خالی شد', N'PushSchoolWaitlistSeatOpen');
                SET IDENTITY_INSERT PushTypes OFF;
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 169)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (169, N'در دوره‌ی {0} جا باز شد', N'زودتر ثبت‌نام کنید؛ جای خالی برای همه‌ی افراد لیست انتظار اعلام شده است.', N'/school/course-{1}', NULL, N'school-waitlist-open', 1);
                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1 FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 169
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 170 AND Label <> N'PushSchoolClassReminderDayBefore')
                    THROW 51000, 'PushType ID 170 is already assigned to another label.', 1;
                SET IDENTITY_INSERT PushTypes ON;
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 170)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (170, N'یادآوری یک روز مانده به جلسه‌ی مدرسه', N'PushSchoolClassReminderDayBefore');
                SET IDENTITY_INSERT PushTypes OFF;
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 170)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (170, N'فردا کلاس دارید', N'جلسه‌ی دوره‌ی {0} فردا ساعت {1} برگزار می‌شود.', N'/school/course-{2}', NULL, N'school-class-reminder-day', 1);
                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1 FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 170
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 171 AND Label <> N'PushSchoolSessionMissed')
                    THROW 51000, 'PushType ID 171 is already assigned to another label.', 1;
                SET IDENTITY_INSERT PushTypes ON;
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 171)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (171, N'جلسه‌ی از دست رفته‌ی مدرسه', N'PushSchoolSessionMissed');
                SET IDENTITY_INSERT PushTypes OFF;
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 171)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (171, N'جلسه را از دست دادید', N'در جلسه‌ی دوره‌ی {0} غایب بودید. اگر ضبط جلسه موجود باشد از صفحه‌ی دوره می‌بینید.', N'/school/course-{1}', NULL, N'school-session-missed', 1);
                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1 FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 171
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
                IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = 172 AND Label <> N'PushSchoolCertificateIssued')
                    THROW 51000, 'PushType ID 172 is already assigned to another label.', 1;
                SET IDENTITY_INSERT PushTypes ON;
                IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = 172)
                    INSERT INTO PushTypes (Id, Name, Label) VALUES (172, N'صدور گواهی پایان دوره‌ی مدرسه', N'PushSchoolCertificateIssued');
                SET IDENTITY_INSERT PushTypes OFF;
                IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = 172)
                    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                    VALUES (172, N'گواهی پایان دوره صادر شد', N'گواهی دوره‌ی {0} برای {1} صادر شد. کد گواهی: {2}', N'/school/certificate-{2}', NULL, N'school-certificate', 1);
                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1 FROM PushPatterns pattern
                WHERE pattern.PushTypeId = 172
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE notification FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId IN (169, 170, 171, 172);
                DELETE setting FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (169, 170, 171, 172);
                DELETE FROM PushPatterns WHERE PushTypeId IN (169, 170, 171, 172);
                DELETE FROM PushTypes WHERE Id IN (169, 170, 171, 172);
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolComments_SchoolCourses_SchoolCourseId",
                table: "SchoolComments");

            migrationBuilder.DropForeignKey(
                name: "FK_SchoolCourses_SchoolCourses_PrerequisiteCourseId",
                table: "SchoolCourses");

            migrationBuilder.DropTable(
                name: "SchoolAttendances");

            migrationBuilder.DropTable(
                name: "SchoolCertificates");

            migrationBuilder.DropTable(
                name: "SchoolCourseInstructors");

            migrationBuilder.DropTable(
                name: "SchoolCourseWaitlists");

            migrationBuilder.DropTable(
                name: "SchoolProgresses");

            migrationBuilder.DropTable(
                name: "SchoolCourseSkills");

            migrationBuilder.DropIndex(
                name: "IX_SchoolCourses_PrerequisiteCourseId",
                table: "SchoolCourses");

            migrationBuilder.DropIndex(
                name: "IX_SchoolComments_SchoolCourseId",
                table: "SchoolComments");

            migrationBuilder.DropColumn(
                name: "DayReminderPushSentDate",
                table: "SchoolCourseSessions");

            migrationBuilder.DropColumn(
                name: "CertificateEnabled",
                table: "SchoolCourses");

            migrationBuilder.DropColumn(
                name: "FullRefundHoursBeforeStart",
                table: "SchoolCourses");

            migrationBuilder.DropColumn(
                name: "LateCancelRefundPercent",
                table: "SchoolCourses");

            migrationBuilder.DropColumn(
                name: "LateEnrollmentMode",
                table: "SchoolCourses");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "SchoolCourses");

            migrationBuilder.DropColumn(
                name: "PrerequisiteCourseId",
                table: "SchoolCourses");

            migrationBuilder.DropColumn(
                name: "RateAvg",
                table: "SchoolCourses");

            migrationBuilder.DropColumn(
                name: "RateCount",
                table: "SchoolCourses");

            migrationBuilder.DropColumn(
                name: "SchoolCourseId",
                table: "SchoolComments");
        }
    }
}
