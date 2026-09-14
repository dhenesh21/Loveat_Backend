using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ══════════════════════════════════════════════════════════════
    // M71: Predictive churn intervention
    //
    // Honest framing, same as M70: this is a weighted heuristic (days since
    // last booking, booking frequency trend, recent cancellation rate), not
    // a trained churn-prediction model. A real model would need labeled
    // historical churn outcomes to train against, which this project
    // doesn't have yet. Swapping in a real model later only means changing
    // ChurnPredictionService's scoring function — this table's shape and
    // every caller of it stays the same.
    // ══════════════════════════════════════════════════════════════
    public class ChurnRiskScore
    {
        [Key] public int Id { get; set; }

        public int CustomerId { get; set; }
        public User? Customer { get; set; }

        public int RiskScore { get; set; } // 0-100

        [MaxLength(10)]
        public string RiskLevel { get; set; } = "Low"; // Low / Medium / High

        public int DaysSinceLastBooking { get; set; }
        public int TotalBookings { get; set; }
        public int BookingsLast90Days { get; set; }
        public int CancelledLast90Days { get; set; }

        /// <summary>JSON breakdown of what contributed to the score.</summary>
        public string? FactorsJson { get; set; }

        public bool InterventionSent { get; set; } = false;
        public DateTime? InterventionSentAt { get; set; }

        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }

    // ══════════════════════════════════════════════════════════════
    // M72: Voice ordering (Alexa/Google Assistant)
    //
    // This backend can't run an actual Alexa Skill or Google Action — those
    // require registering with Amazon/Google's respective developer consoles
    // and hosting a skill/action endpoint per their protocols. What this
    // provides is the piece that any such skill/action would call into: an
    // endpoint that takes an already-transcribed voice command (speech-to-text
    // happens on Amazon/Google's side before it ever reaches here) and maps
    // it to a LovEat action via simple keyword/pattern matching — not real
    // NLU. Every command is logged so a real NLU model could later be
    // trained against this log and swapped in without changing the endpoint
    // shape.
    // ══════════════════════════════════════════════════════════════
    public class VoiceCommandLog
    {
        [Key] public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [MaxLength(500)]
        public string RawTranscript { get; set; } = "";

        [MaxLength(30)]
        public string DetectedIntent { get; set; } = "Unknown"; // BookChef / CheckStatus / CancelBooking / Unknown

        public string? ParametersJson { get; set; }

        [MaxLength(500)]
        public string ResponseText { get; set; } = "";

        public bool ActionTaken { get; set; } = false;
        public int? RelatedBookingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
