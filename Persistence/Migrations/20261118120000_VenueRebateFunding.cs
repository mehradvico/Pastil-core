using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261118120000_VenueRebateFunding")]
    public partial class VenueRebateFunding : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(name: "SiteFundedPercent", table: "Rebate", type: "decimal(5,2)", nullable: true);
            migrationBuilder.AddColumn<decimal>(name: "ProviderFundedPercent", table: "Rebate", type: "decimal(5,2)", nullable: true);
            migrationBuilder.AddCheckConstraint(
                name: "CK_Rebate_FundingPercent",
                table: "Rebate",
                sql: "(SiteFundedPercent IS NULL AND ProviderFundedPercent IS NULL) OR " +
                     "(SiteFundedPercent IS NOT NULL AND ProviderFundedPercent IS NOT NULL AND " +
                     "SiteFundedPercent BETWEEN 0 AND 100 AND ProviderFundedPercent BETWEEN 0 AND 100 AND " +
                     "SiteFundedPercent + ProviderFundedPercent = 100)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(name: "CK_Rebate_FundingPercent", table: "Rebate");
            migrationBuilder.DropColumn(name: "ProviderFundedPercent", table: "Rebate");
            migrationBuilder.DropColumn(name: "SiteFundedPercent", table: "Rebate");
        }
    }
}
