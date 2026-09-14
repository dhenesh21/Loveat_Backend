namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // Chef document verification (M13)
    // ══════════════════════════════════════════════════════════════
    public class SubmitVerificationRequestDto
    {
        public string DocumentType { get; set; } = ""; // IdProof / AddressProof / CookingCertificate / ProfilePhoto
        public string DocumentUrl { get; set; } = "";
    }

    public class VerificationDto
    {
        public int Id { get; set; }
        public int ChefId { get; set; }
        public string ChefName { get; set; } = "";
        public string DocumentType { get; set; } = "";
        public string DocumentUrl { get; set; } = "";
        public string Status { get; set; } = "";
        public string? RejectionReason { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ChefVerificationStatusDto
    {
        public bool IsFullyVerified { get; set; }
        public List<VerificationDto> Documents { get; set; } = new();
        public List<string> MissingDocumentTypes { get; set; } = new();
    }

    public class ReviewVerificationRequestDto
    {
        public int VerificationId { get; set; }
        public bool Approve { get; set; }
        public string? RejectionReason { get; set; }
    }
}
