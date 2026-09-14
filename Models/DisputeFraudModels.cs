using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M45: Dispute Management ────────────────────────────────────
    public class Dispute
    {
        [Key] public int Id { get; set; }
        public string  DisputeNumber   { get; set; } = ""; // DSP-2024-00001
        public int     BookingId       { get; set; }
        public int     RaisedByUserId  { get; set; }
        public User?   RaisedBy        { get; set; }
        public int?    AgainstUserId   { get; set; }
        public string  DisputeType     { get; set; } = ""; // QualityIssue / Overcharge / NoShow / Damage / Other
        public string  Description     { get; set; } = "";
        public string  Status          { get; set; } = "Open"; // Open / InReview / Resolved / Closed / Escalated
        public string  Priority        { get; set; } = "Medium";
        public decimal? RefundAmount   { get; set; }
        public string? Resolution      { get; set; }
        public int?    ResolvedByAdmin { get; set; }
        public DateTime? ResolvedAt    { get; set; }
        public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt      { get; set; } = DateTime.UtcNow;
        public ICollection<DisputeEvidence> Evidences { get; set; } = new List<DisputeEvidence>();
        public ICollection<DisputeMessage>  Messages  { get; set; } = new List<DisputeMessage>();
    }

    public class DisputeEvidence
    {
        [Key] public int Id { get; set; }
        public int    DisputeId   { get; set; }
        public Dispute? Dispute   { get; set; }
        public int    UploadedBy  { get; set; }
        public string FileUrl     { get; set; } = "";
        public string FileType    { get; set; } = "Image"; // Image / Video / Document
        public string Description { get; set; } = "";
        public DateTime UploadedAt{ get; set; } = DateTime.UtcNow;
    }

    public class DisputeMessage
    {
        [Key] public int Id { get; set; }
        public int    DisputeId  { get; set; }
        public int    SenderId   { get; set; }
        public string SenderRole { get; set; } = ""; // Customer / Chef / Admin
        public string Message    { get; set; } = "";
        public bool   IsInternal { get; set; } = false;
        public DateTime CreatedAt{ get; set; } = DateTime.UtcNow;
    }

    // ── M46: Fraud Detection Advanced ─────────────────────────────
    public class FraudDetectionLog
    {
        [Key] public int Id { get; set; }
        public int?   UserId       { get; set; }
        public User?  User         { get; set; }
        public int?   BookingId    { get; set; }
        public string DetectionType{ get; set; } = ""; // FakeBooking / SuspiciousPayment / MultiAccount / BotActivity
        public string Severity     { get; set; } = "Medium";
        public decimal RiskScore   { get; set; }
        public string  Evidence    { get; set; } = "{}"; // JSON
        public string  Action      { get; set; } = "Flagged"; // Flagged / Blocked / Cleared
        public string? AdminNote   { get; set; }
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt{ get; set; }
    }
}

namespace LovEat.API.DTOs
{
    // M45 DTOs
    public class CreateDisputeDto
    {
        public int    BookingId    { get; set; }
        public string DisputeType  { get; set; } = "";
        public string Description  { get; set; } = "";
        public string Priority     { get; set; } = "Medium";
    }

    public class DisputeDto
    {
        public int     Id             { get; set; }
        public string  DisputeNumber  { get; set; } = "";
        public int     BookingId      { get; set; }
        public string  RaisedByName   { get; set; } = "";
        public string  AgainstName    { get; set; } = "";
        public string  DisputeType    { get; set; } = "";
        public string  Description    { get; set; } = "";
        public string  Status         { get; set; } = "";
        public string  Priority       { get; set; } = "";
        public decimal? RefundAmount  { get; set; }
        public string? Resolution     { get; set; }
        public string  CreatedAt      { get; set; } = "";
        public string  UpdatedAt      { get; set; } = "";
        public List<DisputeMessageDto> Messages  { get; set; } = new();
        public List<DisputeEvidenceDto> Evidences{ get; set; } = new();
    }

    public class DisputeMessageDto
    {
        public int    Id         { get; set; }
        public string SenderRole { get; set; } = "";
        public string Message    { get; set; } = "";
        public string CreatedAt  { get; set; } = "";
    }

    public class DisputeEvidenceDto
    {
        public int    Id          { get; set; }
        public string FileUrl     { get; set; } = "";
        public string FileType    { get; set; } = "";
        public string Description { get; set; } = "";
        public string UploadedAt  { get; set; } = "";
    }

    public class ResolveDisputeDto
    {
        public string  Resolution    { get; set; } = "";
        public string  Status        { get; set; } = "Resolved";
        public decimal? RefundAmount { get; set; }
    }

    // M46 DTOs
    public class FraudLogDto
    {
        public int     Id             { get; set; }
        public string  UserName       { get; set; } = "";
        public int?    BookingId      { get; set; }
        public string  DetectionType  { get; set; } = "";
        public string  Severity       { get; set; } = "";
        public decimal RiskScore      { get; set; }
        public string  Action         { get; set; } = "";
        public string? AdminNote      { get; set; }
        public string  DetectedAt     { get; set; } = "";
    }

    public class FraudSummaryDto
    {
        public int    TotalFlagged      { get; set; }
        public int    TotalBlocked      { get; set; }
        public int    PendingReview     { get; set; }
        public decimal AvgRiskScore     { get; set; }
        public List<FraudLogDto> Recent { get; set; } = new();
    }
}
