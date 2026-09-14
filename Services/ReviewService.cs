using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>M16: customer reviews of a chef, one per completed booking.</summary>
    public class ReviewService
    {
        private readonly AppDbContext _db;
        public ReviewService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, ReviewDto? Data)> CreateAsync(int customerId, CreateReviewRequestDto req)
        {
            if (req.Rating is < 1 or > 5) return (false, "Rating must be between 1 and 5.", null);

            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == req.BookingId && b.CustomerId == customerId);
            if (booking == null) return (false, "Booking not found.", null);
            if (booking.Status != "Completed") return (false, "You can only review a completed booking.", null);

            var existing = await _db.Reviews.AnyAsync(r => r.BookingId == req.BookingId);
            if (existing) return (false, "You've already reviewed this booking.", null);

            var review = new Review
            {
                BookingId = req.BookingId,
                CustomerId = customerId,
                ChefId = booking.ChefId,
                Rating = req.Rating,
                Comment = req.Comment,
            };
            _db.Reviews.Add(review);

            // Keep ChefProfile's aggregate rating in sync rather than computing it on every read.
            var profile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == booking.ChefId);
            if (profile != null)
            {
                var newTotal = profile.TotalReviews + 1;
                profile.AverageRating = Math.Round(((profile.AverageRating * profile.TotalReviews) + req.Rating) / newTotal, 2);
                profile.TotalReviews = newTotal;
            }

            await _db.SaveChangesAsync();

            return (true, "Review submitted.", await ToDtoAsync(review.Id));
        }

        public async Task<(bool Success, string Message)> ChefRespondAsync(int chefUserId, ChefRespondToReviewRequestDto req)
        {
            var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == req.ReviewId && r.ChefId == chefUserId);
            if (review == null) return (false, "Review not found.");

            review.ChefResponse = req.Response;
            review.ChefRespondedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return (true, "Response posted.");
        }

        public async Task<ChefReviewSummaryDto> GetChefReviewSummaryAsync(int chefUserId, int recentCount = 10)
        {
            var reviews = await _db.Reviews
                .Where(r => r.ChefId == chefUserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var breakdown = new Dictionary<int, int> { { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 } };
            foreach (var r in reviews)
                if (breakdown.ContainsKey(r.Rating)) breakdown[r.Rating]++;

            var recent = new List<ReviewDto>();
            foreach (var r in reviews.Take(recentCount))
                recent.Add(await ToDtoAsync(r.Id) ?? new ReviewDto { Id = r.Id });

            return new ChefReviewSummaryDto
            {
                AverageRating = reviews.Count == 0 ? 0 : Math.Round((decimal)reviews.Average(r => r.Rating), 2),
                TotalReviews = reviews.Count,
                RatingBreakdown = breakdown,
                RecentReviews = recent,
            };
        }

        private async Task<ReviewDto?> ToDtoAsync(int reviewId)
        {
            var r = await _db.Reviews.FirstOrDefaultAsync(x => x.Id == reviewId);
            if (r == null) return null;

            var customer = await _db.Users.FindAsync(r.CustomerId);
            var chef = await _db.Users.FindAsync(r.ChefId);

            return new ReviewDto
            {
                Id = r.Id,
                BookingId = r.BookingId,
                CustomerId = r.CustomerId,
                CustomerName = customer?.FullName ?? "",
                ChefId = r.ChefId,
                ChefName = chef?.FullName ?? "",
                Rating = r.Rating,
                Comment = r.Comment,
                ChefResponse = r.ChefResponse,
                CreatedAt = r.CreatedAt,
                ChefRespondedAt = r.ChefRespondedAt,
                IsFlagged = r.IsFlagged,
            };
        }

        // ── Admin moderation ────────────────────────────────────────
        public async Task<List<ReviewDto>> GetAllForAdminAsync(bool onlyFlagged = false)
        {
            var query = _db.Reviews.AsQueryable();
            if (onlyFlagged) query = query.Where(r => r.IsFlagged);
            var ids = await query.OrderByDescending(r => r.CreatedAt).Select(r => r.Id).ToListAsync();

            var result = new List<ReviewDto>();
            foreach (var id in ids)
                result.Add(await ToDtoAsync(id) ?? new ReviewDto { Id = id });
            return result;
        }

        public async Task<(bool Success, string Message)> SetFlaggedAsync(int reviewId, bool flagged)
        {
            var review = await _db.Reviews.FindAsync(reviewId);
            if (review == null) return (false, "Review not found.");
            review.IsFlagged = flagged;
            await _db.SaveChangesAsync();
            return (true, flagged ? "Review flagged." : "Review unflagged.");
        }

        public async Task<(bool Success, string Message)> AdminDeleteAsync(int reviewId)
        {
            var review = await _db.Reviews.FindAsync(reviewId);
            if (review == null) return (false, "Review not found.");

            _db.Reviews.Remove(review);

            // Keep ChefProfile's cached aggregate in sync with the removal,
            // the same way CreateAsync keeps it in sync on insert.
            var profile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == review.ChefId);
            if (profile != null && profile.TotalReviews > 0)
            {
                var newTotal = profile.TotalReviews - 1;
                profile.AverageRating = newTotal == 0 ? 0 : Math.Round(((profile.AverageRating * profile.TotalReviews) - review.Rating) / newTotal, 2);
                profile.TotalReviews = newTotal;
            }

            await _db.SaveChangesAsync();
            return (true, "Review removed.");
        }
    }
}
