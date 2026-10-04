using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedPetMicrochipFollowUpNotice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Importance=3 (Critical) ⇒ realtime پنل + پوش واقعی به ادمین‌ها (NoticeEventService.DispatchAsync)
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM NoticeTypes WHERE Label = N'PetMicrochip.FollowUpRequested')
                    UPDATE NoticeTypes
                    SET Title = N'درخواست پیگیری میکروچیپ', Name = N'{requesterName} برای پت با میکروچیپ {microchipCode} درخواست پیگیری توسط پاستیل ثبت کرد.',
                        NavigationTemplate = N'/admin/petmicrochiprequest/{referenceId}', Importance = 3, IsActive = 1
                    WHERE Label = N'PetMicrochip.FollowUpRequested';
                ELSE
                    INSERT INTO NoticeTypes (Label, Title, Name, NavigationTemplate, Importance, IsActive)
                    VALUES (N'PetMicrochip.FollowUpRequested', N'درخواست پیگیری میکروچیپ', N'{requesterName} برای پت با میکروچیپ {microchipCode} درخواست پیگیری توسط پاستیل ثبت کرد.', N'/admin/petmicrochiprequest/{referenceId}', 3, 1);
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM NoticeTypes WHERE Label = N'PetMicrochip.FollowUpRequested';");

        }
    }
}
