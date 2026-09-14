using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M58: Device & Security ─────────────────────────────────────
    public class UserSession
    {
        [Key] public int Id { get; set; }
        public int    UserId       { get; set; }
        public User?  User         { get; set; }
        public string DeviceId     { get; set; } = "";
        public string DeviceName   { get; set; } = ""; // e.g. iPhone 14 / Samsung S23
        public string DeviceType   { get; set; } = ""; // iOS / Android / Web
        public string AppVersion   { get; set; } = "";
        public string? IpAddress   { get; set; }
        public string? Location    { get; set; }
        public string  Token       { get; set; } = ""; // hashed JWT
        public bool    IsActive    { get; set; } = true;
        public bool    IsCurrent   { get; set; } = false;
        public DateTime LoginAt    { get; set; } = DateTime.UtcNow;
        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;
        public DateTime? LoggedOutAt{ get; set; }
    }

    public class SecurityEvent
    {
        [Key] public int Id { get; set; }
        public int    UserId       { get; set; }
        public User?  User         { get; set; }
        public string EventType    { get; set; } = ""; // Login/Logout/PasswordChange/NewDevice/SuspiciousLogin/ForcedLogout
        public string DeviceName   { get; set; } = "";
        public string? IpAddress   { get; set; }
        public string? Location    { get; set; }
        public bool    IsAlert     { get; set; } = false;
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    }

    // ── M59: Corporate Plans ───────────────────────────────────────
    public class CorporatePlan
    {
        [Key] public int Id { get; set; }
        public string PlanName      { get; set; } = ""; // Basic / Standard / Premium / Enterprise
        public int    MealsPerDay   { get; set; }
        public decimal PricePerMeal { get; set; }
        public decimal MonthlyPrice { get; set; }       // computed or fixed
        public string  Features     { get; set; } = "[]"; // JSON list
        public bool    IsActive     { get; set; } = true;
        public int     SortOrder    { get; set; } = 0;
    }

    public class CorporateSubscription
    {
        [Key] public int Id { get; set; }
        public int    CompanyId      { get; set; }       // links to User (corporate account)
        public int    CorporatePlanId{ get; set; }
        public CorporatePlan? Plan   { get; set; }
        public string  CompanyName   { get; set; } = "";
        public string  ContactName   { get; set; } = "";
        public string  ContactPhone  { get; set; } = "";
        public string  ContactEmail  { get; set; } = "";
        public string  GSTNumber     { get; set; } = "";
        public string  BillingAddress{ get; set; } = "";
        public int     EmployeeCount { get; set; }
        public string  Status        { get; set; } = "Active"; // Active/Paused/Cancelled/PendingUpgrade
        public string? PendingPlanId { get; set; }             // upgrade pending approval
        public DateTime StartDate    { get; set; } = DateTime.UtcNow;
        public DateTime? NextBillingDate{ get; set; }
        public decimal MonthlyAmount { get; set; }
        public decimal TotalPaid     { get; set; } = 0;
        public DateTime CreatedAt    { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt    { get; set; } = DateTime.UtcNow;

        // ── Navigation ──
        public ICollection<CorporateBillingRecord> BillingRecords { get; set; } = new List<CorporateBillingRecord>();
    }

    public class CorporateBillingRecord
    {
        [Key] public int Id { get; set; }
        public int    SubscriptionId { get; set; }
        public CorporateSubscription? Subscription { get; set; }
        public decimal Amount        { get; set; }
        public string  Status        { get; set; } = "Paid"; // Paid/Pending/Failed
        public string  InvoiceNumber { get; set; } = "";
        public string  Period        { get; set; } = "";     // e.g. "Dec 2024"
        public DateTime BilledAt     { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt      { get; set; }
    }
}

namespace LovEat.API.DTOs
{
    // M58 DTOs
    public class UserSessionDto
    {
        public int    Id          { get; set; }
        public string DeviceName  { get; set; } = "";
        public string DeviceType  { get; set; } = "";
        public string AppVersion  { get; set; } = "";
        public string? IpAddress  { get; set; }
        public string? Location   { get; set; }
        public bool   IsActive    { get; set; }
        public bool   IsCurrent   { get; set; }
        public string LoginAt     { get; set; } = "";
        public string LastSeenAt  { get; set; } = "";
    }

    public class SecurityEventDto
    {
        public int    Id          { get; set; }
        public string EventType   { get; set; } = "";
        public string DeviceName  { get; set; } = "";
        public string? IpAddress  { get; set; }
        public string? Location   { get; set; }
        public bool   IsAlert     { get; set; }
        public string OccurredAt  { get; set; } = "";
    }

    public class SecurityOverviewDto
    {
        public int    ActiveSessions  { get; set; }
        public int    TotalDevices    { get; set; }
        public bool   HasAlerts       { get; set; }
        public string LastLogin       { get; set; } = "";
        public List<UserSessionDto>   Sessions { get; set; } = new();
        public List<SecurityEventDto> Events   { get; set; } = new();
    }

    // M59 DTOs
    public class CorporatePlanDto
    {
        public int     Id           { get; set; }
        public string  PlanName     { get; set; } = "";
        public int     MealsPerDay  { get; set; }
        public decimal PricePerMeal { get; set; }
        public decimal MonthlyPrice { get; set; }
        public List<string> Features{ get; set; } = new();
        public bool    IsActive     { get; set; }
        public bool    IsPopular    { get; set; }
    }

    public class CorporateSubscriptionDto
    {
        public int     Id              { get; set; }
        public string  CompanyName     { get; set; } = "";
        public string  ContactName     { get; set; } = "";
        public string  ContactPhone    { get; set; } = "";
        public string  ContactEmail    { get; set; } = "";
        public string  GSTNumber       { get; set; } = "";
        public string  PlanName        { get; set; } = "";
        public int     MealsPerDay     { get; set; }
        public decimal MonthlyAmount   { get; set; }
        public decimal TotalPaid       { get; set; }
        public string  Status          { get; set; } = "";
        public string  StartDate       { get; set; } = "";
        public string? NextBillingDate { get; set; }
        public string? PendingUpgradePlan{ get; set; }
        public List<CorporateBillingDto> BillingHistory { get; set; } = new();
    }

    public class CorporateBillingDto
    {
        public int     Id            { get; set; }
        public decimal Amount        { get; set; }
        public string  Status        { get; set; } = "";
        public string  InvoiceNumber { get; set; } = "";
        public string  Period        { get; set; } = "";
        public string  BilledAt      { get; set; } = "";
        public string? PaidAt        { get; set; }
    }

    public class UpgradePlanDto
    {
        public int    NewPlanId     { get; set; }
        public string PaymentMethod { get; set; } = "Invoice";
    }
}
