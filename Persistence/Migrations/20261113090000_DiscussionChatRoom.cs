using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    // پرسش و پاسخ کالا به‌صورت «چت‌روم»: وضعیت تأیید پاسخ‌ها (null=در انتظار)، پاسخ به پیام مشخص، پاسخ فروشگاه.
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261113090000_DiscussionChatRoom")]
    public partial class DiscussionChatRoom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(name: "AdminConfirm", table: "DiscussionAnswers", type: "bit", nullable: true);
            migrationBuilder.AddColumn<long>(name: "ReplyToAnswerId", table: "DiscussionAnswers", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<long>(name: "StoreId", table: "DiscussionAnswers", type: "bigint", nullable: true);
            // پاسخ‌های قبلی: فعال = تأیید‌شده؛ غیرفعال = ردشده/پنهان (تا ادمین دوباره فعالش کند)
            migrationBuilder.Sql("UPDATE [DiscussionAnswers] SET [AdminConfirm] = CASE WHEN [Active] = 1 THEN 1 ELSE 0 END;");
            migrationBuilder.CreateIndex(name: "IX_DiscussionAnswers_StoreId", table: "DiscussionAnswers", column: "StoreId");
            migrationBuilder.AddForeignKey(name: "FK_DiscussionAnswers_Stores_StoreId", table: "DiscussionAnswers", column: "StoreId", principalTable: "Stores", principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_DiscussionAnswers_Stores_StoreId", table: "DiscussionAnswers");
            migrationBuilder.DropIndex(name: "IX_DiscussionAnswers_StoreId", table: "DiscussionAnswers");
            migrationBuilder.DropColumn(name: "StoreId", table: "DiscussionAnswers");
            migrationBuilder.DropColumn(name: "ReplyToAnswerId", table: "DiscussionAnswers");
            migrationBuilder.DropColumn(name: "AdminConfirm", table: "DiscussionAnswers");
        }
    }
}
