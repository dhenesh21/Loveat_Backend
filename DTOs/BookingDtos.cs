namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // Search (M5/M6)
    // ══════════════════════════════════════════════════════════════
    public class SearchChefsRequestDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public decimal RadiusKm { get; set; } = 15;
        public string? Cuisine { get; set; }
        public decimal? MinRating { get; set; }
        public decimal? MaxHourlyRate { get; set; }
        public bool AvailableNow { get; set; } = false;
    }

    public class ChefSearchResultDto
    {
        public int ChefProfileId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string? ProfileImageUrl { get; set; }
        public List<string> Cuisines { get; set; } = new();
        public decimal HourlyRate { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public double DistanceKm { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsVerified { get; set; }
        public string? City { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // Availability (M25, M10)
    // ══════════════════════════════════════════════════════════════
    public class SetGeneralAvailabilityRequestDto
    {
        public bool IsAvailable { get; set; }
    }

    public class SetEmergencyAvailabilityRequestDto
    {
        public bool IsAvailableNow { get; set; }
        public int AvailableForMinutes { get; set; } = 60;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int ServiceRadiusKm { get; set; } = 10;
    }

    public class ChefAvailabilityStatusDto
    {
        public bool GeneralAvailability { get; set; }
        public bool EmergencyAvailableNow { get; set; }
        public DateTime? EmergencyAvailableUntil { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // Booking (M7-M9)
    // ══════════════════════════════════════════════════════════════
    public class CreateBookingRequestDto
    {
        public int ChefId { get; set; } // User.Id of the chef
        public string BookingType { get; set; } = "Instant"; // Instant / Scheduled / Event / Emergency
        public string? Cuisine { get; set; }
        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; } = 120;
        public int GuestCount { get; set; } = 1;
        public string Address { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Notes { get; set; }
        public string? CouponCode { get; set; }
    }

    public class UpdateBookingStatusRequestDto
    {
        public string? CancellationReason { get; set; }
    }

    public class BookingDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public int ChefId { get; set; }
        public string ChefName { get; set; } = "";
        public string BookingType { get; set; } = "";
        public string Status { get; set; } = "";
        public string? Cuisine { get; set; }
        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; }
        public int GuestCount { get; set; }
        public string Address { get; set; } = "";
        public decimal BaseAmount { get; set; }
        public decimal SurchargeAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "";
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // Location (geocode stub + service-area check)
    // ══════════════════════════════════════════════════════════════
    public class GeocodeAddressRequestDto
    {
        public string Address { get; set; } = "";
    }

    public class GeocodeResultDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string FormattedAddress { get; set; } = "";
        public bool IsStub { get; set; } = true; // flips to false once G4 (Google Maps) is wired
    }

    public class ServiceAreaCheckResultDto
    {
        public bool IsCovered { get; set; }
        public string? NearestCity { get; set; }
        public double DistanceToNearestCityKm { get; set; }
    }

    public class AdminCityAnalyticsDto
    {
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public int Chefs { get; set; }
        public int Bookings { get; set; }
        public bool Active { get; set; }
        public bool Launching { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // Wallet (M15)
    // ══════════════════════════════════════════════════════════════
    public class WalletDto
    {
        public decimal Balance { get; set; }
        public List<WalletTransactionDto> RecentTransactions { get; set; } = new();
    }

    public class WalletTransactionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Reason { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class TopUpWalletRequestDto
    {
        public decimal Amount { get; set; }
    }
}
