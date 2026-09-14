namespace LovEat.API.DTOs
{
    // ── M182: Check-in Timeout Auto-Alert ────────────────────────────
    public class BookingTimeoutAlertDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public DateTime ExpectedEndTime { get; set; }
        public string AlertLevel { get; set; } = "";
        public DateTime? NudgeFiredAt { get; set; }
        public DateTime? EscalatedAt { get; set; }
        public int? RelatedSafetyIncidentId { get; set; }
    }

    // ── M183: Dedicated/Same-Chef Monthly Assignment ─────────────────
    public class SetPreferredChefRequestDto
    {
        public int? PreferredChefId { get; set; } // null clears it
        public bool AutoAssignSameChef { get; set; } = true;
        public string FallbackPolicy { get; set; } = "NotifyCustomer"; // NotifyCustomer / AutoAssignAlternate
    }

    // ── M184: Party Staff Add-on ──────────────────────────────────────
    public class AddEventStaffRequestDto
    {
        public int BookingId { get; set; }
        public string Role { get; set; } = "Waiter"; // Bartender / Waiter / Server
        public int Count { get; set; } = 1;
        public decimal RatePerPerson { get; set; }
    }

    public class EventStaffRequestDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string Role { get; set; } = "";
        public int Count { get; set; }
        public decimal RatePerPerson { get; set; }
        public string Status { get; set; } = "";
        public List<string> AssignedStaffNames { get; set; } = new();
    }

    public class AssignSupportStaffRequestDto
    {
        public int EventStaffRequestId { get; set; }
        public int StaffMemberId { get; set; }
    }
}
