using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class ReferralService
    {
        private readonly AppDbContext _db;
        private readonly LoyaltyService _loyalty;

        private const int PointsPerReferral      = 150; // referrer gets
        private const int ReferredUserBonusPoints = 100; // new user gets

        public ReferralService(AppDbContext db, LoyaltyService loyalty)
        {
            _db      = db;
            _loyalty = loyalty;
        }

        // ── Get or create referral code for user ──────────────────
        public async Task<ReferralStatusDto> GetReferralStatusAsync(int userId)
        {
            var code = await _db.ReferralCodes
                .Include(r => r.Uses).ThenInclude(u => u.ReferredUser)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId == userId);

            if (code == null)
            {
                var user = await _db.Users.FindAsync(userId);
                code = new ReferralCode
                {
                    UserId = userId,
                    Code   = GenerateCode(user?.FullName ?? "USER", userId),
                };
                _db.ReferralCodes.Add(code);
                await _db.SaveChangesAsync();
            }

            return new ReferralStatusDto
            {
                ReferralCode         = code.Code,
                ReferralLink         = $"https://loveat.in/join?ref={code.Code}",
                TotalReferrals       = code.TotalReferrals,
                SuccessfulReferrals  = code.SuccessfulReferrals,
                PendingReferrals     = code.TotalReferrals - code.SuccessfulReferrals,
                PointsEarned         = code.PointsEarned,
                PointsPerReferral    = PointsPerReferral,
                RecentReferrals      = code.Uses.OrderByDescending(u => u.UsedAt).Take(10).Select(u => new ReferralUseDto
                {
                    ReferredUserName = u.ReferredUser?.FullName ?? "User",
                    Status           = u.Status,
                    Points           = u.Points,
                    UsedAt           = u.UsedAt.ToString("MMM dd, yyyy"),
                }).ToList(),
            };
        }

        // ── Apply referral code (called on new user signup) ───────
        public async Task<ApplyReferralResponseDto> ApplyReferralCodeAsync(int newUserId, string code)
        {
            var referralCode = await _db.ReferralCodes
                .Include(r => r.Uses)
                .FirstOrDefaultAsync(r => r.Code == code && r.IsActive);

            if (referralCode == null)
                return new ApplyReferralResponseDto { Success = false, Message = "Invalid referral code" };

            if (referralCode.UserId == newUserId)
                return new ApplyReferralResponseDto { Success = false, Message = "Cannot use your own referral code" };

            // Check if already used
            var alreadyUsed = await _db.ReferralUses.AnyAsync(u => u.ReferredUserId == newUserId);
            if (alreadyUsed)
                return new ApplyReferralResponseDto { Success = false, Message = "Referral already applied" };

            // Record use
            var use = new ReferralUse
            {
                ReferralCodeId = referralCode.Id,
                ReferredUserId = newUserId,
                Status         = "Pending",
                Points         = 0,
            };
            _db.ReferralUses.Add(use);

            referralCode.TotalReferrals++;
            await _db.SaveChangesAsync();

            // Award bonus to new user
            await _loyalty.AwardPointsAsync(newUserId, ReferredUserBonusPoints, $"Welcome bonus via referral code {code}", "Bonus");

            return new ApplyReferralResponseDto
            {
                Success      = true,
                Message      = $"Referral applied! You earned {ReferredUserBonusPoints} bonus points.",
                BonusPoints  = ReferredUserBonusPoints,
            };
        }

        // ── Complete referral after first booking ─────────────────
        public async Task CompleteReferralAsync(int referredUserId)
        {
            var use = await _db.ReferralUses
                .Include(u => u.ReferralCode)
                .FirstOrDefaultAsync(u => u.ReferredUserId == referredUserId && u.Status == "Pending");

            if (use == null) return;

            use.Status      = "Completed";
            use.Points      = PointsPerReferral;
            use.CompletedAt = DateTime.UtcNow;

            use.ReferralCode!.SuccessfulReferrals++;
            use.ReferralCode!.PointsEarned += PointsPerReferral;

            await _db.SaveChangesAsync();

            // Award points to referrer
            await _loyalty.AwardPointsAsync(
                use.ReferralCode.UserId,
                PointsPerReferral,
                $"Referral bonus — friend completed first booking",
                "Bonus"
            );
        }

        // ── Admin stats ────────────────────────────────────────────
        public async Task<AdminReferralStatsDto> GetAdminStatsAsync()
        {
            var codes = await _db.ReferralCodes.Include(r => r.User).ToListAsync();
            var uses  = await _db.ReferralUses.ToListAsync();

            return new AdminReferralStatsDto
            {
                TotalCodes          = codes.Count,
                TotalReferrals      = uses.Count,
                CompletedReferrals  = uses.Count(u => u.Status == "Completed"),
                PointsAwarded       = codes.Sum(c => c.PointsEarned),
                TopReferrers        = codes.OrderByDescending(c => c.SuccessfulReferrals).Take(10).Select(c => new ReferralLeaderDto
                {
                    UserId       = c.UserId,
                    UserName     = c.User?.FullName ?? "Unknown",
                    Code         = c.Code,
                    Referrals    = c.SuccessfulReferrals,
                    PointsEarned = c.PointsEarned,
                }).ToList(),
            };
        }

        // ── Helper ─────────────────────────────────────────────────
        private static string GenerateCode(string name, int userId)
        {
            var prefix = new string(name.ToUpper().Where(char.IsLetter).Take(4).ToArray());
            return $"{prefix}{userId:D3}";
        }
    }
}
