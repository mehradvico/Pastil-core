using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixCompanionReserveUserPushUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // بدون تغییر ساختار جدول؛ فقط آدرس دو پوش «اپراتور خدمت را کامل/لغو کرد» از فهرست کلی رزروها (/reserve)
            // به جزئیات همان رزرو (token3 = شناسه‌ی رزرو، {2}) اصلاح می‌شود تا هم کلیک عادی و هم دکمه‌ی
            // «بله تایید می‌کنم» (PushNotificationService.SendSingleAsync) کاربر را مستقیم به همان رزرو ببرند.
            migrationBuilder.Sql(@"
UPDATE p
SET p.Url = N'/reserve/{2}'
FROM PushPatterns p
JOIN PushTypes t ON t.Id = p.PushTypeId
WHERE t.Label IN (N'PushCompleteReserveUser', N'PushCancelReserveUser')
  AND p.Url = N'/reserve';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
UPDATE p
SET p.Url = N'/reserve'
FROM PushPatterns p
JOIN PushTypes t ON t.Id = p.PushTypeId
WHERE t.Label IN (N'PushCompleteReserveUser', N'PushCancelReserveUser')
  AND p.Url = N'/reserve/{2}';
");
        }
    }
}
