using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConsultationPushTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // بدون تغییر ساختار جدول؛ فقط چهار نوع پوش «مشاوره آنلاین» (شناسه‌های ۷۱ تا ۷۴) با الگوی متنی.
            // Url پوش ۷۳ از token3 می‌آید ({2}): کاربر به گفتگو/تماس، نماینده به صفحه‌ی مدیریت.
            migrationBuilder.Sql(@"
DECLARE @PushTypes TABLE (Id BIGINT NOT NULL, Name NVARCHAR(200) NOT NULL, Label NVARCHAR(200) NOT NULL, Title NVARCHAR(200) NOT NULL, Body NVARCHAR(500) NOT NULL, Url NVARCHAR(500) NOT NULL, Tag NVARCHAR(100) NOT NULL);

INSERT INTO @PushTypes (Id, Name, Label, Title, Body, Url, Tag)
VALUES
    (71, N'خرید مشاوره آنلاین (اعلان به نماینده)', N'PushConsultationPurchasedAgent', N'رزرو مشاوره جدید', N'{0} یک مشاوره‌ی {2} ({3} دقیقه‌ای) خریده است؛ بیا ارتباط را برقرار کن.', N'/consultations/manage', N'consultation-purchased-agent'),
    (72, N'خرید مشاوره آنلاین (اعلان به کاربر)', N'PushConsultationPurchasedUser', N'مشاوره‌ی شما ثبت شد', N'مشاوره‌ی شما در {0} ثبت شد؛ منتظر شروع نماینده باشید.', N'/consultations', N'consultation-purchased-user'),
    (73, N'نزدیک پایان پنجره‌ی مشاوره', N'PushConsultationEndingSoon', N'پایان نزدیک مشاوره', N'۵ دقیقه‌ی دیگر پنجره‌ی مشاوره با {0} تمام می‌شود.', N'{2}', N'consultation-ending-soon'),
    (74, N'بازپرداخت مشاوره‌ی شروع‌نشده', N'PushConsultationExpiredRefund', N'بازپرداخت مشاوره', N'مشاوره‌ی شما در {0} شروع نشد؛ مبلغ به کیف پول شما برگشت.', N'/consultations', N'consultation-expired-refund');

IF EXISTS (SELECT 1 FROM @PushTypes s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
    THROW 51000, 'A consultation PushType ID is already assigned to another label.', 1;

IF EXISTS (SELECT 1 FROM @PushTypes s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
    THROW 51000, 'A consultation PushType label is already assigned to another ID.', 1;

SET IDENTITY_INSERT PushTypes ON;

INSERT INTO PushTypes (Id, Name, Label)
SELECT s.Id, s.Name, s.Label FROM @PushTypes s WHERE NOT EXISTS (SELECT 1 FROM PushTypes t WHERE t.Id = s.Id);

SET IDENTITY_INSERT PushTypes OFF;

INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
SELECT s.Id, s.Title, s.Body, s.Url, NULL, s.Tag, 1 FROM @PushTypes s
WHERE NOT EXISTS (SELECT 1 FROM PushPatterns p WHERE p.PushTypeId = s.Id);

INSERT INTO PushSettings (PushPatternId, IsEnabled)
SELECT p.Id, 1 FROM PushPatterns p
WHERE p.PushTypeId IN (71, 72, 73, 74) AND NOT EXISTS (SELECT 1 FROM PushSettings x WHERE x.PushPatternId = p.Id);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE setting
FROM PushSettings setting
INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
WHERE pattern.PushTypeId IN (71, 72, 73, 74);

DELETE FROM PushPatterns WHERE PushTypeId IN (71, 72, 73, 74);
DELETE FROM PushTypes WHERE Id IN (71, 72, 73, 74) AND Label IN (N'PushConsultationPurchasedAgent', N'PushConsultationPurchasedUser', N'PushConsultationEndingSoon', N'PushConsultationExpiredRefund');
");
        }
    }
}
