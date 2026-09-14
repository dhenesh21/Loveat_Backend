using Microsoft.EntityFrameworkCore;
using LovEat.API.Models;

namespace LovEat.API.Data
{
    /// <summary>
    /// Master EF Core database context for LovEat.
    ///
    /// This file merges every "AppDbContext_BatchXX_additions.cs" snippet (batches 15–33)
    /// that used to require manual copy-paste. Nothing further needs to be copied from
    /// those files — they're kept purely as historical notes under
    /// Data/_reference_batch_notes/ and are excluded from compilation (see .csproj).
    ///
    /// PHASE 2 TODO: the core foundation models (User, UserProfile, ChefProfile, Booking,
    /// Review, PaymentSubscription, OtpVerification, UserLocation, ChatNotification,
    /// AppSettings, ChefVerification, EmergencyAvailability, TiffinTeamBooking,
    /// FraudHeatMap, AIAnalytics) are still empty files in API/Models/. Their DbSet
    /// declarations are stubbed out below in commented form — uncomment each one as its
    /// model class is built, since many of the entities below (LoyaltyPoints, Coupon,
    /// Invoice, etc.) declare a `User? User` or `int UserId` navigation that only
    /// compiles once the real User model exists.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ══════════════════════════════════════════════════════════════
        // PHASE 2 — Foundation models ✅ built. Some model files contain more
        // than one class (documented per-file); every class gets its own
        // DbSet below.
        // ══════════════════════════════════════════════════════════════
        // Chef Ops Suite (Business overview)
        public DbSet<ChefInventoryItem> ChefInventoryItems { get; set; }
        public DbSet<ChefStaffMember> ChefStaffMembers { get; set; }
        public DbSet<ChefPackage> ChefPackages { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<ChefProfile> ChefProfiles { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }
        public DbSet<UserLocation> UserLocations { get; set; }
        public DbSet<AppSettings> AppSettingsEntries { get; set; }
        public DbSet<ChefVerification> ChefVerifications { get; set; }
        public DbSet<EmergencyAvailability> EmergencyAvailabilities { get; set; }
        public DbSet<FraudHeatMap> FraudHeatMaps { get; set; }
        public DbSet<AIAnalytics> AIAnalyticsEntries { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Bug fixes (post Batch 38 audit): refresh token persistence and
        // device token storage — both were flagged gaps, now real tables.
        // ══════════════════════════════════════════════════════════════
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<DeviceToken> DeviceTokens { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 39 — M77 Live Streaming, M78 Gamified Leaderboard
        // ══════════════════════════════════════════════════════════════
        public DbSet<LiveStream> LiveStreams { get; set; }
        public DbSet<StreamComment> StreamComments { get; set; }
        public DbSet<LeaderboardSeason> LeaderboardSeasons { get; set; }
        public DbSet<ChefLeaderboardEntry> ChefLeaderboardEntries { get; set; }
        public DbSet<LeaderboardReward> LeaderboardRewards { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 40 — M79 White-label Licensing, M80 Carbon Footprint Tracker
        // ══════════════════════════════════════════════════════════════
        public DbSet<WhiteLabelClient> WhiteLabelClients { get; set; }
        public DbSet<WhiteLabelUsageLog> WhiteLabelUsageLogs { get; set; }
        public DbSet<IngredientCarbonFactor> IngredientCarbonFactors { get; set; }
        public DbSet<BookingCarbonEstimate> BookingCarbonEstimates { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 41 — M81 B2B API, M82 Chef-to-Chef Ingredient Marketplace
        // ══════════════════════════════════════════════════════════════
        public DbSet<B2BPartner> B2BPartners { get; set; }
        public DbSet<B2BBookingRequest> B2BBookingRequests { get; set; }
        public DbSet<IngredientListing> IngredientListings { get; set; }
        public DbSet<IngredientSwapRequest> IngredientSwapRequests { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 42 — M83 Super Admin Dashboard, M84 Franchise Management
        // ══════════════════════════════════════════════════════════════
        public DbSet<PlatformHealthMetric> PlatformHealthMetrics { get; set; }
        public DbSet<Franchise> Franchises { get; set; }
        public DbSet<FranchiseSettings> FranchiseSettings { get; set; }

        // Batch 43 — M85 City Management, M86 Regional Manager
        public DbSet<CityManagerAssignment> CityManagerAssignments { get; set; }
        public DbSet<CityPerformanceSnapshot> CityPerformanceSnapshots { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<RegionCity> RegionCities { get; set; }

        // M87-M88 (Batch 44): Commission Engine + Dynamic Commission Rules
        public DbSet<CommissionProfile> CommissionProfiles { get; set; }
        public DbSet<CommissionProfileRule> CommissionProfileRules { get; set; }
        public DbSet<CommissionRuleAuditLog> CommissionRuleAuditLogs { get; set; }
        public DbSet<DynamicCommissionEvaluation> DynamicCommissionEvaluations { get; set; }

        // M89-M90 (Batch 45): CMS Management + Banner/Promotion Management
        public DbSet<CmsContentBlock> CmsContentBlocks { get; set; }
        public DbSet<CmsContentBlockVersion> CmsContentBlockVersions { get; set; }
        public DbSet<CmsPageRevision> CmsPageRevisions { get; set; }
        public DbSet<PromoCampaign> PromoCampaigns { get; set; }
        public DbSet<PromoCampaignVariant> PromoCampaignVariants { get; set; }
        public DbSet<PromoCampaignDailyStat> PromoCampaignDailyStats { get; set; }

        // M91-M92 (Batch 46): Feature Flag Management + Announcement Management
        public DbSet<FeatureFlag> FeatureFlags { get; set; }
        public DbSet<FeatureFlagAuditLog> FeatureFlagAuditLogs { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementReceipt> AnnouncementReceipts { get; set; }

        // M93-M94 (Batch 47): GST Management + TDS Management
        public DbSet<ChefGstProfile> ChefGstProfiles { get; set; }
        public DbSet<GstHsnSacCode> GstHsnSacCodes { get; set; }
        public DbSet<GstReturnPeriod> GstReturnPeriods { get; set; }
        public DbSet<TdsRateConfig> TdsRateConfigs { get; set; }
        public DbSet<TdsDeduction> TdsDeductions { get; set; }
        public DbSet<TdsReturn> TdsReturns { get; set; }
        public DbSet<TdsCertificate> TdsCertificates { get; set; }

        // M95-M96 (Batch 48): Vendor Settlement + Automatic Invoice
        public DbSet<SettlementBatch> SettlementBatches { get; set; }
        public DbSet<SettlementBatchItem> SettlementBatchItems { get; set; }
        public DbSet<VendorSettlementHold> VendorSettlementHolds { get; set; }
        public DbSet<AutoInvoiceConfig> AutoInvoiceConfigs { get; set; }
        public DbSet<AutoInvoiceRunLog> AutoInvoiceRunLogs { get; set; }
        public DbSet<InvoiceDeliveryLog> InvoiceDeliveryLogs { get; set; }

        // M97-M100 (Batch 49-50): Credit Notes + Debit Notes + Refund Dashboard + Financial Reports
        public DbSet<CreditNote> CreditNotes { get; set; }
        public DbSet<DebitNote> DebitNotes { get; set; }
        public DbSet<RefundRequest> RefundRequests { get; set; }
        public DbSet<FinancialReportSnapshot> FinancialReportSnapshots { get; set; }

        // M101-M104 (Batch 51-52): Expense Management + Ticket Routing + Live Chat
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<TicketQueue> TicketQueues { get; set; }
        public DbSet<TicketAssignmentRule> TicketAssignmentRules { get; set; }
        public DbSet<TicketActivityLog> TicketActivityLogs { get; set; }
        public DbSet<LiveChatSession> LiveChatSessions { get; set; }
        public DbSet<LiveChatMessage> LiveChatMessages { get; set; }

        // M105-M108 (Batch 53-54): Call Center + Escalation Matrix + SLA Management + Complaint Tracking
        public DbSet<CallLog> CallLogs { get; set; }
        public DbSet<EscalationLevel> EscalationLevels { get; set; }
        public DbSet<EscalationEvent> EscalationEvents { get; set; }
        public DbSet<TicketEscalationState> TicketEscalationStates { get; set; }
        public DbSet<SlaPolicy> SlaPolicies { get; set; }
        public DbSet<SlaTracker> SlaTrackers { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<ComplaintUpdate> ComplaintUpdates { get; set; }

        // M109-M112 (Batch 55-56): Internal Notes + Feedback Center + Smart Pricing + AI Fraud Scoring
        public DbSet<InternalNote> InternalNotes { get; set; }
        public DbSet<FeedbackSurvey> FeedbackSurveys { get; set; }
        public DbSet<FeedbackResponse> FeedbackResponses { get; set; }
        public DbSet<PricingSuggestion> PricingSuggestions { get; set; }
        public DbSet<RiskSignalDefinition> RiskSignalDefinitions { get; set; }

        // M113-M116 (Batch 57-58): Churn Prediction + Suggested Replies + Sentiment Analysis + Chef Quality Scoring
        // (ChurnRiskScore DbSet already registered above — see Batch 36 M71.)
        public DbSet<ReplyTemplate> ReplyTemplates { get; set; }
        public DbSet<SentimentScore> SentimentScores { get; set; }
        public DbSet<ChefQualityScore> ChefQualityScores { get; set; }

        // M117-M120 (Batch 59-60): Content Moderation + Cancellation Risk + Cohort Retention + Revenue Analytics
        public DbSet<ModerationFlag> ModerationFlags { get; set; }
        public DbSet<CancellationRiskScore> CancellationRiskScores { get; set; }
        public DbSet<CohortRetentionSnapshot> CohortRetentionSnapshots { get; set; }
        public DbSet<RevenueAnalyticsSnapshot> RevenueAnalyticsSnapshots { get; set; }

        // M121-M124 (Batch 61-62): CLV + Booking Funnel + Chef Performance + Geo Heatmap
        public DbSet<CustomerLtvSnapshot> CustomerLtvSnapshots { get; set; }
        public DbSet<BookingFunnelSnapshot> BookingFunnelSnapshots { get; set; }
        public DbSet<ChefPerformanceAnalyticsSnapshot> ChefPerformanceAnalyticsSnapshots { get; set; }
        public DbSet<GeoHeatmapCell> GeoHeatmapCells { get; set; }

        // M125-M128 (Batch 63-64): Report Builder + Scheduled Reports + A/B Testing + Data Export
        public DbSet<ReportDefinition> ReportDefinitions { get; set; }
        public DbSet<ScheduledReport> ScheduledReports { get; set; }
        public DbSet<ScheduledReportRun> ScheduledReportRuns { get; set; }
        public DbSet<ExperimentDefinition> ExperimentDefinitions { get; set; }
        public DbSet<ExperimentEvent> ExperimentEvents { get; set; }
        public DbSet<DataExportJob> DataExportJobs { get; set; }

        // M129-M132 (Batch 65-66): Corporate Accounts + Bulk Ordering + API Keys + Webhooks
        public DbSet<CorporateAccount> CorporateAccounts { get; set; }
        public DbSet<CorporateAccountMember> CorporateAccountMembers { get; set; }
        public DbSet<BulkOrderRequest> BulkOrderRequests { get; set; }
        public DbSet<BulkOrderLineItem> BulkOrderLineItems { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }
        public DbSet<WebhookDeliveryLog> WebhookDeliveryLogs { get; set; }

        // M133-M136 (Batch 67-68): SLA Contracts + Multi-Location + Custom Billing + Enterprise Reporting
        public DbSet<SlaContract> SlaContracts { get; set; }
        public DbSet<SlaContractBreach> SlaContractBreaches { get; set; }
        public DbSet<CorporateLocation> CorporateLocations { get; set; }
        public DbSet<CorporateInvoice> CorporateInvoices { get; set; }
        public DbSet<EnterpriseUsageSnapshot> EnterpriseUsageSnapshots { get; set; }

        // M137-M140 (Batch 69-70): SSO + Enterprise Audit Log + Chef Inventory + Recipe Costing
        public DbSet<SsoConnection> SsoConnections { get; set; }
        public DbSet<SsoLoginLog> SsoLoginLogs { get; set; }
        public DbSet<EnterpriseAuditEntry> EnterpriseAuditEntries { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }

        // M141-M144 (Batch 71-72): Staff Management + Shift Scheduling + Service Packages + Chef Promo Codes
        public DbSet<StaffMember> StaffMembers { get; set; }
        public DbSet<StaffShiftLog> StaffShiftLogs { get; set; }
        public DbSet<ChefShift> ChefShifts { get; set; }
        public DbSet<ServicePackage> ServicePackages { get; set; }
        public DbSet<PackageMenuItem> PackageMenuItems { get; set; }
        public DbSet<ChefPromoCode> ChefPromoCodes { get; set; }
        public DbSet<ChefPromoRedemption> ChefPromoRedemptions { get; set; }

        // M145-M148 (Batch 73-74): Tax Assistant + Chef Loyalty + Business Goals + Multi-Outlet
        public DbSet<ChefTaxEstimate> ChefTaxEstimates { get; set; }
        public DbSet<ChefBusinessExpense> ChefBusinessExpenses { get; set; }
        public DbSet<ChefLoyaltyProgram> ChefLoyaltyPrograms { get; set; }
        public DbSet<ChefCustomerLoyaltyBalance> ChefCustomerLoyaltyBalances { get; set; }
        public DbSet<ChefLoyaltyTransaction> ChefLoyaltyTransactions { get; set; }
        public DbSet<ChefBusinessGoal> ChefBusinessGoals { get; set; }
        public DbSet<ChefOutlet> ChefOutlets { get; set; }

        // M149-M152 (Batch 75-76): Wishlist + Gamification + Preferences + Reorder
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<BadgeDefinition> BadgeDefinitions { get; set; }
        public DbSet<CustomerBadge> CustomerBadges { get; set; }
        public DbSet<CustomerPreferences> CustomerPreferences { get; set; }
        public DbSet<QuickReorderCombo> QuickReorderCombos { get; set; }

        // M153-M156 (Batch 77-78): Split Bill + Meal Scheduling + Accessibility + FAQ
        public DbSet<BillSplit> BillSplits { get; set; }
        public DbSet<BillSplitParticipant> BillSplitParticipants { get; set; }
        public DbSet<MealSchedule> MealSchedules { get; set; }
        public DbSet<AccessibilitySettings> AccessibilitySettings { get; set; }
        public DbSet<FaqCategory> FaqCategories { get; set; }
        public DbSet<FaqArticle> FaqArticles { get; set; }

        // M157-M160 (Batch 79-80): Tracking + Weather + Contracts + MenuBuild
        public DbSet<OrderTrackingEvent> OrderTrackingEvents { get; set; }
        public DbSet<WeatherRecommendationRule> WeatherRecommendationRules { get; set; }
        public DbSet<DigitalContract> DigitalContracts { get; set; }
        public DbSet<CustomMenuBuild> CustomMenuBuilds { get; set; }

        // M161-M164 (Batch 81-82): 2FA + Session Mgmt + Data Privacy + Security Audit
        public DbSet<TwoFactorSetting> TwoFactorSettings { get; set; }
        // (UserSessions DbSet already registered above — see Batch 29 M58.)
        public DbSet<DataPrivacyRequest> DataPrivacyRequests { get; set; }
        public DbSet<SecurityAuditEntry> SecurityAuditEntries { get; set; }

        // M165-M168 (Batch 83-84): IP Rules + Data Masking + Vulnerability Tracking + Compliance Checklist
        public DbSet<IpRule> IpRules { get; set; }
        public DbSet<DataMaskingPolicy> DataMaskingPolicies { get; set; }
        public DbSet<VulnerabilityFinding> VulnerabilityFindings { get; set; }
        public DbSet<ComplianceChecklistItem> ComplianceChecklistItems { get; set; }

        // M169-M172 (Batch 85-86): Incident/OnCall + Backup + Runbook + Change Management
        public DbSet<SystemIncident> SystemIncidents { get; set; }
        public DbSet<OnCallSchedule> OnCallSchedules { get; set; }
        public DbSet<BackupRecord> BackupRecords { get; set; }
        public DbSet<Runbook> Runbooks { get; set; }
        public DbSet<ChangeRequest> ChangeRequests { get; set; }

        // M173-M177 (Batch 87-88): Capacity + Dependency Health + Cost + API Version + DR Drill
        public DbSet<CapacityForecast> CapacityForecasts { get; set; }
        public DbSet<DependencyHealthCheck> DependencyHealthChecks { get; set; }
        public DbSet<InfraCostEntry> InfraCostEntries { get; set; }
        public DbSet<ApiVersionRecord> ApiVersionRecords { get; set; }
        public DbSet<DrDrillLog> DrDrillLogs { get; set; }

        // M179-M184: Safety Suite (merged from the safety-hardening branch)
        public DbSet<SafetyPreference> SafetyPreferences { get; set; }
        public DbSet<TrustedContactAlert> TrustedContactAlerts { get; set; }
        public DbSet<ArrivalVerification> ArrivalVerifications { get; set; }
        public DbSet<BookingTimeoutAlert> BookingTimeoutAlerts { get; set; }
        public DbSet<EventStaffRequest> EventStaffRequests { get; set; }
        public DbSet<SupportStaffAssignment> SupportStaffAssignments { get; set; }

        // Review.cs → Review (M16) + CustomerRating (M37, chef rates customer)
        public DbSet<Review> Reviews { get; set; }
        public DbSet<CustomerRating> CustomerRatings { get; set; }

        // PaymentSubscription.cs → Payment (M14) + WalletTransaction (M15) + Subscription (M19)
        public DbSet<Payment> Payments { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }

        // ChatNotification.cs → ChatMessage + ChatThread (M17) + Notification (M18)
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatThread> ChatThreads { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // TiffinTeamBooking.cs → TeamBookingAssignment (M11) + CorporateTiffinBooking (M12)
        public DbSet<TeamBookingAssignment> TeamBookingAssignments { get; set; }
        public DbSet<CorporateTiffinBooking> CorporateTiffinBookings { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 15 — M29 Loyalty, M30 Referral
        // ══════════════════════════════════════════════════════════════
        public DbSet<LoyaltyPoints> LoyaltyPoints { get; set; }
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
        public DbSet<ReferralCode> ReferralCodes { get; set; }
        public DbSet<ReferralUse> ReferralUses { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 16 — M31 Support, M32 Push Campaigns
        // ══════════════════════════════════════════════════════════════
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportMessage> SupportMessages { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<NotificationCampaign> NotificationCampaigns { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 17 — M33 Chef Earnings Insights
        // ══════════════════════════════════════════════════════════════
        public DbSet<ChefEarningsSnapshot> ChefEarningsSnapshots { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 18 — M35 Dynamic Pricing, M36 Menu
        // ══════════════════════════════════════════════════════════════
        public DbSet<PricingRule> PricingRules { get; set; }
        public DbSet<PricingHistory> PricingHistories { get; set; }
        public DbSet<ChefMenuItem> ChefMenuItems { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 19 — M20 Coupons, M24 Live Tracking
        // ══════════════════════════════════════════════════════════════
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUsage> CouponUsages { get; set; }
        public DbSet<BookingTracking> BookingTrackings { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 20 — M30 Quality Check, M36 Safety, M37 Grocery,
        //            M38/39 Meal Planning, M40 Chef Portfolio
        // ══════════════════════════════════════════════════════════════
        public DbSet<QualityCheck> QualityChecks { get; set; }
        public DbSet<SafetyAlert> SafetyAlerts { get; set; }
        public DbSet<GroceryList> GroceryLists { get; set; }
        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<ChefPortfolioItem> ChefPortfolioItems { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 21 — M41 Commission, M42 Settlement & Payout
        // ══════════════════════════════════════════════════════════════
        public DbSet<CommissionRule> CommissionRules { get; set; }
        public DbSet<CommissionLedger> CommissionLedgers { get; set; }
        public DbSet<ChefBankAccount> ChefBankAccounts { get; set; }
        public DbSet<SettlementRequest> SettlementRequests { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 22 — M43 Invoice & Tax, M44 Transaction Ledger
        // ══════════════════════════════════════════════════════════════
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<TransactionRecord> TransactionRecords { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 23 — M45 Dispute Management, M46 Fraud Detection (advanced)
        // ══════════════════════════════════════════════════════════════
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<DisputeEvidence> DisputeEvidences { get; set; }
        public DbSet<DisputeMessage> DisputeMessages { get; set; }
        public DbSet<FraudDetectionLog> FraudDetectionLogs { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 24 — M47 Safety Incident, M48 RBAC
        // ══════════════════════════════════════════════════════════════
        public DbSet<SafetyIncident> SafetyIncidents { get; set; }
        public DbSet<AdminRole> AdminRoles { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 25 — M49 Audit Logs, M50 CMS
        // ══════════════════════════════════════════════════════════════
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CmsBanner> CmsBanners { get; set; }
        public DbSet<CmsPage> CmsPages { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 26 — M52 Campaign+, M53 CRM
        // ══════════════════════════════════════════════════════════════
        public DbSet<UserSegment> UserSegments { get; set; }
        public DbSet<CrmNote> CrmNotes { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 27 — M54 Service Areas, M55 AI Analytics
        // ══════════════════════════════════════════════════════════════
        public DbSet<ServiceCity> ServiceCities { get; set; }
        public DbSet<ServiceZone> ServiceZones { get; set; }
        public DbSet<DemandPrediction> DemandPredictions { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 28 — M56 Business Dashboard, M57 Chef Performance
        // ══════════════════════════════════════════════════════════════
        public DbSet<ChefPerformanceSnapshot> ChefPerformanceSnapshots { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 29 — M58 Device Security, M59 Corporate Plans
        // ══════════════════════════════════════════════════════════════
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<SecurityEvent> SecurityEvents { get; set; }
        public DbSet<CorporatePlan> CorporatePlans { get; set; }
        public DbSet<CorporateSubscription> CorporateSubscriptions { get; set; }
        public DbSet<CorporateBillingRecord> CorporateBillingRecords { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 30 — M60 Marketplace Expansion
        // NOTE: LovEat.API.Models.MarketplaceService (this entity) and
        // LovEat.API.Services.MarketplaceService (the service class) share
        // the same short name in different namespaces. That's a pre-existing
        // naming collision in the codebase — fully qualify whichever one you
        // need if you ever `using` both namespaces in the same file (Program.cs
        // already does this correctly).
        // ══════════════════════════════════════════════════════════════
        public DbSet<MarketplaceService> MarketplaceServices { get; set; }
        public DbSet<ChefServiceOffering> ChefServiceOfferings { get; set; }
        public DbSet<ExpansionCity> ExpansionCities { get; set; }
        public DbSet<CityWaitlist> CityWaitlists { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 31 — M61 Background Verification, M62 Insurance
        // ══════════════════════════════════════════════════════════════
        public DbSet<BackgroundVerification> BackgroundVerifications { get; set; }
        public DbSet<InsurancePolicy> InsurancePolicies { get; set; }
        public DbSet<InsuranceClaim> InsuranceClaims { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 32 — M63 FSSAI License Tracking, M64 Video Consultations
        // ══════════════════════════════════════════════════════════════
        public DbSet<FSSAILicense> FSSAILicenses { get; set; }
        public DbSet<VideoConsultation> VideoConsultations { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 33 — M65 AI Dietary Matching, M66 Social Features
        // ══════════════════════════════════════════════════════════════
        public DbSet<UserDietaryProfile> UserDietaryProfiles { get; set; }
        public DbSet<DietaryMatchScore> DietaryMatchScores { get; set; }
        public DbSet<ChefFollow> ChefFollows { get; set; }
        public DbSet<MealPost> MealPosts { get; set; }
        public DbSet<MealPostLike> MealPostLikes { get; set; }
        public DbSet<MealPostComment> MealPostComments { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 34 — M67 Equipment Marketplace, M68 Chef Certifications
        // ══════════════════════════════════════════════════════════════
        public DbSet<EquipmentListing> EquipmentListings { get; set; }
        public DbSet<EquipmentRequest> EquipmentRequests { get; set; }
        public DbSet<CertificationCourse> CertificationCourses { get; set; }
        public DbSet<ChefCertification> ChefCertifications { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 35 — M69 Group Bookings, M70 ML Chef-Customer Matching
        // ══════════════════════════════════════════════════════════════
        public DbSet<GroupBooking> GroupBookings { get; set; }
        public DbSet<GroupBookingSplit> GroupBookingSplits { get; set; }
        public DbSet<ChefMatchScore> ChefMatchScores { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 36 — M71 Churn Prediction, M72 Voice Ordering
        // ══════════════════════════════════════════════════════════════
        public DbSet<ChurnRiskScore> ChurnRiskScores { get; set; }
        public DbSet<VoiceCommandLog> VoiceCommandLogs { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 37 — M73 Multi-currency, M74 Multi-country Tax
        // ══════════════════════════════════════════════════════════════
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<TaxRule> TaxRules { get; set; }

        // ══════════════════════════════════════════════════════════════
        // Batch 38 — M75 Localization, M76 Meal Box Delivery
        // ══════════════════════════════════════════════════════════════
        public DbSet<SupportedLanguage> SupportedLanguages { get; set; }
        public DbSet<TranslationString> TranslationStrings { get; set; }
        public DbSet<MealBoxPlan> MealBoxPlans { get; set; }
        public DbSet<MealBoxSubscription> MealBoxSubscriptions { get; set; }
        public DbSet<MealBoxDelivery> MealBoxDeliveries { get; set; }


        protected override void OnModelCreating(ModelBuilder mb)
        {
            base.OnModelCreating(mb);

            // ── Phase 2 — Foundation models ──────────────────────────
            mb.Entity<User>().HasIndex(u => u.PhoneNumber).IsUnique();
            mb.Entity<User>().HasIndex(u => u.Email);
            mb.Entity<UserProfile>().HasIndex(p => p.UserId).IsUnique();
            mb.Entity<ChefProfile>().HasIndex(c => c.UserId).IsUnique();
            mb.Entity<Booking>().HasIndex(b => b.CustomerId);
            mb.Entity<Booking>().HasIndex(b => b.ChefId);
            mb.Entity<Booking>().HasIndex(b => b.Status);
            mb.Entity<Booking>().HasIndex(b => b.ScheduledAt);
            mb.Entity<Review>().HasIndex(r => r.BookingId).IsUnique();
            mb.Entity<Review>().HasIndex(r => r.ChefId);
            mb.Entity<CustomerRating>().HasIndex(r => r.BookingId).IsUnique();
            mb.Entity<Payment>().HasIndex(p => p.BookingId);
            mb.Entity<Payment>().HasIndex(p => p.GatewayTransactionRef);
            mb.Entity<WalletTransaction>().HasIndex(w => w.UserId);
            mb.Entity<Subscription>().HasIndex(s => s.UserId);
            mb.Entity<OtpVerification>().HasIndex(o => new { o.Phone, o.Purpose });
            mb.Entity<UserLocation>().HasIndex(l => l.UserId);
            mb.Entity<ChatMessage>().HasIndex(m => m.BookingId);
            mb.Entity<ChatMessage>().HasIndex(m => new { m.SenderId, m.ReceiverId });
            mb.Entity<ChatThread>().HasIndex(t => new { t.User1Id, t.User2Id });
            mb.Entity<Notification>().HasIndex(n => n.UserId);
            mb.Entity<Notification>().HasIndex(n => n.IsRead);
            mb.Entity<AppSettings>().HasIndex(a => a.Key).IsUnique();
            mb.Entity<ChefVerification>().HasIndex(v => v.ChefId);
            mb.Entity<ChefVerification>().HasIndex(v => v.Status);
            mb.Entity<EmergencyAvailability>().HasIndex(e => e.ChefId).IsUnique();
            mb.Entity<EmergencyAvailability>().HasIndex(e => e.IsAvailableNow);
            mb.Entity<TeamBookingAssignment>().HasIndex(t => t.BookingId);
            mb.Entity<TeamBookingAssignment>().HasIndex(t => t.ChefId);
            mb.Entity<CorporateTiffinBooking>().HasIndex(c => c.CompanyUserId);
            mb.Entity<CorporateTiffinBooking>().HasIndex(c => c.Status);
            mb.Entity<FraudHeatMap>().HasIndex(f => new { f.City, f.Period, f.PeriodKey });
            mb.Entity<AIAnalytics>().HasIndex(a => a.AnalyticsType);
            mb.Entity<AIAnalytics>().HasIndex(a => a.UserId);

            // ── Bug fixes: RefreshToken, DeviceToken ──────────────────
            mb.Entity<RefreshToken>().HasIndex(t => t.TokenHash).IsUnique();
            mb.Entity<RefreshToken>().HasIndex(t => t.UserId);
            mb.Entity<DeviceToken>().HasIndex(d => new { d.UserId, d.Token }).IsUnique();

            // Every FK above that points at Users would otherwise default to
            // cascade-delete, and several entities hold more than one FK into
            // the same Users table (e.g. Booking.CustomerId + Booking.ChefId).
            // SQL Server/PostgreSQL will reject that as "multiple cascade
            // paths" when the migration is applied, so all of them are set to
            // Restrict here — deleting a user should never silently wipe
            // their bookings/reviews/payments history.
            foreach (var fk in mb.Model.GetEntityTypes()
                         .SelectMany(e => e.GetForeignKeys())
                         .Where(fk => fk.PrincipalEntityType.ClrType == typeof(User)))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // ── Batch 15 ──────────────────────────────────────────────
            mb.Entity<LoyaltyPoints>().HasIndex(l => l.UserId).IsUnique();
            mb.Entity<LoyaltyPoints>().Property(l => l.Tier).HasDefaultValue("Bronze");
            mb.Entity<LoyaltyTransaction>().HasIndex(t => t.UserId);
            mb.Entity<LoyaltyTransaction>().HasIndex(t => t.CreatedAt);
            mb.Entity<ReferralCode>().HasIndex(r => r.Code).IsUnique();
            mb.Entity<ReferralCode>().HasIndex(r => r.UserId).IsUnique();
            mb.Entity<ReferralUse>().HasIndex(u => u.ReferredUserId).IsUnique();

            // ── Batch 16 ──────────────────────────────────────────────
            mb.Entity<SupportTicket>().HasIndex(t => t.UserId);
            mb.Entity<SupportTicket>().HasIndex(t => t.Status);
            mb.Entity<SupportTicket>().HasIndex(t => t.TicketNumber).IsUnique();
            mb.Entity<SupportMessage>().HasIndex(m => m.TicketId);
            mb.Entity<FAQ>().HasIndex(f => new { f.Category, f.Audience });
            mb.Entity<NotificationCampaign>().HasIndex(c => c.Status);
            mb.Entity<NotificationCampaign>().HasIndex(c => c.ScheduledAt);

            mb.Entity<FAQ>().HasData(
                new FAQ { Id = 1, Question = "How do I cancel a booking?", Answer = "Go to My Bookings, tap the booking, then Cancel. Cancellations made 2+ hours before are fully refunded.", Category = "Booking", Audience = "Customer", SortOrder = 1, IsActive = true },
                new FAQ { Id = 2, Question = "When will I get my refund?", Answer = "Refunds are processed within 5-7 business days to your original payment method.", Category = "Payment", Audience = "Customer", SortOrder = 2, IsActive = true },
                new FAQ { Id = 3, Question = "How are chefs verified?", Answer = "All chefs go through identity verification, cooking skill assessment, and background checks.", Category = "General", Audience = "Customer", SortOrder = 3, IsActive = true },
                new FAQ { Id = 4, Question = "Can I reschedule a booking?", Answer = "Yes. Contact the chef via chat at least 3 hours before the booking to agree on a new time.", Category = "Booking", Audience = "Customer", SortOrder = 4, IsActive = true },
                new FAQ { Id = 5, Question = "What payment methods are accepted?", Answer = "We accept UPI, credit/debit cards, net banking, and LovEat wallet.", Category = "Payment", Audience = "All", SortOrder = 5, IsActive = true },
                new FAQ { Id = 6, Question = "When do I receive my earnings?", Answer = "Earnings are credited to your LovEat wallet within 24 hours of booking completion.", Category = "Payment", Audience = "Chef", SortOrder = 1, IsActive = true },
                new FAQ { Id = 7, Question = "What if a customer cancels last minute?", Answer = "If cancellation is within 2 hours of the booking, you receive a 50% cancellation fee.", Category = "Booking", Audience = "Chef", SortOrder = 2, IsActive = true },
                new FAQ { Id = 8, Question = "How long does verification take?", Answer = "Chef verification typically takes 3-5 business days after all documents are submitted.", Category = "Verification", Audience = "Chef", SortOrder = 3, IsActive = true },
                new FAQ { Id = 9, Question = "Can I set my own prices?", Answer = "Yes. You set your hourly rate. LovEat charges a 15% platform commission on completed bookings.", Category = "General", Audience = "Chef", SortOrder = 4, IsActive = true }
            );

            // ── Batch 17 ──────────────────────────────────────────────
            mb.Entity<ChefEarningsSnapshot>().HasIndex(e => new { e.ChefId, e.Period, e.PeriodKey }).IsUnique();
            mb.Entity<ChefEarningsSnapshot>().HasIndex(e => e.ChefId);
            mb.Entity<ChefEarningsSnapshot>().HasIndex(e => e.CreatedAt);

            // ── Batch 18 ──────────────────────────────────────────────
            mb.Entity<PricingRule>().HasIndex(r => r.RuleType);
            mb.Entity<PricingRule>().HasIndex(r => r.IsActive);
            mb.Entity<PricingHistory>().HasIndex(h => h.BookingId);
            mb.Entity<ChefMenuItem>().HasIndex(m => m.ChefProfileId);
            mb.Entity<ChefMenuItem>().HasIndex(m => new { m.ChefProfileId, m.Cuisine });

            mb.Entity<PricingRule>().HasData(
                new PricingRule { Id = 1, RuleName = "Peak Hour Surcharge", RuleType = "PeakHour", AppliesTo = "All", MultiplierPercent = 20, Description = "6-9 PM peak demand hours", StartHour = 18, EndHour = 21, IsActive = true },
                new PricingRule { Id = 2, RuleName = "Weekend Premium", RuleType = "Weekend", AppliesTo = "All", MultiplierPercent = 15, Description = "Saturday and Sunday booking premium", DaysOfWeek = "Sat,Sun", IsActive = true },
                new PricingRule { Id = 3, RuleName = "Emergency Booking", RuleType = "Emergency", AppliesTo = "BookingType", TargetValue = "Emergency", MultiplierPercent = 30, Description = "Same-hour emergency booking fee", IsActive = true },
                new PricingRule { Id = 4, RuleName = "Morning Peak", RuleType = "PeakHour", AppliesTo = "All", MultiplierPercent = 10, Description = "7-10 AM breakfast peak", StartHour = 7, EndHour = 10, IsActive = true }
            );

            // ── Batch 19 ──────────────────────────────────────────────
            mb.Entity<Coupon>().HasIndex(c => c.Code).IsUnique();
            mb.Entity<Coupon>().HasIndex(c => c.IsActive);
            mb.Entity<CouponUsage>().HasIndex(u => new { u.CouponId, u.UserId });
            mb.Entity<BookingTracking>().HasIndex(t => t.BookingId).IsUnique();
            mb.Entity<BookingTracking>().HasIndex(t => t.ChefId);

            mb.Entity<Coupon>().HasData(
                new Coupon { Id = 1, Code = "WELCOME100", Description = "Welcome offer for new users", DiscountType = "Flat", DiscountValue = 100, MinOrderAmount = 500, UsageLimit = 0, ValidFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, UsageLimitPerUser = 1 },
                new Coupon { Id = 2, Code = "LOVEAT20", Description = "20% off on all bookings", DiscountType = "Percent", DiscountValue = 20, MinOrderAmount = 300, MaxDiscount = 200, UsageLimit = 500, ValidFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, UsageLimitPerUser = 1 },
                new Coupon { Id = 3, Code = "CHEF15", Description = "15% off, chef discovery promo", DiscountType = "Percent", DiscountValue = 15, MinOrderAmount = 0, MaxDiscount = 150, UsageLimit = 0, ValidFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc), IsActive = true, UsageLimitPerUser = 2 }
            );

            // ── Batch 20 ──────────────────────────────────────────────
            mb.Entity<QualityCheck>().HasIndex(q => q.BookingId).IsUnique();
            mb.Entity<QualityCheck>().HasIndex(q => q.CustomerId);
            mb.Entity<SafetyAlert>().HasIndex(a => a.UserId);
            mb.Entity<SafetyAlert>().HasIndex(a => a.Status);
            mb.Entity<GroceryList>().HasIndex(g => g.BookingId).IsUnique();
            mb.Entity<GroceryList>().HasIndex(g => g.ChefId);
            mb.Entity<MealPlan>().HasIndex(p => new { p.UserId, p.WeekStart });
            mb.Entity<ChefPortfolioItem>().HasIndex(p => p.ChefProfileId);
            mb.Entity<ChefPortfolioItem>().HasIndex(p => new { p.ChefProfileId, p.IsActive });

            // ── Batch 21 ──────────────────────────────────────────────
            mb.Entity<CommissionLedger>().HasIndex(l => l.BookingId).IsUnique();
            mb.Entity<CommissionLedger>().HasIndex(l => l.ChefId);
            mb.Entity<CommissionLedger>().HasIndex(l => l.Status);
            mb.Entity<ChefBankAccount>().HasIndex(b => b.ChefId);
            mb.Entity<SettlementRequest>().HasIndex(r => new { r.ChefId, r.Status });

            mb.Entity<CommissionRule>().HasData(
                new CommissionRule { Id = 1, RuleName = "Default Commission", AppliesTo = "All", PlatformPercent = 15, ChefPercent = 85, IsActive = true },
                new CommissionRule { Id = 2, RuleName = "Emergency Booking", AppliesTo = "BookingType", TargetValue = "Emergency", PlatformPercent = 12, ChefPercent = 88, IsActive = true },
                new CommissionRule { Id = 3, RuleName = "Event Cooking", AppliesTo = "BookingType", TargetValue = "Event", PlatformPercent = 18, ChefPercent = 82, IsActive = true }
            );

            // ── Batch 22 ──────────────────────────────────────────────
            mb.Entity<Invoice>().HasIndex(i => i.InvoiceNumber).IsUnique();
            mb.Entity<Invoice>().HasIndex(i => i.BookingId).IsUnique();
            mb.Entity<Invoice>().HasIndex(i => i.CustomerId);
            mb.Entity<TransactionRecord>().HasIndex(t => t.TransactionId).IsUnique();
            mb.Entity<TransactionRecord>().HasIndex(t => t.UserId);
            mb.Entity<TransactionRecord>().HasIndex(t => t.Type);
            mb.Entity<TransactionRecord>().HasIndex(t => t.CreatedAt);

            // ── Batch 23 ──────────────────────────────────────────────
            mb.Entity<Dispute>().HasIndex(d => d.DisputeNumber).IsUnique();
            mb.Entity<Dispute>().HasIndex(d => d.BookingId);
            mb.Entity<Dispute>().HasIndex(d => d.Status);
            mb.Entity<DisputeMessage>().HasIndex(m => m.DisputeId);
            mb.Entity<FraudDetectionLog>().HasIndex(l => l.UserId);
            mb.Entity<FraudDetectionLog>().HasIndex(l => l.Action);

            // ── Batch 24 ──────────────────────────────────────────────
            mb.Entity<SafetyIncident>().HasIndex(i => i.IncidentNumber).IsUnique();
            mb.Entity<SafetyIncident>().HasIndex(i => i.Status);
            mb.Entity<SafetyIncident>().HasIndex(i => i.Severity);
            mb.Entity<AdminRole>().HasIndex(r => r.RoleName).IsUnique();
            mb.Entity<AdminUser>().HasIndex(u => u.UserId).IsUnique();

            // ── Batch 25 ──────────────────────────────────────────────
            mb.Entity<AuditLog>().HasIndex(l => l.AdminUserId);
            mb.Entity<AuditLog>().HasIndex(l => l.Module);
            mb.Entity<AuditLog>().HasIndex(l => l.CreatedAt);
            mb.Entity<CmsPage>().HasIndex(p => p.Slug).IsUnique();
            mb.Entity<CmsBanner>().HasIndex(b => b.IsActive);

            mb.Entity<CmsPage>().HasData(
                new CmsPage { Id = 1, Slug = "terms", Title = "Terms & Conditions", Content = "## Terms & Conditions\n\nPlaceholder — replace with real legal copy before launch (see release checklist).", IsActive = true, UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new CmsPage { Id = 2, Slug = "privacy", Title = "Privacy Policy", Content = "## Privacy Policy\n\nPlaceholder — must be DPDP Act 2023 compliant for Indian users before launch.", IsActive = true, UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new CmsPage { Id = 3, Slug = "about", Title = "About LovEat", Content = "## About LovEat\n\nHome chef booking platform for Tamil Nadu.", IsActive = true, UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new CmsPage { Id = 4, Slug = "refund", Title = "Refund Policy", Content = "## Refund Policy\n\nRefunds processed in 5-7 business days.", IsActive = true, UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // ── Batch 26 ──────────────────────────────────────────────
            mb.Entity<UserSegment>().HasIndex(s => s.Name).IsUnique();
            mb.Entity<CrmNote>().HasIndex(n => n.UserId);

            mb.Entity<UserSegment>().HasData(
                new UserSegment { Id = 1, Name = "VIP Users", Description = "10+ bookings", UserCount = 0, IsActive = true },
                new UserSegment { Id = 2, Name = "Regular Users", Description = "3-9 bookings", UserCount = 0, IsActive = true },
                new UserSegment { Id = 3, Name = "New Users", Description = "1-2 bookings", UserCount = 0, IsActive = true },
                new UserSegment { Id = 4, Name = "Inactive Users", Description = "No booking in 60 days", UserCount = 0, IsActive = true },
                new UserSegment { Id = 5, Name = "Churn Risk", Description = "Active then stopped", UserCount = 0, IsActive = true }
            );

            // ── Batch 27 ──────────────────────────────────────────────
            mb.Entity<ServiceCity>().HasIndex(c => c.CityName).IsUnique();
            mb.Entity<ServiceZone>().HasIndex(z => z.ServiceCityId);
            mb.Entity<DemandPrediction>().HasIndex(d => new { d.City, d.Date, d.Hour });

            mb.Entity<ServiceCity>().HasData(
                new ServiceCity { Id = 1, CityName = "Coimbatore", State = "Tamil Nadu", Latitude = 11.0168m, Longitude = 76.9558m, RadiusKm = 25, IsActive = true, ChefCount = 48, BookingCount = 1240 },
                new ServiceCity { Id = 2, CityName = "Chennai", State = "Tamil Nadu", Latitude = 13.0827m, Longitude = 80.2707m, RadiusKm = 35, IsActive = true, ChefCount = 85, BookingCount = 3200 },
                new ServiceCity { Id = 3, CityName = "Madurai", State = "Tamil Nadu", Latitude = 9.9252m, Longitude = 78.1198m, RadiusKm = 20, IsActive = true, ChefCount = 22, BookingCount = 480 },
                new ServiceCity { Id = 4, CityName = "Trichy", State = "Tamil Nadu", Latitude = 10.7905m, Longitude = 78.7047m, RadiusKm = 18, IsActive = true, ChefCount = 14, BookingCount = 310 },
                new ServiceCity { Id = 5, CityName = "Bangalore", State = "Karnataka", Latitude = 12.9716m, Longitude = 77.5946m, RadiusKm = 30, IsActive = false, IsLaunching = true }
            );

            // ── Batch 28 ──────────────────────────────────────────────
            mb.Entity<ChefPerformanceSnapshot>().HasIndex(s => new { s.ChefId, s.Period, s.PeriodKey }).IsUnique();
            mb.Entity<ChefPerformanceSnapshot>().HasIndex(s => s.ChefId);

            // ── Batch 29 ──────────────────────────────────────────────
            mb.Entity<UserSession>().HasIndex(s => new { s.UserId, s.DeviceId });
            mb.Entity<UserSession>().HasIndex(s => s.IsActive);
            mb.Entity<SecurityEvent>().HasIndex(e => e.UserId);
            mb.Entity<SecurityEvent>().HasIndex(e => e.OccurredAt);
            mb.Entity<CorporateSubscription>().HasIndex(s => s.CompanyId).IsUnique();
            mb.Entity<CorporateBillingRecord>().HasIndex(b => b.SubscriptionId);
            mb.Entity<CorporateBillingRecord>().HasOne(b => b.Subscription).WithMany(s => s.BillingRecords).HasForeignKey(b => b.SubscriptionId).OnDelete(DeleteBehavior.Cascade);

            mb.Entity<CorporatePlan>().HasData(
                new CorporatePlan { Id = 1, PlanName = "Basic", MealsPerDay = 20, PricePerMeal = 90, MonthlyPrice = 46800, SortOrder = 1, Features = "[\"Dedicated chef\",\"Mon-Fri only\",\"Lunch only\",\"Email support\"]" },
                new CorporatePlan { Id = 2, PlanName = "Standard", MealsPerDay = 40, PricePerMeal = 85, MonthlyPrice = 88400, SortOrder = 2, Features = "[\"Dedicated chef\",\"Mon-Sat\",\"Lunch + Snack\",\"Phone support\",\"Monthly report\"]" },
                new CorporatePlan { Id = 3, PlanName = "Premium", MealsPerDay = 80, PricePerMeal = 80, MonthlyPrice = 166400, SortOrder = 3, Features = "[\"2 dedicated chefs\",\"Mon-Sun\",\"All meals\",\"Priority support\",\"Weekly reports\",\"Custom menu\"]" },
                new CorporatePlan { Id = 4, PlanName = "Enterprise", MealsPerDay = 200, PricePerMeal = 75, MonthlyPrice = 390000, SortOrder = 4, Features = "[\"Chef team\",\"24/7 availability\",\"All meals\",\"Account manager\",\"Daily reports\",\"Custom menu\",\"GST invoicing\"]" }
            );

            // ── Batch 30 ──────────────────────────────────────────────
            mb.Entity<MarketplaceService>().HasIndex(s => s.ServiceName).IsUnique();
            mb.Entity<ChefServiceOffering>().HasIndex(o => new { o.ChefProfileId, o.MarketplaceServiceId }).IsUnique();
            mb.Entity<ExpansionCity>().HasIndex(c => c.CityName).IsUnique();
            mb.Entity<CityWaitlist>().HasIndex(w => new { w.UserId, w.CityName }).IsUnique();

            // ── Batch 31 ──────────────────────────────────────────────
            mb.Entity<BackgroundVerification>().HasIndex(v => new { v.ChefId, v.VerificationType });
            mb.Entity<BackgroundVerification>().HasIndex(v => v.Status);
            mb.Entity<InsurancePolicy>().HasIndex(p => p.PolicyNumber).IsUnique();
            mb.Entity<InsurancePolicy>().HasIndex(p => p.UserId);
            mb.Entity<InsuranceClaim>().HasIndex(c => c.ClaimRef).IsUnique();
            mb.Entity<InsuranceClaim>().HasIndex(c => c.Status);

            // ── Batch 32 ──────────────────────────────────────────────
            mb.Entity<FSSAILicense>().HasIndex(l => l.ChefId).IsUnique();
            mb.Entity<FSSAILicense>().HasIndex(l => l.LicenseNumber).IsUnique();
            mb.Entity<FSSAILicense>().HasIndex(l => l.Status);
            mb.Entity<FSSAILicense>().HasIndex(l => l.ExpiryDate);
            mb.Entity<VideoConsultation>().HasIndex(v => v.CustomerId);
            mb.Entity<VideoConsultation>().HasIndex(v => v.ChefId);
            mb.Entity<VideoConsultation>().HasIndex(v => v.Status);
            mb.Entity<VideoConsultation>().HasIndex(v => v.ScheduledAt);

            // ── Batch 33 ──────────────────────────────────────────────
            mb.Entity<UserDietaryProfile>().HasIndex(p => p.UserId).IsUnique();
            mb.Entity<DietaryMatchScore>().HasIndex(s => new { s.UserId, s.ChefId }).IsUnique();
            mb.Entity<ChefFollow>().HasIndex(f => new { f.FollowerId, f.ChefId }).IsUnique();
            mb.Entity<MealPostLike>().HasIndex(l => new { l.PostId, l.UserId }).IsUnique();
            mb.Entity<MealPost>().HasIndex(p => p.ChefId);
            mb.Entity<MealPost>().HasIndex(p => p.CreatedAt);
            mb.Entity<MealPostComment>().HasIndex(c => c.PostId);
            mb.Entity<MealPostComment>().HasOne(c => c.Post).WithMany(p => p.Comments).HasForeignKey(c => c.PostId).OnDelete(DeleteBehavior.Cascade);

            // ── Batch 34 ──────────────────────────────────────────────
            mb.Entity<EquipmentListing>().HasIndex(l => l.ChefId);
            mb.Entity<EquipmentListing>().HasIndex(l => new { l.City, l.Category, l.IsAvailable });
            mb.Entity<EquipmentRequest>().HasIndex(r => r.ListingId);
            mb.Entity<EquipmentRequest>().HasIndex(r => r.RequestedByChefId);
            mb.Entity<CertificationCourse>().HasIndex(c => c.Category);
            mb.Entity<ChefCertification>().HasIndex(c => new { c.ChefId, c.CourseId }).IsUnique();

            // ── Batch 35 ──────────────────────────────────────────────
            mb.Entity<GroupBooking>().HasIndex(g => g.CustomerId);
            mb.Entity<GroupBookingSplit>().HasIndex(s => s.GroupBookingId);
            mb.Entity<GroupBookingSplit>().HasIndex(s => s.BookingId).IsUnique();
            mb.Entity<ChefMatchScore>().HasIndex(s => new { s.CustomerId, s.ChefId }).IsUnique();
            mb.Entity<ChefMatchScore>().HasIndex(s => s.ComputedAt);

            // ── Batch 36 ──────────────────────────────────────────────
            mb.Entity<ChurnRiskScore>().HasIndex(s => s.CustomerId).IsUnique();
            mb.Entity<ChurnRiskScore>().HasIndex(s => s.RiskLevel);
            mb.Entity<VoiceCommandLog>().HasIndex(v => v.UserId);
            mb.Entity<VoiceCommandLog>().HasIndex(v => v.CreatedAt);

            // ── Batch 37 ──────────────────────────────────────────────
            mb.Entity<Currency>().HasIndex(c => c.Code).IsUnique();
            mb.Entity<TaxRule>().HasIndex(r => new { r.Country, r.Region });

            // ── Batch 38 ──────────────────────────────────────────────
            mb.Entity<SupportedLanguage>().HasIndex(l => l.Code).IsUnique();
            mb.Entity<TranslationString>().HasIndex(t => new { t.Key, t.LanguageCode }).IsUnique();
            mb.Entity<MealBoxSubscription>().HasIndex(s => s.CustomerId);
            mb.Entity<MealBoxSubscription>().HasIndex(s => s.Status);
            mb.Entity<MealBoxDelivery>().HasIndex(d => d.SubscriptionId);
            mb.Entity<MealBoxDelivery>().HasIndex(d => d.ScheduledDate);

            // ── Batch 39 ──────────────────────────────────────────────
            mb.Entity<LiveStream>().HasIndex(s => s.ChefId);
            mb.Entity<LiveStream>().HasIndex(s => s.Status);
            mb.Entity<StreamComment>().HasIndex(c => c.StreamId);
            mb.Entity<LeaderboardSeason>().HasIndex(s => new { s.StartDate, s.EndDate });
            mb.Entity<ChefLeaderboardEntry>().HasIndex(e => new { e.SeasonId, e.ChefId }).IsUnique();
            mb.Entity<LeaderboardReward>().HasIndex(r => r.SeasonId);

            // ── Batch 40 ──────────────────────────────────────────────
            mb.Entity<WhiteLabelClient>().HasIndex(c => c.ApiKey).IsUnique();
            mb.Entity<WhiteLabelUsageLog>().HasIndex(l => l.ClientId);
            mb.Entity<IngredientCarbonFactor>().HasIndex(f => f.IngredientName).IsUnique();
            mb.Entity<BookingCarbonEstimate>().HasIndex(e => e.BookingId);

            // ── Batch 41 ──────────────────────────────────────────────
            mb.Entity<B2BPartner>().HasIndex(p => p.ApiKey).IsUnique();
            mb.Entity<B2BBookingRequest>().HasIndex(r => r.PartnerId);
            mb.Entity<IngredientListing>().HasIndex(l => l.ChefId);
            mb.Entity<IngredientListing>().HasIndex(l => new { l.City, l.IsAvailable });
            mb.Entity<IngredientSwapRequest>().HasIndex(r => r.ListingId);
            mb.Entity<IngredientSwapRequest>().HasIndex(r => r.RequestedByChefId);

            // ── Batch 42 ──────────────────────────────────────────────
            mb.Entity<PlatformHealthMetric>().HasIndex(m => new { m.MetricName, m.RecordedAt });
            mb.Entity<Franchise>().HasIndex(f => f.RegionOrCity);
            mb.Entity<FranchiseSettings>().HasIndex(s => new { s.FranchiseId, s.SettingKey }).IsUnique();
            mb.Entity<CityManagerAssignment>().HasIndex(a => new { a.ServiceCityId, a.IsActive });
            mb.Entity<CityPerformanceSnapshot>().HasIndex(s => new { s.ServiceCityId, s.PeriodKey }).IsUnique();
            mb.Entity<RegionCity>().HasIndex(rc => new { rc.RegionId, rc.ServiceCityId }).IsUnique();

            // M87-M88: Commission Engine + Dynamic Commission Rules
            mb.Entity<CommissionProfile>().HasIndex(p => p.Status);
            mb.Entity<CommissionProfileRule>().HasIndex(r => new { r.CommissionProfileId, r.Priority });
            mb.Entity<CommissionProfileRule>().HasOne(r => r.CommissionProfile).WithMany(p => p.Rules).HasForeignKey(r => r.CommissionProfileId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CommissionRuleAuditLog>().HasOne(l => l.ChangedBy).WithMany().HasForeignKey(l => l.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<CommissionProfile>().HasOne(p => p.CreatedBy).WithMany().HasForeignKey(p => p.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<CommissionProfile>().HasOne(p => p.ApprovedBy).WithMany().HasForeignKey(p => p.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<DynamicCommissionEvaluation>().HasOne(e => e.Chef).WithMany().HasForeignKey(e => e.ChefId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<DynamicCommissionEvaluation>().HasIndex(e => new { e.ChefId, e.EvaluatedAt });

            // M89-M90: CMS Management + Banner/Promotion Management
            mb.Entity<CmsContentBlock>().HasIndex(b => new { b.Key, b.Locale }).IsUnique();
            mb.Entity<CmsContentBlockVersion>().HasIndex(v => new { v.CmsContentBlockId, v.Version });
            mb.Entity<CmsContentBlockVersion>().HasOne(v => v.SavedBy).WithMany().HasForeignKey(v => v.SavedByUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<CmsPageRevision>().HasOne(r => r.EditedBy).WithMany().HasForeignKey(r => r.EditedByUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<CmsPageRevision>().HasIndex(r => r.CmsPageId);
            mb.Entity<PromoCampaign>().HasIndex(c => new { c.Status, c.StartAt, c.EndAt });
            mb.Entity<PromoCampaign>().HasOne(c => c.CreatedBy).WithMany().HasForeignKey(c => c.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<PromoCampaignVariant>().HasOne(v => v.PromoCampaign).WithMany(c => c.Variants).HasForeignKey(v => v.PromoCampaignId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<PromoCampaignDailyStat>().HasIndex(s => new { s.PromoCampaignVariantId, s.DateKey }).IsUnique();
            mb.Entity<PromoCampaignDailyStat>().HasOne(s => s.PromoCampaign).WithMany().HasForeignKey(s => s.PromoCampaignId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<PromoCampaignDailyStat>().HasOne(s => s.Variant).WithMany().HasForeignKey(s => s.PromoCampaignVariantId).OnDelete(DeleteBehavior.Cascade);

            // M91-M92: Feature Flag Management + Announcement Management
            mb.Entity<FeatureFlag>().HasIndex(f => f.Key).IsUnique();
            mb.Entity<FeatureFlagAuditLog>().HasOne(l => l.ChangedBy).WithMany().HasForeignKey(l => l.ChangedByUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<FeatureFlag>().HasOne(f => f.CreatedBy).WithMany().HasForeignKey(f => f.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Announcement>().HasIndex(a => new { a.Status, a.StartAt, a.EndAt });
            mb.Entity<Announcement>().HasOne(a => a.CreatedBy).WithMany().HasForeignKey(a => a.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<AnnouncementReceipt>().HasIndex(r => new { r.AnnouncementId, r.UserId }).IsUnique();
            mb.Entity<AnnouncementReceipt>().HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);

            // M93-M94: GST Management + TDS Management
            mb.Entity<ChefGstProfile>().HasIndex(p => p.ChefId).IsUnique();
            mb.Entity<ChefGstProfile>().HasOne(p => p.Chef).WithMany().HasForeignKey(p => p.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefGstProfile>().HasOne(p => p.VerifiedByAdmin).WithMany().HasForeignKey(p => p.VerifiedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<GstHsnSacCode>().HasIndex(c => c.Code).IsUnique();
            mb.Entity<GstReturnPeriod>().HasIndex(p => new { p.PeriodKey, p.ReturnType }).IsUnique();
            mb.Entity<GstReturnPeriod>().HasOne(p => p.FiledByAdmin).WithMany().HasForeignKey(p => p.FiledByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<TdsRateConfig>().HasIndex(c => c.Section).IsUnique();
            mb.Entity<TdsDeduction>().HasOne(d => d.Chef).WithMany().HasForeignKey(d => d.ChefId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<TdsDeduction>().HasOne(d => d.SettlementRequest).WithMany().HasForeignKey(d => d.SettlementRequestId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<TdsDeduction>().HasIndex(d => new { d.ChefId, d.FinancialYear, d.Quarter });
            mb.Entity<TdsReturn>().HasIndex(r => new { r.FinancialYear, r.Quarter }).IsUnique();
            mb.Entity<TdsReturn>().HasOne(r => r.FiledByAdmin).WithMany().HasForeignKey(r => r.FiledByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<TdsCertificate>().HasOne(c => c.Chef).WithMany().HasForeignKey(c => c.ChefId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<TdsCertificate>().HasIndex(c => new { c.ChefId, c.FinancialYear, c.Quarter });

            // M95-M96: Vendor Settlement + Automatic Invoice
            mb.Entity<SettlementBatch>().HasOne(b => b.CreatedByAdmin).WithMany().HasForeignKey(b => b.CreatedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<SettlementBatch>().HasOne(b => b.ApprovedByAdmin).WithMany().HasForeignKey(b => b.ApprovedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<SettlementBatchItem>().HasOne(i => i.SettlementBatch).WithMany(b => b.Items).HasForeignKey(i => i.SettlementBatchId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<SettlementBatchItem>().HasOne(i => i.SettlementRequest).WithMany().HasForeignKey(i => i.SettlementRequestId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<SettlementBatchItem>().HasOne(i => i.Chef).WithMany().HasForeignKey(i => i.ChefId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<VendorSettlementHold>().HasIndex(h => new { h.ChefId, h.IsActive });
            mb.Entity<VendorSettlementHold>().HasOne(h => h.Chef).WithMany().HasForeignKey(h => h.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<VendorSettlementHold>().HasOne(h => h.HeldByAdmin).WithMany().HasForeignKey(h => h.HeldByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<AutoInvoiceRunLog>().HasIndex(l => l.BookingId);
            mb.Entity<InvoiceDeliveryLog>().HasOne(d => d.Invoice).WithMany().HasForeignKey(d => d.InvoiceId).OnDelete(DeleteBehavior.Cascade);

            // M97-M100: Credit Notes + Debit Notes + Refund Dashboard + Financial Reports
            mb.Entity<CreditNote>().HasIndex(n => n.CreditNoteNumber).IsUnique();
            mb.Entity<CreditNote>().HasOne(n => n.Invoice).WithMany().HasForeignKey(n => n.InvoiceId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<CreditNote>().HasOne(n => n.Customer).WithMany().HasForeignKey(n => n.CustomerId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<CreditNote>().HasOne(n => n.IssuedByAdmin).WithMany().HasForeignKey(n => n.IssuedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<DebitNote>().HasIndex(n => n.DebitNoteNumber).IsUnique();
            mb.Entity<DebitNote>().HasOne(n => n.Invoice).WithMany().HasForeignKey(n => n.InvoiceId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<DebitNote>().HasOne(n => n.Party).WithMany().HasForeignKey(n => n.PartyId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<DebitNote>().HasOne(n => n.IssuedByAdmin).WithMany().HasForeignKey(n => n.IssuedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<RefundRequest>().HasOne(r => r.Booking).WithMany().HasForeignKey(r => r.BookingId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<RefundRequest>().HasOne(r => r.Customer).WithMany().HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<RefundRequest>().HasOne(r => r.ProcessedByAdmin).WithMany().HasForeignKey(r => r.ProcessedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<RefundRequest>().HasIndex(r => r.Status);
            mb.Entity<FinancialReportSnapshot>().HasIndex(s => s.PeriodKey).IsUnique();
            mb.Entity<FinancialReportSnapshot>().HasOne(s => s.GeneratedByAdmin).WithMany().HasForeignKey(s => s.GeneratedByAdminId).OnDelete(DeleteBehavior.Restrict);

            // M101-M104: Expense Management + Ticket Routing + Live Chat
            mb.Entity<Expense>().HasOne(e => e.SubmittedByAdmin).WithMany().HasForeignKey(e => e.SubmittedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Expense>().HasOne(e => e.ApprovedByAdmin).WithMany().HasForeignKey(e => e.ApprovedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Expense>().HasIndex(e => e.Status);
            mb.Entity<TicketAssignmentRule>().HasOne(r => r.TicketQueue).WithMany().HasForeignKey(r => r.TicketQueueId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<TicketAssignmentRule>().HasOne(r => r.DefaultAssigneeAdmin).WithMany().HasForeignKey(r => r.DefaultAssigneeAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<TicketAssignmentRule>().HasIndex(r => new { r.Category, r.Priority });
            mb.Entity<TicketActivityLog>().HasOne(l => l.Ticket).WithMany().HasForeignKey(l => l.TicketId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<TicketActivityLog>().HasOne(l => l.ChangedByAdmin).WithMany().HasForeignKey(l => l.ChangedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<LiveChatSession>().HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<LiveChatSession>().HasOne(s => s.AgentAdmin).WithMany().HasForeignKey(s => s.AgentAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<LiveChatSession>().HasIndex(s => s.Status);
            mb.Entity<LiveChatMessage>().HasOne(m => m.LiveChatSession).WithMany().HasForeignKey(m => m.LiveChatSessionId).OnDelete(DeleteBehavior.Cascade);

            // M105-M108: Call Center + Escalation Matrix + SLA Management + Complaint Tracking
            mb.Entity<CallLog>().HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<CallLog>().HasOne(c => c.AgentAdmin).WithMany().HasForeignKey(c => c.AgentAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<CallLog>().HasOne(c => c.LinkedTicket).WithMany().HasForeignKey(c => c.LinkedTicketId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<EscalationLevel>().HasIndex(l => l.Level).IsUnique();
            mb.Entity<EscalationLevel>().HasOne(l => l.NotifyAdmin).WithMany().HasForeignKey(l => l.NotifyAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<EscalationEvent>().HasOne(e => e.Ticket).WithMany().HasForeignKey(e => e.TicketId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<EscalationEvent>().HasOne(e => e.EscalatedByAdmin).WithMany().HasForeignKey(e => e.EscalatedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<TicketEscalationState>().HasIndex(s => s.TicketId).IsUnique();
            mb.Entity<TicketEscalationState>().HasOne(s => s.Ticket).WithMany().HasForeignKey(s => s.TicketId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<SlaTracker>().HasIndex(t => t.TicketId).IsUnique();
            mb.Entity<SlaTracker>().HasOne(t => t.Ticket).WithMany().HasForeignKey(t => t.TicketId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<SlaTracker>().HasOne(t => t.SlaPolicy).WithMany().HasForeignKey(t => t.SlaPolicyId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Complaint>().HasIndex(c => c.ComplaintNumber).IsUnique();
            mb.Entity<Complaint>().HasOne(c => c.Complainant).WithMany().HasForeignKey(c => c.ComplainantUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Complaint>().HasOne(c => c.AgainstChef).WithMany().HasForeignKey(c => c.AgainstChefId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<Complaint>().HasOne(c => c.Booking).WithMany().HasForeignKey(c => c.BookingId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<Complaint>().HasOne(c => c.AssignedAdmin).WithMany().HasForeignKey(c => c.AssignedAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<ComplaintUpdate>().HasOne(u => u.Complaint).WithMany().HasForeignKey(u => u.ComplaintId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ComplaintUpdate>().HasOne(u => u.AddedByAdmin).WithMany().HasForeignKey(u => u.AddedByAdminId).OnDelete(DeleteBehavior.Restrict);

            // M109-M112: Internal Notes + Feedback Center + Smart Pricing + AI Fraud Scoring
            mb.Entity<InternalNote>().HasIndex(n => new { n.EntityType, n.EntityId });
            mb.Entity<InternalNote>().HasOne(n => n.CreatedByAdmin).WithMany().HasForeignKey(n => n.CreatedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<FeedbackResponse>().HasOne(r => r.FeedbackSurvey).WithMany().HasForeignKey(r => r.FeedbackSurveyId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<FeedbackResponse>().HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<PricingSuggestion>().HasIndex(p => new { p.City, p.GeneratedAt });
            mb.Entity<RiskSignalDefinition>().HasIndex(s => s.SignalKey).IsUnique();

            // M113-M116: Churn Prediction + Suggested Replies + Sentiment Analysis + Chef Quality Scoring
            // (ChurnRiskScore FK config already exists above — see Batch 36 M71.)
            mb.Entity<SentimentScore>().HasIndex(s => new { s.SourceType, s.SourceId }).IsUnique();
            mb.Entity<ChefQualityScore>().HasOne(s => s.Chef).WithMany().HasForeignKey(s => s.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefQualityScore>().HasIndex(s => new { s.ChefId, s.ComputedAt });

            // M117-M120: Content Moderation + Cancellation Risk + Cohort Retention + Revenue Analytics
            mb.Entity<ModerationFlag>().HasOne(f => f.ReviewedByAdmin).WithMany().HasForeignKey(f => f.ReviewedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<ModerationFlag>().HasIndex(f => new { f.SourceType, f.SourceId });
            mb.Entity<CancellationRiskScore>().HasOne(s => s.Booking).WithMany().HasForeignKey(s => s.BookingId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CancellationRiskScore>().HasIndex(s => s.BookingId);
            mb.Entity<CohortRetentionSnapshot>().HasIndex(s => new { s.CohortMonth, s.MonthOffset }).IsUnique();
            mb.Entity<RevenueAnalyticsSnapshot>().HasIndex(s => new { s.DateKey, s.City, s.Category });

            // M121-M124: CLV + Booking Funnel + Chef Performance + Geo Heatmap
            mb.Entity<CustomerLtvSnapshot>().HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CustomerLtvSnapshot>().HasIndex(s => new { s.CustomerId, s.ComputedAt });
            mb.Entity<BookingFunnelSnapshot>().HasIndex(s => s.PeriodKey).IsUnique();
            mb.Entity<ChefPerformanceAnalyticsSnapshot>().HasOne(s => s.Chef).WithMany().HasForeignKey(s => s.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefPerformanceAnalyticsSnapshot>().HasIndex(s => new { s.ChefId, s.PeriodKey }).IsUnique();
            mb.Entity<GeoHeatmapCell>().HasIndex(c => new { c.PeriodKey, c.GridLatitude, c.GridLongitude });

            // M125-M128: Report Builder + Scheduled Reports + A/B Testing + Data Export
            mb.Entity<ReportDefinition>().HasOne(d => d.CreatedByAdmin).WithMany().HasForeignKey(d => d.CreatedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<ScheduledReport>().HasOne(s => s.ReportDefinition).WithMany().HasForeignKey(s => s.ReportDefinitionId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ScheduledReportRun>().HasOne(r => r.ScheduledReport).WithMany().HasForeignKey(r => r.ScheduledReportId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ExperimentEvent>().HasOne(e => e.Experiment).WithMany().HasForeignKey(e => e.ExperimentId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ExperimentEvent>().HasIndex(e => new { e.ExperimentId, e.UserId, e.EventType });
            mb.Entity<DataExportJob>().HasOne(j => j.RequestedByAdmin).WithMany().HasForeignKey(j => j.RequestedByAdminId).OnDelete(DeleteBehavior.Restrict);

            // M129-M132: Corporate Accounts + Bulk Ordering + API Keys + Webhooks
            mb.Entity<CorporateAccount>().HasOne(a => a.OwnerUser).WithMany().HasForeignKey(a => a.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<CorporateAccountMember>().HasOne(m => m.CorporateAccount).WithMany().HasForeignKey(m => m.CorporateAccountId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CorporateAccountMember>().HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CorporateAccountMember>().HasIndex(m => new { m.CorporateAccountId, m.UserId }).IsUnique();
            mb.Entity<BulkOrderRequest>().HasOne(o => o.CorporateAccount).WithMany().HasForeignKey(o => o.CorporateAccountId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<BulkOrderRequest>().HasOne(o => o.RequestedByUser).WithMany().HasForeignKey(o => o.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<BulkOrderLineItem>().HasOne(li => li.BulkOrderRequest).WithMany().HasForeignKey(li => li.BulkOrderRequestId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<BulkOrderLineItem>().HasOne(li => li.Chef).WithMany().HasForeignKey(li => li.ChefId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<ApiKey>().HasIndex(k => k.KeyHash).IsUnique();
            mb.Entity<ApiKey>().HasOne(k => k.CorporateAccount).WithMany().HasForeignKey(k => k.CorporateAccountId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<ApiKey>().HasOne(k => k.CreatedByAdmin).WithMany().HasForeignKey(k => k.CreatedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<WebhookSubscription>().HasOne(s => s.CorporateAccount).WithMany().HasForeignKey(s => s.CorporateAccountId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<WebhookDeliveryLog>().HasOne(l => l.WebhookSubscription).WithMany().HasForeignKey(l => l.WebhookSubscriptionId).OnDelete(DeleteBehavior.Cascade);

            // M133-M136: SLA Contracts + Multi-Location + Custom Billing + Enterprise Reporting
            mb.Entity<SlaContract>().HasOne(c => c.CorporateAccount).WithMany().HasForeignKey(c => c.CorporateAccountId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<SlaContract>().HasIndex(c => c.ContractNumber).IsUnique();
            mb.Entity<SlaContractBreach>().HasOne(b => b.SlaContract).WithMany().HasForeignKey(b => b.SlaContractId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CorporateLocation>().HasOne(l => l.CorporateAccount).WithMany().HasForeignKey(l => l.CorporateAccountId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CorporateInvoice>().HasOne(i => i.CorporateAccount).WithMany().HasForeignKey(i => i.CorporateAccountId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CorporateInvoice>().HasIndex(i => i.InvoiceNumber).IsUnique();
            mb.Entity<EnterpriseUsageSnapshot>().HasOne(s => s.CorporateAccount).WithMany().HasForeignKey(s => s.CorporateAccountId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<EnterpriseUsageSnapshot>().HasIndex(s => new { s.CorporateAccountId, s.PeriodKey }).IsUnique();

            // M137-M140: SSO + Enterprise Audit Log + Chef Inventory + Recipe Costing
            mb.Entity<SsoConnection>().HasOne(c => c.CorporateAccount).WithMany().HasForeignKey(c => c.CorporateAccountId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<SsoConnection>().HasIndex(c => c.EmailDomain).IsUnique();
            mb.Entity<SsoLoginLog>().HasOne(l => l.SsoConnection).WithMany().HasForeignKey(l => l.SsoConnectionId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<SsoLoginLog>().HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<EnterpriseAuditEntry>().HasOne(e => e.CorporateAccount).WithMany().HasForeignKey(e => e.CorporateAccountId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<EnterpriseAuditEntry>().HasOne(e => e.ActorUser).WithMany().HasForeignKey(e => e.ActorUserId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<EnterpriseAuditEntry>().HasIndex(e => new { e.CorporateAccountId, e.OccurredAt });
            mb.Entity<InventoryItem>().HasOne(i => i.Chef).WithMany().HasForeignKey(i => i.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<InventoryItem>().HasIndex(i => i.ChefId);
            mb.Entity<InventoryTransaction>().HasOne(t => t.InventoryItem).WithMany().HasForeignKey(t => t.InventoryItemId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<RecipeIngredient>().HasOne(i => i.ChefMenuItem).WithMany().HasForeignKey(i => i.ChefMenuItemId).OnDelete(DeleteBehavior.Cascade);

            // M141-M144: Staff Management + Shift Scheduling + Service Packages + Chef Promo Codes
            mb.Entity<StaffMember>().HasOne(s => s.Chef).WithMany().HasForeignKey(s => s.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<StaffShiftLog>().HasOne(l => l.StaffMember).WithMany().HasForeignKey(l => l.StaffMemberId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefShift>().HasOne(s => s.Chef).WithMany().HasForeignKey(s => s.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefShift>().HasIndex(s => new { s.ChefId, s.DayOfWeek });
            mb.Entity<ServicePackage>().HasOne(p => p.Chef).WithMany().HasForeignKey(p => p.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<PackageMenuItem>().HasOne(i => i.ServicePackage).WithMany().HasForeignKey(i => i.ServicePackageId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<PackageMenuItem>().HasOne(i => i.ChefMenuItem).WithMany().HasForeignKey(i => i.ChefMenuItemId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<ChefPromoCode>().HasOne(p => p.Chef).WithMany().HasForeignKey(p => p.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefPromoCode>().HasIndex(p => new { p.ChefId, p.Code }).IsUnique();
            mb.Entity<ChefPromoRedemption>().HasOne(r => r.ChefPromoCode).WithMany().HasForeignKey(r => r.ChefPromoCodeId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefPromoRedemption>().HasOne(r => r.Customer).WithMany().HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);

            // M145-M148: Tax Assistant + Chef Loyalty + Business Goals + Multi-Outlet
            mb.Entity<ChefTaxEstimate>().HasOne(e => e.Chef).WithMany().HasForeignKey(e => e.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefTaxEstimate>().HasIndex(e => new { e.ChefId, e.FinancialYear });
            mb.Entity<ChefBusinessExpense>().HasOne(e => e.Chef).WithMany().HasForeignKey(e => e.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefLoyaltyProgram>().HasOne(p => p.Chef).WithMany().HasForeignKey(p => p.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefLoyaltyProgram>().HasIndex(p => p.ChefId).IsUnique();
            mb.Entity<ChefCustomerLoyaltyBalance>().HasOne(b => b.Customer).WithMany().HasForeignKey(b => b.CustomerId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<ChefCustomerLoyaltyBalance>().HasIndex(b => new { b.ChefId, b.CustomerId }).IsUnique();
            mb.Entity<ChefBusinessGoal>().HasOne(g => g.Chef).WithMany().HasForeignKey(g => g.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefBusinessGoal>().HasIndex(g => new { g.ChefId, g.PeriodKey, g.GoalType }).IsUnique();
            mb.Entity<ChefOutlet>().HasOne(o => o.Chef).WithMany().HasForeignKey(o => o.ChefId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ChefOutlet>().HasIndex(o => o.ChefId);

            // M149-M152: Wishlist + Gamification + Preferences + Reorder
            mb.Entity<WishlistItem>().HasOne(w => w.Customer).WithMany().HasForeignKey(w => w.CustomerId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<WishlistItem>().HasIndex(w => new { w.CustomerId, w.ItemType, w.ItemId }).IsUnique();
            mb.Entity<BadgeDefinition>().HasIndex(d => d.Key).IsUnique();
            mb.Entity<CustomerBadge>().HasOne(b => b.Customer).WithMany().HasForeignKey(b => b.CustomerId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CustomerBadge>().HasOne(b => b.BadgeDefinition).WithMany().HasForeignKey(b => b.BadgeDefinitionId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<CustomerBadge>().HasIndex(b => new { b.CustomerId, b.BadgeDefinitionId }).IsUnique();
            mb.Entity<CustomerPreferences>().HasOne(p => p.Customer).WithMany().HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CustomerPreferences>().HasIndex(p => p.CustomerId).IsUnique();
            mb.Entity<QuickReorderCombo>().HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<QuickReorderCombo>().HasOne(c => c.Chef).WithMany().HasForeignKey(c => c.ChefId).OnDelete(DeleteBehavior.Restrict);

            // M153-M156: Split Bill + Meal Scheduling + Accessibility + FAQ
            mb.Entity<BillSplit>().HasOne(s => s.Booking).WithMany().HasForeignKey(s => s.BookingId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<BillSplitParticipant>().HasOne(p => p.BillSplit).WithMany().HasForeignKey(p => p.BillSplitId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<BillSplitParticipant>().HasOne(p => p.Customer).WithMany().HasForeignKey(p => p.CustomerId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<MealSchedule>().HasOne(s => s.Customer).WithMany().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<MealSchedule>().HasOne(s => s.Chef).WithMany().HasForeignKey(s => s.ChefId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<AccessibilitySettings>().HasOne(a => a.Customer).WithMany().HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<AccessibilitySettings>().HasIndex(a => a.CustomerId).IsUnique();
            mb.Entity<FaqArticle>().HasOne(a => a.FaqCategory).WithMany().HasForeignKey(a => a.FaqCategoryId).OnDelete(DeleteBehavior.Cascade);

            // M157-M160: Tracking + Weather + Contracts + MenuBuild
            mb.Entity<OrderTrackingEvent>().HasOne(e => e.Booking).WithMany().HasForeignKey(e => e.BookingId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<DigitalContract>().HasOne(c => c.Booking).WithMany().HasForeignKey(c => c.BookingId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CustomMenuBuild>().HasOne(b => b.Customer).WithMany().HasForeignKey(b => b.CustomerId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<CustomMenuBuild>().HasOne(b => b.Chef).WithMany().HasForeignKey(b => b.ChefId).OnDelete(DeleteBehavior.Restrict);

            // M161-M164: 2FA + Session Mgmt + Data Privacy + Security Audit
            mb.Entity<TwoFactorSetting>().HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<TwoFactorSetting>().HasIndex(t => t.UserId).IsUnique();
            mb.Entity<DataPrivacyRequest>().HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<SecurityAuditEntry>().HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<SecurityAuditEntry>().HasIndex(e => new { e.UserId, e.OccurredAt });

            // M165-M168: IP Rules + Data Masking + Vulnerability + Compliance
            mb.Entity<IpRule>().HasOne(r => r.CreatedByAdmin).WithMany().HasForeignKey(r => r.CreatedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<IpRule>().HasIndex(r => r.IpAddress);
            mb.Entity<VulnerabilityFinding>().HasOne(f => f.AssignedToAdmin).WithMany().HasForeignKey(f => f.AssignedToAdminId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<ComplianceChecklistItem>().HasOne(i => i.OwnerAdmin).WithMany().HasForeignKey(i => i.OwnerAdminId).OnDelete(DeleteBehavior.SetNull);

            // M169-M172: Incident/OnCall + Backup + Runbook + Change Management
            mb.Entity<SystemIncident>().HasOne(i => i.OnCallAdmin).WithMany().HasForeignKey(i => i.OnCallAdminId).OnDelete(DeleteBehavior.SetNull);
            mb.Entity<OnCallSchedule>().HasOne(s => s.Admin).WithMany().HasForeignKey(s => s.AdminId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<Runbook>().HasOne(r => r.CreatedByAdmin).WithMany().HasForeignKey(r => r.CreatedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<ChangeRequest>().HasOne(c => c.RequestedByAdmin).WithMany().HasForeignKey(c => c.RequestedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<ChangeRequest>().HasOne(c => c.ApprovedByAdmin).WithMany().HasForeignKey(c => c.ApprovedByAdminId).OnDelete(DeleteBehavior.SetNull);

            // M173-M177: Capacity + Dependency Health + Cost + API Version + DR Drill
            mb.Entity<DrDrillLog>().HasOne(d => d.ConductedByAdmin).WithMany().HasForeignKey(d => d.ConductedByAdminId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<DependencyHealthCheck>().HasIndex(c => new { c.ServiceName, c.CheckedAt });
            mb.Entity<InfraCostEntry>().HasIndex(e => new { e.PeriodKey, e.ServiceCategory });
            // (DbSets for the block below are declared further up, alongside the other DbSet<> properties.)

            // M179-M184: Safety Suite (Gender-Preference, Trusted Contact, Arrival
            // Verification, Booking Timeout Alert, Dedicated Chef, Event Staff) —
            // merged in from the parallel safety-hardening branch.
            mb.Entity<SafetyPreference>().HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<SafetyPreference>().HasIndex(p => p.UserId).IsUnique();
            mb.Entity<TrustedContactAlert>().HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<TrustedContactAlert>().HasOne(a => a.Booking).WithMany().HasForeignKey(a => a.BookingId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<TrustedContactAlert>().HasIndex(a => a.BookingId);
            mb.Entity<ArrivalVerification>().HasOne(v => v.Booking).WithMany().HasForeignKey(v => v.BookingId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<ArrivalVerification>().HasOne(v => v.Chef).WithMany().HasForeignKey(v => v.ChefId).OnDelete(DeleteBehavior.Restrict);
            mb.Entity<ArrivalVerification>().HasIndex(v => v.BookingId).IsUnique();
            mb.Entity<BookingTimeoutAlert>().HasOne(a => a.Booking).WithMany().HasForeignKey(a => a.BookingId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<BookingTimeoutAlert>().HasIndex(a => a.BookingId).IsUnique();
            mb.Entity<BookingTimeoutAlert>().HasIndex(a => new { a.ResolvedAt, a.ExpectedEndTime });
            mb.Entity<EventStaffRequest>().HasOne(r => r.Booking).WithMany().HasForeignKey(r => r.BookingId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<EventStaffRequest>().HasIndex(r => r.BookingId);
            mb.Entity<SupportStaffAssignment>().HasOne(a => a.EventStaffRequest).WithMany().HasForeignKey(a => a.EventStaffRequestId).OnDelete(DeleteBehavior.Cascade);
            mb.Entity<Subscription>().HasOne(s => s.PreferredChef).WithMany().HasForeignKey(s => s.PreferredChefId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
