using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCoaches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CoachReserveId",
                table: "Wallets",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Coaches",
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
                    table.PrimaryKey("PK_Coaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coaches_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coaches_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coaches_Pictures_BackgroundPictureId",
                        column: x => x.BackgroundPictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coaches_Pictures_IconId",
                        column: x => x.IconId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coaches_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coaches_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coaches_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Coaches_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachPets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachId = table.Column<long>(type: "bigint", nullable: false),
                    PetId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachPets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachPets_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachPets_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachPictures_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachServices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_CoachServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachServices_Assistances_AssistanceId",
                        column: x => x.AssistanceId,
                        principalTable: "Assistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachServices_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachTimes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachId = table.Column<long>(type: "bigint", nullable: false),
                    WeekDayId = table.Column<long>(type: "bigint", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EndTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachTimes_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachTimes_WeekDays_WeekDayId",
                        column: x => x.WeekDayId,
                        principalTable: "WeekDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UserAccept = table.Column<bool>(type: "bit", nullable: true),
                    InviteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachUsers_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachZones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachId = table.Column<long>(type: "bigint", nullable: false),
                    StateId = table.Column<long>(type: "bigint", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachZones_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachZones_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachZones_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachZones_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachServicePackages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachServiceId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_CoachServicePackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachServicePackages_CoachServices_CoachServiceId",
                        column: x => x.CoachServiceId,
                        principalTable: "CoachServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachServicePackages_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachServiceUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachServiceId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachServiceUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachServiceUsers_CoachServices_CoachServiceId",
                        column: x => x.CoachServiceId,
                        principalTable: "CoachServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachServiceUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachReserves",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReserveCode = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    CoachId = table.Column<long>(type: "bigint", nullable: false),
                    CoachServiceId = table.Column<long>(type: "bigint", nullable: false),
                    CoachServicePackageId = table.Column<long>(type: "bigint", nullable: false),
                    BookerId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    AssigneeUserId = table.Column<long>(type: "bigint", nullable: true),
                    CoachTimeId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_CoachReserves", x => x.Id);
                    table.CheckConstraint("CK_CoachReserve_Dates", "[EndDate] >= [DoDate]");
                    table.CheckConstraint("CK_CoachReserve_Prices", "[Price] >= 0 AND [OnlineAmount] >= 0 AND [PaymentPrice] >= 0 AND [WalletPrice] >= 0 AND [RebatePrice] >= 0");
                    table.ForeignKey(
                        name: "FK_CoachReserves_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserves_CoachServicePackages_CoachServicePackageId",
                        column: x => x.CoachServicePackageId,
                        principalTable: "CoachServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserves_CoachServices_CoachServiceId",
                        column: x => x.CoachServiceId,
                        principalTable: "CoachServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserves_CoachTimes_CoachTimeId",
                        column: x => x.CoachTimeId,
                        principalTable: "CoachTimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserves_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserves_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserves_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserves_Users_AssigneeUserId",
                        column: x => x.AssigneeUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserves_Users_BookerId",
                        column: x => x.BookerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachServicePackagePictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachServicePackageId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachServicePackagePictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachServicePackagePictures_CoachServicePackages_CoachServicePackageId",
                        column: x => x.CoachServicePackageId,
                        principalTable: "CoachServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachServicePackagePictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachServicePackageTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachServicePackageId = table.Column<long>(type: "bigint", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PrePaymentPrice = table.Column<double>(type: "float", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachServicePackageTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachServicePackageTypes_CoachServicePackages_CoachServicePackageId",
                        column: x => x.CoachServicePackageId,
                        principalTable: "CoachServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CoachId = table.Column<long>(type: "bigint", nullable: false),
                    CoachReserveId = table.Column<long>(type: "bigint", nullable: true),
                    IsReserved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachComments_CoachReserves_CoachReserveId",
                        column: x => x.CoachReserveId,
                        principalTable: "CoachReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachComments_Coaches_CoachId",
                        column: x => x.CoachId,
                        principalTable: "Coaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachComments_Comments_Id",
                        column: x => x.Id,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoachReserveItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CoachReserveId = table.Column<long>(type: "bigint", nullable: false),
                    CoachServiceId = table.Column<long>(type: "bigint", nullable: false),
                    CoachServicePackageId = table.Column<long>(type: "bigint", nullable: false),
                    PackagePrice = table.Column<double>(type: "float", nullable: false),
                    OnlineAmount = table.Column<double>(type: "float", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ProviderShare = table.Column<double>(type: "float", nullable: false),
                    SiteShare = table.Column<double>(type: "float", nullable: false),
                    PackageName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachReserveItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachReserveItems_CoachReserves_CoachReserveId",
                        column: x => x.CoachReserveId,
                        principalTable: "CoachReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserveItems_CoachServicePackages_CoachServicePackageId",
                        column: x => x.CoachServicePackageId,
                        principalTable: "CoachServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoachReserveItems_CoachServices_CoachServiceId",
                        column: x => x.CoachServiceId,
                        principalTable: "CoachServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettlementCoaches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettlementId = table.Column<long>(type: "bigint", nullable: false),
                    CoachReserveId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementCoaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementCoaches_CoachReserves_CoachReserveId",
                        column: x => x.CoachReserveId,
                        principalTable: "CoachReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementCoaches_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CoachReserveId",
                table: "Wallets",
                column: "CoachReserveId",
                unique: true,
                filter: "[CoachReserveId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CoachComments_CoachId",
                table: "CoachComments",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachComments_CoachReserveId",
                table: "CoachComments",
                column: "CoachReserveId",
                unique: true,
                filter: "[CoachReserveId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_BackgroundPictureId",
                table: "Coaches",
                column: "BackgroundPictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_CityId_Active_Approved_Deleted",
                table: "Coaches",
                columns: new[] { "CityId", "Active", "Approved", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_IconId",
                table: "Coaches",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_LegacyCompanionId",
                table: "Coaches",
                column: "LegacyCompanionId",
                unique: true,
                filter: "[LegacyCompanionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_NeighborhoodId",
                table: "Coaches",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_OwnerId",
                table: "Coaches",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_PictureId",
                table: "Coaches",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_ProviderId_Deleted",
                table: "Coaches",
                columns: new[] { "ProviderId", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_ReferralCode",
                table: "Coaches",
                column: "ReferralCode",
                unique: true,
                filter: "[ReferralCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_StateId",
                table: "Coaches",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachPets_CoachId_PetId",
                table: "CoachPets",
                columns: new[] { "CoachId", "PetId" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachPets_PetId",
                table: "CoachPets",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachPictures_CoachId",
                table: "CoachPictures",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachPictures_PictureId",
                table: "CoachPictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserveItems_CoachReserveId",
                table: "CoachReserveItems",
                column: "CoachReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserveItems_CoachServiceId",
                table: "CoachReserveItems",
                column: "CoachServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserveItems_CoachServicePackageId",
                table: "CoachReserveItems",
                column: "CoachServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserves_AddressId",
                table: "CoachReserves",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserves_AssigneeUserId",
                table: "CoachReserves",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserves_BookerId_CreateDate",
                table: "CoachReserves",
                columns: new[] { "BookerId", "CreateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserves_CoachId_DoDate_EndDate",
                table: "CoachReserves",
                columns: new[] { "CoachId", "DoDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserves_CoachServiceId",
                table: "CoachReserves",
                column: "CoachServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserves_CoachServicePackageId",
                table: "CoachReserves",
                column: "CoachServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserves_CoachTimeId",
                table: "CoachReserves",
                column: "CoachTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserves_OwnerDecision_OwnerApprovalDeadline",
                table: "CoachReserves",
                columns: new[] { "OwnerDecision", "OwnerApprovalDeadline" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserves_RebateId",
                table: "CoachReserves",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserves_ReserveCode",
                table: "CoachReserves",
                column: "ReserveCode",
                unique: true,
                filter: "[ReserveCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CoachReserves_UserPetId",
                table: "CoachReserves",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachServicePackagePictures_CoachServicePackageId",
                table: "CoachServicePackagePictures",
                column: "CoachServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachServicePackagePictures_PictureId",
                table: "CoachServicePackagePictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachServicePackages_CoachServiceId",
                table: "CoachServicePackages",
                column: "CoachServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachServicePackages_PictureId",
                table: "CoachServicePackages",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachServicePackageTypes_CoachServicePackageId_Mode",
                table: "CoachServicePackageTypes",
                columns: new[] { "CoachServicePackageId", "Mode" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachServices_AssistanceId",
                table: "CoachServices",
                column: "AssistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachServices_CoachId_AssistanceId",
                table: "CoachServices",
                columns: new[] { "CoachId", "AssistanceId" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachServiceUsers_CoachServiceId_UserId",
                table: "CoachServiceUsers",
                columns: new[] { "CoachServiceId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachServiceUsers_UserId",
                table: "CoachServiceUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachTimes_CoachId_WeekDayId",
                table: "CoachTimes",
                columns: new[] { "CoachId", "WeekDayId" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachTimes_WeekDayId",
                table: "CoachTimes",
                column: "WeekDayId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachUsers_CoachId_UserId",
                table: "CoachUsers",
                columns: new[] { "CoachId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachUsers_UserId",
                table: "CoachUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachZones_CityId",
                table: "CoachZones",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachZones_CoachId_CityId",
                table: "CoachZones",
                columns: new[] { "CoachId", "CityId" });

            migrationBuilder.CreateIndex(
                name: "IX_CoachZones_NeighborhoodId",
                table: "CoachZones",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachZones_StateId",
                table: "CoachZones",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCoaches_CoachReserveId",
                table: "SettlementCoaches",
                column: "CoachReserveId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCoaches_SettlementId",
                table: "SettlementCoaches",
                column: "SettlementId");

            migrationBuilder.AddForeignKey(
                name: "FK_Wallets_CoachReserves_CoachReserveId",
                table: "Wallets",
                column: "CoachReserveId",
                principalTable: "CoachReserves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // داده‌ی اولیه: نوع پرداخت/تخفیف/امتیاز رزرو مربی (Codes) + الگوی پوش‌ها (PushTypes/PushPatterns/PushSettings).
            migrationBuilder.Sql("""
                DECLARE @PaymentTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'PaymentType_PansionReserve');
                IF @PaymentTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'PaymentType_CoachReserve')
                    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
                    VALUES ('PaymentType_CoachReserve', 'coach-reserve', @PaymentTypeGroupId,
                            (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @PaymentTypeGroupId), 1, N'پرداخت رزرو مربی');

                DECLARE @RebateTypeGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'RebateType_PansionReserve');
                IF @RebateTypeGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'RebateType_CoachReserve')
                    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
                    VALUES ('RebateType_CoachReserve', 'coach-reserve', @RebateTypeGroupId,
                            (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @RebateTypeGroupId), 1, N'تخفیف رزرو مربی');

                DECLARE @ScoreGroupId BIGINT = (SELECT TOP (1) CodeGroupId FROM Codes WHERE Label = 'ScoreTransactionType_PansionReserve');
                IF @ScoreGroupId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Codes WHERE Label = 'ScoreTransactionType_CoachReserve')
                    INSERT INTO Codes (Label, Value, CodeGroupId, Priority, Active, Name)
                    VALUES ('ScoreTransactionType_CoachReserve', 'coach-reserve', @ScoreGroupId,
                            (SELECT ISNULL(MAX(Priority), 0) + 1 FROM Codes WHERE CodeGroupId = @ScoreGroupId), 1, N'رزرو مربی');
                """);

            migrationBuilder.Sql("""
                DECLARE @Types TABLE (Id bigint, Name nvarchar(200), Label nvarchar(200), Title nvarchar(200), Body nvarchar(500), Url nvarchar(200), Tag nvarchar(200));
                INSERT INTO @Types (Id, Name, Label, Title, Body, Url, Tag) VALUES
                    (154, N'رزرو مربی منتظر تأیید (به مربی)', N'PushCoachReserveApprovalRequired', N'رزرو جدید منتظر تأیید', N'رزرو جدید {1} در {0} منتظر تأیید شماست.', N'/companionProfile/coachReserve', N'coach-reserve-approval'),
                    (155, N'رزرو مربی تأیید شد (به کاربر)', N'PushCoachReserveApproved', N'رزرو شما تأیید شد', N'رزرو {1} در {0} توسط مربی تأیید شد.', N'/reserve', N'coach-reserve-approved'),
                    (156, N'رزرو مربی رد شد (به کاربر)', N'PushCoachReserveRejected', N'رزرو شما رد شد', N'رزرو شما در {0} رد شد و مبلغ به کیف پول شما برگشت. دلیل: {1}', N'/reserve', N'coach-reserve-rejected'),
                    (157, N'مربی به رزرو پاسخ نداد، رزرو لغو شد (به کاربر)', N'PushCoachReserveExpired', N'رزرو شما لغو شد', N'مربی {0} در مهلت مقرر به رزرو شما پاسخ نداد؛ رزرو لغو و مبلغ به کیف پول شما برگشت.', N'/reserve', N'coach-reserve-expired'),
                    (158, N'رزرو مربی توسط کاربر لغو شد (به مربی)', N'PushCoachReserveCancelledByBooker', N'رزرو لغو شد', N'رزرو {0} در تاریخ {1} توسط مشتری لغو شد.', N'/companionProfile/coachReserve', N'coach-reserve-cancelled'),
                    (159, N'مربی تأیید شد (به مالک)', N'PushCoachApproved', N'مربی شما تأیید شد', N'مربی {0} تأیید شد و اکنون در پاستیل نمایش داده می‌شود.', N'/companionProfile', N'coach-approved'),
                    (160, N'مربی رد شد (به مالک)', N'PushCoachRejected', N'مربی شما رد شد', N'مربی {0} رد شد. دلیل: {1}', N'/companionProfile', N'coach-rejected'),
                    (161, N'تغییر زمان رزرو مربی (به کاربر)', N'PushCoachReserveRescheduled', N'زمان رزرو شما تغییر کرد', N'زمان رزرو شما در {0} به {1} ساعت {2} تغییر کرد.', N'/reserve', N'coach-reserve-rescheduled'),
                    (162, N'تغییر زمان رزرو مربی (به مربی)', N'PushCoachReserveRescheduledForCoach', N'زمان رزرو تغییر کرد', N'زمان رزرو {0} به {1} ساعت {2} تغییر کرد.', N'/companionProfile/coachReserve', N'coach-reserve-rescheduled-coach'),
                    (163, N'دعوت به پرسنل مربی (به کاربر)', N'PushCoachStaffInvite', N'دعوت به همکاری', N'شما به پرسنل {0} دعوت شده‌اید. برای پذیرش وارد اپ شوید.', N'/', N'coach-staff-invite'),
                    (164, N'لغو رزرو مربی توسط پشتیبانی (به کاربر)', N'PushCoachReserveCancelledByAdmin', N'رزرو شما لغو شد', N'رزرو شما در {0} توسط پشتیبانی لغو شد. دلیل: {1}', N'/reserve', N'coach-reserve-cancelled-admin');

                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Id = s.Id WHERE t.Label <> s.Label)
                    THROW 51000, 'A coach push type ID is already assigned to another label.', 1;
                IF EXISTS (SELECT 1 FROM @Types s INNER JOIN PushTypes t ON t.Label = s.Label WHERE t.Id <> s.Id)
                    THROW 51000, 'A coach push label is already assigned to another ID.', 1;

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
                WHERE pattern.PushTypeId IN (154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164)
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
                WHERE pattern.PushTypeId IN (154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164);

                DELETE setting
                FROM PushSettings setting
                INNER JOIN PushPatterns pattern ON pattern.Id = setting.PushPatternId
                WHERE pattern.PushTypeId IN (154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164);

                DELETE FROM PushPatterns WHERE PushTypeId IN (154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164);
                DELETE FROM PushTypes
                WHERE (Id = 154 AND Label = N'PushCoachReserveApprovalRequired')
                   OR (Id = 155 AND Label = N'PushCoachReserveApproved')
                   OR (Id = 156 AND Label = N'PushCoachReserveRejected')
                   OR (Id = 157 AND Label = N'PushCoachReserveExpired')
                   OR (Id = 158 AND Label = N'PushCoachReserveCancelledByBooker')
                   OR (Id = 159 AND Label = N'PushCoachApproved')
                   OR (Id = 160 AND Label = N'PushCoachRejected')
                   OR (Id = 161 AND Label = N'PushCoachReserveRescheduled')
                   OR (Id = 162 AND Label = N'PushCoachReserveRescheduledForCoach')
                   OR (Id = 163 AND Label = N'PushCoachStaffInvite')
                   OR (Id = 164 AND Label = N'PushCoachReserveCancelledByAdmin');

                DELETE FROM Codes WHERE Label = 'PaymentType_CoachReserve' AND NOT EXISTS (SELECT 1 FROM Payments WHERE TypeId = Codes.Id);
                DELETE FROM Codes WHERE Label = 'RebateType_CoachReserve' AND NOT EXISTS (SELECT 1 FROM Rebate WHERE TypeId = Codes.Id);
                DELETE FROM Codes WHERE Label = 'ScoreTransactionType_CoachReserve' AND NOT EXISTS (SELECT 1 FROM ScoreTransactions WHERE TransactionTypeId = Codes.Id);
                """);


            migrationBuilder.DropForeignKey(
                name: "FK_Wallets_CoachReserves_CoachReserveId",
                table: "Wallets");

            migrationBuilder.DropTable(
                name: "CoachComments");

            migrationBuilder.DropTable(
                name: "CoachPets");

            migrationBuilder.DropTable(
                name: "CoachPictures");

            migrationBuilder.DropTable(
                name: "CoachReserveItems");

            migrationBuilder.DropTable(
                name: "CoachServicePackagePictures");

            migrationBuilder.DropTable(
                name: "CoachServicePackageTypes");

            migrationBuilder.DropTable(
                name: "CoachServiceUsers");

            migrationBuilder.DropTable(
                name: "CoachUsers");

            migrationBuilder.DropTable(
                name: "CoachZones");

            migrationBuilder.DropTable(
                name: "SettlementCoaches");

            migrationBuilder.DropTable(
                name: "CoachReserves");

            migrationBuilder.DropTable(
                name: "CoachServicePackages");

            migrationBuilder.DropTable(
                name: "CoachTimes");

            migrationBuilder.DropTable(
                name: "CoachServices");

            migrationBuilder.DropTable(
                name: "Coaches");

            migrationBuilder.DropIndex(
                name: "IX_Wallets_CoachReserveId",
                table: "Wallets");

            migrationBuilder.DropColumn(
                name: "CoachReserveId",
                table: "Wallets");
        }
    }
}
