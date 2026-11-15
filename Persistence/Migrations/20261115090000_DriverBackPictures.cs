using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Persistence.Context;

#nullable disable

namespace Persistence.Migrations
{
    // پشتِ گواهینامه و پشتِ کارت خودرو راننده (CertificatePictureId/VehicleCardPictureId = روی کارت). هر دو nullable.
    [DbContext(typeof(DataBaseContext))]
    [Migration("20261115090000_DriverBackPictures")]
    public partial class DriverBackPictures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(name: "CertificateBackPictureId", table: "Drivers", type: "bigint", nullable: true);
            migrationBuilder.AddColumn<long>(name: "VehicleCardBackPictureId", table: "Drivers", type: "bigint", nullable: true);

            migrationBuilder.CreateIndex(name: "IX_Drivers_CertificateBackPictureId", table: "Drivers", column: "CertificateBackPictureId");
            migrationBuilder.CreateIndex(name: "IX_Drivers_VehicleCardBackPictureId", table: "Drivers", column: "VehicleCardBackPictureId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Pictures_CertificateBackPictureId", table: "Drivers", column: "CertificateBackPictureId",
                principalTable: "Pictures", principalColumn: "Id");
            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Pictures_VehicleCardBackPictureId", table: "Drivers", column: "VehicleCardBackPictureId",
                principalTable: "Pictures", principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Drivers_Pictures_VehicleCardBackPictureId", table: "Drivers");
            migrationBuilder.DropForeignKey(name: "FK_Drivers_Pictures_CertificateBackPictureId", table: "Drivers");
            migrationBuilder.DropIndex(name: "IX_Drivers_VehicleCardBackPictureId", table: "Drivers");
            migrationBuilder.DropIndex(name: "IX_Drivers_CertificateBackPictureId", table: "Drivers");
            migrationBuilder.DropColumn(name: "VehicleCardBackPictureId", table: "Drivers");
            migrationBuilder.DropColumn(name: "CertificateBackPictureId", table: "Drivers");
        }
    }
}
