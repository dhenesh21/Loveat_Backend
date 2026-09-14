using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M145: Chef Business Suite — Tax Filing Assistant ────────────
    public class ChefTaxAssistantService
    {
        private readonly AppDbContext _db;
        public ChefTaxAssistantService(AppDbContext db) => _db = db;

        // Presumptive taxation estimate (Section 44AD style): taxable
        // income = grossIncome * presumptivePercent%, minus logged business
        // expenses actually paid out-of-pocket beyond the presumptive rate
        // (a simplified illustrative estimate, not tax advice).
        public async Task<(bool Success, string Message, ChefTaxEstimateDto? Estimate)> GenerateAsync(GenerateTaxEstimateRequestDto req)
        {
            if (!int.TryParse(req.FinancialYear.Split('-')[0], out var fyStartYear)) return (false, "FinancialYear must be like '2026-27'.", null);
            var fyStart = new DateTime(fyStartYear, 4, 1, 0, 0, 0, DateTimeKind.Utc);
            var fyEnd = fyStart.AddYears(1);

            var grossIncome = await _db.Bookings.Where(b => b.ChefId == req.ChefId && b.Status == "Completed" && b.CreatedAt >= fyStart && b.CreatedAt < fyEnd).SumAsync(b => (decimal?)b.TotalAmount) ?? 0;
            var expenses = await _db.ChefBusinessExpenses.Where(e => e.ChefId == req.ChefId && e.ExpenseDate >= fyStart && e.ExpenseDate < fyEnd).SumAsync(e => (decimal?)e.Amount) ?? 0;

            var presumptiveIncome = Math.Round(grossIncome * req.PresumptiveIncomePercent / 100, 2);
            var taxableIncome = Math.Max(0, presumptiveIncome - Math.Min(expenses, presumptiveIncome)); // expenses can't push taxable income negative
            // Simplified slab-free flat estimate for illustration: 10% of taxable income above a nominal exemption.
            var taxLiability = Math.Round(Math.Max(0, taxableIncome - 250000) * 0.10m, 2);

            var estimate = new ChefTaxEstimate
            {
                ChefId = req.ChefId, FinancialYear = req.FinancialYear, GrossIncome = grossIncome, DeductibleExpenses = expenses,
                PresumptiveIncomePercent = req.PresumptiveIncomePercent, EstimatedTaxableIncome = taxableIncome, EstimatedTaxLiability = taxLiability,
            };
            _db.ChefTaxEstimates.Add(estimate);
            await _db.SaveChangesAsync();

            return (true, "Estimate generated. This is illustrative only, not professional tax advice.", ToDto(estimate));
        }

        public async Task<List<ChefTaxEstimateDto>> GetHistoryAsync(int chefId)
            => (await _db.ChefTaxEstimates.Where(e => e.ChefId == chefId).OrderByDescending(e => e.GeneratedAt).ToListAsync()).Select(ToDto).ToList();

        public async Task<ChefBusinessExpenseDto> LogExpenseAsync(LogBusinessExpenseRequestDto req)
        {
            var expense = new ChefBusinessExpense { ChefId = req.ChefId, Category = req.Category, Description = req.Description, Amount = req.Amount, ExpenseDate = req.ExpenseDate };
            _db.ChefBusinessExpenses.Add(expense);
            await _db.SaveChangesAsync();
            return new ChefBusinessExpenseDto { Id = expense.Id, Category = expense.Category, Description = expense.Description, Amount = expense.Amount, ExpenseDate = expense.ExpenseDate };
        }

        public async Task<List<ChefBusinessExpenseDto>> GetExpensesAsync(int chefId, string? financialYear = null)
        {
            var q = _db.ChefBusinessExpenses.Where(e => e.ChefId == chefId);
            if (!string.IsNullOrEmpty(financialYear) && int.TryParse(financialYear.Split('-')[0], out var fyStartYear))
            {
                var fyStart = new DateTime(fyStartYear, 4, 1, 0, 0, 0, DateTimeKind.Utc);
                var fyEnd = fyStart.AddYears(1);
                q = q.Where(e => e.ExpenseDate >= fyStart && e.ExpenseDate < fyEnd);
            }
            return (await q.OrderByDescending(e => e.ExpenseDate).ToListAsync()).Select(e => new ChefBusinessExpenseDto { Id = e.Id, Category = e.Category, Description = e.Description, Amount = e.Amount, ExpenseDate = e.ExpenseDate }).ToList();
        }

        private static ChefTaxEstimateDto ToDto(ChefTaxEstimate e) => new()
        {
            FinancialYear = e.FinancialYear, GrossIncome = e.GrossIncome, DeductibleExpenses = e.DeductibleExpenses,
            PresumptiveIncomePercent = e.PresumptiveIncomePercent, EstimatedTaxableIncome = e.EstimatedTaxableIncome,
            EstimatedTaxLiability = e.EstimatedTaxLiability, GeneratedAt = e.GeneratedAt,
        };
    }

    // ── M146: Chef Business Suite — Chef Loyalty Program ────────────
    public class ChefLoyaltyProgramService
    {
        private readonly AppDbContext _db;
        public ChefLoyaltyProgramService(AppDbContext db) => _db = db;

        public async Task<ChefLoyaltyProgramDto> CreateOrUpdateAsync(CreateChefLoyaltyProgramRequestDto req)
        {
            var program = await _db.ChefLoyaltyPrograms.FirstOrDefaultAsync(p => p.ChefId == req.ChefId);
            if (program == null) { program = new ChefLoyaltyProgram { ChefId = req.ChefId }; _db.ChefLoyaltyPrograms.Add(program); }
            program.PointsPerRupeeSpent = req.PointsPerRupeeSpent;
            program.RedemptionThresholdPoints = req.RedemptionThresholdPoints;
            program.RedemptionValueRupees = req.RedemptionValueRupees;
            await _db.SaveChangesAsync();
            return ToDto(program);
        }

        public async Task<ChefLoyaltyProgramDto?> GetForChefAsync(int chefId)
        {
            var program = await _db.ChefLoyaltyPrograms.FirstOrDefaultAsync(p => p.ChefId == chefId);
            return program == null ? null : ToDto(program);
        }

        public async Task<(bool Success, string Message, decimal PointsEarned)> EarnAsync(EarnChefLoyaltyPointsRequestDto req)
        {
            var program = await _db.ChefLoyaltyPrograms.FirstOrDefaultAsync(p => p.ChefId == req.ChefId && p.IsActive);
            if (program == null) return (false, "This chef doesn't have an active loyalty program.", 0);

            var pointsEarned = Math.Round(req.BookingAmount * program.PointsPerRupeeSpent / 100, 2);
            var balance = await _db.ChefCustomerLoyaltyBalances.FirstOrDefaultAsync(b => b.ChefId == req.ChefId && b.CustomerId == req.CustomerId);
            if (balance == null) { balance = new ChefCustomerLoyaltyBalance { ChefId = req.ChefId, CustomerId = req.CustomerId }; _db.ChefCustomerLoyaltyBalances.Add(balance); }
            balance.PointsBalance += pointsEarned;
            balance.LifetimePointsEarned += pointsEarned;
            balance.UpdatedAt = DateTime.UtcNow;

            _db.ChefLoyaltyTransactions.Add(new ChefLoyaltyTransaction { ChefId = req.ChefId, CustomerId = req.CustomerId, Type = "Earn", Points = pointsEarned, BookingId = req.BookingId });
            await _db.SaveChangesAsync();
            return (true, $"{pointsEarned} points earned.", pointsEarned);
        }

        public async Task<(bool Success, string Message)> RedeemAsync(RedeemChefLoyaltyPointsRequestDto req)
        {
            var balance = await _db.ChefCustomerLoyaltyBalances.FirstOrDefaultAsync(b => b.ChefId == req.ChefId && b.CustomerId == req.CustomerId);
            if (balance == null || balance.PointsBalance < req.PointsToRedeem) return (false, "Not enough points balance.");

            balance.PointsBalance -= req.PointsToRedeem;
            balance.UpdatedAt = DateTime.UtcNow;
            _db.ChefLoyaltyTransactions.Add(new ChefLoyaltyTransaction { ChefId = req.ChefId, CustomerId = req.CustomerId, Type = "Redeem", Points = req.PointsToRedeem });
            await _db.SaveChangesAsync();
            return (true, "Points redeemed.");
        }

        public async Task<List<ChefLoyaltyBalanceDto>> GetTopCustomersAsync(int chefId, int take = 20)
        {
            var balances = await _db.ChefCustomerLoyaltyBalances.Include(b => b.Customer).Where(b => b.ChefId == chefId).OrderByDescending(b => b.LifetimePointsEarned).Take(take).ToListAsync();
            return balances.Select(b => new ChefLoyaltyBalanceDto { CustomerId = b.CustomerId, CustomerName = b.Customer?.FullName, PointsBalance = b.PointsBalance, LifetimePointsEarned = b.LifetimePointsEarned }).ToList();
        }

        private static ChefLoyaltyProgramDto ToDto(ChefLoyaltyProgram p) => new()
        {
            Id = p.Id, PointsPerRupeeSpent = p.PointsPerRupeeSpent, RedemptionThresholdPoints = p.RedemptionThresholdPoints,
            RedemptionValueRupees = p.RedemptionValueRupees, IsActive = p.IsActive,
        };
    }

    // ── M147: Chef Business Suite — Business Goals & Targets ────────
    public class ChefBusinessGoalService
    {
        private readonly AppDbContext _db;
        public ChefBusinessGoalService(AppDbContext db) => _db = db;

        public async Task<ChefBusinessGoalDto> CreateAsync(CreateBusinessGoalRequestDto req)
        {
            var goal = new ChefBusinessGoal { ChefId = req.ChefId, PeriodKey = req.PeriodKey, GoalType = req.GoalType, TargetValue = req.TargetValue };
            _db.ChefBusinessGoals.Add(goal);
            await _db.SaveChangesAsync();
            return await RefreshInternalAsync(goal);
        }

        public async Task<List<ChefBusinessGoalDto>> GetForChefAsync(int chefId)
        {
            var goals = await _db.ChefBusinessGoals.Where(g => g.ChefId == chefId).OrderByDescending(g => g.PeriodKey).ToListAsync();
            var result = new List<ChefBusinessGoalDto>();
            foreach (var g in goals) result.Add(ToDto(g));
            return result;
        }

        // Recomputes ActualValue from real Booking data for the goal's period.
        public async Task<(bool Success, string Message, ChefBusinessGoalDto? Goal)> RefreshAsync(int goalId)
        {
            var goal = await _db.ChefBusinessGoals.FindAsync(goalId);
            if (goal == null) return (false, "Goal not found.", null);
            return (true, "Refreshed.", await RefreshInternalAsync(goal));
        }

        private async Task<ChefBusinessGoalDto> RefreshInternalAsync(ChefBusinessGoal goal)
        {
            if (!DateTime.TryParseExact(goal.PeriodKey + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                return ToDto(goal);
            var monthEnd = monthStart.AddMonths(1);

            var bookings = await _db.Bookings.Where(b => b.ChefId == goal.ChefId && b.CreatedAt >= monthStart && b.CreatedAt < monthEnd).ToListAsync();
            goal.ActualValue = goal.GoalType switch
            {
                "Revenue" => bookings.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount),
                "BookingCount" => bookings.Count,
                "NewCustomers" => bookings.Select(b => b.CustomerId).Distinct().Count(),
                _ => 0,
            };
            goal.LastRefreshedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToDto(goal);
        }

        private static ChefBusinessGoalDto ToDto(ChefBusinessGoal g) => new()
        {
            Id = g.Id, PeriodKey = g.PeriodKey, GoalType = g.GoalType, TargetValue = g.TargetValue, ActualValue = g.ActualValue,
            ProgressPercent = g.TargetValue == 0 ? 0 : Math.Round(Math.Min(g.ActualValue / g.TargetValue * 100, 999), 1),
            IsAchieved = g.ActualValue >= g.TargetValue,
        };
    }

    // ── M148: Chef Business Suite — Multi-Outlet Management ─────────
    public class ChefOutletService
    {
        private readonly AppDbContext _db;
        public ChefOutletService(AppDbContext db) => _db = db;

        public async Task<ChefOutletDto> CreateAsync(CreateChefOutletRequestDto req)
        {
            if (req.IsPrimary)
            {
                var existing = await _db.ChefOutlets.Where(o => o.ChefId == req.ChefId && o.IsPrimary).ToListAsync();
                foreach (var o in existing) o.IsPrimary = false;
            }
            var outlet = new ChefOutlet
            {
                ChefId = req.ChefId, OutletName = req.OutletName, OutletType = req.OutletType, Address = req.Address, City = req.City,
                Latitude = req.Latitude, Longitude = req.Longitude, MaxDailyOrderCapacity = req.MaxDailyOrderCapacity, IsPrimary = req.IsPrimary,
            };
            _db.ChefOutlets.Add(outlet);
            await _db.SaveChangesAsync();
            return ToDto(outlet);
        }

        public async Task<List<ChefOutletDto>> GetForChefAsync(int chefId)
            => (await _db.ChefOutlets.Where(o => o.ChefId == chefId).OrderByDescending(o => o.IsPrimary).ToListAsync()).Select(ToDto).ToList();

        public async Task<(bool Success, string Message)> DeactivateAsync(int outletId)
        {
            var outlet = await _db.ChefOutlets.FindAsync(outletId);
            if (outlet == null) return (false, "Outlet not found.");
            outlet.IsActive = false;
            await _db.SaveChangesAsync();
            return (true, "Outlet deactivated.");
        }

        private static ChefOutletDto ToDto(ChefOutlet o) => new()
        {
            Id = o.Id, OutletName = o.OutletName, OutletType = o.OutletType, Address = o.Address, City = o.City,
            MaxDailyOrderCapacity = o.MaxDailyOrderCapacity, IsPrimary = o.IsPrimary, IsActive = o.IsActive,
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/chef-tax-assistant")]
    public class ChefTaxAssistantController : ControllerBase
    {
        private readonly Services.ChefTaxAssistantService _svc;
        public ChefTaxAssistantController(Services.ChefTaxAssistantService svc) => _svc = svc;

        [HttpPost("estimate"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Generate([FromBody] GenerateTaxEstimateRequestDto req)
        {
            var (success, message, estimate) = await _svc.GenerateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = estimate });
        }

        [HttpGet("chef/{chefId}/estimates"), Authorize]
        public async Task<IActionResult> GetHistory(int chefId) => Ok(new { success = true, data = await _svc.GetHistoryAsync(chefId) });

        [HttpPost("expenses"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> LogExpense([FromBody] LogBusinessExpenseRequestDto req) => Ok(new { success = true, data = await _svc.LogExpenseAsync(req) });

        [HttpGet("chef/{chefId}/expenses"), Authorize]
        public async Task<IActionResult> GetExpenses(int chefId, [FromQuery] string? financialYear) => Ok(new { success = true, data = await _svc.GetExpensesAsync(chefId, financialYear) });
    }

    [ApiController, Route("api/chef-loyalty")]
    public class ChefLoyaltyProgramController : ControllerBase
    {
        private readonly Services.ChefLoyaltyProgramService _svc;
        public ChefLoyaltyProgramController(Services.ChefLoyaltyProgramService svc) => _svc = svc;

        [HttpPost, Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] CreateChefLoyaltyProgramRequestDto req) => Ok(new { success = true, data = await _svc.CreateOrUpdateAsync(req) });

        [HttpGet("chef/{chefId}")]
        public async Task<IActionResult> GetForChef(int chefId)
        {
            var program = await _svc.GetForChefAsync(chefId);
            if (program == null) return NotFound(new { success = false, message = "No loyalty program set up for this chef." });
            return Ok(new { success = true, data = program });
        }

        [HttpPost("earn"), Authorize]
        public async Task<IActionResult> Earn([FromBody] EarnChefLoyaltyPointsRequestDto req)
        {
            var (success, message, points) = await _svc.EarnAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, pointsEarned = points });
        }

        [HttpPost("redeem"), Authorize]
        public async Task<IActionResult> Redeem([FromBody] RedeemChefLoyaltyPointsRequestDto req)
        {
            var (success, message) = await _svc.RedeemAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("chef/{chefId}/top-customers"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> GetTopCustomers(int chefId, [FromQuery] int take = 20) => Ok(new { success = true, data = await _svc.GetTopCustomersAsync(chefId, take) });
    }

    [ApiController, Route("api/chef-business-goals")]
    public class ChefBusinessGoalController : ControllerBase
    {
        private readonly Services.ChefBusinessGoalService _svc;
        public ChefBusinessGoalController(Services.ChefBusinessGoalService svc) => _svc = svc;

        [HttpPost, Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateBusinessGoalRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet("chef/{chefId}"), Authorize]
        public async Task<IActionResult> GetForChef(int chefId) => Ok(new { success = true, data = await _svc.GetForChefAsync(chefId) });

        [HttpPost("refresh"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Refresh([FromBody] RefreshGoalProgressRequestDto req)
        {
            var (success, message, goal) = await _svc.RefreshAsync(req.GoalId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = goal });
        }
    }

    [ApiController, Route("api/chef-outlets")]
    public class ChefOutletController : ControllerBase
    {
        private readonly Services.ChefOutletService _svc;
        public ChefOutletController(Services.ChefOutletService svc) => _svc = svc;

        [HttpPost, Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Create([FromBody] CreateChefOutletRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet("chef/{chefId}")]
        public async Task<IActionResult> GetForChef(int chefId) => Ok(new { success = true, data = await _svc.GetForChefAsync(chefId) });

        [HttpPost("deactivate"), Authorize(Roles = "Chef,Admin")]
        public async Task<IActionResult> Deactivate([FromBody] DeactivateOutletRequestDto req)
        {
            var (success, message) = await _svc.DeactivateAsync(req.OutletId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }
}
