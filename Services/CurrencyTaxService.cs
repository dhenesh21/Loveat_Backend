using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ══════════════════════════════════════════════════════════════
    // M73: Currency Service
    // ══════════════════════════════════════════════════════════════
    public class CurrencyService
    {
        private readonly AppDbContext _db;
        public CurrencyService(AppDbContext db) => _db = db;

        public async Task<List<CurrencyDto>> GetSupportedCurrenciesAsync()
        {
            var list = await _db.Currencies.Where(c => c.IsActive).OrderBy(c => c.Code).ToListAsync();
            return list.Select(ToDto).ToList();
        }

        public async Task<(bool Success, string Message, ConvertAmountResponseDto? Data)> ConvertAsync(decimal amountInINR, string toCurrency)
        {
            if (toCurrency.Equals("INR", StringComparison.OrdinalIgnoreCase))
            {
                return (true, "Converted.", new ConvertAmountResponseDto
                {
                    OriginalAmount = amountInINR, OriginalCurrency = "INR",
                    ConvertedAmount = amountInINR, ToCurrency = "INR", ExchangeRate = 1, IsStubRate = false,
                });
            }

            var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Code == toCurrency.ToUpper() && c.IsActive);
            if (currency == null) return (false, $"Currency '{toCurrency}' is not supported.", null);

            return (true, "Converted.", new ConvertAmountResponseDto
            {
                OriginalAmount = amountInINR,
                OriginalCurrency = "INR",
                ConvertedAmount = Math.Round(amountInINR * currency.ExchangeRateToINR, 2),
                ToCurrency = currency.Code,
                ExchangeRate = currency.ExchangeRateToINR,
                IsStubRate = true, // TODO (G9): false once a live FX API replaces these seeded rates
            });
        }

        public async Task<(bool Success, string Message)> SetPreferredCurrencyAsync(int userId, string code)
        {
            var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Code == code.ToUpper() && c.IsActive);
            if (currency == null) return (false, $"Currency '{code}' is not supported.");

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return (false, "User not found.");

            user.PreferredCurrency = currency.Code;
            await _db.SaveChangesAsync();
            return (true, $"Preferred currency set to {currency.Code}.");
        }

        /// <summary>TODO (G9): manual admin override until a live FX rate API is wired.</summary>
        public async Task<(bool Success, string Message)> UpdateRateAsync(UpdateExchangeRateRequestDto req)
        {
            var currency = await _db.Currencies.FirstOrDefaultAsync(c => c.Code == req.Code.ToUpper());
            if (currency == null) return (false, $"Currency '{req.Code}' not found.");

            currency.ExchangeRateToINR = req.NewRate;
            currency.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, $"{currency.Code} rate updated to {req.NewRate}.");
        }

        /// <summary>Seed rates are representative snapshots, not live market data — see class-level TODO (G9) on the Currency model.</summary>
        public async Task SeedCurrenciesAsync()
        {
            if (await _db.Currencies.AnyAsync()) return;
            _db.Currencies.AddRange(
                new Currency { Code = "INR", Symbol = "₹", Name = "Indian Rupee", ExchangeRateToINR = 1 },
                new Currency { Code = "USD", Symbol = "$", Name = "US Dollar", ExchangeRateToINR = 0.012m },
                new Currency { Code = "EUR", Symbol = "€", Name = "Euro", ExchangeRateToINR = 0.011m },
                new Currency { Code = "GBP", Symbol = "£", Name = "British Pound", ExchangeRateToINR = 0.0095m },
                new Currency { Code = "AED", Symbol = "د.إ", Name = "UAE Dirham", ExchangeRateToINR = 0.044m }
            );
            await _db.SaveChangesAsync();
        }

        private static CurrencyDto ToDto(Currency c) => new()
        {
            Code = c.Code, Symbol = c.Symbol, Name = c.Name,
            ExchangeRateToINR = c.ExchangeRateToINR, IsActive = c.IsActive,
        };
    }

    // ══════════════════════════════════════════════════════════════
    // M74: Tax Calculation Service
    // ══════════════════════════════════════════════════════════════
    public class TaxCalculationService
    {
        private readonly AppDbContext _db;
        public TaxCalculationService(AppDbContext db) => _db = db;

        public async Task<TaxCalculationResultDto> CalculateAsync(CalculateTaxRequestDto req)
        {
            // Prefer a region-specific rule (e.g. a US state) over the country-wide one.
            var rule = await _db.TaxRules
                .Where(r => r.IsActive && r.Country == req.Country && (r.AppliesTo == "All" || r.AppliesTo == "Booking"))
                .OrderByDescending(r => r.Region == req.Region && req.Region != null)
                .FirstOrDefaultAsync();

            if (rule == null)
            {
                return new TaxCalculationResultDto
                {
                    SubTotal = req.Amount, TaxRate = 0, TaxAmount = 0, TotalAmount = req.Amount,
                    TaxName = "None", Country = req.Country, RuleFound = false,
                };
            }

            var taxAmount = Math.Round(req.Amount * rule.Rate / 100m, 2);
            return new TaxCalculationResultDto
            {
                SubTotal = req.Amount,
                TaxRate = rule.Rate,
                TaxAmount = taxAmount,
                TotalAmount = req.Amount + taxAmount,
                TaxName = rule.TaxName,
                Country = req.Country,
                RuleFound = true,
            };
        }

        public async Task<List<TaxRuleDto>> GetRulesAsync(string? country = null)
        {
            var query = _db.TaxRules.AsQueryable();
            if (!string.IsNullOrWhiteSpace(country)) query = query.Where(r => r.Country == country);
            var rules = await query.OrderBy(r => r.Country).ThenBy(r => r.Region).ToListAsync();
            return rules.Select(ToDto).ToList();
        }

        public async Task<(bool Success, string Message, TaxRuleDto? Data)> UpsertRuleAsync(UpsertTaxRuleRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Country) || string.IsNullOrWhiteSpace(req.TaxName))
                return (false, "Country and tax name are required.", null);

            TaxRule rule;
            if (req.Id.HasValue)
            {
                var existing = await _db.TaxRules.FirstOrDefaultAsync(r => r.Id == req.Id);
                if (existing == null) return (false, "Tax rule not found.", null);
                rule = existing;
            }
            else
            {
                rule = new TaxRule();
                _db.TaxRules.Add(rule);
            }

            rule.Country = req.Country;
            rule.Region = req.Region;
            rule.TaxName = req.TaxName;
            rule.Rate = req.Rate;
            rule.AppliesTo = req.AppliesTo;

            await _db.SaveChangesAsync();
            return (true, "Tax rule saved.", ToDto(rule));
        }

        /// <summary>Representative starting rates for the countries in the original roadmap — real compliance requires legal/tax-advisor review per market before going live, this just gives the calculation engine something real to compute against.</summary>
        public async Task SeedRulesAsync()
        {
            if (await _db.TaxRules.AnyAsync()) return;
            _db.TaxRules.AddRange(
                new TaxRule { Country = "India", TaxName = "GST", Rate = 5, AppliesTo = "All" },
                new TaxRule { Country = "United States", Region = "California", TaxName = "SalesTax", Rate = 8.5m, AppliesTo = "All" },
                new TaxRule { Country = "United States", TaxName = "SalesTax", Rate = 6, AppliesTo = "All" }, // country-wide fallback for states without a specific rule
                new TaxRule { Country = "United Kingdom", TaxName = "VAT", Rate = 20, AppliesTo = "All" },
                new TaxRule { Country = "United Arab Emirates", TaxName = "VAT", Rate = 5, AppliesTo = "All" }
            );
            await _db.SaveChangesAsync();
        }

        private static TaxRuleDto ToDto(TaxRule r) => new()
        {
            Id = r.Id, Country = r.Country, Region = r.Region, TaxName = r.TaxName,
            Rate = r.Rate, AppliesTo = r.AppliesTo, IsActive = r.IsActive,
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/currency")]
    public class CurrencyController : ControllerBase
    {
        private readonly Services.CurrencyService _svc;
        public CurrencyController(Services.CurrencyService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var data = await _svc.GetSupportedCurrenciesAsync();
            return Ok(new { success = true, data });
        }

        [HttpPost("convert")]
        [AllowAnonymous]
        public async Task<IActionResult> Convert([FromBody] ConvertAmountRequestDto req)
        {
            var (success, message, data) = await _svc.ConvertAsync(req.AmountInINR, req.ToCurrency);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, data });
        }

        [HttpPut("preference")]
        [Authorize]
        public async Task<IActionResult> SetPreference([FromBody] SetPreferredCurrencyRequestDto req)
        {
            var (success, message) = await _svc.SetPreferredCurrencyAsync(UserId, req.Code);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPut("admin/rate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRate([FromBody] UpdateExchangeRateRequestDto req)
        {
            var (success, message) = await _svc.UpdateRateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("admin/seed")]
        [AllowAnonymous]
        public async Task<IActionResult> Seed()
        {
            await _svc.SeedCurrenciesAsync();
            return Ok(new { success = true });
        }
    }

    [ApiController]
    [Route("api/tax")]
    public class TaxController : ControllerBase
    {
        private readonly Services.TaxCalculationService _svc;
        public TaxController(Services.TaxCalculationService svc) => _svc = svc;

        [HttpPost("calculate")]
        [Authorize]
        public async Task<IActionResult> Calculate([FromBody] CalculateTaxRequestDto req)
        {
            var data = await _svc.CalculateAsync(req);
            return Ok(new { success = true, data });
        }

        [HttpGet("admin/rules")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRules([FromQuery] string? country)
        {
            var data = await _svc.GetRulesAsync(country);
            return Ok(new { success = true, data });
        }

        [HttpPost("admin/rules")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpsertRule([FromBody] UpsertTaxRuleRequestDto req)
        {
            var (success, message, data) = await _svc.UpsertRuleAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data });
        }

        [HttpPost("admin/seed")]
        [AllowAnonymous]
        public async Task<IActionResult> Seed()
        {
            await _svc.SeedRulesAsync();
            return Ok(new { success = true });
        }
    }
}
