using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LovEat.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Permissions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiVersionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeprecationAnnouncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SunsetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiVersionRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSettingsEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByAdminId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettingsEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutoInvoiceConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    TriggerBookingStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AutoSendToCustomer = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoInvoiceConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutoInvoiceRunLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    Detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RunAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoInvoiceRunLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "B2BPartners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CommissionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2BPartners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BackupRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BackupType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    StorageLocation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BadgeDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BadgeDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingFunnelSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodKey = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    CreatedCount = table.Column<int>(type: "int", nullable: false),
                    AcceptedCount = table.Column<int>(type: "int", nullable: false),
                    StartedCount = table.Column<int>(type: "int", nullable: false),
                    CompletedCount = table.Column<int>(type: "int", nullable: false),
                    CancelledCount = table.Column<int>(type: "int", nullable: false),
                    AcceptanceRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompletionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverallConversionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingFunnelSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CapacityForecasts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodKey = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ProjectedBookingCount = table.Column<int>(type: "int", nullable: false),
                    ProjectedPeakConcurrentUsers = table.Column<int>(type: "int", nullable: false),
                    ScalingRecommendation = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapacityForecasts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CertificationCourses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationHours = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ContentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BadgeIconUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificationCourses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChefLoyaltyTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Points = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefLoyaltyTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CityWaitlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CityName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsNotified = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityWaitlists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CmsBanners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subtitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeepLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsBanners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CmsPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CohortRetentionSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohortMonth = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    MonthOffset = table.Column<int>(type: "int", nullable: false),
                    CohortSize = table.Column<int>(type: "int", nullable: false),
                    ActiveCount = table.Column<int>(type: "int", nullable: false),
                    RetentionPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CohortRetentionSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommissionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppliesTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlatformPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChefPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CorporatePlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MealsPerDay = table.Column<int>(type: "int", nullable: false),
                    PricePerMeal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Features = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporatePlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MinOrderAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsageLimit = table.Column<int>(type: "int", nullable: false),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    UsageLimitPerUser = table.Column<int>(type: "int", nullable: true),
                    AppliesTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExchangeRateToINR = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataMaskingPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    MaskingRule = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApplicableRoles = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataMaskingPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DemandPredictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Hour = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    BookingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PredictedDemand = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualDemand = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Confidence = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemandPredictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DependencyHealthChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResponseTimeMs = table.Column<int>(type: "int", nullable: true),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DependencyHealthChecks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpansionCities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LaunchStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlannedLaunch = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualLaunch = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TargetChefs = table.Column<int>(type: "int", nullable: false),
                    CurrentChefs = table.Column<int>(type: "int", nullable: false),
                    WaitlistCount = table.Column<int>(type: "int", nullable: false),
                    LaunchNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpansionCities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExperimentDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VariantAName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    VariantBName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FaqCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FAQs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Audience = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    HelpfulCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackSurveys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SurveyType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetAudience = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackSurveys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Franchises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FranchiseeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    RegionOrCity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RevenueSharePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Franchises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FraudHeatMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Zone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    IncidentCount = table.Column<int>(type: "int", nullable: false),
                    FraudFlagCount = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PeriodKey = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudHeatMaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeoHeatmapCells",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodKey = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    GridLatitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GridLongitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookingCount = table.Column<int>(type: "int", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoHeatmapCells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroceryLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    ItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroceryLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GstHsnSacCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RatePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AppliesTo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GstHsnSacCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InfraCostEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodKey = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ServiceCategory = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BudgetLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfraCostEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IngredientCarbonFactors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngredientName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    CarbonKgPerKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientCarbonFactors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardSeasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardSeasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IconName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsNew = table.Column<bool>(type: "bit", nullable: false),
                    IsComingSoon = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MealBoxPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    MealsPerBox = table.Column<int>(type: "int", nullable: false),
                    PricePerBox = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CuisineType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealBoxPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalReach = table.Column<int>(type: "int", nullable: false),
                    DeliveredCount = table.Column<int>(type: "int", nullable: false),
                    OpenedCount = table.Column<int>(type: "int", nullable: false),
                    CreatedByAdminId = table.Column<int>(type: "int", nullable: true),
                    DeepLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtpVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    OtpCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpVerifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformHealthMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetricName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MetricValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformHealthMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    BaseRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinalRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SurchargeAmt = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RulesApplied = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AppliesTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MultiplierPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DaysOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartHour = table.Column<int>(type: "int", nullable: true),
                    EndHour = table.Column<int>(type: "int", nullable: true),
                    ValidFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingSuggestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    SuggestedMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DemandScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActiveChefSupply = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingSuggestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReplyTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Keywords = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplyTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RevenueAnalyticsSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateKey = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    City = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookingCount = table.Column<int>(type: "int", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevenueAnalyticsSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RiskSignalDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SignalKey = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    WeightPoints = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskSignalDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SentimentScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MatchedKeywords = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AnalyzedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentimentScores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceCities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RadiusKm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsLaunching = table.Column<bool>(type: "bit", nullable: false),
                    ChefCount = table.Column<int>(type: "int", nullable: false),
                    BookingCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceCities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SlaPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResponseTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    ResolutionTimeHours = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportedLanguages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    NativeName = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportedLanguages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    TaxName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AppliesTo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TdsRateConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Section = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RatePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AnnualThresholdAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdsRateConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketQueues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TranslationStrings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslationStrings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPhoneVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    WalletBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PreferredCurrency = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSegments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilterJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSegments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherRecommendationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeatherCondition = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SuggestedCuisines = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherRecommendationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WhiteLabelClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BrandName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Domain = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrimaryColor = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApiKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PlanTier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteLabelClients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CorporateSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CorporatePlanId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GSTNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BillingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmployeeCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PendingPlanId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorporateSubscriptions_CorporatePlans_CorporatePlanId",
                        column: x => x.CorporatePlanId,
                        principalTable: "CorporatePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExperimentEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExperimentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Variant = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperimentEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExperimentEvents_ExperimentDefinitions_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "ExperimentDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FaqArticles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaqCategoryId = table.Column<int>(type: "int", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Audience = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    HelpfulCount = table.Column<int>(type: "int", nullable: false),
                    NotHelpfulCount = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqArticles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaqArticles_FaqCategories_FaqCategoryId",
                        column: x => x.FaqCategoryId,
                        principalTable: "FaqCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FranchiseSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FranchiseId = table.Column<int>(type: "int", nullable: false),
                    SettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FranchiseSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FranchiseSettings_Franchises_FranchiseId",
                        column: x => x.FranchiseId,
                        principalTable: "Franchises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardRewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    RankFrom = table.Column<int>(type: "int", nullable: false),
                    RankTo = table.Column<int>(type: "int", nullable: false),
                    RewardDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RewardType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderboardRewards_LeaderboardSeasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "LeaderboardSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CityPerformanceSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceCityId = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PeriodKey = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalBookings = table.Column<int>(type: "int", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActiveChefs = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityPerformanceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CityPerformanceSnapshots_ServiceCities_ServiceCityId",
                        column: x => x.ServiceCityId,
                        principalTable: "ServiceCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceZones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceCityId = table.Column<int>(type: "int", nullable: false),
                    ZoneName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RadiusKm = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPeakZone = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceZones_ServiceCities_ServiceCityId",
                        column: x => x.ServiceCityId,
                        principalTable: "ServiceCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessibilitySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    FontSize = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    HighContrastMode = table.Column<bool>(type: "bit", nullable: false),
                    ScreenReaderOptimized = table.Column<bool>(type: "bit", nullable: false),
                    ReduceMotion = table.Column<bool>(type: "bit", nullable: false),
                    VoiceGuidanceEnabled = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessibilitySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccessibilitySettings_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AdminRoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminUsers_AdminRoles_AdminRoleId",
                        column: x => x.AdminRoleId,
                        principalTable: "AdminRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdminUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AIAnalyticsEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    AnalyticsType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InputDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutputDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelVersion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAnalyticsEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIAnalyticsEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Announcements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnnouncementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetAudience = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetCity = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsDismissible = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Announcements_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminUserId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_AdminUserId",
                        column: x => x.AdminUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BackgroundVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    VerificationType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifiedByAgency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerificationRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackgroundVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackgroundVerifications_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    BookingType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Cuisine = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SurchargeAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChangeRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedByAdminId = table.Column<int>(type: "int", nullable: false),
                    ApprovedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeployedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangeRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeRequests_Users_ApprovedByAdminId",
                        column: x => x.ApprovedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChangeRequests_Users_RequestedByAdminId",
                        column: x => x.RequestedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatThreads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User1Id = table.Column<int>(type: "int", nullable: false),
                    User2Id = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    LastMessageId = table.Column<int>(type: "int", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnreadCountUser1 = table.Column<int>(type: "int", nullable: false),
                    UnreadCountUser2 = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatThreads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatThreads_Users_User1Id",
                        column: x => x.User1Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatThreads_Users_User2Id",
                        column: x => x.User2Id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefBankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    AccountHolderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IFSC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    UpiId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefBankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefBankAccounts_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefBusinessExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefBusinessExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefBusinessExpenses_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefBusinessGoals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    PeriodKey = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    GoalType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastRefreshedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefBusinessGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefBusinessGoals_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefCertifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProgressPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CertificateUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnrolledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefCertifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefCertifications_CertificationCourses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "CertificationCourses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChefCertifications_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefCustomerLoyaltyBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PointsBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LifetimePointsEarned = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefCustomerLoyaltyBalances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefCustomerLoyaltyBalances_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefEarningsSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PeriodKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlatformCommission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookingsCount = table.Column<int>(type: "int", nullable: false),
                    HoursWorked = table.Column<int>(type: "int", nullable: false),
                    AvgEarningPerHour = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvgEarningPerBooking = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefEarningsSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefEarningsSnapshots_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefFollows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FollowerId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    FollowedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefFollows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefFollows_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChefFollows_Users_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefGstProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Gstin = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    LegalBusinessName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PlaceOfSupplyState = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    IsGstRegistered = table.Column<bool>(type: "bit", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    VerifiedByAdminId = table.Column<int>(type: "int", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefGstProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefGstProfiles_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChefGstProfiles_Users_VerifiedByAdminId",
                        column: x => x.VerifiedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefInventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    QuantityOnHand = table.Column<int>(type: "int", nullable: false),
                    LowStockThreshold = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefInventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefInventoryItems_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefLeaderboardEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeasonId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    BookingsCompleted = table.Column<int>(type: "int", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefLeaderboardEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefLeaderboardEntries_LeaderboardSeasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "LeaderboardSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChefLeaderboardEntries_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefLoyaltyPrograms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    PointsPerRupeeSpent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RedemptionThresholdPoints = table.Column<int>(type: "int", nullable: false),
                    RedemptionValueRupees = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefLoyaltyPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefLoyaltyPrograms_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefMatchScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FactorsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefMatchScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefMatchScores_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChefMatchScores_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefOutlets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    OutletName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    OutletType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    City = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxDailyOrderCapacity = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefOutlets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefOutlets_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TimesBooked = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefPackages_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefPerformanceAnalyticsSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    PeriodKey = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    BookingCount = table.Column<int>(type: "int", nullable: false),
                    Earnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CancelledCount = table.Column<int>(type: "int", nullable: false),
                    CancellationRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefPerformanceAnalyticsSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefPerformanceAnalyticsSnapshots_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefPerformanceSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PeriodKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalBookings = table.Column<int>(type: "int", nullable: false),
                    CompletedBookings = table.Column<int>(type: "int", nullable: false),
                    CancelledBookings = table.Column<int>(type: "int", nullable: false),
                    CompletionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvgRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalEarnings = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalHoursWorked = table.Column<int>(type: "int", nullable: false),
                    AvgResponseTimeMins = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RepeatCustomers = table.Column<int>(type: "int", nullable: false),
                    PerformanceGrade = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefPerformanceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefPerformanceSnapshots_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Cuisines = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Skills = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExperienceYears = table.Column<int>(type: "int", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    AverageRating = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false),
                    TotalBookingsCompleted = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefPromoCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxUses = table.Column<int>(type: "int", nullable: true),
                    UsedCount = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefPromoCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefPromoCodes_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefQualityScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    RatingComponent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompletionComponent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CancellationPenalty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ResponsivenessComponent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverallScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefQualityScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefQualityScores_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefShifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false),
                    SpecificDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    MaxBookingsThisShift = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefShifts_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefStaffMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    StaffName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefStaffMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefStaffMembers_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefTaxEstimates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    FinancialYear = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    GrossIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeductibleExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PresumptiveIncomePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedTaxableIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedTaxLiability = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefTaxEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefTaxEstimates_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefVerifications_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChurnRiskScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    RiskScore = table.Column<int>(type: "int", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DaysSinceLastBooking = table.Column<int>(type: "int", nullable: false),
                    TotalBookings = table.Column<int>(type: "int", nullable: false),
                    BookingsLast90Days = table.Column<int>(type: "int", nullable: false),
                    CancelledLast90Days = table.Column<int>(type: "int", nullable: false),
                    FactorsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InterventionSent = table.Column<bool>(type: "bit", nullable: false),
                    InterventionSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChurnRiskScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChurnRiskScores_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CityManagerAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceCityId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityManagerAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CityManagerAssignments_ServiceCities_ServiceCityId",
                        column: x => x.ServiceCityId,
                        principalTable: "ServiceCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CityManagerAssignments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsContentBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Locale = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DraftContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    PublishedByUserId = table.Column<int>(type: "int", nullable: true),
                    PublishedById = table.Column<int>(type: "int", nullable: true),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsContentBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsContentBlocks_Users_PublishedById",
                        column: x => x.PublishedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsPageRevisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CmsPageId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EditedByUserId = table.Column<int>(type: "int", nullable: false),
                    EditedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsPageRevisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsPageRevisions_CmsPages_CmsPageId",
                        column: x => x.CmsPageId,
                        principalTable: "CmsPages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CmsPageRevisions_Users_EditedByUserId",
                        column: x => x.EditedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommissionLedgers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    BookingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlatformAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChefAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlatformPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionRuleId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionLedgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionLedgers_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommissionProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DefaultPlatformPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DefaultChefPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ClonedFromProfileId = table.Column<int>(type: "int", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    ApprovedByUserId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionProfiles_Users_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionProfiles_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RequirementText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EvidenceNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OwnerAdminId = table.Column<int>(type: "int", nullable: true),
                    LastReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceChecklistItems_Users_OwnerAdminId",
                        column: x => x.OwnerAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CorporateAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    BillingEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Gstin = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentOutstanding = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OwnerUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorporateAccounts_Users_OwnerUserId",
                        column: x => x.OwnerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CorporateTiffinBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyUserId = table.Column<int>(type: "int", nullable: false),
                    PrimaryChefId = table.Column<int>(type: "int", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MealsPerDay = table.Column<int>(type: "int", nullable: false),
                    MealType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RecurringDays = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PricePerMeal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateTiffinBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorporateTiffinBookings_Users_CompanyUserId",
                        column: x => x.CompanyUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorporateTiffinBookings_Users_PrimaryChefId",
                        column: x => x.PrimaryChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CouponUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    DiscountApplied = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CouponUsages_Coupons_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CouponUsages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrmNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AdminId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrmNotes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerBadges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    BadgeDefinitionId = table.Column<int>(type: "int", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerBadges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerBadges_BadgeDefinitions_BadgeDefinitionId",
                        column: x => x.BadgeDefinitionId,
                        principalTable: "BadgeDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerBadges_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerLtvSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    TotalBookings = table.Column<int>(type: "int", nullable: false),
                    TotalSpend = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AverageOrderValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenureDays = table.Column<int>(type: "int", nullable: false),
                    EstimatedAnnualValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Segment = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerLtvSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerLtvSnapshots_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PushNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EmailNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SmsNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PromoNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Theme = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DefaultPaymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ShowSpicyWarnings = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerPreferences_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomMenuBuilds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    BuildName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SelectedItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EstimatedTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomMenuBuilds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomMenuBuilds_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomMenuBuilds_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataExportJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExportType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Format = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DateRangeFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateRangeTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecordCount = table.Column<int>(type: "int", nullable: false),
                    ResultPreview = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedByAdminId = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataExportJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataExportJobs_Users_RequestedByAdminId",
                        column: x => x.RequestedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataPrivacyRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResultDataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataPrivacyRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataPrivacyRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DietaryMatchScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    MatchScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MatchReasons = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietaryMatchScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DietaryMatchScores_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Disputes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisputeNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    RaisedByUserId = table.Column<int>(type: "int", nullable: false),
                    RaisedById = table.Column<int>(type: "int", nullable: true),
                    AgainstUserId = table.Column<int>(type: "int", nullable: true),
                    DisputeType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolvedByAdmin = table.Column<int>(type: "int", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disputes_Users_RaisedById",
                        column: x => x.RaisedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrDrillLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecoveryTimeMinutes = table.Column<int>(type: "int", nullable: true),
                    Findings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConductedByAdminId = table.Column<int>(type: "int", nullable: false),
                    ConductedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrDrillLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrDrillLogs_Users_ConductedByAdminId",
                        column: x => x.ConductedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmergencyAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    IsAvailableNow = table.Column<bool>(type: "bit", nullable: false),
                    AvailableUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentLatitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CurrentLongitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ServiceRadiusKm = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmergencyAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmergencyAvailabilities_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentListings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ListingType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    City = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentListings_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EscalationLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    TriggerAfterHours = table.Column<int>(type: "int", nullable: false),
                    NotifyAdminId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalationLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationLevels_Users_NotifyAdminId",
                        column: x => x.NotifyAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Vendor = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReceiptUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedByAdminId = table.Column<int>(type: "int", nullable: false),
                    ApprovedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expenses_Users_ApprovedByAdminId",
                        column: x => x.ApprovedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_Users_SubmittedByAdminId",
                        column: x => x.SubmittedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeatureFlags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RolloutPercentage = table.Column<int>(type: "int", nullable: false),
                    TargetPlatform = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetAudience = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetCity = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Environment = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureFlags_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeedbackSurveyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackResponses_FeedbackSurveys_FeedbackSurveyId",
                        column: x => x.FeedbackSurveyId,
                        principalTable: "FeedbackSurveys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackResponses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialReportSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodKey = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    GrossRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GstCollected = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlatformCommissionEarned = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TdsDeducted = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalRefundsIssued = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCreditNotesValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDebitNotesValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPlatformRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceCount = table.Column<int>(type: "int", nullable: false),
                    RefundCount = table.Column<int>(type: "int", nullable: false),
                    GeneratedByAdminId = table.Column<int>(type: "int", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialReportSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialReportSnapshots_Users_GeneratedByAdminId",
                        column: x => x.GeneratedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FraudDetectionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    DetectionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RiskScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Evidence = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AdminNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FraudDetectionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FraudDetectionLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FSSAILicenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LicenseType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LicenseDocUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RenewalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RenewalAlertSent = table.Column<bool>(type: "bit", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FSSAILicenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FSSAILicenses_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    TotalGuestCount = table.Column<int>(type: "int", nullable: false),
                    Cuisine = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupBookings_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GstReturnPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodKey = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    ReturnType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TotalInvoiceCount = table.Column<int>(type: "int", nullable: false),
                    TotalTaxableValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalGstCollected = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FilingReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FiledByAdminId = table.Column<int>(type: "int", nullable: true),
                    FiledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GstReturnPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GstReturnPeriods_Users_FiledByAdminId",
                        column: x => x.FiledByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IngredientListings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    IngredientName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ListingType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngredientListings_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InsurancePolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PolicyType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PolicyNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PremiumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PremiumPeriod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CoverageAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PolicyDocUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsurancePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsurancePolicies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InternalNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByAdminId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InternalNotes_Users_CreatedByAdminId",
                        column: x => x.CreatedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    IngredientName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReorderThreshold = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastRestockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GSTPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GSTAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlatformFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GSTIN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IpRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedByAdminId = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IpRules_Users_CreatedByAdminId",
                        column: x => x.CreatedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LiveChatSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AgentAdminId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InitialMessage = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveChatSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveChatSessions_Users_AgentAdminId",
                        column: x => x.AgentAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LiveChatSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LiveStreams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StreamUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IngestUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreamKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewerCount = table.Column<int>(type: "int", nullable: false),
                    PeakViewerCount = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveStreams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveStreams_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false),
                    RedeemedPoints = table.Column<int>(type: "int", nullable: false),
                    PendingPoints = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Bronze"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyPoints_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    BalanceAfter = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MealBoxSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NextDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealBoxSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealBoxSubscriptions_MealBoxPlans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "MealBoxPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealBoxSubscriptions_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MealPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PlanType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WeekStart = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntriesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PreferredChefId = table.Column<int>(type: "int", nullable: true),
                    Preferences = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealPlans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MealPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrls = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cuisine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    CommentCount = table.Column<int>(type: "int", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealPosts_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MealSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    ScheduleName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    PreferredTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EstimatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastGeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealSchedules_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealSchedules_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModerationFlags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    FlagType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MatchedTerms = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FlaggedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationFlags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationFlags_Users_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsPushSent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OnCallSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminId = table.Column<int>(type: "int", nullable: false),
                    ShiftStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShiftEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnCallSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OnCallSchedules_Users_AdminId",
                        column: x => x.AdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromoCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CampaignType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetAudience = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetCity = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    LinkedCouponCode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoCampaigns_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QualityChecks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    TasteScore = table.Column<int>(type: "int", nullable: false),
                    HygieneScore = table.Column<int>(type: "int", nullable: false),
                    PresentationScore = table.Column<int>(type: "int", nullable: false),
                    PortionScore = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WouldReorder = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityChecks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityChecks_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuickReorderCombos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    SourceBookingId = table.Column<int>(type: "int", nullable: true),
                    ComboName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UseCount = table.Column<int>(type: "int", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickReorderCombos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuickReorderCombos_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuickReorderCombos_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReferralCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalReferrals = table.Column<int>(type: "int", nullable: false),
                    SuccessfulReferrals = table.Column<int>(type: "int", nullable: false),
                    PointsEarned = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralCodes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ManagerUserId = table.Column<int>(type: "int", nullable: true),
                    ManagerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regions_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Metric = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GroupBy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateRangeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByAdminId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportDefinitions_Users_CreatedByAdminId",
                        column: x => x.CreatedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Runbooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    StepsMarkdown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByAdminId = table.Column<int>(type: "int", nullable: false),
                    UsageCount = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runbooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Runbooks_Users_CreatedByAdminId",
                        column: x => x.CreatedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SafetyAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AlertType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyAlerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SafetyIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReportedByUserId = table.Column<int>(type: "int", nullable: false),
                    ReportedById = table.Column<int>(type: "int", nullable: true),
                    IsSystemGenerated = table.Column<bool>(type: "bit", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    IncidentType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedAdminId = table.Column<int>(type: "int", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyIncidents_Users_ReportedById",
                        column: x => x.ReportedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SafetyPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PreferredChefGender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AcceptOnlyFemaleVerifiedHouseholds = table.Column<bool>(type: "bit", nullable: false),
                    RequireSecondPersonPresent = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SafetyPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SafetyPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAuditEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityAuditEntries_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecurityEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAlert = table.Column<bool>(type: "bit", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServicePackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    PackageName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValidityDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePackages_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettlementBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    VendorCount = table.Column<int>(type: "int", nullable: false),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTdsAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedByAdminId = table.Column<int>(type: "int", nullable: false),
                    ApprovedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BankFileReference = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementBatches_Users_ApprovedByAdminId",
                        column: x => x.ApprovedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementBatches_Users_CreatedByAdminId",
                        column: x => x.CreatedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    HourlyWage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffMembers_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MealsPerWeek = table.Column<int>(type: "int", nullable: false),
                    PricePerMonth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreferredChefId = table.Column<int>(type: "int", nullable: true),
                    AutoAssignSameChef = table.Column<bool>(type: "bit", nullable: false),
                    FallbackPolicy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_PreferredChefId",
                        column: x => x.PreferredChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TicketNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    AssignedAdminId = table.Column<int>(type: "int", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTickets_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OnCallAdminId = table.Column<int>(type: "int", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostmortemNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemIncidents_Users_OnCallAdminId",
                        column: x => x.OnCallAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TdsCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    FinancialYear = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Quarter = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    CertificateNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTdsAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdsCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TdsCertificates_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TdsReturns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinancialYear = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Quarter = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    TotalDeducteeCount = table.Column<int>(type: "int", nullable: false),
                    TotalGrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalTdsDeducted = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FilingReference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FiledByAdminId = table.Column<int>(type: "int", nullable: true),
                    FiledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdsReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TdsReturns_Users_FiledByAdminId",
                        column: x => x.FiledByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketAssignmentRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    TicketQueueId = table.Column<int>(type: "int", nullable: false),
                    DefaultAssigneeAdminId = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketAssignmentRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketAssignmentRules_TicketQueues_TicketQueueId",
                        column: x => x.TicketQueueId,
                        principalTable: "TicketQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketAssignmentRules_Users_DefaultAssigneeAdminId",
                        column: x => x.DefaultAssigneeAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TwoFactorSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TotpSecretEncrypted = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    BackupCodesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnabledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwoFactorSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TwoFactorSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDietaryProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DietType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HealthGoals = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CuisinePreferences = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpiceLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MedicalConditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalorieTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDietaryProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDietaryProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLocations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DietaryPreference = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    TrustedContactNotifyEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ProfileCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    LoginAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LoggedOutAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VendorSettlementHolds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HeldByAdminId = table.Column<int>(type: "int", nullable: false),
                    HeldAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReleasedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorSettlementHolds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorSettlementHolds_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorSettlementHolds_Users_HeldByAdminId",
                        column: x => x.HeldByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoConsultations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    ConsultType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMins = table.Column<int>(type: "int", nullable: false),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MeetingLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MeetingId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    Feedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoConsultations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoConsultations_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VideoConsultations_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VoiceCommandLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RawTranscript = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DetectedIntent = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ParametersJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ActionTaken = table.Column<bool>(type: "bit", nullable: false),
                    RelatedBookingId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoiceCommandLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VoiceCommandLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VulnerabilityFindings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    AssignedToAdminId = table.Column<int>(type: "int", nullable: true),
                    DiscoveredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VulnerabilityFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VulnerabilityFindings_Users_AssignedToAdminId",
                        column: x => x.AssignedToAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RelatedBookingId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WishlistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishlistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WishlistItems_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WhiteLabelUsageLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ResponseStatus = table.Column<int>(type: "int", nullable: false),
                    CalledAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhiteLabelUsageLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhiteLabelUsageLogs_WhiteLabelClients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "WhiteLabelClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CorporateBillingRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BilledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateBillingRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorporateBillingRecords_CorporateSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "CorporateSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnnouncementReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnnouncementId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DismissedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnouncementReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnnouncementReceipts_Announcements_AnnouncementId",
                        column: x => x.AnnouncementId,
                        principalTable: "Announcements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnnouncementReceipts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArrivalVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    ArrivalOtp = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SelfieUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailedAttempts = table.Column<int>(type: "int", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrivalVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArrivalVerifications_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArrivalVerifications_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "B2BBookingRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnerId = table.Column<int>(type: "int", nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2BBookingRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_B2BBookingRequests_B2BPartners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "B2BPartners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_B2BBookingRequests_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BillSplits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    InitiatedByCustomerId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SplitMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillSplits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillSplits_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillSplits_Users_InitiatedByCustomerId",
                        column: x => x.InitiatedByCustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookingCarbonEstimates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    EstimatedCarbonKg = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BreakdownJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingCarbonEstimates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingCarbonEstimates_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingTimeoutAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ExpectedEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GraceMinutes = table.Column<int>(type: "int", nullable: false),
                    AlertLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NudgeFiredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RelatedSafetyIncidentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingTimeoutAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingTimeoutAlerts_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BookingTrackings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    ChefLat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChefLng = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ETA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingTrackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingTrackings_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CancellationRiskScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    CustomerCancellationRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ChefCancellationRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RiskScore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RiskLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancellationRiskScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CancellationRiskScores_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Complaints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ComplainantUserId = table.Column<int>(type: "int", nullable: false),
                    AgainstChefId = table.Column<int>(type: "int", nullable: true),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResolutionAction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedAdminId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complaints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Complaints_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Complaints_Users_AgainstChefId",
                        column: x => x.AgainstChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Complaints_Users_AssignedAdminId",
                        column: x => x.AssignedAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Complaints_Users_ComplainantUserId",
                        column: x => x.ComplainantUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerRatings_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerRatings_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerRatings_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DigitalContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    TermsText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerAccepted = table.Column<bool>(type: "bit", nullable: false),
                    CustomerAcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CustomerAcceptedIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ChefAccepted = table.Column<bool>(type: "bit", nullable: false),
                    ChefAcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChefAcceptedIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalContracts_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventStaffRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    RatePerPerson = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStaffRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventStaffRequests_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderTrackingEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EstimatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTrackingEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderTrackingEvents_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GatewayTransactionRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Gateway = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GatewayResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefundRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RefundMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransactionRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkedCreditNoteId = table.Column<int>(type: "int", nullable: true),
                    ProcessedByAdminId = table.Column<int>(type: "int", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefundRequests_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefundRequests_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefundRequests_Users_ProcessedByAdminId",
                        column: x => x.ProcessedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChefResponse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsFlagged = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChefRespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeamBookingAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    RoleInTeam = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamBookingAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamBookingAssignments_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamBookingAssignments_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrustedContactAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveryStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustedContactAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrustedContactAlerts_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrustedContactAlerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettlementRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankAccountId = table.Column<int>(type: "int", nullable: true),
                    TransactionRef = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementRequests_ChefBankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "ChefBankAccounts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SettlementRequests_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChefMenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefProfileId = table.Column<int>(type: "int", nullable: false),
                    Cuisine = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DishName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVeg = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefMenuItems_ChefProfiles_ChefProfileId",
                        column: x => x.ChefProfileId,
                        principalTable: "ChefProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefPortfolioItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefProfileId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Cuisine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DishName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefPortfolioItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefPortfolioItems_ChefProfiles_ChefProfileId",
                        column: x => x.ChefProfileId,
                        principalTable: "ChefProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefServiceOfferings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefProfileId = table.Column<int>(type: "int", nullable: false),
                    MarketplaceServiceId = table.Column<int>(type: "int", nullable: false),
                    CustomPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefServiceOfferings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefServiceOfferings_ChefProfiles_ChefProfileId",
                        column: x => x.ChefProfileId,
                        principalTable: "ChefProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChefServiceOfferings_MarketplaceServices_MarketplaceServiceId",
                        column: x => x.MarketplaceServiceId,
                        principalTable: "MarketplaceServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChefPromoRedemptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefPromoCodeId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChefPromoRedemptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChefPromoRedemptions_ChefPromoCodes_ChefPromoCodeId",
                        column: x => x.ChefPromoCodeId,
                        principalTable: "ChefPromoCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChefPromoRedemptions_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CmsContentBlockVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CmsContentBlockId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SavedByUserId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SavedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CmsContentBlockVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CmsContentBlockVersions_CmsContentBlocks_CmsContentBlockId",
                        column: x => x.CmsContentBlockId,
                        principalTable: "CmsContentBlocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CmsContentBlockVersions_Users_SavedByUserId",
                        column: x => x.SavedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommissionProfileRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommissionProfileId = table.Column<int>(type: "int", nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    City = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    MinMonthlyBookingVolume = table.Column<int>(type: "int", nullable: true),
                    MaxMonthlyBookingVolume = table.Column<int>(type: "int", nullable: true),
                    MinChefRating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SeasonStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SeasonEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxChefTenureDays = table.Column<int>(type: "int", nullable: true),
                    OverridePlatformPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DeltaPlatformPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsStackable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionProfileRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionProfileRules_CommissionProfiles_CommissionProfileId",
                        column: x => x.CommissionProfileId,
                        principalTable: "CommissionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommissionRuleAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommissionProfileId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionRuleAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionRuleAuditLogs_CommissionProfiles_CommissionProfileId",
                        column: x => x.CommissionProfileId,
                        principalTable: "CommissionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommissionRuleAuditLogs_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DynamicCommissionEvaluations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    CommissionProfileId = table.Column<int>(type: "int", nullable: false),
                    BookingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComputedPlatformPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComputedChefPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AppliedRulesSummary = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicCommissionEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DynamicCommissionEvaluations_CommissionProfiles_CommissionProfileId",
                        column: x => x.CommissionProfileId,
                        principalTable: "CommissionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DynamicCommissionEvaluations_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Label = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    KeyPrefix = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    KeyHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Scopes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CorporateAccountId = table.Column<int>(type: "int", nullable: true),
                    RateLimitPerMinute = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByAdminId = table.Column<int>(type: "int", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ApiKeys_Users_CreatedByAdminId",
                        column: x => x.CreatedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BulkOrderRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorporateAccountId = table.Column<int>(type: "int", nullable: false),
                    RequestedByUserId = table.Column<int>(type: "int", nullable: false),
                    City = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    MealCount = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VolumeDiscountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkOrderRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulkOrderRequests_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BulkOrderRequests_Users_RequestedByUserId",
                        column: x => x.RequestedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CorporateAccountMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorporateAccountId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateAccountMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorporateAccountMembers_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CorporateAccountMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CorporateInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorporateAccountId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PeriodKey = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GstAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorporateInvoices_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CorporateLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorporateAccountId = table.Column<int>(type: "int", nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    City = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DefaultHeadcount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorporateLocations_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnterpriseAuditEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorporateAccountId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnterpriseAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnterpriseAuditEntries_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnterpriseAuditEntries_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EnterpriseUsageSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorporateAccountId = table.Column<int>(type: "int", nullable: false),
                    PeriodKey = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    TotalOrders = table.Column<int>(type: "int", nullable: false),
                    TotalMeals = table.Column<int>(type: "int", nullable: false),
                    TotalSpend = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActiveMembers = table.Column<int>(type: "int", nullable: false),
                    ActiveLocations = table.Column<int>(type: "int", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnterpriseUsageSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnterpriseUsageSnapshots_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SlaContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorporateAccountId = table.Column<int>(type: "int", nullable: false),
                    ContractNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UptimeGuaranteePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupportResponseTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    PenaltyPercentPerBreachHour = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyContractValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlaContracts_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SsoConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorporateAccountId = table.Column<int>(type: "int", nullable: false),
                    Protocol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdpEntityId = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    IdpSsoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdpCertificate = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmailDomain = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SsoConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SsoConnections_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WebhookSubscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorporateAccountId = table.Column<int>(type: "int", nullable: true),
                    TargetUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EventTypes = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SigningSecret = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookSubscriptions_CorporateAccounts_CorporateAccountId",
                        column: x => x.CorporateAccountId,
                        principalTable: "CorporateAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DisputeEvidences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisputeId = table.Column<int>(type: "int", nullable: false),
                    UploadedBy = table.Column<int>(type: "int", nullable: false),
                    FileUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisputeEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisputeEvidences_Disputes_DisputeId",
                        column: x => x.DisputeId,
                        principalTable: "Disputes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DisputeMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisputeId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    SenderRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DisputeMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DisputeMessages_Disputes_DisputeId",
                        column: x => x.DisputeId,
                        principalTable: "Disputes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EquipmentRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    RequestedByChefId = table.Column<int>(type: "int", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentRequests_EquipmentListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "EquipmentListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentRequests_Users_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FeatureFlagAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeatureFlagId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ChangedByUserId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlagAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeatureFlagAuditLogs_FeatureFlags_FeatureFlagId",
                        column: x => x.FeatureFlagId,
                        principalTable: "FeatureFlags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeatureFlagAuditLogs_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupBookingSplits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupBookingId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupBookingSplits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupBookingSplits_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupBookingSplits_GroupBookings_GroupBookingId",
                        column: x => x.GroupBookingId,
                        principalTable: "GroupBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupBookingSplits_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IngredientSwapRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingId = table.Column<int>(type: "int", nullable: false),
                    RequestedByChefId = table.Column<int>(type: "int", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: true),
                    OfferedItem = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientSwapRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IngredientSwapRequests_IngredientListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "IngredientListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngredientSwapRequests_Users_RequestedById",
                        column: x => x.RequestedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InsuranceClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyId = table.Column<int>(type: "int", nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClaimAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EvidenceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimRef = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FiledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsuranceClaims_InsurancePolicies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "InsurancePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    QuantityChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CreditNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreditNoteNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GstAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LinkedRefundRequestId = table.Column<int>(type: "int", nullable: true),
                    IssuedByAdminId = table.Column<int>(type: "int", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditNotes_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditNotes_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditNotes_Users_IssuedByAdminId",
                        column: x => x.IssuedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DebitNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DebitNoteNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    PartyType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GstAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IssuedByAdminId = table.Column<int>(type: "int", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebitNotes_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DebitNotes_Users_IssuedByAdminId",
                        column: x => x.IssuedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DebitNotes_Users_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceDeliveryLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceDeliveryLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceDeliveryLogs_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiveChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LiveChatSessionId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    SenderRole = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiveChatMessages_LiveChatSessions_LiveChatSessionId",
                        column: x => x.LiveChatSessionId,
                        principalTable: "LiveChatSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StreamComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StreamId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StreamComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StreamComments_LiveStreams_StreamId",
                        column: x => x.StreamId,
                        principalTable: "LiveStreams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StreamComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MealBoxDeliveries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealBoxDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealBoxDeliveries_MealBoxSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "MealBoxSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealPostComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPostComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealPostComments_MealPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "MealPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealPostComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MealPostLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPostLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealPostLikes_MealPosts_PostId",
                        column: x => x.PostId,
                        principalTable: "MealPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromoCampaignVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromoCampaignId = table.Column<int>(type: "int", nullable: false),
                    VariantName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HeadlineText = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SubText = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DeepLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Weight = table.Column<int>(type: "int", nullable: false),
                    ImpressionCount = table.Column<int>(type: "int", nullable: false),
                    ClickCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCampaignVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoCampaignVariants_PromoCampaigns_PromoCampaignId",
                        column: x => x.PromoCampaignId,
                        principalTable: "PromoCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReferralUses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReferralCodeId = table.Column<int>(type: "int", nullable: false),
                    ReferredUserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReferralUses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReferralUses_ReferralCodes_ReferralCodeId",
                        column: x => x.ReferralCodeId,
                        principalTable: "ReferralCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReferralUses_Users_ReferredUserId",
                        column: x => x.ReferredUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegionCities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    ServiceCityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionCities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegionCities_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegionCities_ServiceCities_ServiceCityId",
                        column: x => x.ServiceCityId,
                        principalTable: "ServiceCities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportDefinitionId = table.Column<int>(type: "int", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecipientEmails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRunDue = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledReports_ReportDefinitions_ReportDefinitionId",
                        column: x => x.ReportDefinitionId,
                        principalTable: "ReportDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffShiftLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffMemberId = table.Column<int>(type: "int", nullable: false),
                    ShiftDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoursWorked = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WagePaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffShiftLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffShiftLogs_StaffMembers_StaffMemberId",
                        column: x => x.StaffMemberId,
                        principalTable: "StaffMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CallLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AgentAdminId = table.Column<int>(type: "int", nullable: true),
                    Direction = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    LinkedTicketId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallLogs_SupportTickets_LinkedTicketId",
                        column: x => x.LinkedTicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CallLogs_Users_AgentAdminId",
                        column: x => x.AgentAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CallLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EscalationEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    FromLevel = table.Column<int>(type: "int", nullable: false),
                    ToLevel = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WasAutomatic = table.Column<bool>(type: "bit", nullable: false),
                    EscalatedByAdminId = table.Column<int>(type: "int", nullable: true),
                    EscalatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscalationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscalationEvents_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EscalationEvents_Users_EscalatedByAdminId",
                        column: x => x.EscalatedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SlaTrackers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    SlaPolicyId = table.Column<int>(type: "int", nullable: false),
                    FirstResponseDueAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolutionDueAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstRespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsResponseBreached = table.Column<bool>(type: "bit", nullable: false),
                    IsResolutionBreached = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaTrackers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlaTrackers_SlaPolicies_SlaPolicyId",
                        column: x => x.SlaPolicyId,
                        principalTable: "SlaPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SlaTrackers_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    SenderRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsInternal = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportMessages_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FromValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedByAdminId = table.Column<int>(type: "int", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketActivityLogs_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketActivityLogs_Users_ChangedByAdminId",
                        column: x => x.ChangedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketEscalationStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    CurrentLevel = table.Column<int>(type: "int", nullable: false),
                    EnteredLevelAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketEscalationStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketEscalationStates_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillSplitParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillSplitId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ShareAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillSplitParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillSplitParticipants_BillSplits_BillSplitId",
                        column: x => x.BillSplitId,
                        principalTable: "BillSplits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillSplitParticipants_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintId = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddedByAdminId = table.Column<int>(type: "int", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintUpdates_Complaints_ComplaintId",
                        column: x => x.ComplaintId,
                        principalTable: "Complaints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplaintUpdates_Users_AddedByAdminId",
                        column: x => x.AddedByAdminId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SupportStaffAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventStaffRequestId = table.Column<int>(type: "int", nullable: false),
                    StaffMemberId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportStaffAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportStaffAssignments_EventStaffRequests_EventStaffRequestId",
                        column: x => x.EventStaffRequestId,
                        principalTable: "EventStaffRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupportStaffAssignments_StaffMembers_StaffMemberId",
                        column: x => x.StaffMemberId,
                        principalTable: "StaffMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SettlementBatchItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettlementBatchId = table.Column<int>(type: "int", nullable: false),
                    SettlementRequestId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TdsAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettlementBatchItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SettlementBatchItems_SettlementBatches_SettlementBatchId",
                        column: x => x.SettlementBatchId,
                        principalTable: "SettlementBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SettlementBatchItems_SettlementRequests_SettlementRequestId",
                        column: x => x.SettlementRequestId,
                        principalTable: "SettlementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SettlementBatchItems_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TdsDeductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    SettlementRequestId = table.Column<int>(type: "int", nullable: true),
                    Section = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    GrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TdsRatePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TdsAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPayableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FinancialYear = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Quarter = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    DeductedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TdsDeductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TdsDeductions_SettlementRequests_SettlementRequestId",
                        column: x => x.SettlementRequestId,
                        principalTable: "SettlementRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TdsDeductions_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageMenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServicePackageId = table.Column<int>(type: "int", nullable: false),
                    ChefMenuItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageMenuItems_ChefMenuItems_ChefMenuItemId",
                        column: x => x.ChefMenuItemId,
                        principalTable: "ChefMenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageMenuItems_ServicePackages_ServicePackageId",
                        column: x => x.ServicePackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChefMenuItemId = table.Column<int>(type: "int", nullable: false),
                    IngredientName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    QuantityUsed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_ChefMenuItems_ChefMenuItemId",
                        column: x => x.ChefMenuItemId,
                        principalTable: "ChefMenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BulkOrderLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BulkOrderRequestId = table.Column<int>(type: "int", nullable: false),
                    ChefId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedBookingId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkOrderLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulkOrderLineItems_BulkOrderRequests_BulkOrderRequestId",
                        column: x => x.BulkOrderRequestId,
                        principalTable: "BulkOrderRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BulkOrderLineItems_Users_ChefId",
                        column: x => x.ChefId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SlaContractBreaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SlaContractId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BreachHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreditAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlaContractBreaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SlaContractBreaches_SlaContracts_SlaContractId",
                        column: x => x.SlaContractId,
                        principalTable: "SlaContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SsoLoginLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SsoConnectionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SsoLoginLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SsoLoginLogs_SsoConnections_SsoConnectionId",
                        column: x => x.SsoConnectionId,
                        principalTable: "SsoConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SsoLoginLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "WebhookDeliveryLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WebhookSubscriptionId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResponseStatusCode = table.Column<int>(type: "int", nullable: true),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebhookDeliveryLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebhookDeliveryLogs_WebhookSubscriptions_WebhookSubscriptionId",
                        column: x => x.WebhookSubscriptionId,
                        principalTable: "WebhookSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromoCampaignDailyStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PromoCampaignId = table.Column<int>(type: "int", nullable: false),
                    PromoCampaignVariantId = table.Column<int>(type: "int", nullable: false),
                    DateKey = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Impressions = table.Column<int>(type: "int", nullable: false),
                    Clicks = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromoCampaignDailyStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromoCampaignDailyStats_PromoCampaignVariants_PromoCampaignVariantId",
                        column: x => x.PromoCampaignVariantId,
                        principalTable: "PromoCampaignVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PromoCampaignDailyStats_PromoCampaigns_PromoCampaignId",
                        column: x => x.PromoCampaignId,
                        principalTable: "PromoCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledReportRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduledReportId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResultSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RunAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledReportRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledReportRuns_ScheduledReports_ScheduledReportId",
                        column: x => x.ScheduledReportId,
                        principalTable: "ScheduledReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CmsPages",
                columns: new[] { "Id", "Content", "IsActive", "Slug", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "## Terms & Conditions\n\nPlaceholder — replace with real legal copy before launch (see release checklist).", true, "terms", "Terms & Conditions", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "## Privacy Policy\n\nPlaceholder — must be DPDP Act 2023 compliant for Indian users before launch.", true, "privacy", "Privacy Policy", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "## About LovEat\n\nHome chef booking platform for Tamil Nadu.", true, "about", "About LovEat", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "## Refund Policy\n\nRefunds processed in 5-7 business days.", true, "refund", "Refund Policy", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "CommissionRules",
                columns: new[] { "Id", "AppliesTo", "ChefPercent", "CreatedAt", "IsActive", "PlatformPercent", "RuleName", "TargetValue", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "All", 85m, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(5796), true, 15m, "Default Commission", null, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(5797) },
                    { 2, "BookingType", 88m, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(5802), true, 12m, "Emergency Booking", "Emergency", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(5802) },
                    { 3, "BookingType", 82m, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(5806), true, 18m, "Event Cooking", "Event", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(5806) }
                });

            migrationBuilder.InsertData(
                table: "CorporatePlans",
                columns: new[] { "Id", "Features", "IsActive", "MealsPerDay", "MonthlyPrice", "PlanName", "PricePerMeal", "SortOrder" },
                values: new object[,]
                {
                    { 1, "[\"Dedicated chef\",\"Mon-Fri only\",\"Lunch only\",\"Email support\"]", true, 20, 46800m, "Basic", 90m, 1 },
                    { 2, "[\"Dedicated chef\",\"Mon-Sat\",\"Lunch + Snack\",\"Phone support\",\"Monthly report\"]", true, 40, 88400m, "Standard", 85m, 2 },
                    { 3, "[\"2 dedicated chefs\",\"Mon-Sun\",\"All meals\",\"Priority support\",\"Weekly reports\",\"Custom menu\"]", true, 80, 166400m, "Premium", 80m, 3 },
                    { 4, "[\"Chef team\",\"24/7 availability\",\"All meals\",\"Account manager\",\"Daily reports\",\"Custom menu\",\"GST invoicing\"]", true, 200, 390000m, "Enterprise", 75m, 4 }
                });

            migrationBuilder.InsertData(
                table: "Coupons",
                columns: new[] { "Id", "AppliesTo", "Code", "CreatedAt", "Description", "DiscountType", "DiscountValue", "IsActive", "MaxDiscount", "MinOrderAmount", "TargetValue", "UsageLimit", "UsageLimitPerUser", "UsedCount", "ValidFrom", "ValidTo" },
                values: new object[,]
                {
                    { 1, "All", "WELCOME100", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(3451), "Welcome offer for new users", "Flat", 100m, true, 0m, 500m, null, 0, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "All", "LOVEAT20", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(3470), "20% off on all bookings", "Percent", 20m, true, 200m, 300m, null, 500, 1, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "All", "CHEF15", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(3477), "15% off, chef discovery promo", "Percent", 15m, true, 150m, 0m, null, 0, 2, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "FAQs",
                columns: new[] { "Id", "Answer", "Audience", "Category", "CreatedAt", "HelpfulCount", "IsActive", "Question", "SortOrder" },
                values: new object[,]
                {
                    { 1, "Go to My Bookings, tap the booking, then Cancel. Cancellations made 2+ hours before are fully refunded.", "Customer", "Booking", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(533), 0, true, "How do I cancel a booking?", 1 },
                    { 2, "Refunds are processed within 5-7 business days to your original payment method.", "Customer", "Payment", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(540), 0, true, "When will I get my refund?", 2 },
                    { 3, "All chefs go through identity verification, cooking skill assessment, and background checks.", "Customer", "General", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(543), 0, true, "How are chefs verified?", 3 },
                    { 4, "Yes. Contact the chef via chat at least 3 hours before the booking to agree on a new time.", "Customer", "Booking", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(545), 0, true, "Can I reschedule a booking?", 4 },
                    { 5, "We accept UPI, credit/debit cards, net banking, and LovEat wallet.", "All", "Payment", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(547), 0, true, "What payment methods are accepted?", 5 },
                    { 6, "Earnings are credited to your LovEat wallet within 24 hours of booking completion.", "Chef", "Payment", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(549), 0, true, "When do I receive my earnings?", 1 },
                    { 7, "If cancellation is within 2 hours of the booking, you receive a 50% cancellation fee.", "Chef", "Booking", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(551), 0, true, "What if a customer cancels last minute?", 2 },
                    { 8, "Chef verification typically takes 3-5 business days after all documents are submitted.", "Chef", "Verification", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(553), 0, true, "How long does verification take?", 3 },
                    { 9, "Yes. You set your hourly rate. LovEat charges a 15% platform commission on completed bookings.", "Chef", "General", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(555), 0, true, "Can I set my own prices?", 4 }
                });

            migrationBuilder.InsertData(
                table: "PricingRules",
                columns: new[] { "Id", "AppliesTo", "CreatedAt", "DaysOfWeek", "Description", "EndHour", "IsActive", "MultiplierPercent", "RuleName", "RuleType", "StartHour", "TargetValue", "UpdatedAt", "ValidFrom", "ValidTo" },
                values: new object[,]
                {
                    { 1, "All", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(2411), null, "6-9 PM peak demand hours", 21, true, 20m, "Peak Hour Surcharge", "PeakHour", 18, null, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(2412), null, null },
                    { 2, "All", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(2421), "Sat,Sun", "Saturday and Sunday booking premium", null, true, 15m, "Weekend Premium", "Weekend", null, null, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(2422), null, null },
                    { 3, "BookingType", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(2425), null, "Same-hour emergency booking fee", null, true, 30m, "Emergency Booking", "Emergency", null, "Emergency", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(2426), null, null },
                    { 4, "All", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(2429), null, "7-10 AM breakfast peak", 10, true, 10m, "Morning Peak", "PeakHour", 7, null, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(2429), null, null }
                });

            migrationBuilder.InsertData(
                table: "ServiceCities",
                columns: new[] { "Id", "BookingCount", "ChefCount", "CityName", "CreatedAt", "IsActive", "IsLaunching", "Latitude", "Longitude", "RadiusKm", "State" },
                values: new object[,]
                {
                    { 1, 1240, 48, "Coimbatore", new DateTime(2026, 9, 9, 0, 52, 53, 917, DateTimeKind.Utc).AddTicks(656), true, false, 11.0168m, 76.9558m, 25m, "Tamil Nadu" },
                    { 2, 3200, 85, "Chennai", new DateTime(2026, 9, 9, 0, 52, 53, 917, DateTimeKind.Utc).AddTicks(670), true, false, 13.0827m, 80.2707m, 35m, "Tamil Nadu" },
                    { 3, 480, 22, "Madurai", new DateTime(2026, 9, 9, 0, 52, 53, 917, DateTimeKind.Utc).AddTicks(675), true, false, 9.9252m, 78.1198m, 20m, "Tamil Nadu" },
                    { 4, 310, 14, "Trichy", new DateTime(2026, 9, 9, 0, 52, 53, 917, DateTimeKind.Utc).AddTicks(684), true, false, 10.7905m, 78.7047m, 18m, "Tamil Nadu" },
                    { 5, 0, 0, "Bangalore", new DateTime(2026, 9, 9, 0, 52, 53, 917, DateTimeKind.Utc).AddTicks(687), false, true, 12.9716m, 77.5946m, 30m, "Karnataka" }
                });

            migrationBuilder.InsertData(
                table: "UserSegments",
                columns: new[] { "Id", "CreatedAt", "Description", "FilterJson", "IsActive", "Name", "UpdatedAt", "UserCount" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(9940), "10+ bookings", "{}", true, "VIP Users", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(9940), 0 },
                    { 2, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(9946), "3-9 bookings", "{}", true, "Regular Users", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(9946), 0 },
                    { 3, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(9948), "1-2 bookings", "{}", true, "New Users", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(9949), 0 },
                    { 4, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(9951), "No booking in 60 days", "{}", true, "Inactive Users", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(9951), 0 },
                    { 5, new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(9953), "Active then stopped", "{}", true, "Churn Risk", new DateTime(2026, 9, 9, 0, 52, 53, 916, DateTimeKind.Utc).AddTicks(9953), 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessibilitySettings_CustomerId",
                table: "AccessibilitySettings",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminRoles_RoleName",
                table: "AdminRoles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_AdminRoleId",
                table: "AdminUsers",
                column: "AdminRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_UserId",
                table: "AdminUsers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIAnalyticsEntries_AnalyticsType",
                table: "AIAnalyticsEntries",
                column: "AnalyticsType");

            migrationBuilder.CreateIndex(
                name: "IX_AIAnalyticsEntries_UserId",
                table: "AIAnalyticsEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementReceipts_AnnouncementId_UserId",
                table: "AnnouncementReceipts",
                columns: new[] { "AnnouncementId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnnouncementReceipts_UserId",
                table: "AnnouncementReceipts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatedByUserId",
                table: "Announcements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_Status_StartAt_EndAt",
                table: "Announcements",
                columns: new[] { "Status", "StartAt", "EndAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_CorporateAccountId",
                table: "ApiKeys",
                column: "CorporateAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_CreatedByAdminId",
                table: "ApiKeys",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_KeyHash",
                table: "ApiKeys",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppSettingsEntries_Key",
                table: "AppSettingsEntries",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArrivalVerifications_BookingId",
                table: "ArrivalVerifications",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArrivalVerifications_ChefId",
                table: "ArrivalVerifications",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AdminUserId",
                table: "AuditLogs",
                column: "AdminUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Module",
                table: "AuditLogs",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_AutoInvoiceRunLogs_BookingId",
                table: "AutoInvoiceRunLogs",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_B2BBookingRequests_BookingId",
                table: "B2BBookingRequests",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_B2BBookingRequests_PartnerId",
                table: "B2BBookingRequests",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_B2BPartners_ApiKey",
                table: "B2BPartners",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundVerifications_ChefId_VerificationType",
                table: "BackgroundVerifications",
                columns: new[] { "ChefId", "VerificationType" });

            migrationBuilder.CreateIndex(
                name: "IX_BackgroundVerifications_Status",
                table: "BackgroundVerifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BadgeDefinitions_Key",
                table: "BadgeDefinitions",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BillSplitParticipants_BillSplitId",
                table: "BillSplitParticipants",
                column: "BillSplitId");

            migrationBuilder.CreateIndex(
                name: "IX_BillSplitParticipants_CustomerId",
                table: "BillSplitParticipants",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BillSplits_BookingId",
                table: "BillSplits",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BillSplits_InitiatedByCustomerId",
                table: "BillSplits",
                column: "InitiatedByCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCarbonEstimates_BookingId",
                table: "BookingCarbonEstimates",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingFunnelSnapshots_PeriodKey",
                table: "BookingFunnelSnapshots",
                column: "PeriodKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ChefId",
                table: "Bookings",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerId",
                table: "Bookings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ScheduledAt",
                table: "Bookings",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status",
                table: "Bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BookingTimeoutAlerts_BookingId",
                table: "BookingTimeoutAlerts",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingTimeoutAlerts_ResolvedAt_ExpectedEndTime",
                table: "BookingTimeoutAlerts",
                columns: new[] { "ResolvedAt", "ExpectedEndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_BookingTrackings_BookingId",
                table: "BookingTrackings",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingTrackings_ChefId",
                table: "BookingTrackings",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_BulkOrderLineItems_BulkOrderRequestId",
                table: "BulkOrderLineItems",
                column: "BulkOrderRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BulkOrderLineItems_ChefId",
                table: "BulkOrderLineItems",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_BulkOrderRequests_CorporateAccountId",
                table: "BulkOrderRequests",
                column: "CorporateAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BulkOrderRequests_RequestedByUserId",
                table: "BulkOrderRequests",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_AgentAdminId",
                table: "CallLogs",
                column: "AgentAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_LinkedTicketId",
                table: "CallLogs",
                column: "LinkedTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_CallLogs_UserId",
                table: "CallLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CancellationRiskScores_BookingId",
                table: "CancellationRiskScores",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationCourses_Category",
                table: "CertificationCourses",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_ApprovedByAdminId",
                table: "ChangeRequests",
                column: "ApprovedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeRequests_RequestedByAdminId",
                table: "ChangeRequests",
                column: "RequestedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_BookingId",
                table: "ChatMessages",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverId",
                table: "ChatMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId_ReceiverId",
                table: "ChatMessages",
                columns: new[] { "SenderId", "ReceiverId" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreads_User1Id_User2Id",
                table: "ChatThreads",
                columns: new[] { "User1Id", "User2Id" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatThreads_User2Id",
                table: "ChatThreads",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChefBankAccounts_ChefId",
                table: "ChefBankAccounts",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefBusinessExpenses_ChefId",
                table: "ChefBusinessExpenses",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefBusinessGoals_ChefId_PeriodKey_GoalType",
                table: "ChefBusinessGoals",
                columns: new[] { "ChefId", "PeriodKey", "GoalType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefCertifications_ChefId_CourseId",
                table: "ChefCertifications",
                columns: new[] { "ChefId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefCertifications_CourseId",
                table: "ChefCertifications",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefCustomerLoyaltyBalances_ChefId_CustomerId",
                table: "ChefCustomerLoyaltyBalances",
                columns: new[] { "ChefId", "CustomerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefCustomerLoyaltyBalances_CustomerId",
                table: "ChefCustomerLoyaltyBalances",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefEarningsSnapshots_ChefId",
                table: "ChefEarningsSnapshots",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefEarningsSnapshots_ChefId_Period_PeriodKey",
                table: "ChefEarningsSnapshots",
                columns: new[] { "ChefId", "Period", "PeriodKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefEarningsSnapshots_CreatedAt",
                table: "ChefEarningsSnapshots",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChefFollows_ChefId",
                table: "ChefFollows",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefFollows_FollowerId_ChefId",
                table: "ChefFollows",
                columns: new[] { "FollowerId", "ChefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefGstProfiles_ChefId",
                table: "ChefGstProfiles",
                column: "ChefId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefGstProfiles_VerifiedByAdminId",
                table: "ChefGstProfiles",
                column: "VerifiedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefInventoryItems_ChefId",
                table: "ChefInventoryItems",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefLeaderboardEntries_ChefId",
                table: "ChefLeaderboardEntries",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefLeaderboardEntries_SeasonId_ChefId",
                table: "ChefLeaderboardEntries",
                columns: new[] { "SeasonId", "ChefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefLoyaltyPrograms_ChefId",
                table: "ChefLoyaltyPrograms",
                column: "ChefId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefMatchScores_ChefId",
                table: "ChefMatchScores",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefMatchScores_ComputedAt",
                table: "ChefMatchScores",
                column: "ComputedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChefMatchScores_CustomerId_ChefId",
                table: "ChefMatchScores",
                columns: new[] { "CustomerId", "ChefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefMenuItems_ChefProfileId",
                table: "ChefMenuItems",
                column: "ChefProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefMenuItems_ChefProfileId_Cuisine",
                table: "ChefMenuItems",
                columns: new[] { "ChefProfileId", "Cuisine" });

            migrationBuilder.CreateIndex(
                name: "IX_ChefOutlets_ChefId",
                table: "ChefOutlets",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefPackages_ChefId",
                table: "ChefPackages",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefPerformanceAnalyticsSnapshots_ChefId_PeriodKey",
                table: "ChefPerformanceAnalyticsSnapshots",
                columns: new[] { "ChefId", "PeriodKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefPerformanceSnapshots_ChefId",
                table: "ChefPerformanceSnapshots",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefPerformanceSnapshots_ChefId_Period_PeriodKey",
                table: "ChefPerformanceSnapshots",
                columns: new[] { "ChefId", "Period", "PeriodKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefPortfolioItems_ChefProfileId",
                table: "ChefPortfolioItems",
                column: "ChefProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefPortfolioItems_ChefProfileId_IsActive",
                table: "ChefPortfolioItems",
                columns: new[] { "ChefProfileId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ChefProfiles_UserId",
                table: "ChefProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefPromoCodes_ChefId_Code",
                table: "ChefPromoCodes",
                columns: new[] { "ChefId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefPromoRedemptions_ChefPromoCodeId",
                table: "ChefPromoRedemptions",
                column: "ChefPromoCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefPromoRedemptions_CustomerId",
                table: "ChefPromoRedemptions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefQualityScores_ChefId_ComputedAt",
                table: "ChefQualityScores",
                columns: new[] { "ChefId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChefServiceOfferings_ChefProfileId_MarketplaceServiceId",
                table: "ChefServiceOfferings",
                columns: new[] { "ChefProfileId", "MarketplaceServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChefServiceOfferings_MarketplaceServiceId",
                table: "ChefServiceOfferings",
                column: "MarketplaceServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefShifts_ChefId_DayOfWeek",
                table: "ChefShifts",
                columns: new[] { "ChefId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_ChefStaffMembers_ChefId",
                table: "ChefStaffMembers",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefTaxEstimates_ChefId_FinancialYear",
                table: "ChefTaxEstimates",
                columns: new[] { "ChefId", "FinancialYear" });

            migrationBuilder.CreateIndex(
                name: "IX_ChefVerifications_ChefId",
                table: "ChefVerifications",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ChefVerifications_Status",
                table: "ChefVerifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ChurnRiskScores_CustomerId",
                table: "ChurnRiskScores",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChurnRiskScores_RiskLevel",
                table: "ChurnRiskScores",
                column: "RiskLevel");

            migrationBuilder.CreateIndex(
                name: "IX_CityManagerAssignments_ServiceCityId_IsActive",
                table: "CityManagerAssignments",
                columns: new[] { "ServiceCityId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CityManagerAssignments_UserId",
                table: "CityManagerAssignments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CityPerformanceSnapshots_ServiceCityId_PeriodKey",
                table: "CityPerformanceSnapshots",
                columns: new[] { "ServiceCityId", "PeriodKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CityWaitlists_UserId_CityName",
                table: "CityWaitlists",
                columns: new[] { "UserId", "CityName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsBanners_IsActive",
                table: "CmsBanners",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentBlocks_Key_Locale",
                table: "CmsContentBlocks",
                columns: new[] { "Key", "Locale" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentBlocks_PublishedById",
                table: "CmsContentBlocks",
                column: "PublishedById");

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentBlockVersions_CmsContentBlockId_Version",
                table: "CmsContentBlockVersions",
                columns: new[] { "CmsContentBlockId", "Version" });

            migrationBuilder.CreateIndex(
                name: "IX_CmsContentBlockVersions_SavedByUserId",
                table: "CmsContentBlockVersions",
                column: "SavedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsPageRevisions_CmsPageId",
                table: "CmsPageRevisions",
                column: "CmsPageId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsPageRevisions_EditedByUserId",
                table: "CmsPageRevisions",
                column: "EditedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CmsPages_Slug",
                table: "CmsPages",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CohortRetentionSnapshots_CohortMonth_MonthOffset",
                table: "CohortRetentionSnapshots",
                columns: new[] { "CohortMonth", "MonthOffset" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionLedgers_BookingId",
                table: "CommissionLedgers",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionLedgers_ChefId",
                table: "CommissionLedgers",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionLedgers_Status",
                table: "CommissionLedgers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionProfileRules_CommissionProfileId_Priority",
                table: "CommissionProfileRules",
                columns: new[] { "CommissionProfileId", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionProfiles_ApprovedByUserId",
                table: "CommissionProfiles",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionProfiles_CreatedByUserId",
                table: "CommissionProfiles",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionProfiles_Status",
                table: "CommissionProfiles",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRuleAuditLogs_ChangedByUserId",
                table: "CommissionRuleAuditLogs",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRuleAuditLogs_CommissionProfileId",
                table: "CommissionRuleAuditLogs",
                column: "CommissionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_AgainstChefId",
                table: "Complaints",
                column: "AgainstChefId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_AssignedAdminId",
                table: "Complaints",
                column: "AssignedAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_BookingId",
                table: "Complaints",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ComplainantUserId",
                table: "Complaints",
                column: "ComplainantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_ComplaintNumber",
                table: "Complaints",
                column: "ComplaintNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintUpdates_AddedByAdminId",
                table: "ComplaintUpdates",
                column: "AddedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintUpdates_ComplaintId",
                table: "ComplaintUpdates",
                column: "ComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceChecklistItems_OwnerAdminId",
                table: "ComplianceChecklistItems",
                column: "OwnerAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateAccountMembers_CorporateAccountId_UserId",
                table: "CorporateAccountMembers",
                columns: new[] { "CorporateAccountId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorporateAccountMembers_UserId",
                table: "CorporateAccountMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateAccounts_OwnerUserId",
                table: "CorporateAccounts",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateBillingRecords_SubscriptionId",
                table: "CorporateBillingRecords",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateInvoices_CorporateAccountId",
                table: "CorporateInvoices",
                column: "CorporateAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateInvoices_InvoiceNumber",
                table: "CorporateInvoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorporateLocations_CorporateAccountId",
                table: "CorporateLocations",
                column: "CorporateAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateSubscriptions_CompanyId",
                table: "CorporateSubscriptions",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CorporateSubscriptions_CorporatePlanId",
                table: "CorporateSubscriptions",
                column: "CorporatePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateTiffinBookings_CompanyUserId",
                table: "CorporateTiffinBookings",
                column: "CompanyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateTiffinBookings_PrimaryChefId",
                table: "CorporateTiffinBookings",
                column: "PrimaryChefId");

            migrationBuilder.CreateIndex(
                name: "IX_CorporateTiffinBookings_Status",
                table: "CorporateTiffinBookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_Code",
                table: "Coupons",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_IsActive",
                table: "Coupons",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_CouponId_UserId",
                table: "CouponUsages",
                columns: new[] { "CouponId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_UserId",
                table: "CouponUsages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_CreditNoteNumber",
                table: "CreditNotes",
                column: "CreditNoteNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_CustomerId",
                table: "CreditNotes",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_InvoiceId",
                table: "CreditNotes",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_IssuedByAdminId",
                table: "CreditNotes",
                column: "IssuedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_CrmNotes_UserId",
                table: "CrmNotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBadges_BadgeDefinitionId",
                table: "CustomerBadges",
                column: "BadgeDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerBadges_CustomerId_BadgeDefinitionId",
                table: "CustomerBadges",
                columns: new[] { "CustomerId", "BadgeDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLtvSnapshots_CustomerId_ComputedAt",
                table: "CustomerLtvSnapshots",
                columns: new[] { "CustomerId", "ComputedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPreferences_CustomerId",
                table: "CustomerPreferences",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRatings_BookingId",
                table: "CustomerRatings",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRatings_ChefId",
                table: "CustomerRatings",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerRatings_CustomerId",
                table: "CustomerRatings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMenuBuilds_ChefId",
                table: "CustomMenuBuilds",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMenuBuilds_CustomerId",
                table: "CustomMenuBuilds",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DataExportJobs_RequestedByAdminId",
                table: "DataExportJobs",
                column: "RequestedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_DataPrivacyRequests_UserId",
                table: "DataPrivacyRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_DebitNoteNumber",
                table: "DebitNotes",
                column: "DebitNoteNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_InvoiceId",
                table: "DebitNotes",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_IssuedByAdminId",
                table: "DebitNotes",
                column: "IssuedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_PartyId",
                table: "DebitNotes",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_DemandPredictions_City_Date_Hour",
                table: "DemandPredictions",
                columns: new[] { "City", "Date", "Hour" });

            migrationBuilder.CreateIndex(
                name: "IX_DependencyHealthChecks_ServiceName_CheckedAt",
                table: "DependencyHealthChecks",
                columns: new[] { "ServiceName", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_UserId_Token",
                table: "DeviceTokens",
                columns: new[] { "UserId", "Token" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DietaryMatchScores_ChefId",
                table: "DietaryMatchScores",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_DietaryMatchScores_UserId_ChefId",
                table: "DietaryMatchScores",
                columns: new[] { "UserId", "ChefId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DigitalContracts_BookingId",
                table: "DigitalContracts",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_DisputeEvidences_DisputeId",
                table: "DisputeEvidences",
                column: "DisputeId");

            migrationBuilder.CreateIndex(
                name: "IX_DisputeMessages_DisputeId",
                table: "DisputeMessages",
                column: "DisputeId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_BookingId",
                table: "Disputes",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_DisputeNumber",
                table: "Disputes",
                column: "DisputeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_RaisedById",
                table: "Disputes",
                column: "RaisedById");

            migrationBuilder.CreateIndex(
                name: "IX_Disputes_Status",
                table: "Disputes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DrDrillLogs_ConductedByAdminId",
                table: "DrDrillLogs",
                column: "ConductedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_DynamicCommissionEvaluations_ChefId_EvaluatedAt",
                table: "DynamicCommissionEvaluations",
                columns: new[] { "ChefId", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_DynamicCommissionEvaluations_CommissionProfileId",
                table: "DynamicCommissionEvaluations",
                column: "CommissionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAvailabilities_ChefId",
                table: "EmergencyAvailabilities",
                column: "ChefId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmergencyAvailabilities_IsAvailableNow",
                table: "EmergencyAvailabilities",
                column: "IsAvailableNow");

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseAuditEntries_ActorUserId",
                table: "EnterpriseAuditEntries",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseAuditEntries_CorporateAccountId_OccurredAt",
                table: "EnterpriseAuditEntries",
                columns: new[] { "CorporateAccountId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_EnterpriseUsageSnapshots_CorporateAccountId_PeriodKey",
                table: "EnterpriseUsageSnapshots",
                columns: new[] { "CorporateAccountId", "PeriodKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentListings_ChefId",
                table: "EquipmentListings",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentListings_City_Category_IsAvailable",
                table: "EquipmentListings",
                columns: new[] { "City", "Category", "IsAvailable" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRequests_ListingId",
                table: "EquipmentRequests",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRequests_RequestedByChefId",
                table: "EquipmentRequests",
                column: "RequestedByChefId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentRequests_RequestedById",
                table: "EquipmentRequests",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationEvents_EscalatedByAdminId",
                table: "EscalationEvents",
                column: "EscalatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationEvents_TicketId",
                table: "EscalationEvents",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_Level",
                table: "EscalationLevels",
                column: "Level",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EscalationLevels_NotifyAdminId",
                table: "EscalationLevels",
                column: "NotifyAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_EventStaffRequests_BookingId",
                table: "EventStaffRequests",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpansionCities_CityName",
                table: "ExpansionCities",
                column: "CityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ApprovedByAdminId",
                table: "Expenses",
                column: "ApprovedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_Status",
                table: "Expenses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_SubmittedByAdminId",
                table: "Expenses",
                column: "SubmittedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ExperimentEvents_ExperimentId_UserId_EventType",
                table: "ExperimentEvents",
                columns: new[] { "ExperimentId", "UserId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_FaqArticles_FaqCategoryId",
                table: "FaqArticles",
                column: "FaqCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FAQs_Category_Audience",
                table: "FAQs",
                columns: new[] { "Category", "Audience" });

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlagAuditLogs_ChangedByUserId",
                table: "FeatureFlagAuditLogs",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlagAuditLogs_FeatureFlagId",
                table: "FeatureFlagAuditLogs",
                column: "FeatureFlagId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_CreatedByUserId",
                table: "FeatureFlags",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_Key",
                table: "FeatureFlags",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackResponses_FeedbackSurveyId",
                table: "FeedbackResponses",
                column: "FeedbackSurveyId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackResponses_UserId",
                table: "FeedbackResponses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialReportSnapshots_GeneratedByAdminId",
                table: "FinancialReportSnapshots",
                column: "GeneratedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialReportSnapshots_PeriodKey",
                table: "FinancialReportSnapshots",
                column: "PeriodKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Franchises_RegionOrCity",
                table: "Franchises",
                column: "RegionOrCity");

            migrationBuilder.CreateIndex(
                name: "IX_FranchiseSettings_FranchiseId_SettingKey",
                table: "FranchiseSettings",
                columns: new[] { "FranchiseId", "SettingKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FraudDetectionLogs_Action",
                table: "FraudDetectionLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_FraudDetectionLogs_UserId",
                table: "FraudDetectionLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FraudHeatMaps_City_Period_PeriodKey",
                table: "FraudHeatMaps",
                columns: new[] { "City", "Period", "PeriodKey" });

            migrationBuilder.CreateIndex(
                name: "IX_FSSAILicenses_ChefId",
                table: "FSSAILicenses",
                column: "ChefId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FSSAILicenses_ExpiryDate",
                table: "FSSAILicenses",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_FSSAILicenses_LicenseNumber",
                table: "FSSAILicenses",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FSSAILicenses_Status",
                table: "FSSAILicenses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GeoHeatmapCells_PeriodKey_GridLatitude_GridLongitude",
                table: "GeoHeatmapCells",
                columns: new[] { "PeriodKey", "GridLatitude", "GridLongitude" });

            migrationBuilder.CreateIndex(
                name: "IX_GroceryLists_BookingId",
                table: "GroceryLists",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroceryLists_ChefId",
                table: "GroceryLists",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupBookings_CustomerId",
                table: "GroupBookings",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupBookingSplits_BookingId",
                table: "GroupBookingSplits",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupBookingSplits_ChefId",
                table: "GroupBookingSplits",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupBookingSplits_GroupBookingId",
                table: "GroupBookingSplits",
                column: "GroupBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_GstHsnSacCodes_Code",
                table: "GstHsnSacCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GstReturnPeriods_FiledByAdminId",
                table: "GstReturnPeriods",
                column: "FiledByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_GstReturnPeriods_PeriodKey_ReturnType",
                table: "GstReturnPeriods",
                columns: new[] { "PeriodKey", "ReturnType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InfraCostEntries_PeriodKey_ServiceCategory",
                table: "InfraCostEntries",
                columns: new[] { "PeriodKey", "ServiceCategory" });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientCarbonFactors_IngredientName",
                table: "IngredientCarbonFactors",
                column: "IngredientName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IngredientListings_ChefId",
                table: "IngredientListings",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientListings_City_IsAvailable",
                table: "IngredientListings",
                columns: new[] { "City", "IsAvailable" });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientSwapRequests_ListingId",
                table: "IngredientSwapRequests",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientSwapRequests_RequestedByChefId",
                table: "IngredientSwapRequests",
                column: "RequestedByChefId");

            migrationBuilder.CreateIndex(
                name: "IX_IngredientSwapRequests_RequestedById",
                table: "IngredientSwapRequests",
                column: "RequestedById");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_ClaimRef",
                table: "InsuranceClaims",
                column: "ClaimRef",
                unique: true,
                filter: "[ClaimRef] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_PolicyId",
                table: "InsuranceClaims",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceClaims_Status",
                table: "InsuranceClaims",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePolicies_PolicyNumber",
                table: "InsurancePolicies",
                column: "PolicyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InsurancePolicies_UserId",
                table: "InsurancePolicies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalNotes_CreatedByAdminId",
                table: "InternalNotes",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_InternalNotes_EntityType_EntityId",
                table: "InternalNotes",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ChefId",
                table: "InventoryItems",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_InventoryItemId",
                table: "InventoryTransactions",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDeliveryLogs_InvoiceId",
                table: "InvoiceDeliveryLogs",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BookingId",
                table: "Invoices",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ChefId",
                table: "Invoices",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CustomerId",
                table: "Invoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IpRules_CreatedByAdminId",
                table: "IpRules",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_IpRules_IpAddress",
                table: "IpRules",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardRewards_SeasonId",
                table: "LeaderboardRewards",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardSeasons_StartDate_EndDate",
                table: "LeaderboardSeasons",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LiveChatMessages_LiveChatSessionId",
                table: "LiveChatMessages",
                column: "LiveChatSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveChatSessions_AgentAdminId",
                table: "LiveChatSessions",
                column: "AgentAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveChatSessions_Status",
                table: "LiveChatSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LiveChatSessions_UserId",
                table: "LiveChatSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreams_ChefId",
                table: "LiveStreams",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_LiveStreams_Status",
                table: "LiveStreams",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyPoints_UserId",
                table: "LoyaltyPoints",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_CreatedAt",
                table: "LoyaltyTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyTransactions_UserId",
                table: "LoyaltyTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceServices_ServiceName",
                table: "MarketplaceServices",
                column: "ServiceName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealBoxDeliveries_ScheduledDate",
                table: "MealBoxDeliveries",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_MealBoxDeliveries_SubscriptionId",
                table: "MealBoxDeliveries",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MealBoxSubscriptions_CustomerId",
                table: "MealBoxSubscriptions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_MealBoxSubscriptions_PlanId",
                table: "MealBoxSubscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MealBoxSubscriptions_Status",
                table: "MealBoxSubscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlans_UserId_WeekStart",
                table: "MealPlans",
                columns: new[] { "UserId", "WeekStart" });

            migrationBuilder.CreateIndex(
                name: "IX_MealPostComments_PostId",
                table: "MealPostComments",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPostComments_UserId",
                table: "MealPostComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPostLikes_PostId_UserId",
                table: "MealPostLikes",
                columns: new[] { "PostId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealPosts_ChefId",
                table: "MealPosts",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_MealPosts_CreatedAt",
                table: "MealPosts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MealSchedules_ChefId",
                table: "MealSchedules",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_MealSchedules_CustomerId",
                table: "MealSchedules",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationFlags_ReviewedByAdminId",
                table: "ModerationFlags",
                column: "ReviewedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationFlags_SourceType_SourceId",
                table: "ModerationFlags",
                columns: new[] { "SourceType", "SourceId" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationCampaigns_ScheduledAt",
                table: "NotificationCampaigns",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationCampaigns_Status",
                table: "NotificationCampaigns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OnCallSchedules_AdminId",
                table: "OnCallSchedules",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTrackingEvents_BookingId",
                table: "OrderTrackingEvents",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_OtpVerifications_Phone_Purpose",
                table: "OtpVerifications",
                columns: new[] { "Phone", "Purpose" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageMenuItems_ChefMenuItemId",
                table: "PackageMenuItems",
                column: "ChefMenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageMenuItems_ServicePackageId",
                table: "PackageMenuItems",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId",
                table: "Payments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GatewayTransactionRef",
                table: "Payments",
                column: "GatewayTransactionRef");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformHealthMetrics_MetricName_RecordedAt",
                table: "PlatformHealthMetrics",
                columns: new[] { "MetricName", "RecordedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PricingHistories_BookingId",
                table: "PricingHistories",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingRules_IsActive",
                table: "PricingRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PricingRules_RuleType",
                table: "PricingRules",
                column: "RuleType");

            migrationBuilder.CreateIndex(
                name: "IX_PricingSuggestions_City_GeneratedAt",
                table: "PricingSuggestions",
                columns: new[] { "City", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PromoCampaignDailyStats_PromoCampaignId",
                table: "PromoCampaignDailyStats",
                column: "PromoCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCampaignDailyStats_PromoCampaignVariantId_DateKey",
                table: "PromoCampaignDailyStats",
                columns: new[] { "PromoCampaignVariantId", "DateKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromoCampaigns_CreatedByUserId",
                table: "PromoCampaigns",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCampaigns_Status_StartAt_EndAt",
                table: "PromoCampaigns",
                columns: new[] { "Status", "StartAt", "EndAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PromoCampaignVariants_PromoCampaignId",
                table: "PromoCampaignVariants",
                column: "PromoCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_BookingId",
                table: "QualityChecks",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityChecks_CustomerId",
                table: "QualityChecks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_QuickReorderCombos_ChefId",
                table: "QuickReorderCombos",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_QuickReorderCombos_CustomerId",
                table: "QuickReorderCombos",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_ChefMenuItemId",
                table: "RecipeIngredients",
                column: "ChefMenuItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCodes_Code",
                table: "ReferralCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralCodes_UserId",
                table: "ReferralCodes",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReferralUses_ReferralCodeId",
                table: "ReferralUses",
                column: "ReferralCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralUses_ReferredUserId",
                table: "ReferralUses",
                column: "ReferredUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_BookingId",
                table: "RefundRequests",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_CustomerId",
                table: "RefundRequests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_ProcessedByAdminId",
                table: "RefundRequests",
                column: "ProcessedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_Status",
                table: "RefundRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RegionCities_RegionId_ServiceCityId",
                table: "RegionCities",
                columns: new[] { "RegionId", "ServiceCityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegionCities_ServiceCityId",
                table: "RegionCities",
                column: "ServiceCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_ManagerId",
                table: "Regions",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDefinitions_CreatedByAdminId",
                table: "ReportDefinitions",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueAnalyticsSnapshots_DateKey_City_Category",
                table: "RevenueAnalyticsSnapshots",
                columns: new[] { "DateKey", "City", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_BookingId",
                table: "Reviews",
                column: "BookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ChefId",
                table: "Reviews",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CustomerId",
                table: "Reviews",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskSignalDefinitions_SignalKey",
                table: "RiskSignalDefinitions",
                column: "SignalKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Runbooks_CreatedByAdminId",
                table: "Runbooks",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyAlerts_Status",
                table: "SafetyAlerts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyAlerts_UserId",
                table: "SafetyAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyIncidents_IncidentNumber",
                table: "SafetyIncidents",
                column: "IncidentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SafetyIncidents_ReportedById",
                table: "SafetyIncidents",
                column: "ReportedById");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyIncidents_Severity",
                table: "SafetyIncidents",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyIncidents_Status",
                table: "SafetyIncidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SafetyPreferences_UserId",
                table: "SafetyPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledReportRuns_ScheduledReportId",
                table: "ScheduledReportRuns",
                column: "ScheduledReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledReports_ReportDefinitionId",
                table: "ScheduledReports",
                column: "ReportDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditEntries_UserId_OccurredAt",
                table: "SecurityAuditEntries",
                columns: new[] { "UserId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_OccurredAt",
                table: "SecurityEvents",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityEvents_UserId",
                table: "SecurityEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SentimentScores_SourceType_SourceId",
                table: "SentimentScores",
                columns: new[] { "SourceType", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCities_CityName",
                table: "ServiceCities",
                column: "CityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_ChefId",
                table: "ServicePackages",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceZones_ServiceCityId",
                table: "ServiceZones",
                column: "ServiceCityId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementBatches_ApprovedByAdminId",
                table: "SettlementBatches",
                column: "ApprovedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementBatches_CreatedByAdminId",
                table: "SettlementBatches",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementBatchItems_ChefId",
                table: "SettlementBatchItems",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementBatchItems_SettlementBatchId",
                table: "SettlementBatchItems",
                column: "SettlementBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementBatchItems_SettlementRequestId",
                table: "SettlementBatchItems",
                column: "SettlementRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementRequests_BankAccountId",
                table: "SettlementRequests",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SettlementRequests_ChefId_Status",
                table: "SettlementRequests",
                columns: new[] { "ChefId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SlaContractBreaches_SlaContractId",
                table: "SlaContractBreaches",
                column: "SlaContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SlaContracts_ContractNumber",
                table: "SlaContracts",
                column: "ContractNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlaContracts_CorporateAccountId",
                table: "SlaContracts",
                column: "CorporateAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SlaTrackers_SlaPolicyId",
                table: "SlaTrackers",
                column: "SlaPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_SlaTrackers_TicketId",
                table: "SlaTrackers",
                column: "TicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SsoConnections_CorporateAccountId",
                table: "SsoConnections",
                column: "CorporateAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_SsoConnections_EmailDomain",
                table: "SsoConnections",
                column: "EmailDomain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SsoLoginLogs_SsoConnectionId",
                table: "SsoLoginLogs",
                column: "SsoConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_SsoLoginLogs_UserId",
                table: "SsoLoginLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffMembers_ChefId",
                table: "StaffMembers",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffShiftLogs_StaffMemberId",
                table: "StaffShiftLogs",
                column: "StaffMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamComments_StreamId",
                table: "StreamComments",
                column: "StreamId");

            migrationBuilder.CreateIndex(
                name: "IX_StreamComments_UserId",
                table: "StreamComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PreferredChefId",
                table: "Subscriptions",
                column: "PreferredChefId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportedLanguages_Code",
                table: "SupportedLanguages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportMessages_TicketId",
                table: "SupportMessages",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportStaffAssignments_EventStaffRequestId",
                table: "SupportStaffAssignments",
                column: "EventStaffRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportStaffAssignments_StaffMemberId",
                table: "SupportStaffAssignments",
                column: "StaffMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_Status",
                table: "SupportTickets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_TicketNumber",
                table: "SupportTickets",
                column: "TicketNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_UserId",
                table: "SupportTickets",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemIncidents_OnCallAdminId",
                table: "SystemIncidents",
                column: "OnCallAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRules_Country_Region",
                table: "TaxRules",
                columns: new[] { "Country", "Region" });

            migrationBuilder.CreateIndex(
                name: "IX_TdsCertificates_ChefId_FinancialYear_Quarter",
                table: "TdsCertificates",
                columns: new[] { "ChefId", "FinancialYear", "Quarter" });

            migrationBuilder.CreateIndex(
                name: "IX_TdsDeductions_ChefId_FinancialYear_Quarter",
                table: "TdsDeductions",
                columns: new[] { "ChefId", "FinancialYear", "Quarter" });

            migrationBuilder.CreateIndex(
                name: "IX_TdsDeductions_SettlementRequestId",
                table: "TdsDeductions",
                column: "SettlementRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_TdsRateConfigs_Section",
                table: "TdsRateConfigs",
                column: "Section",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TdsReturns_FiledByAdminId",
                table: "TdsReturns",
                column: "FiledByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_TdsReturns_FinancialYear_Quarter",
                table: "TdsReturns",
                columns: new[] { "FinancialYear", "Quarter" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamBookingAssignments_BookingId",
                table: "TeamBookingAssignments",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamBookingAssignments_ChefId",
                table: "TeamBookingAssignments",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketActivityLogs_ChangedByAdminId",
                table: "TicketActivityLogs",
                column: "ChangedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketActivityLogs_TicketId",
                table: "TicketActivityLogs",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignmentRules_Category_Priority",
                table: "TicketAssignmentRules",
                columns: new[] { "Category", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignmentRules_DefaultAssigneeAdminId",
                table: "TicketAssignmentRules",
                column: "DefaultAssigneeAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketAssignmentRules_TicketQueueId",
                table: "TicketAssignmentRules",
                column: "TicketQueueId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketEscalationStates_TicketId",
                table: "TicketEscalationStates",
                column: "TicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecords_CreatedAt",
                table: "TransactionRecords",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecords_TransactionId",
                table: "TransactionRecords",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecords_Type",
                table: "TransactionRecords",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecords_UserId",
                table: "TransactionRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TranslationStrings_Key_LanguageCode",
                table: "TranslationStrings",
                columns: new[] { "Key", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrustedContactAlerts_BookingId",
                table: "TrustedContactAlerts",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustedContactAlerts_UserId",
                table: "TrustedContactAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorSettings_UserId",
                table: "TwoFactorSettings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDietaryProfiles_UserId",
                table: "UserDietaryProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLocations_UserId",
                table: "UserLocations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_UserId",
                table: "UserProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSegments_Name",
                table: "UserSegments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_IsActive",
                table: "UserSessions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId_DeviceId",
                table: "UserSessions",
                columns: new[] { "UserId", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_VendorSettlementHolds_ChefId_IsActive",
                table: "VendorSettlementHolds",
                columns: new[] { "ChefId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_VendorSettlementHolds_HeldByAdminId",
                table: "VendorSettlementHolds",
                column: "HeldByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoConsultations_ChefId",
                table: "VideoConsultations",
                column: "ChefId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoConsultations_CustomerId",
                table: "VideoConsultations",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoConsultations_ScheduledAt",
                table: "VideoConsultations",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_VideoConsultations_Status",
                table: "VideoConsultations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCommandLogs_CreatedAt",
                table: "VoiceCommandLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VoiceCommandLogs_UserId",
                table: "VoiceCommandLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VulnerabilityFindings_AssignedToAdminId",
                table: "VulnerabilityFindings",
                column: "AssignedToAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_UserId",
                table: "WalletTransactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookDeliveryLogs_WebhookSubscriptionId",
                table: "WebhookDeliveryLogs",
                column: "WebhookSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_WebhookSubscriptions_CorporateAccountId",
                table: "WebhookSubscriptions",
                column: "CorporateAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_WhiteLabelClients_ApiKey",
                table: "WhiteLabelClients",
                column: "ApiKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WhiteLabelUsageLogs_ClientId",
                table: "WhiteLabelUsageLogs",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_WishlistItems_CustomerId_ItemType_ItemId",
                table: "WishlistItems",
                columns: new[] { "CustomerId", "ItemType", "ItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessibilitySettings");

            migrationBuilder.DropTable(
                name: "AdminUsers");

            migrationBuilder.DropTable(
                name: "AIAnalyticsEntries");

            migrationBuilder.DropTable(
                name: "AnnouncementReceipts");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "ApiVersionRecords");

            migrationBuilder.DropTable(
                name: "AppSettingsEntries");

            migrationBuilder.DropTable(
                name: "ArrivalVerifications");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "AutoInvoiceConfigs");

            migrationBuilder.DropTable(
                name: "AutoInvoiceRunLogs");

            migrationBuilder.DropTable(
                name: "B2BBookingRequests");

            migrationBuilder.DropTable(
                name: "BackgroundVerifications");

            migrationBuilder.DropTable(
                name: "BackupRecords");

            migrationBuilder.DropTable(
                name: "BillSplitParticipants");

            migrationBuilder.DropTable(
                name: "BookingCarbonEstimates");

            migrationBuilder.DropTable(
                name: "BookingFunnelSnapshots");

            migrationBuilder.DropTable(
                name: "BookingTimeoutAlerts");

            migrationBuilder.DropTable(
                name: "BookingTrackings");

            migrationBuilder.DropTable(
                name: "BulkOrderLineItems");

            migrationBuilder.DropTable(
                name: "CallLogs");

            migrationBuilder.DropTable(
                name: "CancellationRiskScores");

            migrationBuilder.DropTable(
                name: "CapacityForecasts");

            migrationBuilder.DropTable(
                name: "ChangeRequests");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatThreads");

            migrationBuilder.DropTable(
                name: "ChefBusinessExpenses");

            migrationBuilder.DropTable(
                name: "ChefBusinessGoals");

            migrationBuilder.DropTable(
                name: "ChefCertifications");

            migrationBuilder.DropTable(
                name: "ChefCustomerLoyaltyBalances");

            migrationBuilder.DropTable(
                name: "ChefEarningsSnapshots");

            migrationBuilder.DropTable(
                name: "ChefFollows");

            migrationBuilder.DropTable(
                name: "ChefGstProfiles");

            migrationBuilder.DropTable(
                name: "ChefInventoryItems");

            migrationBuilder.DropTable(
                name: "ChefLeaderboardEntries");

            migrationBuilder.DropTable(
                name: "ChefLoyaltyPrograms");

            migrationBuilder.DropTable(
                name: "ChefLoyaltyTransactions");

            migrationBuilder.DropTable(
                name: "ChefMatchScores");

            migrationBuilder.DropTable(
                name: "ChefOutlets");

            migrationBuilder.DropTable(
                name: "ChefPackages");

            migrationBuilder.DropTable(
                name: "ChefPerformanceAnalyticsSnapshots");

            migrationBuilder.DropTable(
                name: "ChefPerformanceSnapshots");

            migrationBuilder.DropTable(
                name: "ChefPortfolioItems");

            migrationBuilder.DropTable(
                name: "ChefPromoRedemptions");

            migrationBuilder.DropTable(
                name: "ChefQualityScores");

            migrationBuilder.DropTable(
                name: "ChefServiceOfferings");

            migrationBuilder.DropTable(
                name: "ChefShifts");

            migrationBuilder.DropTable(
                name: "ChefStaffMembers");

            migrationBuilder.DropTable(
                name: "ChefTaxEstimates");

            migrationBuilder.DropTable(
                name: "ChefVerifications");

            migrationBuilder.DropTable(
                name: "ChurnRiskScores");

            migrationBuilder.DropTable(
                name: "CityManagerAssignments");

            migrationBuilder.DropTable(
                name: "CityPerformanceSnapshots");

            migrationBuilder.DropTable(
                name: "CityWaitlists");

            migrationBuilder.DropTable(
                name: "CmsBanners");

            migrationBuilder.DropTable(
                name: "CmsContentBlockVersions");

            migrationBuilder.DropTable(
                name: "CmsPageRevisions");

            migrationBuilder.DropTable(
                name: "CohortRetentionSnapshots");

            migrationBuilder.DropTable(
                name: "CommissionLedgers");

            migrationBuilder.DropTable(
                name: "CommissionProfileRules");

            migrationBuilder.DropTable(
                name: "CommissionRuleAuditLogs");

            migrationBuilder.DropTable(
                name: "CommissionRules");

            migrationBuilder.DropTable(
                name: "ComplaintUpdates");

            migrationBuilder.DropTable(
                name: "ComplianceChecklistItems");

            migrationBuilder.DropTable(
                name: "CorporateAccountMembers");

            migrationBuilder.DropTable(
                name: "CorporateBillingRecords");

            migrationBuilder.DropTable(
                name: "CorporateInvoices");

            migrationBuilder.DropTable(
                name: "CorporateLocations");

            migrationBuilder.DropTable(
                name: "CorporateTiffinBookings");

            migrationBuilder.DropTable(
                name: "CouponUsages");

            migrationBuilder.DropTable(
                name: "CreditNotes");

            migrationBuilder.DropTable(
                name: "CrmNotes");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "CustomerBadges");

            migrationBuilder.DropTable(
                name: "CustomerLtvSnapshots");

            migrationBuilder.DropTable(
                name: "CustomerPreferences");

            migrationBuilder.DropTable(
                name: "CustomerRatings");

            migrationBuilder.DropTable(
                name: "CustomMenuBuilds");

            migrationBuilder.DropTable(
                name: "DataExportJobs");

            migrationBuilder.DropTable(
                name: "DataMaskingPolicies");

            migrationBuilder.DropTable(
                name: "DataPrivacyRequests");

            migrationBuilder.DropTable(
                name: "DebitNotes");

            migrationBuilder.DropTable(
                name: "DemandPredictions");

            migrationBuilder.DropTable(
                name: "DependencyHealthChecks");

            migrationBuilder.DropTable(
                name: "DeviceTokens");

            migrationBuilder.DropTable(
                name: "DietaryMatchScores");

            migrationBuilder.DropTable(
                name: "DigitalContracts");

            migrationBuilder.DropTable(
                name: "DisputeEvidences");

            migrationBuilder.DropTable(
                name: "DisputeMessages");

            migrationBuilder.DropTable(
                name: "DrDrillLogs");

            migrationBuilder.DropTable(
                name: "DynamicCommissionEvaluations");

            migrationBuilder.DropTable(
                name: "EmergencyAvailabilities");

            migrationBuilder.DropTable(
                name: "EnterpriseAuditEntries");

            migrationBuilder.DropTable(
                name: "EnterpriseUsageSnapshots");

            migrationBuilder.DropTable(
                name: "EquipmentRequests");

            migrationBuilder.DropTable(
                name: "EscalationEvents");

            migrationBuilder.DropTable(
                name: "EscalationLevels");

            migrationBuilder.DropTable(
                name: "ExpansionCities");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "ExperimentEvents");

            migrationBuilder.DropTable(
                name: "FaqArticles");

            migrationBuilder.DropTable(
                name: "FAQs");

            migrationBuilder.DropTable(
                name: "FeatureFlagAuditLogs");

            migrationBuilder.DropTable(
                name: "FeedbackResponses");

            migrationBuilder.DropTable(
                name: "FinancialReportSnapshots");

            migrationBuilder.DropTable(
                name: "FranchiseSettings");

            migrationBuilder.DropTable(
                name: "FraudDetectionLogs");

            migrationBuilder.DropTable(
                name: "FraudHeatMaps");

            migrationBuilder.DropTable(
                name: "FSSAILicenses");

            migrationBuilder.DropTable(
                name: "GeoHeatmapCells");

            migrationBuilder.DropTable(
                name: "GroceryLists");

            migrationBuilder.DropTable(
                name: "GroupBookingSplits");

            migrationBuilder.DropTable(
                name: "GstHsnSacCodes");

            migrationBuilder.DropTable(
                name: "GstReturnPeriods");

            migrationBuilder.DropTable(
                name: "InfraCostEntries");

            migrationBuilder.DropTable(
                name: "IngredientCarbonFactors");

            migrationBuilder.DropTable(
                name: "IngredientSwapRequests");

            migrationBuilder.DropTable(
                name: "InsuranceClaims");

            migrationBuilder.DropTable(
                name: "InternalNotes");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "InvoiceDeliveryLogs");

            migrationBuilder.DropTable(
                name: "IpRules");

            migrationBuilder.DropTable(
                name: "LeaderboardRewards");

            migrationBuilder.DropTable(
                name: "LiveChatMessages");

            migrationBuilder.DropTable(
                name: "LoyaltyPoints");

            migrationBuilder.DropTable(
                name: "LoyaltyTransactions");

            migrationBuilder.DropTable(
                name: "MealBoxDeliveries");

            migrationBuilder.DropTable(
                name: "MealPlans");

            migrationBuilder.DropTable(
                name: "MealPostComments");

            migrationBuilder.DropTable(
                name: "MealPostLikes");

            migrationBuilder.DropTable(
                name: "MealSchedules");

            migrationBuilder.DropTable(
                name: "ModerationFlags");

            migrationBuilder.DropTable(
                name: "NotificationCampaigns");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OnCallSchedules");

            migrationBuilder.DropTable(
                name: "OrderTrackingEvents");

            migrationBuilder.DropTable(
                name: "OtpVerifications");

            migrationBuilder.DropTable(
                name: "PackageMenuItems");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PlatformHealthMetrics");

            migrationBuilder.DropTable(
                name: "PricingHistories");

            migrationBuilder.DropTable(
                name: "PricingRules");

            migrationBuilder.DropTable(
                name: "PricingSuggestions");

            migrationBuilder.DropTable(
                name: "PromoCampaignDailyStats");

            migrationBuilder.DropTable(
                name: "QualityChecks");

            migrationBuilder.DropTable(
                name: "QuickReorderCombos");

            migrationBuilder.DropTable(
                name: "RecipeIngredients");

            migrationBuilder.DropTable(
                name: "ReferralUses");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RefundRequests");

            migrationBuilder.DropTable(
                name: "RegionCities");

            migrationBuilder.DropTable(
                name: "ReplyTemplates");

            migrationBuilder.DropTable(
                name: "RevenueAnalyticsSnapshots");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "RiskSignalDefinitions");

            migrationBuilder.DropTable(
                name: "Runbooks");

            migrationBuilder.DropTable(
                name: "SafetyAlerts");

            migrationBuilder.DropTable(
                name: "SafetyIncidents");

            migrationBuilder.DropTable(
                name: "SafetyPreferences");

            migrationBuilder.DropTable(
                name: "ScheduledReportRuns");

            migrationBuilder.DropTable(
                name: "SecurityAuditEntries");

            migrationBuilder.DropTable(
                name: "SecurityEvents");

            migrationBuilder.DropTable(
                name: "SentimentScores");

            migrationBuilder.DropTable(
                name: "ServiceZones");

            migrationBuilder.DropTable(
                name: "SettlementBatchItems");

            migrationBuilder.DropTable(
                name: "SlaContractBreaches");

            migrationBuilder.DropTable(
                name: "SlaTrackers");

            migrationBuilder.DropTable(
                name: "SsoLoginLogs");

            migrationBuilder.DropTable(
                name: "StaffShiftLogs");

            migrationBuilder.DropTable(
                name: "StreamComments");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "SupportedLanguages");

            migrationBuilder.DropTable(
                name: "SupportMessages");

            migrationBuilder.DropTable(
                name: "SupportStaffAssignments");

            migrationBuilder.DropTable(
                name: "SystemIncidents");

            migrationBuilder.DropTable(
                name: "TaxRules");

            migrationBuilder.DropTable(
                name: "TdsCertificates");

            migrationBuilder.DropTable(
                name: "TdsDeductions");

            migrationBuilder.DropTable(
                name: "TdsRateConfigs");

            migrationBuilder.DropTable(
                name: "TdsReturns");

            migrationBuilder.DropTable(
                name: "TeamBookingAssignments");

            migrationBuilder.DropTable(
                name: "TicketActivityLogs");

            migrationBuilder.DropTable(
                name: "TicketAssignmentRules");

            migrationBuilder.DropTable(
                name: "TicketEscalationStates");

            migrationBuilder.DropTable(
                name: "TransactionRecords");

            migrationBuilder.DropTable(
                name: "TranslationStrings");

            migrationBuilder.DropTable(
                name: "TrustedContactAlerts");

            migrationBuilder.DropTable(
                name: "TwoFactorSettings");

            migrationBuilder.DropTable(
                name: "UserDietaryProfiles");

            migrationBuilder.DropTable(
                name: "UserLocations");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "UserSegments");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "VendorSettlementHolds");

            migrationBuilder.DropTable(
                name: "VideoConsultations");

            migrationBuilder.DropTable(
                name: "VoiceCommandLogs");

            migrationBuilder.DropTable(
                name: "VulnerabilityFindings");

            migrationBuilder.DropTable(
                name: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "WeatherRecommendationRules");

            migrationBuilder.DropTable(
                name: "WebhookDeliveryLogs");

            migrationBuilder.DropTable(
                name: "WhiteLabelUsageLogs");

            migrationBuilder.DropTable(
                name: "WishlistItems");

            migrationBuilder.DropTable(
                name: "AdminRoles");

            migrationBuilder.DropTable(
                name: "Announcements");

            migrationBuilder.DropTable(
                name: "B2BPartners");

            migrationBuilder.DropTable(
                name: "BillSplits");

            migrationBuilder.DropTable(
                name: "BulkOrderRequests");

            migrationBuilder.DropTable(
                name: "CertificationCourses");

            migrationBuilder.DropTable(
                name: "ChefPromoCodes");

            migrationBuilder.DropTable(
                name: "MarketplaceServices");

            migrationBuilder.DropTable(
                name: "CmsContentBlocks");

            migrationBuilder.DropTable(
                name: "CmsPages");

            migrationBuilder.DropTable(
                name: "Complaints");

            migrationBuilder.DropTable(
                name: "CorporateSubscriptions");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "BadgeDefinitions");

            migrationBuilder.DropTable(
                name: "Disputes");

            migrationBuilder.DropTable(
                name: "CommissionProfiles");

            migrationBuilder.DropTable(
                name: "EquipmentListings");

            migrationBuilder.DropTable(
                name: "ExperimentDefinitions");

            migrationBuilder.DropTable(
                name: "FaqCategories");

            migrationBuilder.DropTable(
                name: "FeatureFlags");

            migrationBuilder.DropTable(
                name: "FeedbackSurveys");

            migrationBuilder.DropTable(
                name: "Franchises");

            migrationBuilder.DropTable(
                name: "GroupBookings");

            migrationBuilder.DropTable(
                name: "IngredientListings");

            migrationBuilder.DropTable(
                name: "InsurancePolicies");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "LeaderboardSeasons");

            migrationBuilder.DropTable(
                name: "LiveChatSessions");

            migrationBuilder.DropTable(
                name: "MealBoxSubscriptions");

            migrationBuilder.DropTable(
                name: "MealPosts");

            migrationBuilder.DropTable(
                name: "ServicePackages");

            migrationBuilder.DropTable(
                name: "PromoCampaignVariants");

            migrationBuilder.DropTable(
                name: "ChefMenuItems");

            migrationBuilder.DropTable(
                name: "ReferralCodes");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "ScheduledReports");

            migrationBuilder.DropTable(
                name: "ServiceCities");

            migrationBuilder.DropTable(
                name: "SettlementBatches");

            migrationBuilder.DropTable(
                name: "SlaContracts");

            migrationBuilder.DropTable(
                name: "SlaPolicies");

            migrationBuilder.DropTable(
                name: "SsoConnections");

            migrationBuilder.DropTable(
                name: "LiveStreams");

            migrationBuilder.DropTable(
                name: "EventStaffRequests");

            migrationBuilder.DropTable(
                name: "StaffMembers");

            migrationBuilder.DropTable(
                name: "SettlementRequests");

            migrationBuilder.DropTable(
                name: "TicketQueues");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropTable(
                name: "WebhookSubscriptions");

            migrationBuilder.DropTable(
                name: "WhiteLabelClients");

            migrationBuilder.DropTable(
                name: "CorporatePlans");

            migrationBuilder.DropTable(
                name: "MealBoxPlans");

            migrationBuilder.DropTable(
                name: "PromoCampaigns");

            migrationBuilder.DropTable(
                name: "ChefProfiles");

            migrationBuilder.DropTable(
                name: "ReportDefinitions");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "ChefBankAccounts");

            migrationBuilder.DropTable(
                name: "CorporateAccounts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
