using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>M19: customer weekly meal subscriptions. Plan catalog is a fixed list for now rather than an admin-editable table — promoting these to real DB-backed plans is a reasonable follow-up once product settles on the actual tiers.</summary>
    public class SubscriptionService
    {
        private readonly AppDbContext _db;
        public SubscriptionService(AppDbContext db) => _db = db;

        private static readonly List<SubscriptionPlanDto> Plans = new()
        {
            new SubscriptionPlanDto { PlanName = "Basic", MealsPerWeek = 7, PricePerMonth = 2800, Features = new() { "1 meal/day", "Fixed chef", "Cancel anytime" } },
            new SubscriptionPlanDto { PlanName = "Standard", MealsPerWeek = 14, PricePerMonth = 5200, Features = new() { "2 meals/day", "Fixed chef", "Priority support" } },
            new SubscriptionPlanDto { PlanName = "Premium", MealsPerWeek = 21, PricePerMonth = 7500, Features = new() { "3 meals/day", "Choice of chefs", "Priority support", "Free rescheduling" } },
        };

        public List<SubscriptionPlanDto> GetPlans() => Plans;

        public async Task<(bool Success, string Message, SubscriptionDto? Data)> SubscribeAsync(int userId, CreateSubscriptionRequestDto req)
        {
            var plan = Plans.FirstOrDefault(p => p.PlanName.Equals(req.PlanName, StringComparison.OrdinalIgnoreCase));
            if (plan == null) return (false, "Unknown plan.", null);

            var existing = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "Active");
            if (existing != null) return (false, "You already have an active subscription. Cancel it first to switch plans.", null);

            var sub = new Subscription
            {
                UserId = userId,
                PlanName = plan.PlanName,
                MealsPerWeek = plan.MealsPerWeek,
                PricePerMonth = plan.PricePerMonth,
                Status = "Active",
            };
            _db.Subscriptions.Add(sub);
            await _db.SaveChangesAsync();

            return (true, "Subscribed.", ToDto(sub));
        }

        public async Task<SubscriptionDto?> GetMineAsync(int userId)
        {
            var sub = await _db.Subscriptions
                .Include(s => s.PreferredChef)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
            return sub == null ? null : ToDto(sub);
        }

        public async Task<(bool Success, string Message)> PauseAsync(int userId)
        {
            var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "Active");
            if (sub == null) return (false, "No active subscription found.");

            sub.Status = "Paused";
            await _db.SaveChangesAsync();
            return (true, "Subscription paused.");
        }

        public async Task<(bool Success, string Message)> ResumeAsync(int userId)
        {
            var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "Paused");
            if (sub == null) return (false, "No paused subscription found.");

            sub.Status = "Active";
            await _db.SaveChangesAsync();
            return (true, "Subscription resumed.");
        }

        public async Task<(bool Success, string Message)> CancelAsync(int userId)
        {
            var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId && s.Status != "Cancelled");
            if (sub == null) return (false, "No subscription found.");

            sub.Status = "Cancelled";
            sub.CancelledAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Subscription cancelled.");
        }

        private static SubscriptionDto ToDto(Subscription s) => new()
        {
            Id = s.Id,
            PlanName = s.PlanName,
            MealsPerWeek = s.MealsPerWeek,
            PricePerMonth = s.PricePerMonth,
            Status = s.Status,
            StartDate = s.StartDate,
            NextBillingDate = s.NextBillingDate,
            PreferredChefId = s.PreferredChefId,
            PreferredChefName = s.PreferredChef?.FullName,
            AutoAssignSameChef = s.AutoAssignSameChef,
            FallbackPolicy = s.FallbackPolicy,
        };
    }
}
