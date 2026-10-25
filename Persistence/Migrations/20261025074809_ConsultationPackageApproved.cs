using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConsultationPackageApproved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "ConsultationPackages",
                type: "bit",
                nullable: false,
                // بسته‌های موجود (که الان روی سایت هستند) تأییدشده فرض می‌شوند؛ بسته‌های جدید را کد صریحاً مقدار می‌دهد
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "ConsultationPackages");
        }
    }
}
