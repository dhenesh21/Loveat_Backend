using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class CommissionService
    {
        private readonly AppDbContext _db;
        private const decimal DEFAULT_PLATFORM_PCT = 15m;

        public CommissionService(AppDbContext db) => _db = db;

        // ── Calculate & record commission for a completed booking ──
        public async Task<CommissionLedgerDto> RecordCommissionAsync(int bookingId, int chefId, decimal bookingAmount)
        {
            // Find applicable rule
            var rule = await _db.CommissionRules
                .Where(r => r.IsActive && r.AppliesTo == "All")
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            decimal platformPct = rule?.PlatformPercent ?? DEFAULT_PLATFORM_PCT;
            decimal chefPct     = 100 - platformPct;
            decimal platformAmt = Math.Round(bookingAmount * platformPct / 100, 2);
            decimal chefAmt     = Math.Round(bookingAmount - platformAmt, 2);

            var entry = new CommissionLedger
            {
                BookingId        = bookingId,
                ChefId           = chefId,
                BookingAmount    = bookingAmount,
                PlatformAmount   = platformAmt,
                ChefAmount       = chefAmt,
                PlatformPercent  = platformPct,
                CommissionRuleId = rule?.Id,
            };
            _db.CommissionLedgers.Add(entry);
            await _db.SaveChangesAsync();

            var chef = await _db.Users.FindAsync(chefId);
            return Map(entry, chef?.FullName ?? "Chef");
        }

        // ── Admin: Get commission summary ──────────────────────────
        public async Task<CommissionSummaryDto> GetSummaryAsync()
        {
            var ledger = await _db.CommissionLedgers.Include(l => l.Chef).ToListAsync();
            var rules  = await _db.CommissionRules.OrderBy(r => r.AppliesTo).ToListAsync();

            return new CommissionSummaryDto
            {
                TotalBookingRevenue = ledger.Sum(l => l.BookingAmount),
                TotalPlatformEarned = ledger.Sum(l => l.PlatformAmount),
                TotalChefPaid       = ledger.Where(l => l.Status == "Settled").Sum(l => l.ChefAmount),
                PendingSettlement   = ledger.Where(l => l.Status == "Pending").Sum(l => l.ChefAmount),
                TotalTransactions   = ledger.Count,
                Recent = ledger.OrderByDescending(l => l.CreatedAt).Take(20).Select(l => Map(l, l.Chef?.FullName ?? "")).ToList(),
                Rules  = rules.Select(r => new CommissionRuleDto
                {
                    Id=r.Id, RuleName=r.RuleName, AppliesTo=r.AppliesTo,
                    TargetValue=r.TargetValue, PlatformPercent=r.PlatformPercent,
                    ChefPercent=r.ChefPercent, IsActive=r.IsActive,
                }).ToList(),
            };
        }

        // ── Create / update rules ──────────────────────────────────
        public async Task<CommissionRuleDto> CreateRuleAsync(CreateCommissionRuleDto dto)
        {
            var rule = new CommissionRule
            {
                RuleName        = dto.RuleName,
                AppliesTo       = dto.AppliesTo,
                TargetValue     = dto.TargetValue,
                PlatformPercent = dto.PlatformPercent,
                ChefPercent     = 100 - dto.PlatformPercent,
            };
            _db.CommissionRules.Add(rule);
            await _db.SaveChangesAsync();
            return new CommissionRuleDto { Id=rule.Id, RuleName=rule.RuleName, AppliesTo=rule.AppliesTo, TargetValue=rule.TargetValue, PlatformPercent=rule.PlatformPercent, ChefPercent=rule.ChefPercent, IsActive=rule.IsActive };
        }

        public async Task<bool> ToggleRuleAsync(int id)
        {
            var rule = await _db.CommissionRules.FindAsync(id);
            if (rule == null) return false;
            rule.IsActive  = !rule.IsActive;
            rule.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        private static CommissionLedgerDto Map(CommissionLedger l, string chefName) => new()
        {
            Id=l.Id, BookingId=l.BookingId, ChefName=chefName,
            BookingAmount=l.BookingAmount, PlatformAmount=l.PlatformAmount,
            ChefAmount=l.ChefAmount, PlatformPercent=l.PlatformPercent,
            Status=l.Status, CreatedAt=l.CreatedAt.ToString("MMM dd, yyyy"),
        };
    }
}
