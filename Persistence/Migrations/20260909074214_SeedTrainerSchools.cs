using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedTrainerSchools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // برای دو مربی موجود («محمد قادرپناه» و «مهدی کی‌منش») یک مدرسه‌ی تربیت به نامِ خودشان
            // ساخته می‌شود - مطابقت نام با LIKE چون فاصله‌ی نیم‌فاصله در نام‌ها ممکن است بین محیط‌ها یکسان نباشد.
            migrationBuilder.Sql("""
                INSERT INTO Schools (Name, CompanionId, Active, ShowToSite, Approve, StateId, CityId, CommentCount, RateAvg, RateCount, Suggested)
                SELECT TOP 1 c.Name, c.Id, 1, 1, 1, ci.StateId, c.CityId, 0, 0, 0, 0
                FROM Companions c
                INNER JOIN Cities ci ON ci.Id = c.CityId
                WHERE c.Name LIKE N'%قادرپناه%'
                  AND NOT EXISTS (SELECT 1 FROM Schools s WHERE s.CompanionId = c.Id);

                INSERT INTO Schools (Name, CompanionId, Active, ShowToSite, Approve, StateId, CityId, CommentCount, RateAvg, RateCount, Suggested)
                SELECT TOP 1 c.Name, c.Id, 1, 1, 1, ci.StateId, c.CityId, 0, 0, 0, 0
                FROM Companions c
                INNER JOIN Cities ci ON ci.Id = c.CityId
                WHERE c.Name LIKE N'%کی%منش%'
                  AND NOT EXISTS (SELECT 1 FROM Schools s WHERE s.CompanionId = c.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE s
                FROM Schools s
                INNER JOIN Companions c ON c.Id = s.CompanionId
                WHERE c.Name LIKE N'%قادرپناه%' OR c.Name LIKE N'%کی%منش%';
                """);
        }
    }
}
