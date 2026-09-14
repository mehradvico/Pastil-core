using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPushSubscriptionProviderAndFcm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FcmToken",
                table: "PushSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "PushSubscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Provider",
                table: "PushSubscriptions",
                type: "bigint",
                nullable: false,
                defaultValue: 1L); // PushProviderEnum.WebPush — همه‌ی ردیف‌های قبلی از وب‌اپ ثبت شده‌اند
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FcmToken",
                table: "PushSubscriptions");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "PushSubscriptions");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "PushSubscriptions");
        }
    }
}
