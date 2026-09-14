using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M63: FSSAI License Tracking ───────────────────────────────
    public class FSSAILicense
    {
        [Key] public int Id { get; set; }
        public int    ChefId          { get; set; }
        public User?  Chef            { get; set; }
        public string LicenseNumber   { get; set; } = "";
        public string LicenseType     { get; set; } = "Basic"; // Basic / State / Central
        public string Status          { get; set; } = "Pending"; // Pending / Active / Expired / Rejected / UnderRenewal
        public string? BusinessName   { get; set; }
        public string? Address        { get; set; }
        public string? LicenseDocUrl  { get; set; }
        public DateTime IssueDate     { get; set; }
        public DateTime ExpiryDate    { get; set; }
        public DateTime? RenewalDate  { get; set; }
        public bool   RenewalAlertSent{ get; set; } = false;
        public string? RejectionReason{ get; set; }
        public DateTime SubmittedAt   { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt     { get; set; } = DateTime.UtcNow;
    }

    // ── M64: Video Consultations ───────────────────────────────────
    public class VideoConsultation
    {
        [Key] public int Id { get; set; }
        public int    CustomerId      { get; set; }
        public User?  Customer        { get; set; }
        public int    ChefId          { get; set; }
        public User?  Chef            { get; set; }
        public int?   BookingId       { get; set; }         // optional — pre-booking consult
        public string ConsultType     { get; set; } = "";  // PreBooking / MenuPlanning / DietConsult / CookingClass
        public string Status          { get; set; } = "Scheduled"; // Scheduled / InProgress / Completed / Cancelled / NoShow
        public DateTime ScheduledAt   { get; set; }
        public int    DurationMins    { get; set; } = 30;
        public decimal Fee            { get; set; } = 0;   // 0 = free pre-booking call
        public string? MeetingLink    { get; set; }        // Jitsi / Daily.co URL
        public string? MeetingId      { get; set; }
        public string? Notes          { get; set; }        // Chef's notes post-call
        public int?   Rating          { get; set; }
        public string? Feedback       { get; set; }
        public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
    }
}

namespace LovEat.API.DTOs
{
    // M63 DTOs
    public class FSSAILicenseDto
    {
        public int     Id              { get; set; }
        public string  LicenseNumber   { get; set; } = "";
        public string  LicenseType     { get; set; } = "";
        public string  Status          { get; set; } = "";
        public string? BusinessName    { get; set; }
        public string? LicenseDocUrl   { get; set; }
        public string  IssueDate       { get; set; } = "";
        public string  ExpiryDate      { get; set; } = "";
        public string? RenewalDate     { get; set; }
        public string? RejectionReason { get; set; }
        public int     DaysToExpiry    { get; set; }
        public bool    IsExpiringSoon  { get; set; }   // within 60 days
    }

    public class SubmitFSSAIDto
    {
        public string  LicenseNumber   { get; set; } = "";
        public string  LicenseType     { get; set; } = "Basic";
        public string? BusinessName    { get; set; }
        public string? Address         { get; set; }
        public string? LicenseDocUrl   { get; set; }
        public DateTime IssueDate      { get; set; }
        public DateTime ExpiryDate     { get; set; }
    }

    public class FSSAIAdminSummaryDto
    {
        public int    TotalLicenses    { get; set; }
        public int    Active           { get; set; }
        public int    ExpiringSoon     { get; set; }
        public int    Expired          { get; set; }
        public int    PendingReview    { get; set; }
        public List<FSSAILicenseChefDto> Licenses { get; set; } = new();
    }

    public class FSSAILicenseChefDto
    {
        public int    ChefId           { get; set; }
        public string ChefName         { get; set; } = "";
        public string Phone            { get; set; } = "";
        public FSSAILicenseDto? License{ get; set; }
    }

    // M64 DTOs
    public class VideoConsultationDto
    {
        public int     Id              { get; set; }
        public string  CustomerName    { get; set; } = "";
        public string  ChefName        { get; set; } = "";
        public string  ConsultType     { get; set; } = "";
        public string  Status          { get; set; } = "";
        public string  ScheduledAt     { get; set; } = "";
        public int     DurationMins    { get; set; }
        public decimal Fee             { get; set; }
        public string? MeetingLink     { get; set; }
        public string? MeetingId       { get; set; }
        public string? Notes           { get; set; }
        public int?    Rating          { get; set; }
        public string? Feedback        { get; set; }
        public string  CreatedAt       { get; set; } = "";
    }

    public class BookConsultationDto
    {
        public int      ChefId         { get; set; }
        public string   ConsultType    { get; set; } = "PreBooking";
        public DateTime ScheduledAt    { get; set; }
        public int      DurationMins   { get; set; } = 30;
        public int?     BookingId      { get; set; }
    }
}
