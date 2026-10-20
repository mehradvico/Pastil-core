using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGroomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groomers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProviderId = table.Column<long>(type: "bigint", nullable: true),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    LegacyCompanionId = table.Column<long>(type: "bigint", nullable: true),
                    IsPersonal = table.Column<bool>(type: "bit", nullable: false),
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
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Regulations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstablishedYear = table.Column<int>(type: "int", nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicenseApproved = table.Column<bool>(type: "bit", nullable: false),
                    InstagramUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ServiceAtHome = table.Column<bool>(type: "bit", nullable: false),
                    ServiceInSalon = table.Column<bool>(type: "bit", nullable: false),
                    PickupAvailable = table.Column<bool>(type: "bit", nullable: false),
                    PickupFee = table.Column<double>(type: "float", nullable: false),
                    HasParking = table.Column<bool>(type: "bit", nullable: false),
                    HasWaitingRoom = table.Column<bool>(type: "bit", nullable: false),
                    HasCamera = table.Column<bool>(type: "bit", nullable: false),
                    MaxConcurrentPets = table.Column<int>(type: "int", nullable: false),
                    StaffCount = table.Column<int>(type: "int", nullable: false),
                    PortfolioCount = table.Column<int>(type: "int", nullable: false),
                    PortfolioLikeCount = table.Column<int>(type: "int", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Groomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groomers_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groomers_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groomers_Pictures_BackgroundPictureId",
                        column: x => x.BackgroundPictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groomers_Pictures_IconId",
                        column: x => x.IconId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groomers_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groomers_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groomers_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groomers_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    GroomerId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerComments_Comments_Id",
                        column: x => x.Id,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroomerComments_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerPets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerId = table.Column<long>(type: "bigint", nullable: false),
                    PetId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerPets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerPets_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerPets_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerPictures_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerServices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_GroomerServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerServices_Assistances_AssistanceId",
                        column: x => x.AssistanceId,
                        principalTable: "Assistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerServices_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerTimes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerId = table.Column<long>(type: "bigint", nullable: false),
                    WeekDayId = table.Column<long>(type: "bigint", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EndTime = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerTimes_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerTimes_WeekDays_WeekDayId",
                        column: x => x.WeekDayId,
                        principalTable: "WeekDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UserAccept = table.Column<bool>(type: "bit", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerUsers_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerZones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerId = table.Column<long>(type: "bigint", nullable: false),
                    StateId = table.Column<long>(type: "bigint", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerZones_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerZones_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerZones_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerZones_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerPortfolioPosts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerId = table.Column<long>(type: "bigint", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PetId = table.Column<long>(type: "bigint", nullable: true),
                    GroomerServiceId = table.Column<long>(type: "bigint", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    CommentCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerPortfolioPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerPortfolioPosts_GroomerServices_GroomerServiceId",
                        column: x => x.GroomerServiceId,
                        principalTable: "GroomerServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerPortfolioPosts_Groomers_GroomerId",
                        column: x => x.GroomerId,
                        principalTable: "Groomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerPortfolioPosts_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerServicePackages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerServiceId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_GroomerServicePackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerServicePackages_GroomerServices_GroomerServiceId",
                        column: x => x.GroomerServiceId,
                        principalTable: "GroomerServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerServicePackages_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerServiceUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerServiceId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerServiceUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerServiceUsers_GroomerServices_GroomerServiceId",
                        column: x => x.GroomerServiceId,
                        principalTable: "GroomerServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerServiceUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerPortfolioComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerPortfolioComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerPortfolioComments_Comments_Id",
                        column: x => x.Id,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroomerPortfolioComments_GroomerPortfolioComments_ParentId",
                        column: x => x.ParentId,
                        principalTable: "GroomerPortfolioComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerPortfolioComments_GroomerPortfolioPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "GroomerPortfolioPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerPortfolioLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerPortfolioLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerPortfolioLikes_GroomerPortfolioPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "GroomerPortfolioPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerPortfolioLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerPortfolioPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerPortfolioPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerPortfolioPictures_GroomerPortfolioPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "GroomerPortfolioPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerPortfolioPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroomerServicePackagePictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroomerServicePackageId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroomerServicePackagePictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroomerServicePackagePictures_GroomerServicePackages_GroomerServicePackageId",
                        column: x => x.GroomerServicePackageId,
                        principalTable: "GroomerServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroomerServicePackagePictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerComments_GroomerId",
                table: "GroomerComments",
                column: "GroomerId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPets_GroomerId_PetId",
                table: "GroomerPets",
                columns: new[] { "GroomerId", "PetId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPets_PetId",
                table: "GroomerPets",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPictures_GroomerId",
                table: "GroomerPictures",
                column: "GroomerId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPictures_PictureId",
                table: "GroomerPictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPortfolioComments_ParentId",
                table: "GroomerPortfolioComments",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPortfolioComments_PostId_Deleted",
                table: "GroomerPortfolioComments",
                columns: new[] { "PostId", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPortfolioLikes_PostId_UserId",
                table: "GroomerPortfolioLikes",
                columns: new[] { "PostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPortfolioLikes_UserId",
                table: "GroomerPortfolioLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPortfolioPictures_PictureId",
                table: "GroomerPortfolioPictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPortfolioPictures_PostId_SortOrder",
                table: "GroomerPortfolioPictures",
                columns: new[] { "PostId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPortfolioPosts_GroomerId_Deleted_Approved_Active_Id",
                table: "GroomerPortfolioPosts",
                columns: new[] { "GroomerId", "Deleted", "Approved", "Active", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPortfolioPosts_GroomerServiceId",
                table: "GroomerPortfolioPosts",
                column: "GroomerServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerPortfolioPosts_PetId",
                table: "GroomerPortfolioPosts",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_BackgroundPictureId",
                table: "Groomers",
                column: "BackgroundPictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_CityId_Active_Approved_Deleted",
                table: "Groomers",
                columns: new[] { "CityId", "Active", "Approved", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_IconId",
                table: "Groomers",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_LegacyCompanionId",
                table: "Groomers",
                column: "LegacyCompanionId",
                unique: true,
                filter: "[LegacyCompanionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_NeighborhoodId",
                table: "Groomers",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_OwnerId",
                table: "Groomers",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_PictureId",
                table: "Groomers",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_ProviderId_Deleted",
                table: "Groomers",
                columns: new[] { "ProviderId", "Deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_ReferralCode",
                table: "Groomers",
                column: "ReferralCode",
                unique: true,
                filter: "[ReferralCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Groomers_StateId",
                table: "Groomers",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerServicePackagePictures_GroomerServicePackageId",
                table: "GroomerServicePackagePictures",
                column: "GroomerServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerServicePackagePictures_PictureId",
                table: "GroomerServicePackagePictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerServicePackages_GroomerServiceId",
                table: "GroomerServicePackages",
                column: "GroomerServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerServicePackages_PictureId",
                table: "GroomerServicePackages",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerServices_AssistanceId",
                table: "GroomerServices",
                column: "AssistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerServices_GroomerId_AssistanceId",
                table: "GroomerServices",
                columns: new[] { "GroomerId", "AssistanceId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerServiceUsers_GroomerServiceId_UserId",
                table: "GroomerServiceUsers",
                columns: new[] { "GroomerServiceId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerServiceUsers_UserId",
                table: "GroomerServiceUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerTimes_GroomerId_WeekDayId",
                table: "GroomerTimes",
                columns: new[] { "GroomerId", "WeekDayId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerTimes_WeekDayId",
                table: "GroomerTimes",
                column: "WeekDayId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerUsers_GroomerId_UserId",
                table: "GroomerUsers",
                columns: new[] { "GroomerId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerUsers_UserId",
                table: "GroomerUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerZones_CityId",
                table: "GroomerZones",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerZones_GroomerId_CityId",
                table: "GroomerZones",
                columns: new[] { "GroomerId", "CityId" });

            migrationBuilder.CreateIndex(
                name: "IX_GroomerZones_NeighborhoodId",
                table: "GroomerZones",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_GroomerZones_StateId",
                table: "GroomerZones",
                column: "StateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroomerComments");

            migrationBuilder.DropTable(
                name: "GroomerPets");

            migrationBuilder.DropTable(
                name: "GroomerPictures");

            migrationBuilder.DropTable(
                name: "GroomerPortfolioComments");

            migrationBuilder.DropTable(
                name: "GroomerPortfolioLikes");

            migrationBuilder.DropTable(
                name: "GroomerPortfolioPictures");

            migrationBuilder.DropTable(
                name: "GroomerServicePackagePictures");

            migrationBuilder.DropTable(
                name: "GroomerServiceUsers");

            migrationBuilder.DropTable(
                name: "GroomerTimes");

            migrationBuilder.DropTable(
                name: "GroomerUsers");

            migrationBuilder.DropTable(
                name: "GroomerZones");

            migrationBuilder.DropTable(
                name: "GroomerPortfolioPosts");

            migrationBuilder.DropTable(
                name: "GroomerServicePackages");

            migrationBuilder.DropTable(
                name: "GroomerServices");

            migrationBuilder.DropTable(
                name: "Groomers");
        }
    }
}
