using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>
    /// M13: basic document verification during chef onboarding (ID proof,
    /// address proof, cooking certificate photos). This is distinct from the
    /// deeper BackgroundVerification model (batch 31, M61) which covers
    /// third-party police/background-check API integration — that one is a
    /// heavier, later-stage check; this one gates whether a chef can accept
    /// their first booking at all.
    /// </summary>
    public class ChefVerification
    {
        [Key] public int Id { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        [MaxLength(30)]
        public string DocumentType { get; set; } = ""; // IdProof / AddressProof / CookingCertificate / ProfilePhoto

        [Required]
        public string DocumentUrl { get; set; } = "";

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected

        [MaxLength(300)]
        public string? RejectionReason { get; set; }

        public int? ReviewedByAdminId { get; set; }
        public DateTime? ReviewedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
