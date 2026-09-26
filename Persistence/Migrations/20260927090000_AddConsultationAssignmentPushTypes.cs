using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(DataBaseContext))]
    [Migration("20260927090000_AddConsultationAssignmentPushTypes")]
    public partial class AddConsultationAssignmentPushTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // بدون تغییر ساختار جدول؛ سه نوع پوش «تخصیص/برداشتن/یادآوری مشاوره‌ی بی‌صاحب» (شناسه‌های ۷۸ تا ۸۰)
            migrationBuilder.Sql(@"
DECLARE @PushTypes TABLE (Id BIGINT NOT NULL, Name NVARCHAR(200) NOT NULL, Label NVARCHAR(200) NOT NULL, Title NVARCHAR(200) NOT NULL, Body NVARCHAR(500) NOT NULL, Url NVARCHAR(500) NOT NULL, Tag NVARCHAR(100) NOT NULL);

INSERT INTO @PushTypes (Id, Name, Label, Title, Body, Url, Tag)
VALUES
    (78, N'تخصیص مشاوره آنلاین به نماینده', N'PushConsultationAssigned', N'مشاوره‌ی جدید به شما محول شد', N'مشاوره‌ی {0} ({2}) به شما محول شد؛ بیا ارتباط را برقرار کن.', N'/consultations/manage', N'consultation-assigned'),
    (79, N'برداشتن مشاوره توسط همکار', N'PushConsultationTakenByColleague', N'مشاوره برداشته شد', N'{0} مشاوره‌ی {2} را برداشت؛ نیازی به اقدام شما نیست.', N'/consultations/manage', N'consultation-taken'),
    (80, N'یادآوری مشاوره‌ی شروع‌نشده', N'PushConsultationUnclaimed', N'مشاوره‌ای منتظر شماست', N'مشاوره‌ی {0} حدود {2} دقیقه است که کسی شروعش نکرده؛ زودتر ارتباط را برقرار کن.', N'/consultations/manage', N'consultation-unclaimed');

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
WHERE p.PushTypeId IN (78, 79, 80) AND NOT EXISTS (SELECT 1 FROM PushSettings x WHERE x.PushPatternId = p.Id);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE setting
FROM PushSettings setting
INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
WHERE pattern.PushTypeId IN (78, 79, 80);

DELETE FROM PushPatterns WHERE PushTypeId IN (78, 79, 80);
DELETE FROM PushTypes WHERE Id IN (78, 79, 80) AND Label IN (N'PushConsultationAssigned', N'PushConsultationTakenByColleague', N'PushConsultationUnclaimed');
");
        }
    }
}
