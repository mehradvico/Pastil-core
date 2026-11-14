using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    // کد تخفیف مختص یک ارائه‌دهنده: TargetType (ClubRewardTargetTypeEnum: 2=Store، 15=Coach، ...) + TargetId. null = بدون محدودیت.
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261114120000_RebateTarget")]
    public partial class RebateTarget : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "TargetType", table: "Rebate", type: "int", nullable: true);
            migrationBuilder.AddColumn<long>(name: "TargetId", table: "Rebate", type: "bigint", nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "TargetId", table: "Rebate");
            migrationBuilder.DropColumn(name: "TargetType", table: "Rebate");
        }
    }
}
