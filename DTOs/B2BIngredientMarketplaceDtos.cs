namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M81: B2B API
    // ══════════════════════════════════════════════════════════════
    public class CreateB2BPartnerRequestDto
    {
        public string CompanyName { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public decimal CommissionRate { get; set; } = 5;
    }

    public class B2BPartnerDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public decimal CommissionRate { get; set; }
        public bool IsActive { get; set; }
        public int TotalBookings { get; set; }
    }

    public class CreateB2BBookingRequestDto
    {
        public string ApiKey { get; set; } = "";
        public string GuestName { get; set; } = "";
        public string GuestPhone { get; set; } = "";
        public int ChefId { get; set; }
        public string? Cuisine { get; set; }
        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; } = 120;
        public int GuestCount { get; set; } = 1;
        public string Address { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class B2BBookingResultDto
    {
        public int RequestId { get; set; }
        public int? BookingId { get; set; }
        public string Status { get; set; } = "";
        public decimal? TotalAmount { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // M82: Chef-to-chef ingredient marketplace
    // ══════════════════════════════════════════════════════════════
    public class CreateIngredientListingRequestDto
    {
        public string IngredientName { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "kg";
        public decimal Price { get; set; } = 0;
        public string ListingType { get; set; } = "Sell";
        public string? Description { get; set; }
    }

    public class IngredientListingDto
    {
        public int Id { get; set; }
        public int ChefId { get; set; }
        public string ChefName { get; set; } = "";
        public string? ChefCity { get; set; }
        public string IngredientName { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "";
        public decimal Price { get; set; }
        public string ListingType { get; set; } = "";
        public string? Description { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class RequestIngredientSwapRequestDto
    {
        public string? OfferedItem { get; set; }
        public string? Message { get; set; }
    }

    public class IngredientSwapRequestDto
    {
        public int Id { get; set; }
        public int ListingId { get; set; }
        public string IngredientName { get; set; } = "";
        public string RequestedByName { get; set; } = "";
        public string? OfferedItem { get; set; }
        public string Status { get; set; } = "";
        public string? Message { get; set; }
    }
}
