namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M16: Customer -> Chef reviews
    // (Chef -> Customer rating already covered by RateCustomerDto in
    // Models/CouponTrackingModels.cs — kept there since that's where the
    // existing CustomerRatingController already looks for it.)
    // ══════════════════════════════════════════════════════════════
    public class CreateReviewRequestDto
    {
        public int BookingId { get; set; }
        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
    }

    public class ReviewDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = "";
        public int ChefId { get; set; }
        public string ChefName { get; set; } = "";
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public string? ChefResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ChefRespondedAt { get; set; }
        // Model has always had this — never surfaced in the DTO or set
        // anywhere, so admin moderation had no way to see or act on it.
        public bool IsFlagged { get; set; }
    }

    public class ChefRespondToReviewRequestDto
    {
        public int ReviewId { get; set; }
        public string Response { get; set; } = "";
    }

    public class ChefReviewSummaryDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingBreakdown { get; set; } = new(); // {5: 40, 4: 10, ...}
        public List<ReviewDto> RecentReviews { get; set; } = new();
    }
}
