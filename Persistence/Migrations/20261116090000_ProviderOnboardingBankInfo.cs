using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    // شماره کارت و شبای تسویه در پیش‌ثبت‌نام ارائه‌دهنده (رمزشده در سطح ستون). هر دو nullable.
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261116090000_ProviderOnboardingBankInfo")]
    public partial class ProviderOnboardingBankInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "CardNumber", table: "ProviderOnboardingApplications", type: "nvarchar(512)", maxLength: 512, nullable: true);
            migrationBuilder.AddColumn<string>(name: "Iban", table: "ProviderOnboardingApplications", type: "nvarchar(512)", maxLength: 512, nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Iban", table: "ProviderOnboardingApplications");
            migrationBuilder.DropColumn(name: "CardNumber", table: "ProviderOnboardingApplications");
        }
    }
}
