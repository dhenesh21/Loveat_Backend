namespace LovEat.API.DTOs
{
    // ── M153: Customer Experience — Split Bill ───────────────────────
    public class SplitParticipantRequestDto { public int CustomerId { get; set; } public decimal? ShareAmount { get; set; } }

    public class CreateBillSplitRequestDto
    {
        public int BookingId { get; set; }
        public int InitiatedByCustomerId { get; set; }
        public string SplitMethod { get; set; } = "Equal";
        public List<SplitParticipantRequestDto> Participants { get; set; } = new();
    }

    public class BillSplitParticipantDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal ShareAmount { get; set; }
        public string PaymentStatus { get; set; } = "";
    }

    public class BillSplitDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public decimal TotalAmount { get; set; }
        public string SplitMethod { get; set; } = "";
        public string Status { get; set; } = "";
        public List<BillSplitParticipantDto> Participants { get; set; } = new();
    }

    public class MarkSplitPaidRequestDto { public int ParticipantId { get; set; } }

    // ── M154: Customer Experience — Meal Scheduling Assistant ────────
    public class CreateMealScheduleRequestDto
    {
        public int CustomerId { get; set; }
        public int ChefId { get; set; }
        public string ScheduleName { get; set; } = "";
        public int DayOfWeek { get; set; }
        public string PreferredTime { get; set; } = "12:00"; // HH:mm
        public decimal EstimatedAmount { get; set; }
    }

    public class MealScheduleDto
    {
        public int Id { get; set; }
        public string ScheduleName { get; set; } = "";
        public string? ChefName { get; set; }
        public int DayOfWeek { get; set; }
        public string PreferredTime { get; set; } = "";
        public decimal EstimatedAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastGeneratedAt { get; set; }
    }

    public class ToggleMealScheduleRequestDto { public int ScheduleId { get; set; } public bool IsActive { get; set; } }
    public class GenerateDueBookingsResultDto { public int GeneratedCount { get; set; } }

    // ── M155: Customer Experience — Accessibility Settings ───────────
    public class UpdateAccessibilitySettingsRequestDto
    {
        public int CustomerId { get; set; }
        public string FontSize { get; set; } = "Medium";
        public bool HighContrastMode { get; set; }
        public bool ScreenReaderOptimized { get; set; }
        public bool ReduceMotion { get; set; }
        public bool VoiceGuidanceEnabled { get; set; }
    }

    public class AccessibilitySettingsDto
    {
        public string FontSize { get; set; } = "";
        public bool HighContrastMode { get; set; }
        public bool ScreenReaderOptimized { get; set; }
        public bool ReduceMotion { get; set; }
        public bool VoiceGuidanceEnabled { get; set; }
    }

    // ── M156: Customer Experience — FAQ / Help Center ────────────────
    public class FaqCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string IconName { get; set; } = "";
        public int ArticleCount { get; set; }
    }

    public class FaqArticleDto
    {
        public int Id { get; set; }
        public string Question { get; set; } = "";
        public string Answer { get; set; } = "";
        public int ViewCount { get; set; }
        public int HelpfulCount { get; set; }
        public int NotHelpfulCount { get; set; }
    }

    public class CreateFaqArticleRequestDto
    {
        public int FaqCategoryId { get; set; }
        public string Question { get; set; } = "";
        public string Answer { get; set; } = "";
        public string Audience { get; set; } = "All";
    }

    public class RateFaqArticleRequestDto { public int ArticleId { get; set; } public bool WasHelpful { get; set; } }
    public class SearchFaqRequestDto { public string Query { get; set; } = ""; public string Audience { get; set; } = "All"; }
}
