using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class LoyaltyService
    {
        private readonly AppDbContext _db;

        // Points config
        private const int PointsPerBooking     = 50;
        private const int PointsPerReview      = 20;
        private const int WelcomeBonusPoints   = 100;
        private const int PointsPerRupee       = 1;   // 1 point per ₹1 spent (optional)

        // Tier thresholds
        private static readonly Dictionary<string, int> TierThresholds = new()
        {
            { "Bronze",   0    },
            { "Silver",   500  },
            { "Gold",     2000 },
            { "Platinum", 5000 },
        };

        public LoyaltyService(AppDbContext db) => _db = db;

        // ── Get loyalty status for a user ─────────────────────────
        public async Task<LoyaltyStatusDto> GetStatusAsync(int userId)
        {
            var loyalty = await _db.LoyaltyPoints
                .FirstOrDefaultAsync(l => l.UserId == userId);

            // Create if doesn't exist
            if (loyalty == null)
            {
                loyalty = new LoyaltyPoints { UserId = userId };
                _db.LoyaltyPoints.Add(loyalty);
                await _db.SaveChangesAsync();
            }

            var transactions = await _db.LoyaltyTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(10)
                .ToListAsync();

            var (nextTier, pointsToNext) = GetNextTierInfo(loyalty.TotalPoints);
            var tierProgress = GetTierProgress(loyalty.TotalPoints);

            return new LoyaltyStatusDto
            {
                TotalPoints      = loyalty.TotalPoints,
                AvailablePoints  = loyalty.AvailablePoints,
                RedeemedPoints   = loyalty.RedeemedPoints,
                PendingPoints    = loyalty.PendingPoints,
                Tier             = loyalty.Tier,
                NextTier         = nextTier,
                PointsToNextTier = pointsToNext,
                TierProgress     = tierProgress,
                RecentTransactions = transactions.Select(t => new LoyaltyTransactionDto
                {
                    Id           = t.Id,
                    Points       = t.Points,
                    Type         = t.Type,
                    Reason       = t.Reason,
                    BalanceAfter = t.BalanceAfter,
                    CreatedAt    = t.CreatedAt.ToString("MMM dd, yyyy"),
                }).ToList(),
                AvailableRewards = GetAvailableRewards(loyalty.AvailablePoints),
            };
        }

        // ── Award points ───────────────────────────────────────────
        public async Task AwardPointsAsync(int userId, int points, string reason, string type = "Earned", int? bookingId = null)
        {
            var loyalty = await _db.LoyaltyPoints.FirstOrDefaultAsync(l => l.UserId == userId)
                          ?? new LoyaltyPoints { UserId = userId };

            if (loyalty.Id == 0) _db.LoyaltyPoints.Add(loyalty);

            loyalty.TotalPoints += points;
            loyalty.Tier         = CalculateTier(loyalty.TotalPoints);
            loyalty.UpdatedAt    = DateTime.UtcNow;

            var tx = new LoyaltyTransaction
            {
                UserId       = userId,
                Points       = points,
                Type         = type,
                Reason       = reason,
                BookingId    = bookingId,
                BalanceAfter = loyalty.AvailablePoints,
            };
            _db.LoyaltyTransactions.Add(tx);
            await _db.SaveChangesAsync();
        }

        // ── Redeem points ──────────────────────────────────────────
        public async Task<RedeemPointsResponseDto> RedeemPointsAsync(int userId, RedeemPointsRequestDto req)
        {
            var loyalty = await _db.LoyaltyPoints.FirstOrDefaultAsync(l => l.UserId == userId);
            if (loyalty == null || loyalty.AvailablePoints < req.PointsToUse)
                return new RedeemPointsResponseDto { Success = false, Message = "Insufficient points" };

            var rewards = GetAvailableRewards(loyalty.AvailablePoints);
            var reward  = rewards.FirstOrDefault(r => r.Id == req.RewardId);
            if (reward == null)
                return new RedeemPointsResponseDto { Success = false, Message = "Reward not found" };

            loyalty.RedeemedPoints += reward.PointsCost;
            loyalty.UpdatedAt       = DateTime.UtcNow;

            var tx = new LoyaltyTransaction
            {
                UserId       = userId,
                Points       = -reward.PointsCost,
                Type         = "Redeemed",
                Reason       = $"Redeemed: {reward.Title}",
                BalanceAfter = loyalty.AvailablePoints,
            };
            _db.LoyaltyTransactions.Add(tx);
            await _db.SaveChangesAsync();

            var coupon = $"LOVEAT{userId}{DateTime.UtcNow:mmss}";
            return new RedeemPointsResponseDto
            {
                Success        = true,
                Message        = "Points redeemed successfully!",
                CouponCode     = coupon,
                PointsDeducted = reward.PointsCost,
                NewBalance     = loyalty.AvailablePoints,
            };
        }

        // ── Admin stats ────────────────────────────────────────────
        public async Task<AdminLoyaltyStatsDto> GetAdminStatsAsync()
        {
            var allLoyalty = await _db.LoyaltyPoints.Include(l => l.User).ToListAsync();
            var allTx      = await _db.LoyaltyTransactions.ToListAsync();

            var tierGroups = allLoyalty.GroupBy(l => l.Tier).ToDictionary(g => g.Key, g => g.Count());
            var totalUsers = allLoyalty.Count;

            return new AdminLoyaltyStatsDto
            {
                TotalUsersWithPoints = totalUsers,
                TotalPointsIssued    = allTx.Where(t => t.Points > 0).Sum(t => (long)t.Points),
                TotalPointsRedeemed  = allTx.Where(t => t.Type == "Redeemed").Sum(t => (long)-t.Points),
                TotalPointsExpired   = allTx.Where(t => t.Type == "Expired").Sum(t => (long)-t.Points),
                PointsValueRedeemed  = allTx.Where(t => t.Type == "Redeemed").Sum(t => (decimal)-t.Points) / 10,
                TierBreakdown        = new[] { "Bronze","Silver","Gold","Platinum" }.Select(tier => new TierDistributionDto
                {
                    Tier    = tier,
                    Count   = tierGroups.GetValueOrDefault(tier, 0),
                    Percent = totalUsers > 0 ? Math.Round(tierGroups.GetValueOrDefault(tier, 0) * 100.0 / totalUsers, 1) : 0,
                }).ToList(),
                TopUsers = allLoyalty.OrderByDescending(l => l.TotalPoints).Take(10).Select(l => new LoyaltyLeaderDto
                {
                    UserId   = l.UserId,
                    UserName = l.User?.FullName ?? "Unknown",
                    Phone    = l.User?.PhoneNumber ?? "",
                    Points   = l.TotalPoints,
                    Tier     = l.Tier,
                }).ToList(),
            };
        }

        // ── Helpers ────────────────────────────────────────────────
        private static string CalculateTier(int points) =>
            points >= 5000 ? "Platinum" :
            points >= 2000 ? "Gold"     :
            points >= 500  ? "Silver"   : "Bronze";

        private static (string nextTier, int pointsNeeded) GetNextTierInfo(int points)
        {
            if (points < 500)  return ("Silver",   500  - points);
            if (points < 2000) return ("Gold",     2000 - points);
            if (points < 5000) return ("Platinum", 5000 - points);
            return ("Platinum", 0);
        }

        private static double GetTierProgress(int points)
        {
            if (points >= 5000) return 1.0;
            if (points >= 2000) return Math.Round((points - 2000.0) / 3000.0, 2);
            if (points >= 500)  return Math.Round((points - 500.0)  / 1500.0, 2);
            return Math.Round(points / 500.0, 2);
        }

        private static List<LoyaltyRewardDto> GetAvailableRewards(int availablePoints) =>
        [
            new() { Id=1, Title="₹50 Off",        Description="Get ₹50 off on your next booking",  PointsCost=200,  RewardType="Discount",    Value=50,  CanRedeem=availablePoints>=200  },
            new() { Id=2, Title="₹100 Off",        Description="Get ₹100 off on your next booking", PointsCost=400,  RewardType="Discount",    Value=100, CanRedeem=availablePoints>=400  },
            new() { Id=3, Title="Free Booking",    Description="One free home cooking session",     PointsCost=1000, RewardType="FreeBooking", Value=0,   CanRedeem=availablePoints>=1000 },
            new() { Id=4, Title="₹200 Cashback",  Description="Wallet cashback ₹200",              PointsCost=800,  RewardType="CashBack",    Value=200, CanRedeem=availablePoints>=800  },
            new() { Id=5, Title="10% Off",         Description="10% off on any booking",            PointsCost=300,  RewardType="Discount",    Value=10,  CanRedeem=availablePoints>=300  },
        ];
    }
}
