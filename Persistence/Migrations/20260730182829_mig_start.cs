using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class mig_start : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankCards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardPrefix = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BaseDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<Point>(type: "geography", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fax = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClickUserGuid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodeGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactUsGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Roles = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactUsGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailHosts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Pop3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Smtp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pop3Port = table.Column<int>(type: "int", nullable: false),
                    SmtpPort = table.Column<int>(type: "int", nullable: false),
                    Ssl = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailHosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrginalName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Protected = table.Column<bool>(type: "bit", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hashtags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hashtags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessageTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pattern = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Newsletters",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Newsletters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NoticeTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Importance = table.Column<byte>(type: "tinyint", nullable: false),
                    NavigationTemplate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtpVerifies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Verify = table.Column<bool>(type: "bit", nullable: true),
                    TryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpVerifies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PastilAiPlans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    DailyChatLimit = table.Column<int>(type: "int", nullable: true),
                    DailyImageLimit = table.Column<int>(type: "int", nullable: true),
                    DailyAudioLimit = table.Column<int>(type: "int", nullable: true),
                    DailyVideoLimit = table.Column<int>(type: "int", nullable: true),
                    PurchaseEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreateDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilAiPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchReportReasons",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsDescriptionRequired = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchReportReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Area = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Controller = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMenu = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Permissions_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Permissions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Pets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuidName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Extension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrginalName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pictures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceCalculations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromTime = table.Column<int>(type: "int", nullable: false),
                    ToTime = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    StopPrice = table.Column<double>(type: "float", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceCalculations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PushTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReminderCycles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cycle = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReminderTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmsProviders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SiteUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ServiceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StaticPages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_StaticPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TripOptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripOptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TripStops",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripStops", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeekDays",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Number = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeekDays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Codes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodeGroupId = table.Column<long>(type: "bigint", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Codes_CodeGroups_CodeGroupId",
                        column: x => x.CodeGroupId,
                        principalTable: "CodeGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryId = table.Column<long>(type: "bigint", nullable: false),
                    Boundary = table.Column<Geometry>(type: "geography", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.Id);
                    table.ForeignKey(
                        name: "FK_States_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailAddresses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailHostId = table.Column<long>(type: "bigint", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailTitle = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailAddresses_EmailHosts_EmailHostId",
                        column: x => x.EmailHostId,
                        principalTable: "EmailHosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Smses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Receptor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSend = table.Column<bool>(type: "bit", nullable: false),
                    Token1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    StatusText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cost = table.Column<double>(type: "float", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SmsTypeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Smses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Smses_MessageTypes_SmsTypeId",
                        column: x => x.SmsTypeId,
                        principalTable: "MessageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assistances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsPersonal = table.Column<bool>(type: "bit", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assistances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assistances_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerficationUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Verfication2Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Banks_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SecondName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    IconId = table.Column<long>(type: "bigint", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Brands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Brands_Pictures_IconId",
                        column: x => x.IconId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Brands_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    IconId = table.Column<long>(type: "bigint", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Categories_Pictures_IconId",
                        column: x => x.IconId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Categories_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DiscountGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscountGroups_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NotifyMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotifyMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotifyMessages_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PetBreeds",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PetBreeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PetBreeds_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PetBreeds_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StoryGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoryGroups_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PushPatterns",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PushTypeId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushPatterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushPatterns_PushTypes_PushTypeId",
                        column: x => x.PushTypeId,
                        principalTable: "PushTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PermissionRole",
                columns: table => new
                {
                    PermissionsId = table.Column<long>(type: "bigint", nullable: false),
                    RolesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionRole", x => new { x.PermissionsId, x.RolesId });
                    table.ForeignKey(
                        name: "FK_PermissionRole_Permissions_PermissionsId",
                        column: x => x.PermissionsId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermissionRole_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NaturalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Locked = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expertise = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestCodeTryCount = table.Column<int>(type: "int", nullable: false),
                    IsFemale = table.Column<bool>(type: "bit", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    CurrentScore = table.Column<double>(type: "float", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SmsNumbers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SmsProviderId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsNumbers_SmsProviders_SmsProviderId",
                        column: x => x.SmsProviderId,
                        principalTable: "SmsProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Features",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Hide = table.Column<bool>(type: "bit", nullable: false),
                    InSearch = table.Column<bool>(type: "bit", nullable: false),
                    IsGroup = table.Column<bool>(type: "bit", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Features", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Features_Codes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MapKeys",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MapKeys_Codes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PushMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PushMessageTypeId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushMessages_Codes_PushMessageTypeId",
                        column: x => x.PushMessageTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PushMessages_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StateId = table.Column<long>(type: "bigint", nullable: false),
                    Boundary = table.Column<Geometry>(type: "geography", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Emails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Receptor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSend = table.Column<bool>(type: "bit", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    StatusText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailTypeId = table.Column<long>(type: "bigint", nullable: false),
                    EmailAddressId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Emails_EmailAddresses_EmailAddressId",
                        column: x => x.EmailAddressId,
                        principalTable: "EmailAddresses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Emails_MessageTypes_EmailTypeId",
                        column: x => x.EmailTypeId,
                        principalTable: "MessageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailAddressId = table.Column<long>(type: "bigint", nullable: false),
                    EmailTypeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailSettings_EmailAddresses_EmailAddressId",
                        column: x => x.EmailAddressId,
                        principalTable: "EmailAddresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmailSettings_MessageTypes_EmailTypeId",
                        column: x => x.EmailTypeId,
                        principalTable: "MessageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssistanceQuestionnaires",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    AssistanceId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistanceQuestionnaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssistanceQuestionnaires_Assistances_AssistanceId",
                        column: x => x.AssistanceId,
                        principalTable: "Assistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Merchants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankId = table.Column<long>(type: "bigint", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrivateKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TerminalKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MerchantNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Merchants_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Banners",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Picture2Id = table.Column<long>(type: "bigint", nullable: true),
                    ClickCount = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Banners_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Banners_Pictures_Picture2Id",
                        column: x => x.Picture2Id,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Banners_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BrandCategory",
                columns: table => new
                {
                    BrandsId = table.Column<long>(type: "bigint", nullable: false),
                    CategoriesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrandCategory", x => new { x.BrandsId, x.CategoriesId });
                    table.ForeignKey(
                        name: "FK_BrandCategory_Brands_BrandsId",
                        column: x => x.BrandsId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BrandCategory_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Details",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UiLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Details_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Details_Codes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Galleries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Galleries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Galleries_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Galleries_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Varieties",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    InSearch = table.Column<bool>(type: "bit", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Varieties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Varieties_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PushSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PushPatternId = table.Column<long>(type: "bigint", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushSettings_PushPatterns_PushPatternId",
                        column: x => x.PushPatternId,
                        principalTable: "PushPatterns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rate = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    DisLikeCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Codes_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ContactUses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    ContactUsGroupId = table.Column<long>(type: "bigint", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactUses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactUses_ContactUsGroups_ContactUsGroupId",
                        column: x => x.ContactUsGroupId,
                        principalTable: "ContactUsGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactUses_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContactUses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoticeTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ActorUserId = table.Column<long>(type: "bigint", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ReferenceId = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NavigationUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeduplicationKey = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreateDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArchiveDueAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArchivedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notices", x => x.Id);
                    table.CheckConstraint("CK_Notices_MetadataJson_IsJson", "[MetadataJson] IS NULL OR ISJSON([MetadataJson]) = 1");
                    table.ForeignKey(
                        name: "FK_Notices_NoticeTypes_NoticeTypeId",
                        column: x => x.NoticeTypeId,
                        principalTable: "NoticeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notices_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilAiConversations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreateDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilAiConversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilAiConversations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilAiDailyUsages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UsageDate = table.Column<DateTime>(type: "date", nullable: false),
                    ChatCount = table.Column<int>(type: "int", nullable: false),
                    ImageCount = table.Column<int>(type: "int", nullable: false),
                    AudioCount = table.Column<int>(type: "int", nullable: false),
                    VideoCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilAiDailyUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilAiDailyUsages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PublishDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    AdminConfirm = table.Column<bool>(type: "bit", nullable: true),
                    VisitCount = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubNews = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsOld = table.Column<bool>(type: "bit", nullable: false),
                    PictureUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: true),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Edited = table.Column<bool>(type: "bit", nullable: false),
                    CommentCount = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Posts_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Posts_Posts_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Posts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Posts_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Posts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PushSubscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    DeviceKey = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Endpoint = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    P256dh = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Auth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScoreTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoreTransactions_Codes_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScoreTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripAddresses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<Point>(type: "geography", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripAddresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserBankCards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShebaNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankCardId = table.Column<long>(type: "bigint", nullable: false),
                    CardHolderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    AdminDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBankCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBankCards_BankCards_BankCardId",
                        column: x => x.BankCardId,
                        principalTable: "BankCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserBankCards_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCategories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PetId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    PetBreedId = table.Column<long>(type: "bigint", nullable: true),
                    PetBreed2Id = table.Column<long>(type: "bigint", nullable: true),
                    IsMixBreed = table.Column<bool>(type: "bit", nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MicroChipCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMale = table.Column<bool>(type: "bit", nullable: false),
                    IsSterile = table.Column<bool>(type: "bit", nullable: false),
                    SpecificDisease = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpecificMedicene = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPets_PetBreeds_PetBreed2Id",
                        column: x => x.PetBreed2Id,
                        principalTable: "PetBreeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPets_PetBreeds_PetBreedId",
                        column: x => x.PetBreedId,
                        principalTable: "PetBreeds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPets_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPets_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenExp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SmsSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SmsNumberId = table.Column<long>(type: "bigint", nullable: false),
                    SmsTypeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsSettings_MessageTypes_SmsTypeId",
                        column: x => x.SmsTypeId,
                        principalTable: "MessageTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SmsSettings_SmsNumbers_SmsNumberId",
                        column: x => x.SmsNumberId,
                        principalTable: "SmsNumbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryFeature",
                columns: table => new
                {
                    CategoriesId = table.Column<long>(type: "bigint", nullable: false),
                    FeaturesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryFeature", x => new { x.CategoriesId, x.FeaturesId });
                    table.ForeignKey(
                        name: "FK_CategoryFeature_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CategoryFeature_Features_FeaturesId",
                        column: x => x.FeaturesId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeatureItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeatureId = table.Column<long>(type: "bigint", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureItems_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<Point>(type: "geography", nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Neighborhoods",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegionNumber = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    Boundary = table.Column<Geometry>(type: "geography", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Neighborhoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Neighborhoods_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<Point>(type: "geography", nullable: true),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    IconId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxDiscountPercent = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CommentCount = table.Column<int>(type: "int", nullable: false),
                    RateAvg = table.Column<double>(type: "float", nullable: false),
                    RateCount = table.Column<int>(type: "int", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Stores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stores_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stores_Codes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stores_Pictures_IconId",
                        column: x => x.IconId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Stores_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GalleryItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    GalleryId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GalleryItems_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GalleryItems_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "VarietyItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VarietyId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VarietyItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VarietyItems_Varieties_VarietyId",
                        column: x => x.VarietyId,
                        principalTable: "Varieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommentLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsLike = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CommentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentLikes_Comments_CommentId",
                        column: x => x.CommentId,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommentLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContactUsItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactUsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactUsItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactUsItems_ContactUses_ContactUsId",
                        column: x => x.ContactUsId,
                        principalTable: "ContactUses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NoticeReads",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoticeId = table.Column<long>(type: "bigint", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: false),
                    AdminNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadMode = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoticeReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoticeReads_Notices_NoticeId",
                        column: x => x.NoticeId,
                        principalTable: "Notices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NoticeReads_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PushNotifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    NoticeId = table.Column<long>(type: "bigint", nullable: true),
                    PushPatternId = table.Column<long>(type: "bigint", nullable: false),
                    IsSend = table.Column<bool>(type: "bit", nullable: false),
                    Token1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    StatusText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PushNotifications_Notices_NoticeId",
                        column: x => x.NoticeId,
                        principalTable: "Notices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PushNotifications_PushPatterns_PushPatternId",
                        column: x => x.PushPatternId,
                        principalTable: "PushPatterns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilAiMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<long>(type: "bigint", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    InputType = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PromptTokens = table.Column<int>(type: "int", nullable: true),
                    CompletionTokens = table.Column<int>(type: "int", nullable: true),
                    DurationMilliseconds = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilAiMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilAiMessages_PastilAiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "PastilAiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryPost",
                columns: table => new
                {
                    CategoriesId = table.Column<long>(type: "bigint", nullable: false),
                    PostsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryPost", x => new { x.CategoriesId, x.PostsId });
                    table.ForeignKey(
                        name: "FK_CategoryPost_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryPost_Posts_PostsId",
                        column: x => x.PostsId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HashtagPost",
                columns: table => new
                {
                    HashtagsId = table.Column<long>(type: "bigint", nullable: false),
                    postsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HashtagPost", x => new { x.HashtagsId, x.postsId });
                    table.ForeignKey(
                        name: "FK_HashtagPost_Hashtags_HashtagsId",
                        column: x => x.HashtagsId,
                        principalTable: "Hashtags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HashtagPost_Posts_postsId",
                        column: x => x.postsId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PostId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostComments_Comments_Id",
                        column: x => x.Id,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostComments_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostFiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    FileId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostFiles_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostFiles_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostPictures_Posts_PostId",
                        column: x => x.PostId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReminderTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ReminderCycleId = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastChecked = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_ReminderCycles_ReminderCycleId",
                        column: x => x.ReminderCycleId,
                        principalTable: "ReminderCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reminders_ReminderTypes_ReminderTypeId",
                        column: x => x.ReminderTypeId,
                        principalTable: "ReminderTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reminders_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPetPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPetPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPetPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPetPictures_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPetRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    OperatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPetRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPetRecords_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserPetRecords_Users_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Companions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsPersonal = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    GoldAccountDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SilverAccountDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SilverAccountCreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    BackgroundPictureId = table.Column<long>(type: "bigint", nullable: true),
                    IconId = table.Column<long>(type: "bigint", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    AddressValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommentCount = table.Column<int>(type: "int", nullable: false),
                    RateAvg = table.Column<double>(type: "float", nullable: false),
                    RateCount = table.Column<int>(type: "int", nullable: false),
                    SearchKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<Point>(type: "geography", nullable: true),
                    CodeId = table.Column<long>(type: "bigint", nullable: true),
                    PetId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Companions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Companions_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Companions_Codes_CodeId",
                        column: x => x.CodeId,
                        principalTable: "Codes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Companions_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Companions_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Companions_Pictures_BackgroundPictureId",
                        column: x => x.BackgroundPictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Companions_Pictures_IconId",
                        column: x => x.IconId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Companions_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Companions_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vehicle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicensePlateNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Rate = table.Column<int>(type: "int", nullable: false),
                    ProfilePictureId = table.Column<long>(type: "bigint", nullable: true),
                    CertificatePictureId = table.Column<long>(type: "bigint", nullable: true),
                    VehicleCardPictureId = table.Column<long>(type: "bigint", nullable: true),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    AdminDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Drivers_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Drivers_Codes_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Drivers_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Drivers_Pictures_CertificatePictureId",
                        column: x => x.CertificatePictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Drivers_Pictures_ProfilePictureId",
                        column: x => x.ProfilePictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Drivers_Pictures_VehicleCardPictureId",
                        column: x => x.VehicleCardPictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Drivers_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Parks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: false),
                    Suggested = table.Column<bool>(type: "bit", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    AddressValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<Point>(type: "geography", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parks_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Parks_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchProfiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    EnergyLevelId = table.Column<long>(type: "bigint", nullable: false),
                    SocialLevelId = table.Column<long>(type: "bigint", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    LiveLocation = table.Column<Point>(type: "geography", nullable: true),
                    CityId = table.Column<long>(type: "bigint", nullable: true),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: true),
                    AdminDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerificationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleteDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastActiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfiles_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchProfiles_Codes_EnergyLevelId",
                        column: x => x.EnergyLevelId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfiles_Codes_SocialLevelId",
                        column: x => x.SocialLevelId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfiles_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchProfiles_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserCurrentLocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Location = table.Column<Point>(type: "geography", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCurrentLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCurrentLocations_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserCurrentLocations_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserCurrentLocations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Deliveries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryTypeId = table.Column<long>(type: "bigint", nullable: false),
                    BasePrice = table.Column<double>(type: "float", nullable: false),
                    MinPriceForFree = table.Column<double>(type: "float", nullable: false),
                    MinCountForFree = table.Column<int>(type: "int", nullable: false),
                    MaxDays = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: true),
                    StateId = table.Column<long>(type: "bigint", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    AfterRent = table.Column<bool>(type: "bit", nullable: false),
                    StoreId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deliveries_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Deliveries_Codes_DeliveryTypeId",
                        column: x => x.DeliveryTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deliveries_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Deliveries_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SellLimitCount = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    BrandId = table.Column<long>(type: "bigint", nullable: true),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    CodeValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BasePrice = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    DiscountGroupId = table.Column<long>(type: "bigint", nullable: true),
                    DiscountEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SellCount = table.Column<int>(type: "int", nullable: false),
                    VisitCount = table.Column<int>(type: "int", nullable: false),
                    RateAvg = table.Column<double>(type: "float", nullable: false),
                    RateCount = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Location = table.Column<Point>(type: "geography", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    VarietyId = table.Column<long>(type: "bigint", nullable: true),
                    Variety2Id = table.Column<long>(type: "bigint", nullable: true),
                    StoreId = table.Column<long>(type: "bigint", nullable: true),
                    AdminDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Codes_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Codes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Codes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Codes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_DiscountGroups_DiscountGroupId",
                        column: x => x.DiscountGroupId,
                        principalTable: "DiscountGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Products_Varieties_Variety2Id",
                        column: x => x.Variety2Id,
                        principalTable: "Varieties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_Varieties_VarietyId",
                        column: x => x.VarietyId,
                        principalTable: "Varieties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StoreComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    StoreId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoreComments_Comments_Id",
                        column: x => x.Id,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoreComments_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoreUser",
                columns: table => new
                {
                    StoresId = table.Column<long>(type: "bigint", nullable: false),
                    UsersId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoreUser", x => new { x.StoresId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_StoreUser_Stores_StoresId",
                        column: x => x.StoresId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoreUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilAiAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    FileId = table.Column<long>(type: "bigint", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilAiAttachments", x => x.Id);
                    table.CheckConstraint("CK_PastilAiAttachments_OneMedia", "([PictureId] IS NOT NULL AND [FileId] IS NULL) OR ([PictureId] IS NULL AND [FileId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_PastilAiAttachments_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilAiAttachments_PastilAiMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "PastilAiMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilAiAttachments_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilAiProviderAttempts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AttemptOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMilliseconds = table.Column<long>(type: "bigint", nullable: true),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PromptTokens = table.Column<int>(type: "int", nullable: true),
                    CompletionTokens = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilAiProviderAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilAiProviderAttempts_PastilAiMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "PastilAiMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionAssistances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false),
                    AssistanceId = table.Column<long>(type: "bigint", nullable: false),
                    IsSinglePackage = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanionTypeId = table.Column<long>(type: "bigint", nullable: false),
                    Approved = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionAssistances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionAssistances_Assistances_AssistanceId",
                        column: x => x.AssistanceId,
                        principalTable: "Assistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionAssistances_Codes_CompanionTypeId",
                        column: x => x.CompanionTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionAssistances_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionComments_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionInsurancePackages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayCount = table.Column<int>(type: "int", nullable: false),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PetId = table.Column<long>(type: "bigint", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionInsurancePackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionInsurancePackages_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionInsurancePackages_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionPets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false),
                    PetId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionPets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionPets_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionPets_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false),
                    ReportValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionReports_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionTypes_Codes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionTypes_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    UserAccept = table.Column<bool>(type: "bit", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionUsers_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionZones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false),
                    StateId = table.Column<long>(type: "bigint", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    NeighborhoodId = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionZones_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionZones_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionZones_Neighborhoods_NeighborhoodId",
                        column: x => x.NeighborhoodId,
                        principalTable: "Neighborhoods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanionZones_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pansions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsSchool = table.Column<bool>(type: "bit", nullable: true),
                    CompanionId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Approve = table.Column<bool>(type: "bit", nullable: false),
                    StateId = table.Column<long>(type: "bigint", nullable: false),
                    CityId = table.Column<long>(type: "bigint", nullable: false),
                    Discription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommentCount = table.Column<int>(type: "int", nullable: false),
                    RateAvg = table.Column<double>(type: "float", nullable: false),
                    RateCount = table.Column<int>(type: "int", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Suggested = table.Column<bool>(type: "bit", nullable: false),
                    PansionPrice = table.Column<double>(type: "float", nullable: false),
                    SchoolPrice = table.Column<double>(type: "float", nullable: false),
                    Regulations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpenHour = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CloseHour = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DailyCommissionPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HourlyCommissionPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pansions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pansions_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pansions_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pansions_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pansions_States_StateId",
                        column: x => x.StateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Settlements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreId = table.Column<long>(type: "bigint", nullable: true),
                    CompanionId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserBankCardId = table.Column<long>(type: "bigint", nullable: false),
                    TrackingCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidPrice = table.Column<double>(type: "float", nullable: false),
                    ItemCount = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settlements_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Settlements_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Settlements_UserBankCards_UserBankCardId",
                        column: x => x.UserBankCardId,
                        principalTable: "UserBankCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DriverUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverUsers_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriverUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParkPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParkId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParkPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParkPictures_Parks_ParkId",
                        column: x => x.ParkId,
                        principalTable: "Parks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParkPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchProfileGoals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PastilMatchProfileId = table.Column<long>(type: "bigint", nullable: false),
                    PastilMatchGoalId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchProfileGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfileGoals_Codes_PastilMatchGoalId",
                        column: x => x.PastilMatchGoalId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfileGoals_PastilMatchProfiles_PastilMatchProfileId",
                        column: x => x.PastilMatchProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchProfileLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LikerProfileId = table.Column<long>(type: "bigint", nullable: false),
                    LikedProfileId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchProfileLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfileLikes_PastilMatchProfiles_LikedProfileId",
                        column: x => x.LikedProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchProfileLikes_PastilMatchProfiles_LikerProfileId",
                        column: x => x.LikerProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderProfileId = table.Column<long>(type: "bigint", nullable: false),
                    ReceiverProfileId = table.Column<long>(type: "bigint", nullable: false),
                    PastilMatchGoalId = table.Column<long>(type: "bigint", nullable: false),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompatibilityPercent = table.Column<int>(type: "int", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchRequests_Codes_PastilMatchGoalId",
                        column: x => x.PastilMatchGoalId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchRequests_Codes_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchRequests_PastilMatchProfiles_ReceiverProfileId",
                        column: x => x.ReceiverProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchRequests_PastilMatchProfiles_SenderProfileId",
                        column: x => x.SenderProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryDistances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromD = table.Column<double>(type: "float", nullable: false),
                    ToD = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    DeliveryId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryDistances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryDistances_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoryProduct",
                columns: table => new
                {
                    CategoriesId = table.Column<long>(type: "bigint", nullable: false),
                    ProductsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryProduct", x => new { x.CategoriesId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_CategoryProduct_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryProduct_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscussionQuestions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AdminConfirm = table.Column<bool>(type: "bit", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    AnswerCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscussionQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscussionQuestions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscussionQuestions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostProduct",
                columns: table => new
                {
                    PostsId = table.Column<long>(type: "bigint", nullable: false),
                    ProductsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostProduct", x => new { x.PostsId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_PostProduct_Posts_PostsId",
                        column: x => x.PostsId,
                        principalTable: "Posts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PostProduct_Products_ProductsId",
                        column: x => x.ProductsId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductComments_Comments_Id",
                        column: x => x.Id,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductComments_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductFeatureValues",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    FeatureItemId = table.Column<long>(type: "bigint", nullable: true),
                    FeatureId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFeatureValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductFeatureValues_FeatureItems_FeatureItemId",
                        column: x => x.FeatureItemId,
                        principalTable: "FeatureItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductFeatureValues_Features_FeatureId",
                        column: x => x.FeatureId,
                        principalTable: "Features",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductFeatureValues_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductFiles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    FileId = table.Column<long>(type: "bigint", nullable: true),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Protected = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductFiles_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductFiles_ProductFiles_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ProductFiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductFiles_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BasePrice = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    DiscountGroupId = table.Column<long>(type: "bigint", nullable: true),
                    DiscountEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VarietyItemId = table.Column<long>(type: "bigint", nullable: true),
                    VarietyItem2Id = table.Column<long>(type: "bigint", nullable: true),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    StoreId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Warranty = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SystemActive = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductItems_DiscountGroups_DiscountGroupId",
                        column: x => x.DiscountGroupId,
                        principalTable: "DiscountGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductItems_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductItems_VarietyItems_VarietyItem2Id",
                        column: x => x.VarietyItem2Id,
                        principalTable: "VarietyItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductItems_VarietyItems_VarietyItemId",
                        column: x => x.VarietyItemId,
                        principalTable: "VarietyItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductLikes_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductPictures_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductRelates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    RelatedProductId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductRelates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductRelates_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductRelates_Products_RelatedProductId",
                        column: x => x.RelatedProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportDetail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductReports_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rebate",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    CodeValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriceValue = table.Column<double>(type: "float", nullable: false),
                    MinCartPrice = table.Column<double>(type: "float", nullable: false),
                    StartDatetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDatetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPriceRebate = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    UseCount = table.Column<int>(type: "int", nullable: false),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    MaxUsePerUser = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: true),
                    ClubRewardId = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rebate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rebate_Codes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rebate_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rebate_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AdminId = table.Column<long>(type: "bigint", nullable: true),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    ImportanceId = table.Column<long>(type: "bigint", nullable: false),
                    TicketCategoryId = table.Column<long>(type: "bigint", nullable: false, defaultValue: 10139L),
                    ProductId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Codes_ImportanceId",
                        column: x => x.ImportanceId,
                        principalTable: "Codes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Codes_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Codes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Codes_TicketCategoryId",
                        column: x => x.TicketCategoryId,
                        principalTable: "Codes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserProducts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    SpotPlayerToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpotPlayerUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpotPlayerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProducts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserProducts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CodeCompanionAssistance",
                columns: table => new
                {
                    CodesId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionAssistancesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeCompanionAssistance", x => new { x.CodesId, x.CompanionAssistancesId });
                    table.ForeignKey(
                        name: "FK_CodeCompanionAssistance_Codes_CodesId",
                        column: x => x.CodesId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CodeCompanionAssistance_CompanionAssistances_CompanionAssistancesId",
                        column: x => x.CompanionAssistancesId,
                        principalTable: "CompanionAssistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanionAssistancePackages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PrePaymentPrice = table.Column<double>(type: "float", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanionAssistanceId = table.Column<long>(type: "bigint", nullable: false),
                    Discription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionAssistancePackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionAssistancePackages_CompanionAssistances_CompanionAssistanceId",
                        column: x => x.CompanionAssistanceId,
                        principalTable: "CompanionAssistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionAssistancePackages_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanionAssistanceReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionAssistanceId = table.Column<long>(type: "bigint", nullable: false),
                    ReportValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionAssistanceReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionAssistanceReports_CompanionAssistances_CompanionAssistanceId",
                        column: x => x.CompanionAssistanceId,
                        principalTable: "CompanionAssistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionAssistanceReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionAssistanceTimes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    WeekDayId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionAssistanceId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionAssistanceTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionAssistanceTimes_CompanionAssistances_CompanionAssistanceId",
                        column: x => x.CompanionAssistanceId,
                        principalTable: "CompanionAssistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionAssistanceTimes_WeekDays_WeekDayId",
                        column: x => x.WeekDayId,
                        principalTable: "WeekDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionAssistanceUsers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ActivationValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionAssistanceId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionAssistanceUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionAssistanceUsers_CompanionAssistances_CompanionAssistanceId",
                        column: x => x.CompanionAssistanceId,
                        principalTable: "CompanionAssistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionAssistanceUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PansionComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    PansionId = table.Column<long>(type: "bigint", nullable: false),
                    IsReserved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PansionComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PansionComments_Comments_Id",
                        column: x => x.Id,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PansionComments_Pansions_PansionId",
                        column: x => x.PansionId,
                        principalTable: "Pansions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PansionPets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PansionId = table.Column<long>(type: "bigint", nullable: false),
                    PetId = table.Column<long>(type: "bigint", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PansionPets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PansionPets_Pansions_PansionId",
                        column: x => x.PansionId,
                        principalTable: "Pansions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PansionPets_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PansionPictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PansionId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PansionPictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PansionPictures_Pansions_PansionId",
                        column: x => x.PansionId,
                        principalTable: "Pansions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PansionPictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoryItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanionId = table.Column<long>(type: "bigint", nullable: true),
                    StoreId = table.Column<long>(type: "bigint", nullable: true),
                    PansionId = table.Column<long>(type: "bigint", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    StoryGroupId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: true),
                    FileId = table.Column<long>(type: "bigint", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    DayCount = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoryItems_Companions_CompanionId",
                        column: x => x.CompanionId,
                        principalTable: "Companions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StoryItems_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StoryItems_Pansions_PansionId",
                        column: x => x.PansionId,
                        principalTable: "Pansions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StoryItems_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StoryItems_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StoryItems_StoryGroups_StoryGroupId",
                        column: x => x.StoryGroupId,
                        principalTable: "StoryGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatches",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PastilMatchRequestId = table.Column<long>(type: "bigint", nullable: false),
                    FirstProfileId = table.Column<long>(type: "bigint", nullable: false),
                    SecondProfileId = table.Column<long>(type: "bigint", nullable: false),
                    PastilMatchGoalId = table.Column<long>(type: "bigint", nullable: false),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    CompatibilityPercent = table.Column<int>(type: "int", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatches_Codes_PastilMatchGoalId",
                        column: x => x.PastilMatchGoalId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatches_Codes_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatches_PastilMatchProfiles_FirstProfileId",
                        column: x => x.FirstProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatches_PastilMatchProfiles_SecondProfileId",
                        column: x => x.SecondProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatches_PastilMatchRequests_PastilMatchRequestId",
                        column: x => x.PastilMatchRequestId,
                        principalTable: "PastilMatchRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscussionAnswers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscussionQuestionId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    DisLikeCount = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscussionAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscussionAnswers_DiscussionQuestions_DiscussionQuestionId",
                        column: x => x.DiscussionQuestionId,
                        principalTable: "DiscussionQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscussionAnswers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    DiscountGroupId = table.Column<long>(type: "bigint", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoreId = table.Column<long>(type: "bigint", nullable: false),
                    Percent = table.Column<int>(type: "int", nullable: false),
                    Synced = table.Column<bool>(type: "bit", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: true),
                    BrandId = table.Column<long>(type: "bigint", nullable: true),
                    ProductId = table.Column<long>(type: "bigint", nullable: true),
                    ProductItemId = table.Column<long>(type: "bigint", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Discounts_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Discounts_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Discounts_Codes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Discounts_DiscountGroups_DiscountGroupId",
                        column: x => x.DiscountGroupId,
                        principalTable: "DiscountGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Discounts_ProductItems_ProductItemId",
                        column: x => x.ProductItemId,
                        principalTable: "ProductItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Discounts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Discounts_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cargoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateGone = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateReturn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FromStateId = table.Column<long>(type: "bigint", nullable: false),
                    ToStateId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    Accompany = table.Column<bool>(type: "bit", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    FromWallet = table.Column<bool>(type: "bit", nullable: false),
                    WalletPrice = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: true),
                    RebatePrice = table.Column<double>(type: "float", nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    DefaultPrice = table.Column<double>(type: "float", nullable: false),
                    NotAccompanyPrice = table.Column<double>(type: "float", nullable: true),
                    ReturnPrice = table.Column<double>(type: "float", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    StatusDetail = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cargoes_Codes_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cargoes_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cargoes_States_FromStateId",
                        column: x => x.FromStateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cargoes_States_ToStateId",
                        column: x => x.ToStateId,
                        principalTable: "States",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cargoes_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UniqueId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressId = table.Column<long>(type: "bigint", nullable: true),
                    MerchantId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemCount = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    RebateId = table.Column<long>(type: "bigint", nullable: true),
                    ReferralCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RebatePrice = table.Column<double>(type: "float", nullable: false),
                    DeliveryId = table.Column<long>(type: "bigint", nullable: true),
                    DeliveryPrice = table.Column<double>(type: "float", nullable: false),
                    Changed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Carts_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Carts_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Carts_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Carts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClubRewards",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequiredScore = table.Column<double>(type: "float", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: false),
                    ValidityDays = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    RebateId1 = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubRewards_Rebate_RebateId1",
                        column: x => x.RebateId1,
                        principalTable: "Rebate",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanionInsurancePackageSales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionInsurancePackageId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false),
                    FromWallet = table.Column<bool>(type: "bit", nullable: false),
                    WalletPrice = table.Column<double>(type: "float", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    AddressId = table.Column<long>(type: "bigint", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: true),
                    RebatePrice = table.Column<double>(type: "float", nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    ManualPayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionInsurancePackageSales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionInsurancePackageSales_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionInsurancePackageSales_CompanionInsurancePackages_CompanionInsurancePackageId",
                        column: x => x.CompanionInsurancePackageId,
                        principalTable: "CompanionInsurancePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionInsurancePackageSales_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanionInsurancePackageSales_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PansionReserves",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PansionId = table.Column<long>(type: "bigint", nullable: false),
                    BookerId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetId = table.Column<long>(type: "bigint", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    FromWallet = table.Column<bool>(type: "bit", nullable: false),
                    WalletPrice = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false),
                    IsReserved = table.Column<bool>(type: "bit", nullable: false),
                    IsCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SchoolCreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: true),
                    RebatePrice = table.Column<double>(type: "float", nullable: false),
                    StatusId = table.Column<long>(type: "bigint", nullable: false),
                    HourCount = table.Column<int>(type: "int", nullable: false),
                    DayCount = table.Column<int>(type: "int", nullable: false),
                    CompanionShare = table.Column<double>(type: "float", nullable: false),
                    SiteShare = table.Column<double>(type: "float", nullable: false),
                    Permitted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PansionReserves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PansionReserves_Codes_StatusId",
                        column: x => x.StatusId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PansionReserves_Pansions_PansionId",
                        column: x => x.PansionId,
                        principalTable: "Pansions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PansionReserves_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PansionReserves_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PansionReserves_Users_BookerId",
                        column: x => x.BookerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductOrders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AddressId = table.Column<long>(type: "bigint", nullable: true),
                    PaymentTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ProductOrderStatusId = table.Column<long>(type: "bigint", nullable: false),
                    ProductOrderStateId = table.Column<long>(type: "bigint", nullable: false),
                    ReferralCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<double>(type: "float", nullable: false),
                    BasePrice = table.Column<double>(type: "float", nullable: false),
                    DiscountPrice = table.Column<double>(type: "float", nullable: false),
                    WalletPrice = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false),
                    SiteShare = table.Column<double>(type: "float", nullable: false),
                    StoreShare = table.Column<double>(type: "float", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StateDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: true),
                    RebatePrice = table.Column<double>(type: "float", nullable: false),
                    DeliveryTypeId = table.Column<long>(type: "bigint", nullable: true),
                    DeliveryPrice = table.Column<double>(type: "float", nullable: false),
                    TrackingCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CancelRequestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentOrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChildOrderId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Permitted = table.Column<bool>(type: "bit", nullable: false),
                    ReserveDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOrders_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductOrders_Codes_DeliveryTypeId",
                        column: x => x.DeliveryTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductOrders_Codes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductOrders_Codes_ProductOrderStateId",
                        column: x => x.ProductOrderStateId,
                        principalTable: "Codes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductOrders_Codes_ProductOrderStatusId",
                        column: x => x.ProductOrderStatusId,
                        principalTable: "Codes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductOrders_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductOrders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Origin = table.Column<Point>(type: "geography", nullable: true),
                    Destination = table.Column<Point>(type: "geography", nullable: true),
                    SecondDestination = table.Column<Point>(type: "geography", nullable: true),
                    RouteLength = table.Column<long>(type: "bigint", nullable: false),
                    FromCityId = table.Column<long>(type: "bigint", nullable: true),
                    RoundTrip = table.Column<bool>(type: "bit", nullable: false),
                    FromAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DriverId = table.Column<long>(type: "bigint", nullable: true),
                    Price = table.Column<double>(type: "float", nullable: false),
                    UserDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserRate = table.Column<int>(type: "int", nullable: false),
                    ConnectionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TripStartDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    DriverStatusId = table.Column<long>(type: "bigint", nullable: false),
                    TripStatusId = table.Column<long>(type: "bigint", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    ManualPayDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserPetId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TripStopId = table.Column<long>(type: "bigint", nullable: true),
                    DriverShare = table.Column<double>(type: "float", nullable: false),
                    SiteShare = table.Column<double>(type: "float", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: true),
                    RebatePrice = table.Column<double>(type: "float", nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false),
                    FromWallet = table.Column<bool>(type: "bit", nullable: false),
                    WalletPrice = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trips_Cities_FromCityId",
                        column: x => x.FromCityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trips_Codes_DriverStatusId",
                        column: x => x.DriverStatusId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trips_Codes_TripStatusId",
                        column: x => x.TripStatusId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trips_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trips_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trips_TripStops_TripStopId",
                        column: x => x.TripStopId,
                        principalTable: "TripStops",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trips_UserPets_UserPetId",
                        column: x => x.UserPetId,
                        principalTable: "UserPets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trips_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserRebates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRebates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRebates_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRebates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TicketId = table.Column<long>(type: "bigint", nullable: false),
                    FileId = table.Column<long>(type: "bigint", nullable: true),
                    ReplyToTicketItemId = table.Column<long>(type: "bigint", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSeen = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SeenDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketItems_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketItems_TicketItems_ReplyToTicketItemId",
                        column: x => x.ReplyToTicketItemId,
                        principalTable: "TicketItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketItems_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionAssistancePackagePictures",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionAssistancePackageId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionAssistancePackagePictures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionAssistancePackagePictures_CompanionAssistancePackages_CompanionAssistancePackageId",
                        column: x => x.CompanionAssistancePackageId,
                        principalTable: "CompanionAssistancePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionAssistancePackagePictures_Pictures_PictureId",
                        column: x => x.PictureId,
                        principalTable: "Pictures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionReserves",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookerId = table.Column<long>(type: "bigint", nullable: false),
                    PrePaymentPrice = table.Column<double>(type: "float", nullable: false),
                    OperatorFinalPrice = table.Column<double>(type: "float", nullable: false),
                    OperatorStuffPrice = table.Column<double>(type: "float", nullable: false),
                    OperatorWagesPrice = table.Column<double>(type: "float", nullable: false),
                    FromWallet = table.Column<bool>(type: "bit", nullable: false),
                    WalletPrice = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false),
                    PackagePrice = table.Column<double>(type: "float", nullable: false),
                    AddressId = table.Column<long>(type: "bigint", nullable: true),
                    CompanionAssistanceId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionAssistanceTypeId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionAssistanceTimeId = table.Column<long>(type: "bigint", nullable: true),
                    CompanionAssistanceUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsFemale = table.Column<bool>(type: "bit", nullable: true),
                    BookerDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssistanceDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsReserved = table.Column<bool>(type: "bit", nullable: false),
                    IsCancel = table.Column<bool>(type: "bit", nullable: false),
                    CancelDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StateId = table.Column<long>(type: "bigint", nullable: false),
                    DoDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DoneDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperatorStateId = table.Column<long>(type: "bigint", nullable: false),
                    OperatorChangeStateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OperatorDetail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserResponse = table.Column<bool>(type: "bit", nullable: true),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    RebateId = table.Column<long>(type: "bigint", nullable: true),
                    RebatePrice = table.Column<double>(type: "float", nullable: false),
                    CompanionShare = table.Column<double>(type: "float", nullable: false),
                    SiteShare = table.Column<double>(type: "float", nullable: false),
                    Permitted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionReserves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionReserves_Addresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanionReserves_Codes_CompanionAssistanceTypeId",
                        column: x => x.CompanionAssistanceTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReserves_Codes_OperatorStateId",
                        column: x => x.OperatorStateId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReserves_Codes_StateId",
                        column: x => x.StateId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReserves_CompanionAssistanceTimes_CompanionAssistanceTimeId",
                        column: x => x.CompanionAssistanceTimeId,
                        principalTable: "CompanionAssistanceTimes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanionReserves_CompanionAssistanceUsers_CompanionAssistanceUserId",
                        column: x => x.CompanionAssistanceUserId,
                        principalTable: "CompanionAssistanceUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanionReserves_CompanionAssistances_CompanionAssistanceId",
                        column: x => x.CompanionAssistanceId,
                        principalTable: "CompanionAssistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReserves_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanionReserves_Users_BookerId",
                        column: x => x.BookerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoryUserLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoryItemId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryUserLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoryUserLikes_StoryItems_StoryItemId",
                        column: x => x.StoryItemId,
                        principalTable: "StoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoryUserLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchBlocks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlockerUserId = table.Column<long>(type: "bigint", nullable: false),
                    BlockedUserId = table.Column<long>(type: "bigint", nullable: false),
                    PastilMatchId = table.Column<long>(type: "bigint", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchBlocks_PastilMatches_PastilMatchId",
                        column: x => x.PastilMatchId,
                        principalTable: "PastilMatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchBlocks_Users_BlockedUserId",
                        column: x => x.BlockedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchBlocks_Users_BlockerUserId",
                        column: x => x.BlockerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PastilMatchId = table.Column<long>(type: "bigint", nullable: false),
                    SenderProfileId = table.Column<long>(type: "bigint", nullable: true),
                    PastilMatchMessageTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ReplyToMessageId = table.Column<long>(type: "bigint", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    EditDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    PinDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchMessages_Codes_PastilMatchMessageTypeId",
                        column: x => x.PastilMatchMessageTypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchMessages_PastilMatchMessages_ReplyToMessageId",
                        column: x => x.ReplyToMessageId,
                        principalTable: "PastilMatchMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchMessages_PastilMatchProfiles_SenderProfileId",
                        column: x => x.SenderProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchMessages_PastilMatches_PastilMatchId",
                        column: x => x.PastilMatchId,
                        principalTable: "PastilMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DiscussionAnswerLikes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsLike = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DiscussionAnswerId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscussionAnswerLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscussionAnswerLikes_DiscussionAnswers_DiscussionAnswerId",
                        column: x => x.DiscussionAnswerId,
                        principalTable: "DiscussionAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscussionAnswerLikes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CartStores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<long>(type: "bigint", nullable: false),
                    ItemCount = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    StoreId = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryId = table.Column<long>(type: "bigint", nullable: true),
                    DeliveryPrice = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartStores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartStores_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CartStores_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CartStores_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MerchantId = table.Column<long>(type: "bigint", nullable: true),
                    ProductOrderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CompanionReserveId = table.Column<long>(type: "bigint", nullable: true),
                    TripId = table.Column<long>(type: "bigint", nullable: true),
                    CargoId = table.Column<long>(type: "bigint", nullable: true),
                    CompanionInsurancePackageSaleId = table.Column<long>(type: "bigint", nullable: true),
                    RefNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: true),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    FileId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    CallBackTypeLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CallBackId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GatewayStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Codes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "Codes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalTable: "Merchants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_ProductOrders_ProductOrderId",
                        column: x => x.ProductOrderId,
                        principalTable: "ProductOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductOrderStores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<double>(type: "float", nullable: false),
                    BasePrice = table.Column<double>(type: "float", nullable: false),
                    DiscountPrice = table.Column<double>(type: "float", nullable: false),
                    StoreId = table.Column<long>(type: "bigint", nullable: false),
                    ProductOrderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Edited = table.Column<bool>(type: "bit", nullable: false),
                    DeliveryId = table.Column<long>(type: "bigint", nullable: true),
                    DeliveryPrice = table.Column<double>(type: "float", nullable: false),
                    PaymentPrice = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOrderStores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOrderStores_Deliveries_DeliveryId",
                        column: x => x.DeliveryId,
                        principalTable: "Deliveries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductOrderStores_ProductOrders_ProductOrderId",
                        column: x => x.ProductOrderId,
                        principalTable: "ProductOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductOrderStores_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettlementStores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductOrderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SettlementId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementStores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementStores_ProductOrders_ProductOrderId",
                        column: x => x.ProductOrderId,
                        principalTable: "ProductOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SettlementStores_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripTripOption",
                columns: table => new
                {
                    TripOptionsId = table.Column<long>(type: "bigint", nullable: false),
                    TripsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripTripOption", x => new { x.TripOptionsId, x.TripsId });
                    table.ForeignKey(
                        name: "FK_TripTripOption_TripOptions_TripOptionsId",
                        column: x => x.TripOptionsId,
                        principalTable: "TripOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripTripOption_Trips_TripsId",
                        column: x => x.TripsId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionAssistancePackageCompanionReserve",
                columns: table => new
                {
                    CompanionAssistancePackagesId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionReservesId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionAssistancePackageCompanionReserve", x => new { x.CompanionAssistancePackagesId, x.CompanionReservesId });
                    table.ForeignKey(
                        name: "FK_CompanionAssistancePackageCompanionReserve_CompanionAssistancePackages_CompanionAssistancePackagesId",
                        column: x => x.CompanionAssistancePackagesId,
                        principalTable: "CompanionAssistancePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionAssistancePackageCompanionReserve_CompanionReserves_CompanionReservesId",
                        column: x => x.CompanionReservesId,
                        principalTable: "CompanionReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionReserveComments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    CompanionReserveId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionReserveComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionReserveComments_Comments_Id",
                        column: x => x.Id,
                        principalTable: "Comments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanionReserveComments_CompanionReserves_CompanionReserveId",
                        column: x => x.CompanionReserveId,
                        principalTable: "CompanionReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionReserveUserPet",
                columns: table => new
                {
                    CompanionReservesId = table.Column<long>(type: "bigint", nullable: false),
                    UserPetsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionReserveUserPet", x => new { x.CompanionReservesId, x.UserPetsId });
                    table.ForeignKey(
                        name: "FK_CompanionReserveUserPet_CompanionReserves_CompanionReservesId",
                        column: x => x.CompanionReservesId,
                        principalTable: "CompanionReserves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReserveUserPet_UserPets_UserPetsId",
                        column: x => x.UserPetsId,
                        principalTable: "UserPets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SettlementCompanions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanionReserveId = table.Column<long>(type: "bigint", nullable: true),
                    PansionReserveId = table.Column<long>(type: "bigint", nullable: true),
                    SettlementId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementCompanions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementCompanions_CompanionReserves_CompanionReserveId",
                        column: x => x.CompanionReserveId,
                        principalTable: "CompanionReserves",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SettlementCompanions_PansionReserves_PansionReserveId",
                        column: x => x.PansionReserveId,
                        principalTable: "PansionReserves",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SettlementCompanions_Settlements_SettlementId",
                        column: x => x.SettlementId,
                        principalTable: "Settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchMessageAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PastilMatchMessageId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchMessageAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchMessageAttachments_PastilMatchMessages_PastilMatchMessageId",
                        column: x => x.PastilMatchMessageId,
                        principalTable: "PastilMatchMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchMessageReactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PastilMatchMessageId = table.Column<long>(type: "bigint", nullable: false),
                    ReactorProfileId = table.Column<long>(type: "bigint", nullable: false),
                    Reaction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchMessageReactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchMessageReactions_PastilMatchMessages_PastilMatchMessageId",
                        column: x => x.PastilMatchMessageId,
                        principalTable: "PastilMatchMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchMessageReactions_PastilMatchProfiles_ReactorProfileId",
                        column: x => x.ReactorProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PastilMatchReports",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReporterUserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportedUserId = table.Column<long>(type: "bigint", nullable: false),
                    ReportedProfileId = table.Column<long>(type: "bigint", nullable: true),
                    PastilMatchId = table.Column<long>(type: "bigint", nullable: true),
                    PastilMatchMessageId = table.Column<long>(type: "bigint", nullable: true),
                    PastilMatchReportReasonId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdminDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilMatchReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_PastilMatchMessages_PastilMatchMessageId",
                        column: x => x.PastilMatchMessageId,
                        principalTable: "PastilMatchMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_PastilMatchProfiles_ReportedProfileId",
                        column: x => x.ReportedProfileId,
                        principalTable: "PastilMatchProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_PastilMatchReportReasons_PastilMatchReportReasonId",
                        column: x => x.PastilMatchReportReasonId,
                        principalTable: "PastilMatchReportReasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_PastilMatches_PastilMatchId",
                        column: x => x.PastilMatchId,
                        principalTable: "PastilMatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_Users_ReportedUserId",
                        column: x => x.ReportedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilMatchReports_Users_ReporterUserId",
                        column: x => x.ReporterUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CartStoreId = table.Column<long>(type: "bigint", nullable: false),
                    ProductItemId = table.Column<long>(type: "bigint", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_CartStores_CartStoreId",
                        column: x => x.CartStoreId,
                        principalTable: "CartStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CartItems_ProductItems_ProductItemId",
                        column: x => x.ProductItemId,
                        principalTable: "ProductItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CartItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PastilAiSubscriptions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    PlanId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentId = table.Column<long>(type: "bigint", nullable: true),
                    RebateId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PriceSnapshot = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RebatePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FromWallet = table.Column<bool>(type: "bit", nullable: false),
                    WalletPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreateDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PastilAiSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PastilAiSubscriptions_PastilAiPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "PastilAiPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilAiSubscriptions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilAiSubscriptions_Rebate_RebateId",
                        column: x => x.RebateId,
                        principalTable: "Rebate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PastilAiSubscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductOrderItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductItemId = table.Column<long>(type: "bigint", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    BasePrice = table.Column<double>(type: "float", nullable: false),
                    DiscountPrice = table.Column<double>(type: "float", nullable: false),
                    DiscountPercent = table.Column<int>(type: "int", nullable: false),
                    ProductOrderStoreId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Edited = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductOrderItems_ProductItems_ProductItemId",
                        column: x => x.ProductItemId,
                        principalTable: "ProductItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductOrderItems_ProductOrderStores_ProductOrderStoreId",
                        column: x => x.ProductOrderStoreId,
                        principalTable: "ProductOrderStores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanionReserveCommentRates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rate = table.Column<int>(type: "int", nullable: false),
                    AssistanceQuestionnaireId = table.Column<long>(type: "bigint", nullable: false),
                    CompanionReserveCommentId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanionReserveCommentRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanionReserveCommentRates_AssistanceQuestionnaires_AssistanceQuestionnaireId",
                        column: x => x.AssistanceQuestionnaireId,
                        principalTable: "AssistanceQuestionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanionReserveCommentRates_CompanionReserveComments_CompanionReserveCommentId",
                        column: x => x.CompanionReserveCommentId,
                        principalTable: "CompanionReserveComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsIncrease = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentId = table.Column<long>(type: "bigint", nullable: true),
                    ProductOrderId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CompanionReserveId = table.Column<long>(type: "bigint", nullable: true),
                    PansionReserveId = table.Column<long>(type: "bigint", nullable: true),
                    TripId = table.Column<long>(type: "bigint", nullable: true),
                    CargoId = table.Column<long>(type: "bigint", nullable: true),
                    CompanionInsurancePackageSaleId = table.Column<long>(type: "bigint", nullable: true),
                    PastilAiSubscriptionId = table.Column<long>(type: "bigint", nullable: true),
                    Painding = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_Cargoes_CargoId",
                        column: x => x.CargoId,
                        principalTable: "Cargoes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Wallets_CompanionInsurancePackageSales_CompanionInsurancePackageSaleId",
                        column: x => x.CompanionInsurancePackageSaleId,
                        principalTable: "CompanionInsurancePackageSales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Wallets_CompanionReserves_CompanionReserveId",
                        column: x => x.CompanionReserveId,
                        principalTable: "CompanionReserves",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Wallets_PansionReserves_PansionReserveId",
                        column: x => x.PansionReserveId,
                        principalTable: "PansionReserves",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Wallets_PastilAiSubscriptions_PastilAiSubscriptionId",
                        column: x => x.PastilAiSubscriptionId,
                        principalTable: "PastilAiSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wallets_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Wallets_ProductOrders_ProductOrderId",
                        column: x => x.ProductOrderId,
                        principalTable: "ProductOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Wallets_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Wallets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "PastilAiPlans",
                columns: new[] { "Id", "Active", "Code", "CreateDateUtc", "DailyAudioLimit", "DailyChatLimit", "DailyImageLimit", "DailyVideoLimit", "Deleted", "Description", "DurationDays", "Name", "Price", "PurchaseEnabled", "SortOrder", "UpdateDateUtc" },
                values: new object[,]
                {
                    { 1L, true, "Free", new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc), 0, 3, 1, 0, false, "پلن رایگان PastilAI", 30, "PastilAI", 0m, false, 0, new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, true, "Plus", new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc), 5, 30, 10, 1, false, "پلن پیشرفته PastilAI", 30, "PastilAI+", 0m, false, 10, new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3L, true, "Pro", new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, false, "پلن نامحدود PastilAI", 30, "PastilAI Pro", 0m, false, 20, new DateTime(2026, 7, 26, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CityId",
                table: "Addresses",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId",
                table: "Addresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistanceQuestionnaires_AssistanceId",
                table: "AssistanceQuestionnaires",
                column: "AssistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Assistances_PictureId",
                table: "Assistances",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_PictureId",
                table: "Banks",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_CategoryId",
                table: "Banners",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_Picture2Id",
                table: "Banners",
                column: "Picture2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Banners_PictureId",
                table: "Banners",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_BrandCategory_CategoriesId",
                table: "BrandCategory",
                column: "CategoriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_IconId",
                table: "Brands",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Brands_PictureId",
                table: "Brands",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargoes_FromStateId",
                table: "Cargoes",
                column: "FromStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargoes_RebateId",
                table: "Cargoes",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargoes_StatusId",
                table: "Cargoes",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargoes_ToStateId",
                table: "Cargoes",
                column: "ToStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Cargoes_UserPetId",
                table: "Cargoes",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartStoreId",
                table: "CartItems",
                column: "CartStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductItemId",
                table: "CartItems",
                column: "ProductItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_UserId",
                table: "CartItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_AddressId",
                table: "Carts",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_DeliveryId",
                table: "Carts",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_MerchantId",
                table: "Carts",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_RebateId",
                table: "Carts",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartStores_CartId",
                table: "CartStores",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartStores_DeliveryId",
                table: "CartStores",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_CartStores_StoreId",
                table: "CartStores",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_IconId",
                table: "Categories",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_PictureId",
                table: "Categories",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryFeature_FeaturesId",
                table: "CategoryFeature",
                column: "FeaturesId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryPost_PostsId",
                table: "CategoryPost",
                column: "PostsId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryProduct_ProductsId",
                table: "CategoryProduct",
                column: "ProductsId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_StateId",
                table: "Cities",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubRewards_RebateId1",
                table: "ClubRewards",
                column: "RebateId1");

            migrationBuilder.CreateIndex(
                name: "IX_CodeCompanionAssistance_CompanionAssistancesId",
                table: "CodeCompanionAssistance",
                column: "CompanionAssistancesId");

            migrationBuilder.CreateIndex(
                name: "IX_Codes_CodeGroupId",
                table: "Codes",
                column: "CodeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLikes_CommentId",
                table: "CommentLikes",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLikes_UserId",
                table: "CommentLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_StatusId",
                table: "Comments",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistancePackageCompanionReserve_CompanionReservesId",
                table: "CompanionAssistancePackageCompanionReserve",
                column: "CompanionReservesId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistancePackagePictures_CompanionAssistancePackageId",
                table: "CompanionAssistancePackagePictures",
                column: "CompanionAssistancePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistancePackagePictures_PictureId",
                table: "CompanionAssistancePackagePictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistancePackages_CompanionAssistanceId",
                table: "CompanionAssistancePackages",
                column: "CompanionAssistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistancePackages_PictureId",
                table: "CompanionAssistancePackages",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistanceReports_CompanionAssistanceId",
                table: "CompanionAssistanceReports",
                column: "CompanionAssistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistanceReports_UserId",
                table: "CompanionAssistanceReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistances_AssistanceId",
                table: "CompanionAssistances",
                column: "AssistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistances_CompanionId",
                table: "CompanionAssistances",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistances_CompanionTypeId",
                table: "CompanionAssistances",
                column: "CompanionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistanceTimes_CompanionAssistanceId",
                table: "CompanionAssistanceTimes",
                column: "CompanionAssistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistanceTimes_WeekDayId",
                table: "CompanionAssistanceTimes",
                column: "WeekDayId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistanceUsers_CompanionAssistanceId",
                table: "CompanionAssistanceUsers",
                column: "CompanionAssistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionAssistanceUsers_UserId",
                table: "CompanionAssistanceUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionComments_CompanionId",
                table: "CompanionComments",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionInsurancePackages_CompanionId",
                table: "CompanionInsurancePackages",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionInsurancePackages_PetId",
                table: "CompanionInsurancePackages",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionInsurancePackageSales_AddressId",
                table: "CompanionInsurancePackageSales",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionInsurancePackageSales_CompanionInsurancePackageId",
                table: "CompanionInsurancePackageSales",
                column: "CompanionInsurancePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionInsurancePackageSales_RebateId",
                table: "CompanionInsurancePackageSales",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionInsurancePackageSales_UserPetId",
                table: "CompanionInsurancePackageSales",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionPets_CompanionId",
                table: "CompanionPets",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionPets_PetId",
                table: "CompanionPets",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReports_CompanionId",
                table: "CompanionReports",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReports_UserId",
                table: "CompanionReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveCommentRates_AssistanceQuestionnaireId",
                table: "CompanionReserveCommentRates",
                column: "AssistanceQuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveCommentRates_CompanionReserveCommentId",
                table: "CompanionReserveCommentRates",
                column: "CompanionReserveCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveComments_CompanionReserveId",
                table: "CompanionReserveComments",
                column: "CompanionReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_AddressId",
                table: "CompanionReserves",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_BookerId",
                table: "CompanionReserves",
                column: "BookerId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_CompanionAssistanceId",
                table: "CompanionReserves",
                column: "CompanionAssistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_CompanionAssistanceTimeId",
                table: "CompanionReserves",
                column: "CompanionAssistanceTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_CompanionAssistanceTypeId",
                table: "CompanionReserves",
                column: "CompanionAssistanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_CompanionAssistanceUserId",
                table: "CompanionReserves",
                column: "CompanionAssistanceUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_OperatorStateId",
                table: "CompanionReserves",
                column: "OperatorStateId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_RebateId",
                table: "CompanionReserves",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserves_StateId",
                table: "CompanionReserves",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionReserveUserPet_UserPetsId",
                table: "CompanionReserveUserPet",
                column: "UserPetsId");

            migrationBuilder.CreateIndex(
                name: "IX_Companions_BackgroundPictureId",
                table: "Companions",
                column: "BackgroundPictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Companions_CityId",
                table: "Companions",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Companions_CodeId",
                table: "Companions",
                column: "CodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Companions_IconId",
                table: "Companions",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Companions_NeighborhoodId",
                table: "Companions",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Companions_OwnerId",
                table: "Companions",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Companions_PetId",
                table: "Companions",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Companions_PictureId",
                table: "Companions",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionTypes_CompanionId",
                table: "CompanionTypes",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionTypes_TypeId",
                table: "CompanionTypes",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionUsers_CompanionId",
                table: "CompanionUsers",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionUsers_UserId",
                table: "CompanionUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionZones_CityId",
                table: "CompanionZones",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionZones_CompanionId",
                table: "CompanionZones",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionZones_NeighborhoodId",
                table: "CompanionZones",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanionZones_StateId",
                table: "CompanionZones",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactUses_ContactUsGroupId",
                table: "ContactUses",
                column: "ContactUsGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactUses_FileId",
                table: "ContactUses",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactUses_UserId",
                table: "ContactUses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactUsItems_ContactUsId",
                table: "ContactUsItems",
                column: "ContactUsId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_CityId",
                table: "Deliveries",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_DeliveryTypeId",
                table: "Deliveries",
                column: "DeliveryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_StateId",
                table: "Deliveries",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_Deliveries_StoreId",
                table: "Deliveries",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryDistances_DeliveryId",
                table: "DeliveryDistances",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_Details_CategoryId",
                table: "Details",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Details_TypeId",
                table: "Details",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountGroups_PictureId",
                table: "DiscountGroups",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_BrandId",
                table: "Discounts",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_CategoryId",
                table: "Discounts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_DiscountGroupId",
                table: "Discounts",
                column: "DiscountGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_ProductId",
                table: "Discounts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_ProductItemId",
                table: "Discounts",
                column: "ProductItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_StoreId",
                table: "Discounts",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_TypeId",
                table: "Discounts",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionAnswerLikes_DiscussionAnswerId",
                table: "DiscussionAnswerLikes",
                column: "DiscussionAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionAnswerLikes_UserId",
                table: "DiscussionAnswerLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionAnswers_DiscussionQuestionId",
                table: "DiscussionAnswers",
                column: "DiscussionQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionAnswers_UserId",
                table: "DiscussionAnswers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionQuestions_ProductId",
                table: "DiscussionQuestions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionQuestions_UserId",
                table: "DiscussionQuestions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CertificatePictureId",
                table: "Drivers",
                column: "CertificatePictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CityId",
                table: "Drivers",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_NeighborhoodId",
                table: "Drivers",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_OwnerId",
                table: "Drivers",
                column: "OwnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_ProfilePictureId",
                table: "Drivers",
                column: "ProfilePictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_StatusId",
                table: "Drivers",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_VehicleCardPictureId",
                table: "Drivers",
                column: "VehicleCardPictureId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverUsers_DriverId",
                table: "DriverUsers",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverUsers_UserId",
                table: "DriverUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailAddresses_EmailHostId",
                table: "EmailAddresses",
                column: "EmailHostId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_EmailAddressId",
                table: "Emails",
                column: "EmailAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Emails_EmailTypeId",
                table: "Emails",
                column: "EmailTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSettings_EmailAddressId",
                table: "EmailSettings",
                column: "EmailAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailSettings_EmailTypeId",
                table: "EmailSettings",
                column: "EmailTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureItems_FeatureId",
                table: "FeatureItems",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_Features_TypeId",
                table: "Features",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_CategoryId",
                table: "Galleries",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_PictureId",
                table: "Galleries",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryItems_GalleryId",
                table: "GalleryItems",
                column: "GalleryId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryItems_PictureId",
                table: "GalleryItems",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_HashtagPost_postsId",
                table: "HashtagPost",
                column: "postsId");

            migrationBuilder.CreateIndex(
                name: "IX_MapKeys_TypeId",
                table: "MapKeys",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_BankId",
                table: "Merchants",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Neighborhoods_CityId",
                table: "Neighborhoods",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_NoticeReads_AdminId_ReadAtUtc",
                table: "NoticeReads",
                columns: new[] { "AdminId", "ReadAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NoticeReads_NoticeId",
                table: "NoticeReads",
                column: "NoticeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notices_ActorUserId",
                table: "Notices",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notices_ArchivedAtUtc_ArchiveDueAtUtc_CreateDateUtc",
                table: "Notices",
                columns: new[] { "ArchivedAtUtc", "ArchiveDueAtUtc", "CreateDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notices_DeduplicationKey",
                table: "Notices",
                column: "DeduplicationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notices_NoticeTypeId_CreateDateUtc",
                table: "Notices",
                columns: new[] { "NoticeTypeId", "CreateDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Notices_ReferenceType_ReferenceId",
                table: "Notices",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_NoticeTypes_Label",
                table: "NoticeTypes",
                column: "Label",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotifyMessages_PictureId",
                table: "NotifyMessages",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_PansionComments_PansionId",
                table: "PansionComments",
                column: "PansionId");

            migrationBuilder.CreateIndex(
                name: "IX_PansionPets_PansionId",
                table: "PansionPets",
                column: "PansionId");

            migrationBuilder.CreateIndex(
                name: "IX_PansionPets_PetId",
                table: "PansionPets",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_PansionPictures_PansionId",
                table: "PansionPictures",
                column: "PansionId");

            migrationBuilder.CreateIndex(
                name: "IX_PansionPictures_PictureId",
                table: "PansionPictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_PansionReserves_BookerId",
                table: "PansionReserves",
                column: "BookerId");

            migrationBuilder.CreateIndex(
                name: "IX_PansionReserves_PansionId",
                table: "PansionReserves",
                column: "PansionId");

            migrationBuilder.CreateIndex(
                name: "IX_PansionReserves_RebateId",
                table: "PansionReserves",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_PansionReserves_StatusId",
                table: "PansionReserves",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PansionReserves_UserPetId",
                table: "PansionReserves",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_Pansions_CityId",
                table: "Pansions",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Pansions_CompanionId",
                table: "Pansions",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_Pansions_PictureId",
                table: "Pansions",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Pansions_StateId",
                table: "Pansions",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkPictures_ParkId",
                table: "ParkPictures",
                column: "ParkId");

            migrationBuilder.CreateIndex(
                name: "IX_ParkPictures_PictureId",
                table: "ParkPictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Parks_NeighborhoodId",
                table: "Parks",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Parks_PictureId",
                table: "Parks",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiAttachments_FileId",
                table: "PastilAiAttachments",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiAttachments_MessageId",
                table: "PastilAiAttachments",
                column: "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiAttachments_PictureId",
                table: "PastilAiAttachments",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiConversations_UserId_UpdateDateUtc",
                table: "PastilAiConversations",
                columns: new[] { "UserId", "UpdateDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiDailyUsages_UserId_UsageDate",
                table: "PastilAiDailyUsages",
                columns: new[] { "UserId", "UsageDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiMessages_ConversationId_Id",
                table: "PastilAiMessages",
                columns: new[] { "ConversationId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiPlans_Code",
                table: "PastilAiPlans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiProviderAttempts_MessageId_AttemptOrder",
                table: "PastilAiProviderAttempts",
                columns: new[] { "MessageId", "AttemptOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiSubscriptions_PaymentId",
                table: "PastilAiSubscriptions",
                column: "PaymentId",
                unique: true,
                filter: "[PaymentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiSubscriptions_PlanId",
                table: "PastilAiSubscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiSubscriptions_RebateId",
                table: "PastilAiSubscriptions",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilAiSubscriptions_UserId_Status_EndDateUtc",
                table: "PastilAiSubscriptions",
                columns: new[] { "UserId", "Status", "EndDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchBlocks_BlockedUserId",
                table: "PastilMatchBlocks",
                column: "BlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchBlocks_BlockerUserId",
                table: "PastilMatchBlocks",
                column: "BlockerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchBlocks_PastilMatchId",
                table: "PastilMatchBlocks",
                column: "PastilMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatches_FirstProfileId",
                table: "PastilMatches",
                column: "FirstProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatches_PastilMatchGoalId",
                table: "PastilMatches",
                column: "PastilMatchGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatches_PastilMatchRequestId",
                table: "PastilMatches",
                column: "PastilMatchRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatches_SecondProfileId",
                table: "PastilMatches",
                column: "SecondProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatches_StatusId",
                table: "PastilMatches",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessageAttachments_PastilMatchMessageId",
                table: "PastilMatchMessageAttachments",
                column: "PastilMatchMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessageReactions_PastilMatchMessageId",
                table: "PastilMatchMessageReactions",
                column: "PastilMatchMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessageReactions_ReactorProfileId",
                table: "PastilMatchMessageReactions",
                column: "ReactorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessages_PastilMatchId",
                table: "PastilMatchMessages",
                column: "PastilMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessages_PastilMatchMessageTypeId",
                table: "PastilMatchMessages",
                column: "PastilMatchMessageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessages_ReplyToMessageId",
                table: "PastilMatchMessages",
                column: "ReplyToMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchMessages_SenderProfileId",
                table: "PastilMatchMessages",
                column: "SenderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfileGoals_PastilMatchGoalId",
                table: "PastilMatchProfileGoals",
                column: "PastilMatchGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfileGoals_PastilMatchProfileId",
                table: "PastilMatchProfileGoals",
                column: "PastilMatchProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfileLikes_LikedProfileId",
                table: "PastilMatchProfileLikes",
                column: "LikedProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfileLikes_LikerProfileId",
                table: "PastilMatchProfileLikes",
                column: "LikerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfiles_CityId",
                table: "PastilMatchProfiles",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfiles_EnergyLevelId",
                table: "PastilMatchProfiles",
                column: "EnergyLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfiles_NeighborhoodId",
                table: "PastilMatchProfiles",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfiles_SocialLevelId",
                table: "PastilMatchProfiles",
                column: "SocialLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchProfiles_UserPetId",
                table: "PastilMatchProfiles",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_PastilMatchId",
                table: "PastilMatchReports",
                column: "PastilMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_PastilMatchMessageId",
                table: "PastilMatchReports",
                column: "PastilMatchMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_PastilMatchReportReasonId",
                table: "PastilMatchReports",
                column: "PastilMatchReportReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_ReportedProfileId",
                table: "PastilMatchReports",
                column: "ReportedProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_ReportedUserId",
                table: "PastilMatchReports",
                column: "ReportedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchReports_ReporterUserId",
                table: "PastilMatchReports",
                column: "ReporterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchRequests_PastilMatchGoalId",
                table: "PastilMatchRequests",
                column: "PastilMatchGoalId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchRequests_ReceiverProfileId",
                table: "PastilMatchRequests",
                column: "ReceiverProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchRequests_SenderProfileId",
                table: "PastilMatchRequests",
                column: "SenderProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PastilMatchRequests_StatusId",
                table: "PastilMatchRequests",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FileId",
                table: "Payments",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MerchantId",
                table: "Payments",
                column: "MerchantId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProductOrderId",
                table: "Payments",
                column: "ProductOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TypeId",
                table: "Payments",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionRole_RolesId",
                table: "PermissionRole",
                column: "RolesId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ParentId",
                table: "Permissions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PetBreeds_PetId",
                table: "PetBreeds",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_PetBreeds_PictureId",
                table: "PetBreeds",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_PostId",
                table: "PostComments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostFiles_FileId",
                table: "PostFiles",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_PostFiles_PostId",
                table: "PostFiles",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostPictures_PictureId",
                table: "PostPictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_PostPictures_PostId",
                table: "PostPictures",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostProduct_ProductsId",
                table: "PostProduct",
                column: "ProductsId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_AdminId",
                table: "Posts",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CategoryId",
                table: "Posts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_ParentId",
                table: "Posts",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_PictureId",
                table: "Posts",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComments_ProductId",
                table: "ProductComments",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFeatureValues_FeatureId",
                table: "ProductFeatureValues",
                column: "FeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFeatureValues_FeatureItemId",
                table: "ProductFeatureValues",
                column: "FeatureItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFeatureValues_ProductId",
                table: "ProductFeatureValues",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFiles_FileId",
                table: "ProductFiles",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFiles_ParentId",
                table: "ProductFiles",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFiles_ProductId",
                table: "ProductFiles",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductItems_DiscountGroupId",
                table: "ProductItems",
                column: "DiscountGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductItems_ProductId",
                table: "ProductItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductItems_StoreId",
                table: "ProductItems",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductItems_VarietyItem2Id",
                table: "ProductItems",
                column: "VarietyItem2Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductItems_VarietyItemId",
                table: "ProductItems",
                column: "VarietyItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLikes_ProductId",
                table: "ProductLikes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLikes_UserId",
                table: "ProductLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrderItems_ProductItemId",
                table: "ProductOrderItems",
                column: "ProductItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrderItems_ProductOrderStoreId",
                table: "ProductOrderItems",
                column: "ProductOrderStoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrders_AddressId",
                table: "ProductOrders",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrders_DeliveryTypeId",
                table: "ProductOrders",
                column: "DeliveryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrders_PaymentTypeId",
                table: "ProductOrders",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrders_ProductOrderStateId",
                table: "ProductOrders",
                column: "ProductOrderStateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrders_ProductOrderStatusId",
                table: "ProductOrders",
                column: "ProductOrderStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrders_RebateId",
                table: "ProductOrders",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrders_UserId",
                table: "ProductOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrderStores_DeliveryId",
                table: "ProductOrderStores",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrderStores_ProductOrderId",
                table: "ProductOrderStores",
                column: "ProductOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductOrderStores_StoreId",
                table: "ProductOrderStores",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPictures_PictureId",
                table: "ProductPictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductPictures_ProductId",
                table: "ProductPictures",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRelates_ProductId",
                table: "ProductRelates",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductRelates_RelatedProductId",
                table: "ProductRelates",
                column: "RelatedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReports_ProductId",
                table: "ProductReports",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductReports_UserId",
                table: "ProductReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_BrandId",
                table: "Products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DiscountGroupId",
                table: "Products",
                column: "DiscountGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_PictureId",
                table: "Products",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StatusId",
                table: "Products",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_StoreId",
                table: "Products",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_TypeId",
                table: "Products",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_UserId",
                table: "Products",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Variety2Id",
                table: "Products",
                column: "Variety2Id");

            migrationBuilder.CreateIndex(
                name: "IX_Products_VarietyId",
                table: "Products",
                column: "VarietyId");

            migrationBuilder.CreateIndex(
                name: "IX_PushMessages_PictureId",
                table: "PushMessages",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_PushMessages_PushMessageTypeId",
                table: "PushMessages",
                column: "PushMessageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_NoticeId",
                table: "PushNotifications",
                column: "NoticeId");

            migrationBuilder.CreateIndex(
                name: "IX_PushNotifications_PushPatternId",
                table: "PushNotifications",
                column: "PushPatternId");

            migrationBuilder.CreateIndex(
                name: "IX_PushPatterns_PushTypeId",
                table: "PushPatterns",
                column: "PushTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PushSettings_PushPatternId",
                table: "PushSettings",
                column: "PushPatternId");

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptions_Endpoint",
                table: "PushSubscriptions",
                column: "Endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscriptions_UserId",
                table: "PushSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rebate_ProductId",
                table: "Rebate",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Rebate_TypeId",
                table: "Rebate",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Rebate_UserId",
                table: "Rebate",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_ReminderCycleId",
                table: "Reminders",
                column: "ReminderCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_ReminderTypeId",
                table: "Reminders",
                column: "ReminderTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserPetId",
                table: "Reminders",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreTransactions_TransactionTypeId",
                table: "ScoreTransactions",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreTransactions_UserId",
                table: "ScoreTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCompanions_CompanionReserveId",
                table: "SettlementCompanions",
                column: "CompanionReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCompanions_PansionReserveId",
                table: "SettlementCompanions",
                column: "PansionReserveId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementCompanions_SettlementId",
                table: "SettlementCompanions",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_CompanionId",
                table: "Settlements",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_StoreId",
                table: "Settlements",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Settlements_UserBankCardId",
                table: "Settlements",
                column: "UserBankCardId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementStores_ProductOrderId",
                table: "SettlementStores",
                column: "ProductOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementStores_SettlementId",
                table: "SettlementStores",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_Smses_SmsTypeId",
                table: "Smses",
                column: "SmsTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsNumbers_SmsProviderId",
                table: "SmsNumbers",
                column: "SmsProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsSettings_SmsNumberId",
                table: "SmsSettings",
                column: "SmsNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsSettings_SmsTypeId",
                table: "SmsSettings",
                column: "SmsTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_States_CountryId",
                table: "States",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreComments_StoreId",
                table: "StoreComments",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_CityId",
                table: "Stores",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_IconId",
                table: "Stores",
                column: "IconId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_PictureId",
                table: "Stores",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Stores_TypeId",
                table: "Stores",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreUser_UsersId",
                table: "StoreUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryGroups_PictureId",
                table: "StoryGroups",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryItems_CompanionId",
                table: "StoryItems",
                column: "CompanionId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryItems_FileId",
                table: "StoryItems",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryItems_PansionId",
                table: "StoryItems",
                column: "PansionId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryItems_PictureId",
                table: "StoryItems",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryItems_StoreId",
                table: "StoryItems",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryItems_StoryGroupId",
                table: "StoryItems",
                column: "StoryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryUserLikes_StoryItemId",
                table: "StoryUserLikes",
                column: "StoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryUserLikes_UserId",
                table: "StoryUserLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketItems_FileId",
                table: "TicketItems",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketItems_ReplyToTicketItemId",
                table: "TicketItems",
                column: "ReplyToTicketItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketItems_TicketId_Id",
                table: "TicketItems",
                columns: new[] { "TicketId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketItems_TicketId_IsSeen_UserId",
                table: "TicketItems",
                columns: new[] { "TicketId", "IsSeen", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketItems_UserId",
                table: "TicketItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AdminId_StatusId_UpdateDate",
                table: "Tickets",
                columns: new[] { "AdminId", "StatusId", "UpdateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ImportanceId",
                table: "Tickets",
                column: "ImportanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ProductId",
                table: "Tickets",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_StatusId_TicketCategoryId_UpdateDate",
                table: "Tickets",
                columns: new[] { "StatusId", "TicketCategoryId", "UpdateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketCategoryId",
                table: "Tickets",
                column: "TicketCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_UserId_UpdateDate",
                table: "Tickets",
                columns: new[] { "UserId", "UpdateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TripAddresses_UserId",
                table: "TripAddresses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_DriverId",
                table: "Trips",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_DriverStatusId",
                table: "Trips",
                column: "DriverStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_FromCityId",
                table: "Trips",
                column: "FromCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_RebateId",
                table: "Trips",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_TripStatusId",
                table: "Trips",
                column: "TripStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_TripStopId",
                table: "Trips",
                column: "TripStopId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_UserId",
                table: "Trips",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Trips_UserPetId",
                table: "Trips",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_TripTripOption_TripsId",
                table: "TripTripOption",
                column: "TripsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBankCards_BankCardId",
                table: "UserBankCards",
                column: "BankCardId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBankCards_UserId",
                table: "UserBankCards",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategories_CategoryId",
                table: "UserCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategories_UserId",
                table: "UserCategories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCurrentLocations_CityId",
                table: "UserCurrentLocations",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCurrentLocations_NeighborhoodId",
                table: "UserCurrentLocations",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCurrentLocations_UserId",
                table: "UserCurrentLocations",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPetPictures_PictureId",
                table: "UserPetPictures",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPetPictures_UserPetId",
                table: "UserPetPictures",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPetRecords_OperatorId",
                table: "UserPetRecords",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPetRecords_UserPetId",
                table: "UserPetRecords",
                column: "UserPetId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPets_PetBreed2Id",
                table: "UserPets",
                column: "PetBreed2Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserPets_PetBreedId",
                table: "UserPets",
                column: "PetBreedId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPets_PetId",
                table: "UserPets",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPets_PictureId",
                table: "UserPets",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPets_UserId",
                table: "UserPets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProducts_ProductId",
                table: "UserProducts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProducts_UserId",
                table: "UserProducts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRebates_RebateId",
                table: "UserRebates",
                column: "RebateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRebates_UserId",
                table: "UserRebates",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PictureId",
                table: "Users",
                column: "PictureId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTokens_UserId",
                table: "UserTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Varieties_CategoryId",
                table: "Varieties",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_VarietyItems_VarietyId",
                table: "VarietyItems",
                column: "VarietyId");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CargoId",
                table: "Wallets",
                column: "CargoId",
                unique: true,
                filter: "[CargoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CompanionInsurancePackageSaleId",
                table: "Wallets",
                column: "CompanionInsurancePackageSaleId",
                unique: true,
                filter: "[CompanionInsurancePackageSaleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_CompanionReserveId",
                table: "Wallets",
                column: "CompanionReserveId",
                unique: true,
                filter: "[CompanionReserveId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_PansionReserveId",
                table: "Wallets",
                column: "PansionReserveId",
                unique: true,
                filter: "[PansionReserveId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_PastilAiSubscriptionId",
                table: "Wallets",
                column: "PastilAiSubscriptionId",
                unique: true,
                filter: "[PastilAiSubscriptionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_PaymentId",
                table: "Wallets",
                column: "PaymentId",
                unique: true,
                filter: "[PaymentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_ProductOrderId",
                table: "Wallets",
                column: "ProductOrderId",
                unique: true,
                filter: "[ProductOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_TripId",
                table: "Wallets",
                column: "TripId",
                unique: true,
                filter: "[TripId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                table: "Wallets",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminSettings");

            migrationBuilder.DropTable(
                name: "Banners");

            migrationBuilder.DropTable(
                name: "BaseDetails");

            migrationBuilder.DropTable(
                name: "BrandCategory");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "CategoryFeature");

            migrationBuilder.DropTable(
                name: "CategoryPost");

            migrationBuilder.DropTable(
                name: "CategoryProduct");

            migrationBuilder.DropTable(
                name: "ClubRewards");

            migrationBuilder.DropTable(
                name: "CodeCompanionAssistance");

            migrationBuilder.DropTable(
                name: "CommentLikes");

            migrationBuilder.DropTable(
                name: "CompanionAssistancePackageCompanionReserve");

            migrationBuilder.DropTable(
                name: "CompanionAssistancePackagePictures");

            migrationBuilder.DropTable(
                name: "CompanionAssistanceReports");

            migrationBuilder.DropTable(
                name: "CompanionComments");

            migrationBuilder.DropTable(
                name: "CompanionPets");

            migrationBuilder.DropTable(
                name: "CompanionReports");

            migrationBuilder.DropTable(
                name: "CompanionReserveCommentRates");

            migrationBuilder.DropTable(
                name: "CompanionReserveUserPet");

            migrationBuilder.DropTable(
                name: "CompanionTypes");

            migrationBuilder.DropTable(
                name: "CompanionUsers");

            migrationBuilder.DropTable(
                name: "CompanionZones");

            migrationBuilder.DropTable(
                name: "ContactUsItems");

            migrationBuilder.DropTable(
                name: "DeliveryDistances");

            migrationBuilder.DropTable(
                name: "Details");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropTable(
                name: "DiscussionAnswerLikes");

            migrationBuilder.DropTable(
                name: "DriverUsers");

            migrationBuilder.DropTable(
                name: "Emails");

            migrationBuilder.DropTable(
                name: "EmailSettings");

            migrationBuilder.DropTable(
                name: "GalleryItems");

            migrationBuilder.DropTable(
                name: "HashtagPost");

            migrationBuilder.DropTable(
                name: "MapKeys");

            migrationBuilder.DropTable(
                name: "Newsletters");

            migrationBuilder.DropTable(
                name: "NoticeReads");

            migrationBuilder.DropTable(
                name: "NotifyMessages");

            migrationBuilder.DropTable(
                name: "OtpVerifies");

            migrationBuilder.DropTable(
                name: "PansionComments");

            migrationBuilder.DropTable(
                name: "PansionPets");

            migrationBuilder.DropTable(
                name: "PansionPictures");

            migrationBuilder.DropTable(
                name: "ParkPictures");

            migrationBuilder.DropTable(
                name: "PastilAiAttachments");

            migrationBuilder.DropTable(
                name: "PastilAiDailyUsages");

            migrationBuilder.DropTable(
                name: "PastilAiProviderAttempts");

            migrationBuilder.DropTable(
                name: "PastilMatchBlocks");

            migrationBuilder.DropTable(
                name: "PastilMatchMessageAttachments");

            migrationBuilder.DropTable(
                name: "PastilMatchMessageReactions");

            migrationBuilder.DropTable(
                name: "PastilMatchProfileGoals");

            migrationBuilder.DropTable(
                name: "PastilMatchProfileLikes");

            migrationBuilder.DropTable(
                name: "PastilMatchReports");

            migrationBuilder.DropTable(
                name: "PermissionRole");

            migrationBuilder.DropTable(
                name: "PostComments");

            migrationBuilder.DropTable(
                name: "PostFiles");

            migrationBuilder.DropTable(
                name: "PostPictures");

            migrationBuilder.DropTable(
                name: "PostProduct");

            migrationBuilder.DropTable(
                name: "PriceCalculations");

            migrationBuilder.DropTable(
                name: "ProductComments");

            migrationBuilder.DropTable(
                name: "ProductFeatureValues");

            migrationBuilder.DropTable(
                name: "ProductFiles");

            migrationBuilder.DropTable(
                name: "ProductLikes");

            migrationBuilder.DropTable(
                name: "ProductOrderItems");

            migrationBuilder.DropTable(
                name: "ProductPictures");

            migrationBuilder.DropTable(
                name: "ProductRelates");

            migrationBuilder.DropTable(
                name: "ProductReports");

            migrationBuilder.DropTable(
                name: "PushMessages");

            migrationBuilder.DropTable(
                name: "PushNotifications");

            migrationBuilder.DropTable(
                name: "PushSettings");

            migrationBuilder.DropTable(
                name: "PushSubscriptions");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "ScoreTransactions");

            migrationBuilder.DropTable(
                name: "SettlementCompanions");

            migrationBuilder.DropTable(
                name: "SettlementStores");

            migrationBuilder.DropTable(
                name: "Smses");

            migrationBuilder.DropTable(
                name: "SmsSettings");

            migrationBuilder.DropTable(
                name: "StaticPages");

            migrationBuilder.DropTable(
                name: "StoreComments");

            migrationBuilder.DropTable(
                name: "StoreUser");

            migrationBuilder.DropTable(
                name: "StoryUserLikes");

            migrationBuilder.DropTable(
                name: "TicketItems");

            migrationBuilder.DropTable(
                name: "TripAddresses");

            migrationBuilder.DropTable(
                name: "TripTripOption");

            migrationBuilder.DropTable(
                name: "UserCategories");

            migrationBuilder.DropTable(
                name: "UserCurrentLocations");

            migrationBuilder.DropTable(
                name: "UserPetPictures");

            migrationBuilder.DropTable(
                name: "UserPetRecords");

            migrationBuilder.DropTable(
                name: "UserProducts");

            migrationBuilder.DropTable(
                name: "UserRebates");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DropTable(
                name: "CartStores");

            migrationBuilder.DropTable(
                name: "CompanionAssistancePackages");

            migrationBuilder.DropTable(
                name: "AssistanceQuestionnaires");

            migrationBuilder.DropTable(
                name: "CompanionReserveComments");

            migrationBuilder.DropTable(
                name: "ContactUses");

            migrationBuilder.DropTable(
                name: "DiscussionAnswers");

            migrationBuilder.DropTable(
                name: "EmailAddresses");

            migrationBuilder.DropTable(
                name: "Galleries");

            migrationBuilder.DropTable(
                name: "Hashtags");

            migrationBuilder.DropTable(
                name: "Parks");

            migrationBuilder.DropTable(
                name: "PastilAiMessages");

            migrationBuilder.DropTable(
                name: "PastilMatchMessages");

            migrationBuilder.DropTable(
                name: "PastilMatchReportReasons");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "FeatureItems");

            migrationBuilder.DropTable(
                name: "ProductItems");

            migrationBuilder.DropTable(
                name: "ProductOrderStores");

            migrationBuilder.DropTable(
                name: "Notices");

            migrationBuilder.DropTable(
                name: "PushPatterns");

            migrationBuilder.DropTable(
                name: "ReminderCycles");

            migrationBuilder.DropTable(
                name: "ReminderTypes");

            migrationBuilder.DropTable(
                name: "Settlements");

            migrationBuilder.DropTable(
                name: "MessageTypes");

            migrationBuilder.DropTable(
                name: "SmsNumbers");

            migrationBuilder.DropTable(
                name: "StoryItems");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "TripOptions");

            migrationBuilder.DropTable(
                name: "Cargoes");

            migrationBuilder.DropTable(
                name: "CompanionInsurancePackageSales");

            migrationBuilder.DropTable(
                name: "PansionReserves");

            migrationBuilder.DropTable(
                name: "PastilAiSubscriptions");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "CompanionReserves");

            migrationBuilder.DropTable(
                name: "ContactUsGroups");

            migrationBuilder.DropTable(
                name: "DiscussionQuestions");

            migrationBuilder.DropTable(
                name: "EmailHosts");

            migrationBuilder.DropTable(
                name: "PastilAiConversations");

            migrationBuilder.DropTable(
                name: "PastilMatches");

            migrationBuilder.DropTable(
                name: "Features");

            migrationBuilder.DropTable(
                name: "VarietyItems");

            migrationBuilder.DropTable(
                name: "NoticeTypes");

            migrationBuilder.DropTable(
                name: "PushTypes");

            migrationBuilder.DropTable(
                name: "UserBankCards");

            migrationBuilder.DropTable(
                name: "SmsProviders");

            migrationBuilder.DropTable(
                name: "StoryGroups");

            migrationBuilder.DropTable(
                name: "CompanionInsurancePackages");

            migrationBuilder.DropTable(
                name: "Pansions");

            migrationBuilder.DropTable(
                name: "PastilAiPlans");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "TripStops");

            migrationBuilder.DropTable(
                name: "Deliveries");

            migrationBuilder.DropTable(
                name: "CompanionAssistanceTimes");

            migrationBuilder.DropTable(
                name: "CompanionAssistanceUsers");

            migrationBuilder.DropTable(
                name: "PastilMatchRequests");

            migrationBuilder.DropTable(
                name: "BankCards");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Merchants");

            migrationBuilder.DropTable(
                name: "ProductOrders");

            migrationBuilder.DropTable(
                name: "WeekDays");

            migrationBuilder.DropTable(
                name: "CompanionAssistances");

            migrationBuilder.DropTable(
                name: "PastilMatchProfiles");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Rebate");

            migrationBuilder.DropTable(
                name: "Assistances");

            migrationBuilder.DropTable(
                name: "Companions");

            migrationBuilder.DropTable(
                name: "UserPets");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Neighborhoods");

            migrationBuilder.DropTable(
                name: "PetBreeds");

            migrationBuilder.DropTable(
                name: "Brands");

            migrationBuilder.DropTable(
                name: "DiscountGroups");

            migrationBuilder.DropTable(
                name: "Stores");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Varieties");

            migrationBuilder.DropTable(
                name: "Pets");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Codes");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "States");

            migrationBuilder.DropTable(
                name: "CodeGroups");

            migrationBuilder.DropTable(
                name: "Pictures");

            migrationBuilder.DropTable(
                name: "Countries");
        }
    }
}
