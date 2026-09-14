using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M53: CRM ──────────────────────────────────────────────────
    public class UserSegment
    {
        [Key] public int Id { get; set; }
        public string  Name        { get; set; } = "";
        public string  Description { get; set; } = "";
        public string  FilterJson  { get; set; } = "{}"; // criteria JSON
        public int     UserCount   { get; set; } = 0;
        public bool    IsActive    { get; set; } = true;
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt  { get; set; } = DateTime.UtcNow;
    }

    public class CrmNote
    {
        [Key] public int Id { get; set; }
        public int    UserId     { get; set; }
        public User?  User       { get; set; }
        public int    AdminId    { get; set; }
        public string Note       { get; set; } = "";
        public string NoteType   { get; set; } = "General"; // General / Warning / VIP / Churn Risk
        public DateTime CreatedAt{ get; set; } = DateTime.UtcNow;
    }
}

namespace LovEat.API.DTOs
{
    // M52 extended
    public class CampaignAnalyticsDto
    {
        public int    TotalCampaigns  { get; set; }
        public int    SentThisMonth   { get; set; }
        public long   TotalReach      { get; set; }
        public double AvgOpenRate     { get; set; }
        public double AvgClickRate    { get; set; }
        public List<CampaignPerformanceDto> TopPerforming { get; set; } = new();
        public List<MonthlyStatsDto>        MonthlyTrend  { get; set; } = new();
    }

    public class CampaignPerformanceDto
    {
        public string Title      { get; set; } = "";
        public int    Reach      { get; set; }
        public double OpenRate   { get; set; }
        public string Type       { get; set; } = "";
        public string SentAt     { get; set; } = "";
    }

    public class MonthlyStatsDto
    {
        public string Month      { get; set; } = "";
        public int    Campaigns  { get; set; }
        public long   Reach      { get; set; }
        public double AvgOpen    { get; set; }
    }

    // M53
    public class UserSegmentDto
    {
        public int    Id          { get; set; }
        public string Name        { get; set; } = "";
        public string Description { get; set; } = "";
        public int    UserCount   { get; set; }
        public bool   IsActive    { get; set; }
        public string CreatedAt   { get; set; } = "";
    }

    public class CrmUserDto
    {
        public int     UserId        { get; set; }
        public string  FullName      { get; set; } = "";
        public string  Phone         { get; set; } = "";
        public string  City          { get; set; } = "";
        public int     TotalBookings { get; set; }
        public decimal TotalSpend    { get; set; }
        public string  LastActive    { get; set; } = "";
        public string  JoinDate      { get; set; } = "";
        public string  Segment       { get; set; } = "";
        public string  LoyaltyTier   { get; set; } = "";
        public List<CrmNoteDto> Notes{ get; set; } = new();
    }

    public class CrmNoteDto
    {
        public int    Id        { get; set; }
        public string Note      { get; set; } = "";
        public string NoteType  { get; set; } = "";
        public string AdminName { get; set; } = "";
        public string CreatedAt { get; set; } = "";
    }

    public class CrmSummaryDto
    {
        public int     TotalUsers      { get; set; }
        public int     ActiveThisMonth { get; set; }
        public int     NewThisMonth    { get; set; }
        public int     ChurnRisk       { get; set; }
        public double  RetentionRate   { get; set; }
        public List<UserSegmentDto> Segments { get; set; } = new();
    }
}
