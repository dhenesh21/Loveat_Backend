namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M69: Group bookings
    // ══════════════════════════════════════════════════════════════
    public class CreateGroupBookingRequestDto
    {
        public int TotalGuestCount { get; set; }
        public string? Cuisine { get; set; }
        public DateTime ScheduledAt { get; set; }
        public int DurationMinutes { get; set; } = 120;
        public string Address { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        /// <summary>How many guests one chef can comfortably serve — the group is split into chunks of this size. Defaults to 15.</summary>
        public int MaxGuestsPerChef { get; set; } = 15;
    }

    public class GroupBookingSplitDto
    {
        public int BookingId { get; set; }
        public int ChefId { get; set; }
        public string ChefName { get; set; } = "";
        public int GuestCount { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = ""; // mirrors the underlying Booking's status
    }

    public class GroupBookingDto
    {
        public int Id { get; set; }
        public int TotalGuestCount { get; set; }
        public string? Cuisine { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Address { get; set; } = "";
        public string Status { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public List<GroupBookingSplitDto> Splits { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // M70: Chef-customer matching
    // ══════════════════════════════════════════════════════════════
    public class ChefMatchDto
    {
        public int ChefProfileId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string? ProfileImageUrl { get; set; }
        public List<string> Cuisines { get; set; } = new();
        public decimal HourlyRate { get; set; }
        public decimal AverageRating { get; set; }
        public decimal MatchScore { get; set; }
        public List<string> Reasons { get; set; } = new(); // e.g. "You've booked this chef 3 times before"
    }
}
