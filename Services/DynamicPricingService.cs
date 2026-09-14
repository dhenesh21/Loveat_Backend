using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class DynamicPricingService
    {
        private readonly AppDbContext _db;
        public DynamicPricingService(AppDbContext db) => _db = db;

        // ── Calculate price for a booking request ─────────────────
        public async Task<PriceCalculationResultDto> CalculatePriceAsync(PriceCalculationRequestDto req)
        {
            var chef = await _db.ChefProfiles.FirstOrDefaultAsync(c => c.UserId == req.ChefId);
            if (chef == null) throw new Exception("Chef not found");

            // ChefProfile only stores one base HourlyRate (no separate
            // Emergency/Event rate columns exist) — apply standard
            // surcharge multipliers for the higher-demand booking types
            // instead of introducing new schema columns.
            decimal baseRate   = req.BookingType switch
            {
                "Emergency" => chef.HourlyRate * 1.5m, // +50% emergency surcharge
                "Event"     => chef.HourlyRate * 1.25m, // +25% event surcharge
                _           => chef.HourlyRate,
            };

            var activeRules = await _db.PricingRules
                .Where(r => r.IsActive)
                .ToListAsync();

            var appliedRules   = new List<AppliedRuleDto>();
            decimal totalPct   = 0;
            string  dayOfWeek  = req.BookingDate.DayOfWeek.ToString()[..3]; // Mon, Tue etc.

            foreach (var rule in activeRules)
            {
                bool applies = true;

                // Check day of week
                if (!string.IsNullOrEmpty(rule.DaysOfWeek))
                {
                    var days = rule.DaysOfWeek.Split(',');
                    if (!days.Any(d => dayOfWeek.StartsWith(d.Trim()[..3], StringComparison.OrdinalIgnoreCase)))
                        applies = false;
                }

                // Check hour range
                if (applies && rule.StartHour.HasValue && rule.EndHour.HasValue)
                    if (req.StartHour < rule.StartHour || req.StartHour >= rule.EndHour)
                        applies = false;

                // Check booking type target
                if (applies && rule.AppliesTo == "BookingType" && rule.TargetValue != req.BookingType)
                    applies = false;

                // Check city target
                if (applies && rule.AppliesTo == "City" && rule.TargetValue != req.City)
                    applies = false;

                // Check date validity
                if (applies && !string.IsNullOrEmpty(rule.ValidFrom))
                    if (req.BookingDate < DateTime.Parse(rule.ValidFrom)) applies = false;
                if (applies && !string.IsNullOrEmpty(rule.ValidTo))
                    if (req.BookingDate > DateTime.Parse(rule.ValidTo)) applies = false;

                if (applies)
                {
                    decimal added = baseRate * (rule.MultiplierPercent / 100);
                    appliedRules.Add(new AppliedRuleDto
                    {
                        RuleName    = rule.RuleName,
                        RuleType    = rule.RuleType,
                        Percent     = rule.MultiplierPercent,
                        AmountAdded = Math.Round(added, 2),
                    });
                    totalPct += rule.MultiplierPercent;
                }
            }

            decimal surcharge  = baseRate * (totalPct / 100);
            decimal finalRate  = baseRate + surcharge;
            decimal totalEst   = finalRate * req.DurationHours;

            return new PriceCalculationResultDto
            {
                BaseRate         = Math.Round(baseRate, 2),
                FinalRate        = Math.Round(finalRate, 2),
                TotalSurcharge   = Math.Round(surcharge, 2),
                SurchargePercent = Math.Round(totalPct, 1),
                AppliedRules     = appliedRules,
                EstimatedTotal   = Math.Round(totalEst, 2),
            };
        }

        // ── CRUD for pricing rules ─────────────────────────────────
        public async Task<List<PricingRuleDto>> GetAllRulesAsync()
        {
            var rules = await _db.PricingRules.OrderBy(r => r.RuleType).ToListAsync();
            return rules.Select(MapRule).ToList();
        }

        public async Task<PricingRuleDto> CreateRuleAsync(CreatePricingRuleDto dto)
        {
            var rule = new PricingRule
            {
                RuleName          = dto.RuleName,
                RuleType          = dto.RuleType,
                AppliesTo         = dto.AppliesTo,
                TargetValue       = dto.TargetValue,
                MultiplierPercent = dto.MultiplierPercent,
                Description       = dto.Description,
                DaysOfWeek        = dto.DaysOfWeek,
                StartHour         = dto.StartHour,
                EndHour           = dto.EndHour,
                ValidFrom         = dto.ValidFrom,
                ValidTo           = dto.ValidTo,
            };
            _db.PricingRules.Add(rule);
            await _db.SaveChangesAsync();
            return MapRule(rule);
        }

        public async Task<bool> ToggleRuleAsync(int id)
        {
            var rule = await _db.PricingRules.FindAsync(id);
            if (rule == null) return false;
            rule.IsActive  = !rule.IsActive;
            rule.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRuleAsync(int id)
        {
            var rule = await _db.PricingRules.FindAsync(id);
            if (rule == null) return false;
            _db.PricingRules.Remove(rule);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<AdminPricingStatsDto> GetStatsAsync()
        {
            var rules = await _db.PricingRules.ToListAsync();
            return new AdminPricingStatsDto
            {
                TotalRules   = rules.Count,
                ActiveRules  = rules.Count(r => r.IsActive),
                AvgSurcharge = rules.Where(r => r.IsActive && r.MultiplierPercent > 0)
                                    .Select(r => r.MultiplierPercent)
                                    .DefaultIfEmpty(0).Average(),
                TotalSurchargeCollected = 48600, // from PricingHistory in production
                Rules = rules.Select(MapRule).ToList(),
            };
        }

        private static PricingRuleDto MapRule(PricingRule r) => new()
        {
            Id = r.Id, RuleName = r.RuleName, RuleType = r.RuleType,
            AppliesTo = r.AppliesTo, TargetValue = r.TargetValue,
            MultiplierPercent = r.MultiplierPercent, Description = r.Description,
            IsActive = r.IsActive, DaysOfWeek = r.DaysOfWeek,
            StartHour = r.StartHour, EndHour = r.EndHour,
            ValidFrom = r.ValidFrom, ValidTo = r.ValidTo,
        };
    }
}
