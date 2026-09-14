namespace LovEat.API.DTOs
{
    // ── Support DTOs ───────────────────────────────────────────────
    public class CreateTicketDto
    {
        public string Subject     { get; set; } = "";
        public string Category    { get; set; } = "General";
        public string Priority    { get; set; } = "Medium";
        public string Description { get; set; } = "";
        public int?   BookingId   { get; set; }
    }

    public class TicketDto
    {
        public int    Id           { get; set; }
        public string TicketNumber { get; set; } = "";
        public string Subject      { get; set; } = "";
        public string Category     { get; set; } = "";
        public string Priority     { get; set; } = "";
        public string Status       { get; set; } = "";
        public string Description  { get; set; } = "";
        public string? Resolution  { get; set; }
        public int?   Rating       { get; set; }
        public string CreatedAt    { get; set; } = "";
        public string UpdatedAt    { get; set; } = "";
        public List<SupportMessageDto> Messages { get; set; } = new();
    }

    public class SupportMessageDto
    {
        public int    Id         { get; set; }
        public int    SenderId   { get; set; }
        public string SenderRole { get; set; } = "";
        public string Message    { get; set; } = "";
        public string CreatedAt  { get; set; } = "";
    }

    public class ReplyTicketDto
    {
        public int    TicketId  { get; set; }
        public string Message   { get; set; } = "";
        public bool   IsInternal{ get; set; } = false;
    }

    public class UpdateTicketStatusDto
    {
        public string Status     { get; set; } = "";
        public string? Resolution{ get; set; }
        public int?   Rating     { get; set; }
    }

    public class FaqDto
    {
        public int    Id       { get; set; }
        public string Question { get; set; } = "";
        public string Answer   { get; set; } = "";
        public string Category { get; set; } = "";
        public string Audience { get; set; } = "";
        public int    HelpfulCount { get; set; }
    }

    public class AdminSupportStatsDto
    {
        public int OpenTickets      { get; set; }
        public int InProgressTickets{ get; set; }
        public int ResolvedToday    { get; set; }
        public double AvgResolutionHours { get; set; }
        public double AvgRating     { get; set; }
        public List<TicketDto> RecentTickets { get; set; } = new();
    }

    // ── Campaign DTOs ──────────────────────────────────────────────
    public class CreateCampaignDto
    {
        public string  Title        { get; set; } = "";
        public string  Body         { get; set; } = "";
        public string  Target       { get; set; } = "All";
        public string? TargetCity   { get; set; }
        public string  Type         { get; set; } = "Promo";
        public DateTime? ScheduledAt{ get; set; }
        public string? DeepLink     { get; set; }
        public string? ImageUrl     { get; set; }
    }

    public class CampaignDto
    {
        public int     Id             { get; set; }
        public string  Title          { get; set; } = "";
        public string  Body           { get; set; } = "";
        public string  Target         { get; set; } = "";
        public string? TargetCity     { get; set; }
        public string  Type           { get; set; } = "";
        public string  Status         { get; set; } = "";
        public string? ScheduledAt    { get; set; }
        public string? SentAt         { get; set; }
        public int     TotalReach     { get; set; }
        public int     DeliveredCount { get; set; }
        public int     OpenedCount    { get; set; }
        public double  OpenRate       { get; set; }
        public string? DeepLink       { get; set; }
        public string  CreatedAt      { get; set; } = "";
    }

    public class CampaignStatsDto
    {
        public int    TotalCampaigns  { get; set; }
        public int    SentThisMonth   { get; set; }
        public long   TotalReach      { get; set; }
        public double AvgOpenRate     { get; set; }
        public List<CampaignDto> Recent { get; set; } = new();
    }
}
