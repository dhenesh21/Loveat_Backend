namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M71: Churn prediction
    // ══════════════════════════════════════════════════════════════
    public class ChurnRiskDto
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public string? CustomerPhone { get; set; }
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = "";
        public int DaysSinceLastBooking { get; set; }
        public int TotalBookings { get; set; }
        public int BookingsLast90Days { get; set; }
        public bool InterventionSent { get; set; }
        public DateTime ComputedAt { get; set; }
    }

    public class ChurnInterventionResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string? CouponCode { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // M72: Voice ordering
    // ══════════════════════════════════════════════════════════════
    public class VoiceCommandRequestDto
    {
        public string Transcript { get; set; } = "";

        /// <summary>Needed only for the "book a chef" intent — the voice transcript alone carries no location. Supplied by the client app/skill from device GPS.</summary>
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Address { get; set; }
    }

    public class VoiceCommandResponseDto
    {
        public string DetectedIntent { get; set; } = "";
        public string ResponseText { get; set; } = "";
        public bool ActionTaken { get; set; }
        public int? RelatedBookingId { get; set; }
    }
}
