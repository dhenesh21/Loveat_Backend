using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ══════════════════════════════════════════════════════════════
    // Phase 18/19 — Safety Suite Phase 2
    // M182: Check-in Timeout Auto-Alert
    // M183: Dedicated/Same-Chef Monthly Assignment (extends Subscription)
    // M184: Party Staff Add-on (Bartender/Waiter/Server)
    // ══════════════════════════════════════════════════════════════

    // ── M182: Check-in Timeout Auto-Alert ───────────────────────────
    /// <summary>
    /// One row per booking, created when a booking is Accepted. A polling
    /// background service (BookingTimeoutMonitorService) scans for bookings
    /// still open past ExpectedEndTime + GraceMinutes and escalates in two
    /// steps: Nudge (push to both parties) then Escalated (SafetyIncident +
    /// trusted-contact notify via M180's service).
    /// </summary>
    public class BookingTimeoutAlert
    {
        [Key] public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public DateTime ExpectedEndTime { get; set; }
        public int GraceMinutes { get; set; } = 30;

        [MaxLength(20)]
        public string AlertLevel { get; set; } = "None"; // None / Nudge / Escalated

        public DateTime? NudgeFiredAt { get; set; }
        public DateTime? EscalatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public int? RelatedSafetyIncidentId { get; set; }
    }

    // ── M184: Party Staff Add-on ─────────────────────────────────────
    /// <summary>
    /// Only valid where the parent Booking.BookingType == "Event". Lets a
    /// customer request supporting staff (bartender/waiter/server) alongside
    /// the chef for an event booking. Fulfilled from the chef's own
    /// StaffMember pool (M141) — StaffMember.Role is extended to include
    /// these three values.
    /// </summary>
    public class EventStaffRequest
    {
        [Key] public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        [MaxLength(20)]
        public string Role { get; set; } = "Waiter"; // Bartender / Waiter / Server

        public int Count { get; set; } = 1;
        public decimal RatePerPerson { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Requested"; // Requested / Assigned / Confirmed / Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SupportStaffAssignment
    {
        [Key] public int Id { get; set; }

        public int EventStaffRequestId { get; set; }
        public EventStaffRequest? EventStaffRequest { get; set; }

        public int StaffMemberId { get; set; }
        public StaffMember? StaffMember { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "Assigned"; // Assigned / Confirmed / NoShow / Completed
    }
}
