namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M-extra: customer-initiated fraud report (distinct from batch 23's
    // automatic FraudDetectionService.AnalyzeBookingAsync — this is the
    // "Fraud report" screen a customer/chef fills in themselves)
    // ══════════════════════════════════════════════════════════════
    public class SubmitFraudReportRequestDto
    {
        public int? BookingId { get; set; }
        public string DetectionType { get; set; } = "UserReported";
        public string Description { get; set; } = "";
    }

    // ══════════════════════════════════════════════════════════════
    // M21: Fraud/safety heat map
    // ══════════════════════════════════════════════════════════════
    public class HeatMapPointDto
    {
        public string City { get; set; } = "";
        public string? Zone { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int RiskScore { get; set; }
        public int IncidentCount { get; set; }
        public int FraudFlagCount { get; set; }
    }
}
