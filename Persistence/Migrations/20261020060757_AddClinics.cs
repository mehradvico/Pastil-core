using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClinicReserveId",
                table: "Wallets",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clinics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderId = table.Column<long>(type: "bigint", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    LegacyCompanionId = table.Column<long>(type: "bigint", nullable: true),
                    ReferralCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    BackgroundPictureId = table.Column<long>(type: "bigint", nullable: true),
                    IconId = table.Column<long>(type: "bigint", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AddressValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StateId = table.Column<long>(type: "bigint", nullable: true),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true),
                    Location = table.Column<Point>(type: "geography", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ShowToSite = table.Column<bool>(type: "bit", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Suggested = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CommentCount = table.Column<int>(type: "int", nullable: false),
                    RateAvg = table.Column<double>(type: "float", nullable: false),
                    RateCount = table.Column<int>(type: "int", nullable: false),
                    SearchKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GoldAccountDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SilverAccountDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Regulations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstablishedYear = table.Column<int>(type: "int", nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicenseApproved = table.Column<bool>(type: "bit", nullable: false),
                    InstagramUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ServiceAtHome = table.Column<bool>(type: "bit", nullable: false),
                    ServiceInPerson = table.Column<bool>(type: "bit", nullable: false),
                    ServiceOnline = table.Column<bool>(type: "bit", nullable: false),
                    HasParking = table.Column<bool>(type: "bit", nullable: false),
                    HasWaitingRoom = table.Column<bool>(type: "bit", nullable: false),
                    HasCamera = table.Column<bool>(type: "bit", nullable: false),
                    MaxConcurrentPets = table.Column<int>(type: "int", nullable: false),
                    StaffCount = table.Column<int>(type: "int", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoH1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoMinDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoPictureAlt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoUrlText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SeoNoIndex = table.Column<bool>(type: "bit", nullable: false),
                    SeoNoFollow = table.Column<bool>(type: "bit", nullable: false),
                    SeoCanonical = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clinics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clinics_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clinics_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clinics_Pictures_BackgroundPictureId",
                        column: x => x.BackgroundPictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clinics_Pictures_IconId",
                        column: x => x.IconId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clinics_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clinics_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clinics_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clinics_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicPets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<long>(type: "bigint", nullable: false),
                    PetId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicPets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicPets_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicPets_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicPictures_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicServices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<long>(type: "bigint", nullable: false),
                    AssistanceId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicServices_Assistances_AssistanceId",
                        column: x => x.AssistanceId,
                        principalTable: "Assistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicServices_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicTimes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<long>(type: "bigint", nullable: false),
                    WeekDayId = table.Column<long>(type: "bigint", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EndTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicTimes_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicTimes_WeekDays_WeekDayId",
                        column: x => x.WeekDayId,
                        principalTable: "WeekDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<long>(type: "bigint", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicTypes_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicTypes_Codes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UserAccept = table.Column<bool>(type: "bit", nullable: true),
                    InviteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicUsers_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicZones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicId = table.Column<long>(type: "bigint", nullable: false),
                    StateId = table.Column<long>(type: "bigint", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicZones_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicZones_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicZones_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicZones_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicServicePackages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicServiceId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PrePaymentPrice = table.Column<double>(type: "float", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PetSize = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicServicePackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicServicePackages_ClinicServices_ClinicServiceId",
                        column: x => x.ClinicServiceId,
                        principalTable: "ClinicServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicServicePackages_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicServiceUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicServiceId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicServiceUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicServiceUsers_ClinicServices_ClinicServiceId",
                        column: x => x.ClinicServiceId,
                        principalTable: "ClinicServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicServiceUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicReserves",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReserveCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ClinicId = table.Column<long>(type: "bigint", nullable: false),
                    ClinicServiceId = table.Column<long>(type: "bigint", nullable: false),
                    ClinicServicePackageId = table.Column<long>(type: "bigint", nullable: false),
                    BookerId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    AssigneeUserId = table.Column<long>(type: "bigint", nullable: true),
                    ClinicTimeId = table.Column<long>(type: "bigint", nullable: true),
                    DoDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    AddressId = table.Column<long>(type: "bigint", nullable: true),
                    BookerNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PackagePrice = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    OnlineAmount = table.Column<double>(type: "float", nullable: false),
                    FromWallet = table.Column<bool>(type: "bit", nullable: false),
                    WalletPrice = table.Column<double>(type: "float", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: true),
                    RebatePrice = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProviderShare = table.Column<double>(type: "float", nullable: false),
                    SiteShare = table.Column<double>(type: "float", nullable: false),
                    Permitted = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsReserved = table.Column<bool>(type: "bit", nullable: false),
                    IsCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByBooker = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DoneDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RescheduleCount = table.Column<int>(type: "int", nullable: false),
                    OriginalDoDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRescheduleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerDecision = table.Column<int>(type: "int", nullable: false),
                    OwnerDecisionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OwnerDecisionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerApprovalDeadline = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicReserves", x => x.Id);
                    table.CheckConstraint("CK_ClinicReserve_Dates", "[EndDate] >= [DoDate]");
                    table.CheckConstraint("CK_ClinicReserve_Prices", "[Price] >= 0 AND [OnlineAmount] >= 0 AND [PaymentPrice] >= 0 AND [WalletPrice] >= 0 AND [RebatePrice] >= 0");
                    table.ForeignKey(
                        name: "FK_ClinicReserves_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserves_ClinicServicePackages_ClinicServicePackageId",
                        column: x => x.ClinicServicePackageId,
                        principalTable: "ClinicServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserves_ClinicServices_ClinicServiceId",
                        column: x => x.ClinicServiceId,
                        principalTable: "ClinicServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserves_ClinicTimes_ClinicTimeId",
                        column: x => x.ClinicTimeId,
                        principalTable: "ClinicTimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserves_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserves_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserves_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserves_Users_AssigneeUserId",
                        column: x => x.AssigneeUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserves_Users_BookerId",
                        column: x => x.BookerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicServicePackagePictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicServicePackageId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicServicePackagePictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicServicePackagePictures_ClinicServicePackages_ClinicServicePackageId",
                        column: x => x.ClinicServicePackageId,
                        principalTable: "ClinicServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicServicePackagePictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicServicePackageTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicServicePackageId = table.Column<long>(type: "bigint", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PrePaymentPrice = table.Column<double>(type: "float", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicServicePackageTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicServicePackageTypes_ClinicServicePackages_ClinicServicePackageId",
                        column: x => x.ClinicServicePackageId,
                        principalTable: "ClinicServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ClinicId = table.Column<long>(type: "bigint", nullable: false),
                    ClinicReserveId = table.Column<long>(type: "bigint", nullable: true),
                    IsReserved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicComments_ClinicReserves_ClinicReserveId",
                        column: x => x.ClinicReserveId,
                        principalTable: "ClinicReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicComments_Clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "Clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicComments_Comments_Id",
                        column: x => x.Id,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicReserveItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClinicReserveId = table.Column<long>(type: "bigint", nullable: false),
                    ClinicServiceId = table.Column<long>(type: "bigint", nullable: false),
                    ClinicServicePackageId = table.Column<long>(type: "bigint", nullable: false),
                    PackagePrice = table.Column<double>(type: "float", nullable: false),
                    OnlineAmount = table.Column<double>(type: "float", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProviderShare = table.Column<double>(type: "float", nullable: false),
                    SiteShare = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicReserveItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicReserveItems_ClinicReserves_ClinicReserveId",
                        column: x => x.ClinicReserveId,
                        principalTable: "ClinicReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserveItems_ClinicServicePackages_ClinicServicePackageId",
                        column: x => x.ClinicServicePackageId,
                        principalTable: "ClinicServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicReserveItems_ClinicServices_ClinicServiceId",
                        column: x => x.ClinicServiceId,
                        principalTable: "ClinicServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettlementClinics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettlementId = table.Column<long>(type: "bigint", nullable: false),
                    ClinicReserveId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementClinics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementClinics_ClinicReserves_ClinicReserveId",
                        column: x => x.ClinicReserveId,
                        principalTable: "ClinicReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementClinics_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_ClinicReserveId",
                table: "Wallets",
                column: "ClinicReserveId",
                unique: true,
                filter: "[ClinicReserveId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicComments_ClinicId",
                table: "ClinicComments",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicComments_ClinicReserveId",
                table: "ClinicComments",
                column: "ClinicReserveId",
                unique: true,
                filter: "[ClinicReserveId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPets_ClinicId_PetId",
                table: "ClinicPets",
                columns: new[] { "ClinicId", "PetId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPets_PetId",
                table: "ClinicPets",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPictures_ClinicId",
                table: "ClinicPictures",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicPictures_PictureId",
                table: "ClinicPictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserveItems_ClinicReserveId",
                table: "ClinicReserveItems",
                column: "ClinicReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserveItems_ClinicServiceId",
                table: "ClinicReserveItems",
                column: "ClinicServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserveItems_ClinicServicePackageId",
                table: "ClinicReserveItems",
                column: "ClinicServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserves_AddressId",
                table: "ClinicReserves",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserves_AssigneeUserId",
                table: "ClinicReserves",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserves_BookerId_CreateDate",
                table: "ClinicReserves",
                columns: new[] { "BookerId", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserves_ClinicId_DoDate_EndDate",
                table: "ClinicReserves",
                columns: new[] { "ClinicId", "DoDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserves_ClinicServiceId",
                table: "ClinicReserves",
                column: "ClinicServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserves_ClinicServicePackageId",
                table: "ClinicReserves",
                column: "ClinicServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserves_ClinicTimeId",
                table: "ClinicReserves",
                column: "ClinicTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserves_OwnerDecision_OwnerApprovalDeadline",
                table: "ClinicReserves",
                columns: new[] { "OwnerDecision", "OwnerApprovalDeadline" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserves_RebateId",
                table: "ClinicReserves",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserves_ReserveCode",
                table: "ClinicReserves",
                column: "ReserveCode",
                unique: true,
                filter: "[ReserveCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicReserves_UserPetId",
                table: "ClinicReserves",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_BackgroundPictureId",
                table: "Clinics",
                column: "BackgroundPictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_CityId_Active_Approved_Deleted",
                table: "Clinics",
                columns: new[] { "CityId", "Active", "Approved", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_IconId",
                table: "Clinics",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_LegacyCompanionId",
                table: "Clinics",
                column: "LegacyCompanionId",
                unique: true,
                filter: "[LegacyCompanionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_NeighborhoodId",
                table: "Clinics",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_OwnerId",
                table: "Clinics",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_PictureId",
                table: "Clinics",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_ProviderId_Deleted",
                table: "Clinics",
                columns: new[] { "ProviderId", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_ReferralCode",
                table: "Clinics",
                column: "ReferralCode",
                unique: true,
                filter: "[ReferralCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Clinics_StateId",
                table: "Clinics",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicServicePackagePictures_ClinicServicePackageId",
                table: "ClinicServicePackagePictures",
                column: "ClinicServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicServicePackagePictures_PictureId",
                table: "ClinicServicePackagePictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicServicePackages_ClinicServiceId",
                table: "ClinicServicePackages",
                column: "ClinicServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicServicePackages_PictureId",
                table: "ClinicServicePackages",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicServicePackageTypes_ClinicServicePackageId_Mode",
                table: "ClinicServicePackageTypes",
                columns: new[] { "ClinicServicePackageId", "Mode" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicServices_AssistanceId",
                table: "ClinicServices",
                column: "AssistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicServices_ClinicId_AssistanceId",
                table: "ClinicServices",
                columns: new[] { "ClinicId", "AssistanceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicServiceUsers_ClinicServiceId_UserId",
                table: "ClinicServiceUsers",
                columns: new[] { "ClinicServiceId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicServiceUsers_UserId",
                table: "ClinicServiceUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicTimes_ClinicId_WeekDayId",
                table: "ClinicTimes",
                columns: new[] { "ClinicId", "WeekDayId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicTimes_WeekDayId",
                table: "ClinicTimes",
                column: "WeekDayId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicTypes_ClinicId_TypeId",
                table: "ClinicTypes",
                columns: new[] { "ClinicId", "TypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicTypes_TypeId",
                table: "ClinicTypes",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicUsers_ClinicId_UserId",
                table: "ClinicUsers",
                columns: new[] { "ClinicId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicUsers_UserId",
                table: "ClinicUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicZones_CityId",
                table: "ClinicZones",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicZones_ClinicId_CityId",
                table: "ClinicZones",
                columns: new[] { "ClinicId", "CityId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicZones_NeighborhoodId",
                table: "ClinicZones",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicZones_StateId",
                table: "ClinicZones",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementClinics_ClinicReserveId",
                table: "SettlementClinics",
                column: "ClinicReserveId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementClinics_SettlementId",
                table: "SettlementClinics",
                column: "SettlementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_ClinicReserves_ClinicReserveId",
                table: "Wallets",
                column: "ClinicReserveId",
                principalTable: "ClinicReserves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            // داده‌ی اولیه: نوع پرداخت/تخفیف/امتیاز رزرو کلینیک (Codes) + الگوی پوش‌ها (PushTypes/PushPatterns/PushSettings).
            migrationBuilder.Sql("""
                DECLARE @PaymentTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'PaymentType_PansionReserve');
                IF @PaymentTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'PaymentType_ClinicReserve')
                    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
                    VALUES ('PaymentType_ClinicReserve', 'clinic-reserve', @PaymentTypeGroupId,
                            (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @PaymentTypeGroupId), 1, N'پرداخت رزرو کلینیک');

                DECLARE @RebateTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'RebateType_PansionReserve');
                IF @RebateTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'RebateType_ClinicReserve')
                    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
                    VALUES ('RebateType_ClinicReserve', 'clinic-reserve', @RebateTypeGroupId,
                            (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @RebateTypeGroupId), 1, N'تخفیف رزرو کلینیک');

                DECLARE @ScoreGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'ScoreTransactionType_PansionReserve');
                IF @ScoreGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'ScoreTransactionType_ClinicReserve')
                    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
                    VALUES ('ScoreTransactionType_ClinicReserve', 'clinic-reserve', @ScoreGroupId,
                            (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @ScoreGroupId), 1, N'رزرو کلینیک');
                """);

            migrationBuilder.Sql("""
                DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
                INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
                    (137, N'رزرو کلینیک منتظر تأیید (به کلینیک)', N'PushClinicReserveApprovalRequired', N'رزرو جدید منتظر تأیید', N'رزرو جدید {1} در {0} منتظر تأیید شماست.', N'/companionProfile/clinicReserve', N'clinic-reserve-approval'),
                    (138, N'رزرو کلینیک تأیید شد (به کاربر)', N'PushClinicReserveApproved', N'رزرو شما تأیید شد', N'رزرو {1} در {0} توسط کلینیک تأیید شد.', N'/reserve', N'clinic-reserve-approved'),
                    (139, N'رزرو کلینیک رد شد (به کاربر)', N'PushClinicReserveRejected', N'رزرو شما رد شد', N'رزرو شما در {0} رد شد و مبلغ به کیف پول شما برگشت. دلیل: {1}', N'/reserve', N'clinic-reserve-rejected'),
                    (140, N'کلینیک به رزرو پاسخ نداد، رزرو لغو شد (به کاربر)', N'PushClinicReserveExpired', N'رزرو شما لغو شد', N'کلینیک {0} در مهلت مقرر به رزرو شما پاسخ نداد؛ رزرو لغو و مبلغ به کیف پول شما برگشت.', N'/reserve', N'clinic-reserve-expired'),
                    (141, N'رزرو کلینیک توسط کاربر لغو شد (به کلینیک)', N'PushClinicReserveCancelledByBooker', N'رزرو لغو شد', N'رزرو {0} در تاریخ {1} توسط مشتری لغو شد.', N'/companionProfile/clinicReserve', N'clinic-reserve-cancelled'),
                    (142, N'کلینیک تأیید شد (به مالک)', N'PushClinicApproved', N'کلینیک شما تأیید شد', N'کلینیک {0} تأیید شد و اکنون در پاستیل نمایش داده می‌شود.', N'/companionProfile', N'clinic-approved'),
                    (143, N'کلینیک رد شد (به مالک)', N'PushClinicRejected', N'کلینیک شما رد شد', N'کلینیک {0} رد شد. دلیل: {1}', N'/companionProfile', N'clinic-rejected'),
                    (144, N'تغییر زمان رزرو کلینیک (به کاربر)', N'PushClinicReserveRescheduled', N'زمان رزرو شما تغییر کرد', N'زمان رزرو شما در {0} به {1} ساعت {2} تغییر کرد.', N'/reserve', N'clinic-reserve-rescheduled'),
                    (145, N'تغییر زمان رزرو کلینیک (به کلینیک)', N'PushClinicReserveRescheduledForClinic', N'زمان رزرو تغییر کرد', N'زمان رزرو {0} به {1} ساعت {2} تغییر کرد.', N'/companionProfile/clinicReserve', N'clinic-reserve-rescheduled-clinic'),
                    (146, N'دعوت به پرسنل کلینیک (به کاربر)', N'PushClinicStaffInvite', N'دعوت به همکاری', N'شما به پرسنل {0} دعوت شده‌اید. برای پذیرش وارد اپ شوید.', N'/', N'clinic-staff-invite'),
                    (147, N'لغو رزرو کلینیک توسط پشتیبانی (به کاربر)', N'PushClinicReserveCancelledByAdmin', N'رزرو شما لغو شد', N'رزرو شما در {0} توسط پشتیبانی لغو شد. دلیل: {1}', N'/reserve', N'clinic-reserve-cancelled-admin');

                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
                    THROW 51000, 'A clinic push type ID is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
                    THROW 51000, 'A clinic push label is already assigned to another ID.', 1;

                SET IDENTITY_INSERT PushTypes ON;
                INSERT INTO PushTypes (Id, Name, Label)
                SELECT s.Id, s.Name, s.Label FROM @Types s
                WHERE NOT EXISTS (SELECT 1 FROM PushTypes t WHERE t.Id = s.Id);
                SET IDENTITY_INSERT PushTypes OFF;

                INSERT INTO PushPatterns (PushTypeId, Title, Body, Url, Icon, Tag, IsActive)
                SELECT s.Id, s.Title, s.Body, s.Url, NULL, s.Tag, 1 FROM @Types s
                WHERE NOT EXISTS (SELECT 1 FROM PushPatterns p WHERE p.PushTypeId = s.Id);

                INSERT INTO PushSettings (PushPatternId, IsEnabled)
                SELECT pattern.Id, 1
                FROM PushPatterns pattern
                WHERE pattern.PushTypeId IN (137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147)
                  AND NOT EXISTS (SELECT 1 FROM PushSettings setting WHERE setting.PushPatternId = pattern.Id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE notification
                FROM PushNotifications notification
                INNER JOIN PushPatterns pattern ON pattern.Id = notification.PushPatternId
                WHERE pattern.PushTypeId IN (137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147);

                DELETE FROM PushPatterns WHERE PushTypeId IN (137, 138, 139, 140, 141, 142, 143, 144, 145, 146, 147);
                DELETE FROM PushTypes
                WHERE (Id = 137 AND Label = N'PushClinicReserveApprovalRequired')
                   OR (Id = 138 AND Label = N'PushClinicReserveApproved')
                   OR (Id = 139 AND Label = N'PushClinicReserveRejected')
                   OR (Id = 140 AND Label = N'PushClinicReserveExpired')
                   OR (Id = 141 AND Label = N'PushClinicReserveCancelledByBooker')
                   OR (Id = 142 AND Label = N'PushClinicApproved')
                   OR (Id = 143 AND Label = N'PushClinicRejected')
                   OR (Id = 144 AND Label = N'PushClinicReserveRescheduled')
                   OR (Id = 145 AND Label = N'PushClinicReserveRescheduledForClinic')
                   OR (Id = 146 AND Label = N'PushClinicStaffInvite')
                   OR (Id = 147 AND Label = N'PushClinicReserveCancelledByAdmin');

                DELETE FROM Codes WHERE Label = 'PaymentType_ClinicReserve' AND NOT EXISTS (SELECT 1 FROM Payments WHERE TypeId = Codes.Id);
                DELETE FROM Codes WHERE Label = 'RebateType_ClinicReserve' AND NOT EXISTS (SELECT 1 FROM Rebate WHERE TypeId = Codes.Id);
                DELETE FROM Codes WHERE Label = 'ScoreTransactionType_ClinicReserve' AND NOT EXISTS (SELECT 1 FROM ScoreTransactions WHERE TransactionTypeId = Codes.Id);
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_ClinicReserves_ClinicReserveId",
                table: "Wallets");

            migrationBuilder.DropTable(
                name: "ClinicComments");

            migrationBuilder.DropTable(
                name: "ClinicPets");

            migrationBuilder.DropTable(
                name: "ClinicPictures");

            migrationBuilder.DropTable(
                name: "ClinicReserveItems");

            migrationBuilder.DropTable(
                name: "ClinicServicePackagePictures");

            migrationBuilder.DropTable(
                name: "ClinicServicePackageTypes");

            migrationBuilder.DropTable(
                name: "ClinicServiceUsers");

            migrationBuilder.DropTable(
                name: "ClinicTypes");

            migrationBuilder.DropTable(
                name: "ClinicUsers");

            migrationBuilder.DropTable(
                name: "ClinicZones");

            migrationBuilder.DropTable(
                name: "SettlementClinics");

            migrationBuilder.DropTable(
                name: "ClinicReserves");

            migrationBuilder.DropTable(
                name: "ClinicServicePackages");

            migrationBuilder.DropTable(
                name: "ClinicTimes");

            migrationBuilder.DropTable(
                name: "ClinicServices");

            migrationBuilder.DropTable(
                name: "Clinics");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_ClinicReserveId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "ClinicReserveId",
                table: "Wallets");
        }
    }
}
