using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    // زمان/عامل لغو سفارش و زمان «در حال پردازش» شدن برای «زمان‌بندی سفارش» در پنل. هر چهار ستون nullable.
    // لغوهای قدیمی: زمان از ثبت برگشت پول لغو در کیف پول (Wallets.Name = 'OrderCancelRefund:{orderId}') پر می‌شود؛ عامل نامشخص می‌ماند.
    // هم‌ارز backend/scripts/OrderCancelAndProcessingDates.sql.
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261115180000_OrderCancelAndProcessingDates")]
    public partial class OrderCancelAndProcessingDates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<System.DateTime>(name: "CancelledDate", table: "ProductOrders", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<int>(name: "CancelledByActor", table: "ProductOrders", type: "int", nullable: true);
            migrationBuilder.AddColumn<long>(name: "CancelledStoreId", table: "ProductOrders", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<System.DateTime>(name: "ProcessingDate", table: "ProductOrders", type: "datetime2", nullable: true);

            migrationBuilder.Sql("""
                UPDATE o SET o.CancelledDate = w.CreateDate
                FROM ProductOrders o
                CROSS APPLY (
                    SELECT TOP 1 CreateDate FROM Wallets
                    WHERE Name = N'OrderCancelRefund:' + o.Id AND Deleted = 0
                    ORDER BY CreateDate
                ) w
                WHERE o.CancelledDate IS NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ProcessingDate", table: "ProductOrders");
            migrationBuilder.DropColumn(name: "CancelledStoreId", table: "ProductOrders");
            migrationBuilder.DropColumn(name: "CancelledByActor", table: "ProductOrders");
            migrationBuilder.DropColumn(name: "CancelledDate", table: "ProductOrders");
        }
    }
}
