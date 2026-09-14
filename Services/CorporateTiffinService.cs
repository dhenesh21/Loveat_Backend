using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>M12: recurring corporate tiffin subscriptions — daily meals for a company rather than a one-off booking. Distinct from batch 29's CorporatePlan (fixed-tier pricing catalog); this is the actual live subscription a company has signed up for.</summary>
    public class CorporateTiffinService
    {
        private readonly AppDbContext _db;
        public CorporateTiffinService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, CorporateTiffinDto? Data)> CreateAsync(int companyUserId, CreateCorporateTiffinRequestDto req)
        {
            if (req.MealsPerDay <= 0) return (false, "Meals per day must be positive.", null);

            var monthlyAmount = req.PricePerMeal * req.MealsPerDay * ApproxWeekdaysPerMonth(req.RecurringDays);

            var booking = new CorporateTiffinBooking
            {
                CompanyUserId = companyUserId,
                PrimaryChefId = req.PrimaryChefId,
                CompanyName = req.CompanyName,
                MealsPerDay = req.MealsPerDay,
                MealType = req.MealType,
                RecurringDays = req.RecurringDays,
                PricePerMeal = req.PricePerMeal,
                MonthlyAmount = monthlyAmount,
                StartDate = req.StartDate,
                Status = "Active",
            };

            _db.CorporateTiffinBookings.Add(booking);
            await _db.SaveChangesAsync();

            return (true, "Corporate tiffin plan created.", await ToDtoAsync(booking.Id));
        }

        public async Task<List<CorporateTiffinDto>> GetMineAsync(int companyUserId)
        {
            var bookings = await _db.CorporateTiffinBookings
                .Where(b => b.CompanyUserId == companyUserId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var result = new List<CorporateTiffinDto>();
            foreach (var b in bookings)
                result.Add(await ToDtoAsync(b.Id) ?? new CorporateTiffinDto { Id = b.Id });

            return result;
        }

        /// <summary>Admin: all corporate tiffin subscriptions across every company</summary>
        public async Task<List<CorporateTiffinDto>> GetAllAsync()
        {
            var bookings = await _db.CorporateTiffinBookings
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var result = new List<CorporateTiffinDto>();
            foreach (var b in bookings)
                result.Add(await ToDtoAsync(b.Id) ?? new CorporateTiffinDto { Id = b.Id });

            return result;
        }

        public async Task<(bool Success, string Message)> SetStatusAsync(int companyUserId, int bookingId, string status)
        {
            var booking = await _db.CorporateTiffinBookings.FirstOrDefaultAsync(b => b.Id == bookingId && b.CompanyUserId == companyUserId);
            if (booking == null) return (false, "Corporate tiffin plan not found.");

            booking.Status = status;
            if (status == "Cancelled") booking.EndDate = DateTime.UtcNow;
            booking.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return (true, $"Plan marked {status}.");
        }

        private static int ApproxWeekdaysPerMonth(string recurringDays)
        {
            var count = recurringDays.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
            return count switch
            {
                <= 0 => 22,
                _ => (int)Math.Round(count * 4.33),
            };
        }

        private async Task<CorporateTiffinDto?> ToDtoAsync(int id)
        {
            var b = await _db.CorporateTiffinBookings.FirstOrDefaultAsync(x => x.Id == id);
            if (b == null) return null;

            var chef = b.PrimaryChefId.HasValue ? await _db.Users.FindAsync(b.PrimaryChefId.Value) : null;

            return new CorporateTiffinDto
            {
                Id = b.Id,
                CompanyName = b.CompanyName,
                MealsPerDay = b.MealsPerDay,
                MealType = b.MealType,
                RecurringDays = b.RecurringDays,
                PricePerMeal = b.PricePerMeal,
                MonthlyAmount = b.MonthlyAmount,
                Status = b.Status,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                PrimaryChefName = chef?.FullName,
            };
        }
    }
}
