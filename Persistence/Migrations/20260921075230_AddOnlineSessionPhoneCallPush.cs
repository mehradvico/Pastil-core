using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOnlineSessionPhoneCallPush : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // بدون تغییر ساختار جدول؛ فقط نوع و الگوی پوش «زنگ خوردن تماس جلسه‌ی آنلاین» (شناسه ۷۰) ثبت می‌شود.
            // متن الگو مثل تماس رزرو (۶۳) است؛ فقط اطلاع‌رسانی است و صفحه‌ای ندارد (مسیر /).
            migrationBuilder.Sql(@"
DECLARE @PushTypeId BIGINT = 70;
DECLARE @PushTypeLabel NVARCHAR(200) = N'PushOnlineSessionPhoneCallStarted';

IF EXISTS (SELECT 1 FROM PushTypes WHERE Id = @PushTypeId AND Label <> @PushTypeLabel)
    THROW 51000, 'PushType ID 70 is already assigned to another label.', 1;

IF EXISTS (SELECT 1 FROM PushTypes WHERE Label = @PushTypeLabel AND Id <> @PushTypeId)
    THROW 51000, 'PushOnlineSessionPhoneCallStarted label is already assigned to another ID.', 1;

SET IDENTITY_INSERT PushTypes ON;

IF NOT EXISTS (SELECT 1 FROM PushTypes WHERE Id = @PushTypeId)
    INSERT INTO PushTypes (Id, Name, Label) VALUES (@PushTypeId, N'شروع تماس تلفنی جلسه‌ی آنلاین', @PushTypeLabel);

SET IDENTITY_INSERT PushTypes OFF;

IF NOT EXISTS (SELECT 1 FROM PushPatterns WHERE PushTypeId = @PushTypeId)
    INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
    VALUES (@PushTypeId, N'تماس تلفنی', N'{0} در حال تماس تلفنی با شماست. لطفاً تماس را پاسخ دهید.', N'/', NULL, N'online-session-phone-call', 1);

INSERT INTO PushSettings (PushPatternId, IsEnabled)
SELECT p.Id, 1 FROM PushPatterns p
WHERE p.PushTypeId = @PushTypeId AND NOT EXISTS (SELECT 1 FROM PushSettings x WHERE x.PushPatternId = p.Id);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE setting
FROM PushSettings setting
INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
WHERE pattern.PushTypeId = 70;

DELETE FROM PushPatterns WHERE PushTypeId = 70;
DELETE FROM PushTypes WHERE Id = 70 AND Label = N'PushOnlineSessionPhoneCallStarted';
");
        }
    }
}
