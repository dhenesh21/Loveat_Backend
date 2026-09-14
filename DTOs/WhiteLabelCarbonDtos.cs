namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M79: White-label licensing
    // ══════════════════════════════════════════════════════════════
    public class CreateWhiteLabelClientRequestDto
    {
        public string BrandName { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string? Domain { get; set; }
        public string? PrimaryColor { get; set; }
        public string? LogoUrl { get; set; }
        public string PlanTier { get; set; } = "Basic";
    }

    public class WhiteLabelClientDto
    {
        public int Id { get; set; }
        public string BrandName { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string? Domain { get; set; }
        public string ApiKey { get; set; } = "";
        public string PlanTier { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalApiCalls { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // M80: Carbon footprint tracking
    // ══════════════════════════════════════════════════════════════
    public class EstimateBookingCarbonRequestDto
    {
        public int BookingId { get; set; }

        /// <summary>Rough ingredient list for the meal — e.g. ["chicken", "rice", "vegetables"]. Without a real recipe/inventory system, the customer or chef supplies this at estimate time.</summary>
        public List<string> Ingredients { get; set; } = new();

        /// <summary>Roughly how many kg of each ingredient per guest — defaults to a flat estimate if not supplied.</summary>
        public decimal? EstimatedKgPerGuest { get; set; }
    }

    public class CarbonEstimateDto
    {
        public int BookingId { get; set; }
        public decimal EstimatedCarbonKg { get; set; }
        public string ComparisonNote { get; set; } = ""; // e.g. "About the same as driving 12km"
        public Dictionary<string, decimal> BreakdownByIngredient { get; set; } = new();
    }

    public class CustomerCarbonSummaryDto
    {
        public int TotalBookingsTracked { get; set; }
        public decimal TotalCarbonKg { get; set; }
        public decimal AverageCarbonPerBookingKg { get; set; }
    }
}
