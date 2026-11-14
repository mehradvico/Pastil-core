using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    // تخفیف محصول دو‌لایه: سهم فروشگاه + سهم پاستیل (پاستیل روی قیمتِ بعد از تخفیف فروشگاه). حسابداری سفارش: PastilFundedAmount و PastilDiscountUnit.
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261113120000_PastilFundedDiscount")]
    public partial class PastilFundedDiscount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(name: "FundedByPastil", table: "Discounts", type: "bit", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<int>(name: "PastilDiscountPercent", table: "ProductItems", type: "int", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<double>(name: "PastilDiscountAmount", table: "ProductItems", type: "float", nullable: false, defaultValue: 0.0);
            migrationBuilder.AddColumn<double>(name: "PastilDiscountUnit", table: "ProductOrderItems", type: "float", nullable: false, defaultValue: 0.0);
            migrationBuilder.AddColumn<double>(name: "PastilFundedAmount", table: "ProductOrders", type: "float", nullable: false, defaultValue: 0.0);
            // procedure جدید (بدون ردیف تخفیفِ پاستیل دقیقاً مثل قبل رفتار می‌کند)
            migrationBuilder.Sql(PastilDiscountProcedure.Sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(PastilDiscountProcedure.PreviousSql);   // procedure قبلی، قبل از حذف ستون‌هایی که procedure جدید به آن‌ها اشاره می‌کند
            migrationBuilder.DropColumn(name: "PastilFundedAmount", table: "ProductOrders");
            migrationBuilder.DropColumn(name: "PastilDiscountUnit", table: "ProductOrderItems");
            migrationBuilder.DropColumn(name: "PastilDiscountAmount", table: "ProductItems");
            migrationBuilder.DropColumn(name: "PastilDiscountPercent", table: "ProductItems");
            migrationBuilder.DropColumn(name: "FundedByPastil", table: "Discounts");
        }
    }
}
