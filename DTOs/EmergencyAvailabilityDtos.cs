namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M10: Emergency cook booking convenience flow
    // (Chef-side "mark myself available now" DTOs live in BookingDtos.cs
    // alongside AvailabilityService, since that's where they're consumed.
    // These are the customer-side "book me someone right now" DTOs.)
    // ══════════════════════════════════════════════════════════════
    public class BookEmergencyChefRequestDto
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string Address { get; set; } = "";
        public string? Cuisine { get; set; }
        public int GuestCount { get; set; } = 1;
        public string? Notes { get; set; }
    }

    // ── Admin: live emergency monitor ────────────────────────────
    public class AdminEmergencyRequestDto
    {
        public int Id { get; set; }
        public string Customer { get; set; } = "";
        public string? Chef { get; set; }
        public string RequestedAt { get; set; } = "";
        public string Status { get; set; } = "";
        public string Address { get; set; } = "";
    }

    public class AdminAvailableChefDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? City { get; set; }
        public string AvailableUntil { get; set; } = "";
    }

    public class AdminEmergencyOverviewDto
    {
        public List<AdminEmergencyRequestDto> ActiveRequests { get; set; } = new();
        public List<AdminAvailableChefDto> AvailableChefs { get; set; } = new();
    }
}
