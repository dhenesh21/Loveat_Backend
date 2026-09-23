using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LovEat.API.Data;
using LovEat.API.Helpers;
using Serilog;
using AspNetCoreRateLimit;

// ═══════════════════════════════════════════════════════════════
// Structured logging (P1 #7: central logging) — Serilog writes to
// console always, and to a rolling file when Serilog:WriteToFile is
// true (set in appsettings.Production.json). Bootstrap logger catches
// startup failures before the host is fully configured.
// ═══════════════════════════════════════════════════════════════
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console();
    if (context.Configuration.GetValue<bool>("Serilog:WriteToFile"))
    {
        var logDir = context.Configuration["Serilog:LogDirectory"] ?? "logs";
        configuration.WriteTo.File(Path.Combine(logDir, "loveat-api-.log"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30);
    }
});

// ═══════════════════════════════════════════════════════════════
// Database — provider selected at runtime by "Database:Provider":
// "Sqlite" (default, dev-only, file-based, no concurrent-write safety)
// or "Postgres" (production — set Database:Provider=Postgres and
// ConnectionStrings:DefaultConnection to a real Postgres connection
// string via environment variables before deploying).
// ═══════════════════════════════════════════════════════════════
// ═══════════════════════════════════════════════════════════════
// Database — SQL Server
// ═══════════════════════════════════════════════════════════════
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});


// ═══════════════════════════════════════════════════════════════
// JWT auth
// ═══════════════════════════════════════════════════════════════
builder.Services.AddSingleton<JwtHelper>();

var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LovEat.API";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LovEat.Clients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };

    // Lets SignalR (ChatHub, once built in Phase 5) read the JWT from the
    // query string, since browsers/RN clients can't set auth headers on
    // WebSocket upgrade requests.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ═══════════════════════════════════════════════════════════════
// CORS — wildcard is dev-only, see release checklist item under
// "Backend / API — CORS policy"
// ═══════════════════════════════════════════════════════════════
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("LovEatClients", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
        }
    });
});

// ═══════════════════════════════════════════════════════════════
// SignalR — registered now so ChatHub.cs (Phase 5) can be dropped in
// and mapped below without touching this section again.
// ═══════════════════════════════════════════════════════════════
builder.Services.AddSignalR();

// ═══════════════════════════════════════════════════════════════
// Controllers + Swagger
// ═══════════════════════════════════════════════════════════════
builder.Services.AddControllers();
builder.Services.AddHttpClient(); // M132: Webhook Management — outbound webhook delivery

// ═══════════════════════════════════════════════════════════════
// Rate limiting (P1 #9) — IP-based, configurable via appsettings
// "RateLimiting:PermitLimitPerMinute". Disabled entirely in dev unless
// RateLimiting:EnableRateLimiting is explicitly set.
// ═══════════════════════════════════════════════════════════════
var rateLimitEnabled = builder.Configuration.GetValue<bool>("RateLimiting:EnableRateLimiting");
if (rateLimitEnabled)
{
    var permitLimit = builder.Configuration.GetValue<int>("RateLimiting:PermitLimitPerMinute", 120);
    builder.Services.Configure<IpRateLimitOptions>(options =>
    {
        options.GeneralRules = new List<RateLimitRule>
        {
            new() { Endpoint = "*", Period = "1m", Limit = permitLimit },
        };
    });
    builder.Services.AddMemoryCache();
    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
}

// ═══════════════════════════════════════════════════════════════
// Health checks (P1 #8: monitoring/alerts) — /health for load balancer
// / uptime monitor probes; checks the DB connection is reachable.
// ═══════════════════════════════════════════════════════════════
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

// ═══════════════════════════════════════════════════════════════
// Distributed cache (P1 #6: Redis/scaling) — falls back to in-memory
// cache for single-instance dev; switch to real Redis in production by
// setting Redis:Enabled=true + Redis:ConnectionString via env vars.
// Needed once the API runs as more than one instance behind a load
// balancer, so cached data (sessions, rate-limit counters) is shared.
// ═══════════════════════════════════════════════════════════════
if (builder.Configuration.GetValue<bool>("Redis:Enabled"))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["Redis:ConnectionString"];
        options.InstanceName = "LovEat:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LovEat API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter a valid JWT token"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ═══════════════════════════════════════════════════════════════
// Services already built and functional (batches 15-33).
// Phase 3-6 services (Auth, Otp, Booking, Payment, etc.) are commented
// out below — uncomment each line as its Service class is filled in,
// per LovEat_Missing_Files_Plan.md.
// ═══════════════════════════════════════════════════════════════

// ── PHASE 3 ✅ built ──
builder.Services.AddScoped<LovEat.API.Services.AuthService>();
builder.Services.AddScoped<LovEat.API.Services.OtpService>();
builder.Services.AddScoped<LovEat.API.Services.UserProfileService>();
builder.Services.AddScoped<LovEat.API.Services.ChefProfileService>();
builder.Services.AddScoped<LovEat.API.Services.VerificationService>();

// ── PHASE 4 ✅ built ──
builder.Services.AddScoped<LovEat.API.Services.SearchService>();
builder.Services.AddScoped<LovEat.API.Services.AvailabilityService>();
builder.Services.AddScoped<LovEat.API.Services.BookingService>();
builder.Services.AddScoped<LovEat.API.Services.LocationService>();
builder.Services.AddScoped<LovEat.API.Services.WalletService>();
builder.Services.AddScoped<LovEat.API.Services.IPaymentGatewayService, LovEat.API.Services.RazorpayGatewayService>();
builder.Services.AddScoped<LovEat.API.Services.ISmsService, LovEat.API.Services.Msg91SmsService>();
builder.Services.AddScoped<LovEat.API.Services.IPushNotificationService, LovEat.API.Services.FcmPushNotificationService>();

// M169-M172: Incident/OnCall + Backup + Runbook + Change Management
builder.Services.AddScoped<LovEat.API.Services.SystemIncidentService>();
builder.Services.AddScoped<LovEat.API.Services.BackupTrackingService>();
builder.Services.AddScoped<LovEat.API.Services.RunbookService>();
builder.Services.AddScoped<LovEat.API.Services.ChangeManagementService>();

// M173-M177: Capacity + Dependency Health + Cost + API Version + DR Drill
builder.Services.AddScoped<LovEat.API.Services.ChefBusinessSuiteService>();
builder.Services.AddScoped<LovEat.API.Services.ChefPerformanceOverviewService>();
builder.Services.AddScoped<LovEat.API.Services.EquipmentCertificationService>();
builder.Services.AddScoped<LovEat.API.Services.CorporateManagerService>();
builder.Services.AddScoped<LovEat.API.Services.AdminDietarySocialService>();
builder.Services.AddScoped<LovEat.API.Services.AdminGroupMLService>();
builder.Services.AddScoped<LovEat.API.Services.AdminTrackingWeatherContractMenuBuildService>();
builder.Services.AddScoped<LovEat.API.Services.AdminWishlistBadgePreferencesReorderService>();
builder.Services.AddScoped<LovEat.API.Services.CapacityPlanningService>();
builder.Services.AddScoped<LovEat.API.Services.DependencyHealthService>();
builder.Services.AddScoped<LovEat.API.Services.InfraCostService>();
builder.Services.AddScoped<LovEat.API.Services.ApiVersionService>();
builder.Services.AddScoped<LovEat.API.Services.DrDrillService>();
builder.Services.AddScoped<LovEat.API.Services.OpsDashboardService>();

// M179-M184: Safety Suite (merged from the safety-hardening branch)
builder.Services.AddScoped<LovEat.API.Services.SafetyPreferenceService>();
builder.Services.AddScoped<LovEat.API.Services.TrustedContactAlertService>();
builder.Services.AddScoped<LovEat.API.Services.ArrivalVerificationService>();
builder.Services.AddScoped<LovEat.API.Services.BookingTimeoutAlertService>();
builder.Services.AddScoped<LovEat.API.Services.DedicatedChefService>();
builder.Services.AddScoped<LovEat.API.Services.EventStaffService>();
builder.Services.AddScoped<LovEat.API.Services.IFileStorageService, LovEat.API.Services.S3FileStorageService>();
builder.Services.AddScoped<LovEat.API.Services.IStreamingProviderService, LovEat.API.Services.MuxStreamingService>();

// P1 hardening: real recurring background jobs (scheduled report delivery,
// OTP/refresh-token cleanup) — see RecurringJobsBackgroundService for the
// single-instance-only caveat if this ever runs behind a multi-instance
// deployment.
builder.Services.AddHostedService<LovEat.API.Services.RecurringJobsBackgroundService>();

// ── PHASE 5 ✅ built ──
builder.Services.AddScoped<LovEat.API.Services.PaymentService>();
builder.Services.AddScoped<LovEat.API.Services.NotificationService>();
builder.Services.AddScoped<LovEat.API.Services.ChatService>();
builder.Services.AddScoped<LovEat.API.Services.ReviewService>();

// ── PHASE 6 ✅ built (all core backend phases now complete) ──
builder.Services.AddScoped<LovEat.API.Services.SubscriptionService>();
builder.Services.AddScoped<LovEat.API.Services.EmergencyService>();
builder.Services.AddScoped<LovEat.API.Services.TeamBookingService>();
builder.Services.AddScoped<LovEat.API.Services.CorporateTiffinService>();
builder.Services.AddScoped<LovEat.API.Services.SettingsService>();
builder.Services.AddScoped<LovEat.API.Services.FraudService>();
builder.Services.AddScoped<LovEat.API.Services.HeatMapService>();
builder.Services.AddScoped<LovEat.API.Services.AnalyticsService>();
builder.Services.AddScoped<LovEat.API.Services.AIRecommendationService>();

// ── Batch 34 ✅ built (M67 Equipment Marketplace, M68 Chef Certifications) ──
builder.Services.AddScoped<LovEat.API.Services.EquipmentService>();
builder.Services.AddScoped<LovEat.API.Services.CertificationService>();

// ── Batch 35 ✅ built (M69 Group Bookings, M70 ML Chef-Customer Matching) ──
builder.Services.AddScoped<LovEat.API.Services.GroupBookingService>();
builder.Services.AddScoped<LovEat.API.Services.MLMatchingService>();

// ── Batch 36 ✅ built (M71 Churn Prediction, M72 Voice Ordering) ──
builder.Services.AddScoped<LovEat.API.Services.ChurnPredictionService>();
builder.Services.AddScoped<LovEat.API.Services.VoiceOrderingService>();

// ── Batch 37 ✅ built (M73 Multi-currency, M74 Multi-country Tax) ──
builder.Services.AddScoped<LovEat.API.Services.CurrencyService>();
builder.Services.AddScoped<LovEat.API.Services.TaxCalculationService>();

// ── Batch 38 ✅ built (M75 Localization, M76 Meal Box Delivery) — all 76 roadmap modules now complete ──
builder.Services.AddScoped<LovEat.API.Services.LocalizationService>();
builder.Services.AddScoped<LovEat.API.Services.MealBoxService>();

// ── Bug fixes (post Batch 38 audit): real admin authentication ──
builder.Services.AddScoped<LovEat.API.Services.AdminAuthService>();

// ── Batch 39 ✅ built (M77 Live Streaming, M78 Gamified Leaderboard) ──
builder.Services.AddScoped<LovEat.API.Services.LiveStreamService>();
builder.Services.AddScoped<LovEat.API.Services.LeaderboardService>();

// ── Batch 40 ✅ built (M79 White-label Licensing, M80 Carbon Footprint Tracker) ──
builder.Services.AddScoped<LovEat.API.Services.WhiteLabelService>();
builder.Services.AddScoped<LovEat.API.Services.CarbonTrackerService>();

// ── Batch 41 ✅ built (M81 B2B API, M82 Chef-to-Chef Ingredient Marketplace) — all 82 modules now complete ──
builder.Services.AddScoped<LovEat.API.Services.B2BService>();
builder.Services.AddScoped<LovEat.API.Services.IngredientMarketplaceService>();

// ── Batch 42 ✅ built (M83 Super Admin Dashboard, M84 Franchise Management) — Phase 11 start ──
builder.Services.AddScoped<LovEat.API.Services.SuperAdminDashboardService>();
builder.Services.AddScoped<LovEat.API.Services.FranchiseService>();
builder.Services.AddScoped<LovEat.API.Services.CityManagementService>();
builder.Services.AddScoped<LovEat.API.Services.RegionalManagerService>();
builder.Services.AddScoped<LovEat.API.Services.CommissionEngineService>();
builder.Services.AddScoped<LovEat.API.Services.DynamicCommissionRuleService>();
builder.Services.AddScoped<LovEat.API.Services.CmsManagementService>();
builder.Services.AddScoped<LovEat.API.Services.PromoCampaignService>();
builder.Services.AddScoped<LovEat.API.Services.FeatureFlagService>();
builder.Services.AddScoped<LovEat.API.Services.AnnouncementService>();
builder.Services.AddScoped<LovEat.API.Services.GstManagementService>();
builder.Services.AddScoped<LovEat.API.Services.TdsManagementService>();
builder.Services.AddScoped<LovEat.API.Services.VendorSettlementService>();
builder.Services.AddScoped<LovEat.API.Services.AutoInvoiceService>();
builder.Services.AddScoped<LovEat.API.Services.CreditNoteService>();
builder.Services.AddScoped<LovEat.API.Services.DebitNoteService>();
builder.Services.AddScoped<LovEat.API.Services.RefundDashboardService>();
builder.Services.AddScoped<LovEat.API.Services.FinancialReportService>();
builder.Services.AddScoped<LovEat.API.Services.ExpenseService>();
builder.Services.AddScoped<LovEat.API.Services.ProfitLossService>();
builder.Services.AddScoped<LovEat.API.Services.TicketRoutingService>();
builder.Services.AddScoped<LovEat.API.Services.LiveChatService>();
builder.Services.AddScoped<LovEat.API.Services.CallCenterService>();
builder.Services.AddScoped<LovEat.API.Services.EscalationMatrixService>();
builder.Services.AddScoped<LovEat.API.Services.SlaManagementService>();
builder.Services.AddScoped<LovEat.API.Services.ComplaintTrackingService>();
builder.Services.AddScoped<LovEat.API.Services.InternalNoteService>();
builder.Services.AddScoped<LovEat.API.Services.FeedbackCenterService>();
builder.Services.AddScoped<LovEat.API.Services.SmartPricingService>();
builder.Services.AddScoped<LovEat.API.Services.AIFraudScoringService>();
builder.Services.AddScoped<LovEat.API.Services.SuggestedRepliesService>();
builder.Services.AddScoped<LovEat.API.Services.SentimentAnalysisService>();
builder.Services.AddScoped<LovEat.API.Services.ChefQualityScoringService>();
builder.Services.AddScoped<LovEat.API.Services.ContentModerationService>();
builder.Services.AddScoped<LovEat.API.Services.CancellationRiskService>();
builder.Services.AddScoped<LovEat.API.Services.CohortRetentionService>();
builder.Services.AddScoped<LovEat.API.Services.RevenueAnalyticsService>();
builder.Services.AddScoped<LovEat.API.Services.CustomerLtvService>();
builder.Services.AddScoped<LovEat.API.Services.BookingFunnelService>();
builder.Services.AddScoped<LovEat.API.Services.ChefPerformanceAnalyticsService>();
builder.Services.AddScoped<LovEat.API.Services.GeoHeatmapService>();
builder.Services.AddScoped<LovEat.API.Services.ReportBuilderService>();
builder.Services.AddScoped<LovEat.API.Services.ScheduledReportService>();
builder.Services.AddScoped<LovEat.API.Services.ExperimentAnalyticsService>();
builder.Services.AddScoped<LovEat.API.Services.DataExportService>();
builder.Services.AddScoped<LovEat.API.Services.CorporateAccountService>();
builder.Services.AddScoped<LovEat.API.Services.BulkOrderingService>();
builder.Services.AddScoped<LovEat.API.Services.ApiKeyService>();
builder.Services.AddScoped<LovEat.API.Services.WebhookService>();
builder.Services.AddScoped<LovEat.API.Services.SlaContractService>();
builder.Services.AddScoped<LovEat.API.Services.CorporateLocationService>();
builder.Services.AddScoped<LovEat.API.Services.CorporateBillingService>();
builder.Services.AddScoped<LovEat.API.Services.EnterpriseReportingService>();
builder.Services.AddScoped<LovEat.API.Services.SsoConnectionService>();
builder.Services.AddScoped<LovEat.API.Services.EnterpriseAuditService>();
builder.Services.AddScoped<LovEat.API.Services.ChefInventoryService>();
builder.Services.AddScoped<LovEat.API.Services.RecipeCostingService>();
builder.Services.AddScoped<LovEat.API.Services.StaffManagementService>();
builder.Services.AddScoped<LovEat.API.Services.ChefShiftService>();
builder.Services.AddScoped<LovEat.API.Services.ServicePackageService>();
builder.Services.AddScoped<LovEat.API.Services.ChefPromoCodeService>();
builder.Services.AddScoped<LovEat.API.Services.ChefTaxAssistantService>();
builder.Services.AddScoped<LovEat.API.Services.ChefLoyaltyProgramService>();
builder.Services.AddScoped<LovEat.API.Services.ChefBusinessGoalService>();
builder.Services.AddScoped<LovEat.API.Services.ChefOutletService>();
builder.Services.AddScoped<LovEat.API.Services.WishlistService>();
builder.Services.AddScoped<LovEat.API.Services.GamificationService>();
builder.Services.AddScoped<LovEat.API.Services.CustomerPreferencesService>();
builder.Services.AddScoped<LovEat.API.Services.QuickReorderService>();
builder.Services.AddScoped<LovEat.API.Services.BillSplitService>();
builder.Services.AddScoped<LovEat.API.Services.MealScheduleService>();
builder.Services.AddScoped<LovEat.API.Services.AccessibilityService>();
builder.Services.AddScoped<LovEat.API.Services.FaqService>();
builder.Services.AddScoped<LovEat.API.Services.OrderTrackingTimelineService>();
builder.Services.AddScoped<LovEat.API.Services.WeatherRecommendationService>();
builder.Services.AddScoped<LovEat.API.Services.DigitalContractService>();
builder.Services.AddScoped<LovEat.API.Services.CustomMenuBuildService>();
builder.Services.AddScoped<LovEat.API.Services.SecurityAuditService>();
builder.Services.AddScoped<LovEat.API.Services.TwoFactorService>();
builder.Services.AddScoped<LovEat.API.Services.DataPrivacyService>();
builder.Services.AddScoped<LovEat.API.Services.IpRuleService>();
builder.Services.AddScoped<LovEat.API.Services.DataMaskingService>();
builder.Services.AddScoped<LovEat.API.Services.VulnerabilityTrackingService>();
builder.Services.AddScoped<LovEat.API.Services.ComplianceChecklistService>();

// ── Batches 15-33 — already real, wired now ──
builder.Services.AddScoped<LovEat.API.Services.LoyaltyService>();
builder.Services.AddScoped<LovEat.API.Services.ReferralService>();
builder.Services.AddScoped<LovEat.API.Services.SupportService>();
builder.Services.AddScoped<LovEat.API.Services.CampaignService>();
builder.Services.AddScoped<LovEat.API.Services.ChefEarningsService>();
builder.Services.AddScoped<LovEat.API.Services.DynamicPricingService>();
builder.Services.AddScoped<LovEat.API.Services.MenuService>();
builder.Services.AddScoped<LovEat.API.Services.CouponService>();
builder.Services.AddScoped<LovEat.API.Services.CommissionService>();
builder.Services.AddScoped<LovEat.API.Services.SettlementService>();
builder.Services.AddScoped<LovEat.API.Services.InvoiceService>();
builder.Services.AddScoped<LovEat.API.Services.LedgerService>();
builder.Services.AddScoped<LovEat.API.Services.DisputeService>();
builder.Services.AddScoped<LovEat.API.Services.FraudDetectionService>();
builder.Services.AddScoped<LovEat.API.Services.SafetyIncidentService>();
builder.Services.AddScoped<LovEat.API.Services.RBACService>();
builder.Services.AddScoped<LovEat.API.Services.AuditService>();
builder.Services.AddScoped<LovEat.API.Services.CmsService>();
builder.Services.AddScoped<LovEat.API.Services.CrmService>();
builder.Services.AddScoped<LovEat.API.Services.ServiceAreaService>();
builder.Services.AddScoped<LovEat.API.Services.AIAnalyticsService>();
builder.Services.AddScoped<LovEat.API.Services.BusinessDashboardService>();
builder.Services.AddScoped<LovEat.API.Services.ChefPerformanceService>();
builder.Services.AddScoped<LovEat.API.Services.DeviceSecurityService>();
builder.Services.AddScoped<LovEat.API.Services.CorporatePlansService>();
// NOTE: fully qualified — LovEat.API.Models also has a class named
// "MarketplaceService" (the M60 entity). See Data/AppDbContext.cs note.
builder.Services.AddScoped<LovEat.API.Services.MarketplaceService>();
builder.Services.AddScoped<LovEat.API.Services.BackgroundVerificationService>();
builder.Services.AddScoped<LovEat.API.Services.InsuranceService>();
builder.Services.AddScoped<LovEat.API.Services.FSSAIService>();
builder.Services.AddScoped<LovEat.API.Services.VideoConsultationService>();
builder.Services.AddScoped<LovEat.API.Services.DietaryMatchingService>();
builder.Services.AddScoped<LovEat.API.Services.SocialService>();
builder.Services.AddHostedService<LovEat.API.Services.RecurringJobsBackgroundService>();
builder.Services.AddHostedService<LovEat.API.Services.BookingTimeoutMonitorService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
// ═══════════════════════════════════════════════════════════════
// Middleware pipeline
// ═══════════════════════════════════════════════════════════════
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (rateLimitEnabled)
    app.UseIpRateLimiting();

app.UseHttpsRedirection();
app.UseCors("LovEatClients");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// PHASE 5 ✅ ChatHub is built:
app.MapHub<LovEat.API.Hubs.ChatHub>("/hubs/chat");
app.MapHub<LovEat.API.Hubs.LiveChatHub>("/hubs/live-chat");

// ── One-time seed endpoints referenced in the project README ──
// POST /api/rbac/seed        (Batch 24 — admin roles)
// POST /api/marketplace/seed (Batch 30 — services + cities)
// These are implemented inside RBACController / MarketplaceController.

try
{
    Log.Information("LovEat API starting up (env: {Environment})", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "LovEat API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
