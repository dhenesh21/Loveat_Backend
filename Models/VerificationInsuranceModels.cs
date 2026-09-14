using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M61: Background Verification ──────────────────────────────
    public class BackgroundVerification
    {
        [Key] public int Id { get; set; }
        public int    ChefId            { get; set; }
        public User?  Chef              { get; set; }
        public string VerificationType  { get; set; } = ""; // Police / Aadhaar / PAN / Criminal / Address
        public string Status            { get; set; } = "Pending"; // Pending / InProgress / Verified / Failed / Expired
        public string? DocumentNumber   { get; set; }
        public string? DocumentUrl      { get; set; }
        public string? VerifiedByAgency { get; set; }
        public string? RejectionReason  { get; set; }
        public string? VerificationRef  { get; set; } // External agency reference
        public DateTime? VerifiedAt     { get; set; }
        public DateTime? ExpiresAt      { get; set; } // Reverify every 2 years
        public DateTime  SubmittedAt    { get; set; } = DateTime.UtcNow;
        public DateTime  UpdatedAt      { get; set; } = DateTime.UtcNow;
    }

    // ── M62: Insurance ────────────────────────────────────────────
    public class InsurancePolicy
    {
        [Key] public int Id { get; set; }
        public int    UserId          { get; set; }
        public User?  User            { get; set; }
        public string PolicyType      { get; set; } = ""; // ChefLiability / CustomerFoodSafety / AccidentCover
        public string PolicyNumber    { get; set; } = "";
        public string Provider        { get; set; } = ""; // e.g. ICICI Lombard
        public decimal PremiumAmount  { get; set; }
        public string PremiumPeriod   { get; set; } = "Monthly"; // Monthly / Annual
        public decimal CoverageAmount { get; set; }
        public string Status          { get; set; } = "Active"; // Active / Expired / Cancelled / Claimed
        public DateTime StartDate     { get; set; } = DateTime.UtcNow;
        public DateTime EndDate       { get; set; } = DateTime.UtcNow.AddYears(1);
        public string? PolicyDocUrl   { get; set; }
        public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
    }

    public class InsuranceClaim
    {
        [Key] public int Id { get; set; }
        public int    PolicyId        { get; set; }
        public InsurancePolicy? Policy{ get; set; }
        public int?   BookingId       { get; set; }
        public string ClaimType       { get; set; } = ""; // FoodPoisoning / Injury / PropertyDamage / Other
        public string Description     { get; set; } = "";
        public decimal ClaimAmount    { get; set; }
        public string Status          { get; set; } = "Filed"; // Filed / UnderReview / Approved / Rejected / Paid
        public string? EvidenceUrl    { get; set; }
        public string? ClaimRef       { get; set; }
        public string? Resolution     { get; set; }
        public DateTime FiledAt       { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt   { get; set; }
    }
}

namespace LovEat.API.DTOs
{
    // M61 DTOs
    public class BackgroundVerificationDto
    {
        public int     Id                { get; set; }
        public string  VerificationType  { get; set; } = "";
        public string  Status            { get; set; } = "";
        public string? DocumentNumber    { get; set; }
        public string? DocumentUrl       { get; set; }
        public string? VerifiedByAgency  { get; set; }
        public string? RejectionReason   { get; set; }
        public string? VerificationRef   { get; set; }
        public string? VerifiedAt        { get; set; }
        public string? ExpiresAt         { get; set; }
        public string  SubmittedAt       { get; set; } = "";
    }

    public class SubmitVerificationDto
    {
        public string  VerificationType  { get; set; } = "";
        public string? DocumentNumber    { get; set; }
        public string? DocumentUrl       { get; set; }
    }

    public class ChefVerificationSummaryDto
    {
        public int     ChefId            { get; set; }
        public string  ChefName          { get; set; } = "";
        public bool    IsFullyVerified   { get; set; }
        public string  OverallStatus     { get; set; } = "";
        public List<BackgroundVerificationDto> Verifications { get; set; } = new();
    }

    // M62 DTOs
    public class InsurancePolicyDto
    {
        public int     Id               { get; set; }
        public string  PolicyType       { get; set; } = "";
        public string  PolicyNumber     { get; set; } = "";
        public string  Provider         { get; set; } = "";
        public decimal PremiumAmount    { get; set; }
        public string  PremiumPeriod    { get; set; } = "";
        public decimal CoverageAmount   { get; set; }
        public string  Status           { get; set; } = "";
        public string  StartDate        { get; set; } = "";
        public string  EndDate          { get; set; } = "";
        public string? PolicyDocUrl     { get; set; }
        public int     DaysRemaining    { get; set; }
    }

    public class InsuranceClaimDto
    {
        public int     Id               { get; set; }
        public string  ClaimType        { get; set; } = "";
        public string  Description      { get; set; } = "";
        public decimal ClaimAmount      { get; set; }
        public string  Status           { get; set; } = "";
        public string? EvidenceUrl      { get; set; }
        public string? ClaimRef         { get; set; }
        public string? Resolution       { get; set; }
        public string  FiledAt          { get; set; } = "";
        public string? ResolvedAt       { get; set; }
    }

    public class FileClaimDto
    {
        public int     PolicyId         { get; set; }
        public string  ClaimType        { get; set; } = "";
        public string  Description      { get; set; } = "";
        public decimal ClaimAmount      { get; set; }
        public int?    BookingId        { get; set; }
        public string? EvidenceUrl      { get; set; }
    }
}
