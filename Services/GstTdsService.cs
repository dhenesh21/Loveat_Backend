using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M93: GST Management ────────────────────────────────────────
    public class GstManagementService
    {
        private readonly AppDbContext _db;
        public GstManagementService(AppDbContext db) => _db = db;

        public async Task<ChefGstProfileDto> UpsertChefProfileAsync(UpsertChefGstProfileRequestDto req)
        {
            var profile = await _db.ChefGstProfiles.FirstOrDefaultAsync(p => p.ChefId == req.ChefId);
            if (profile == null) { profile = new ChefGstProfile { ChefId = req.ChefId }; _db.ChefGstProfiles.Add(profile); }
            profile.Gstin = req.Gstin;
            profile.LegalBusinessName = req.LegalBusinessName;
            profile.PlaceOfSupplyState = req.PlaceOfSupplyState;
            profile.IsGstRegistered = req.IsGstRegistered;
            profile.IsVerified = false; // any edit requires re-verification
            profile.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return await ToDtoAsync(profile);
        }

        public async Task<(bool Success, string Message)> VerifyAsync(int adminUserId, VerifyChefGstRequestDto req)
        {
            var profile = await _db.ChefGstProfiles.FindAsync(req.ChefGstProfileId);
            if (profile == null) return (false, "GST profile not found.");
            if (req.Approve && profile.IsGstRegistered && string.IsNullOrWhiteSpace(profile.Gstin))
                return (false, "Cannot verify — GSTIN is required for a GST-registered vendor.");
            profile.IsVerified = req.Approve;
            profile.VerifiedByAdminId = req.Approve ? adminUserId : null;
            profile.VerifiedAt = req.Approve ? DateTime.UtcNow : null;
            await _db.SaveChangesAsync();
            return (true, req.Approve ? "GST profile verified." : "Verification revoked.");
        }

        public async Task<List<ChefGstProfileDto>> GetAllProfilesAsync()
        {
            var profiles = await _db.ChefGstProfiles.Include(p => p.Chef).Include(p => p.VerifiedByAdmin).OrderByDescending(p => p.UpdatedAt).ToListAsync();
            var result = new List<ChefGstProfileDto>();
            foreach (var p in profiles) result.Add(await ToDtoAsync(p));
            return result;
        }

        public async Task<GstHsnSacCodeDto> UpsertHsnCodeAsync(UpsertHsnSacCodeRequestDto req)
        {
            var code = await _db.GstHsnSacCodes.FirstOrDefaultAsync(c => c.Code == req.Code);
            if (code == null) { code = new GstHsnSacCode { Code = req.Code }; _db.GstHsnSacCodes.Add(code); }
            code.Description = req.Description;
            code.RatePercent = req.RatePercent;
            code.AppliesTo = req.AppliesTo;
            await _db.SaveChangesAsync();
            return new GstHsnSacCodeDto { Id = code.Id, Code = code.Code, Description = code.Description, RatePercent = code.RatePercent, AppliesTo = code.AppliesTo, IsActive = code.IsActive };
        }

        public async Task<List<GstHsnSacCodeDto>> GetHsnCodesAsync()
            => (await _db.GstHsnSacCodes.OrderBy(c => c.Code).ToListAsync())
                .Select(c => new GstHsnSacCodeDto { Id = c.Id, Code = c.Code, Description = c.Description, RatePercent = c.RatePercent, AppliesTo = c.AppliesTo, IsActive = c.IsActive }).ToList();

        // Aggregates the existing Invoice table for the given month into a
        // return summary. Regeneration overwrites an Open period's totals
        // (in case invoices were amended) but never a Filed one.
        public async Task<(bool Success, string Message, GstReturnPeriodDto? Period)> GenerateReturnAsync(GenerateGstReturnRequestDto req)
        {
            if (!DateTime.TryParseExact(req.PeriodKey + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                return (false, "PeriodKey must be in yyyy-MM format.", null);
            var monthEnd = monthStart.AddMonths(1);

            var existing = await _db.GstReturnPeriods.FirstOrDefaultAsync(p => p.PeriodKey == req.PeriodKey && p.ReturnType == req.ReturnType);
            if (existing != null && existing.Status == "Filed") return (false, "This period is already Filed and cannot be regenerated.", null);

            var invoices = await _db.Invoices.Where(i => i.CreatedAt >= monthStart && i.CreatedAt < monthEnd).ToListAsync();
            var taxableValue = invoices.Sum(i => i.SubTotal);
            var gstCollected = invoices.Sum(i => i.SubTotal * i.GSTPercent / 100);

            var period = existing ?? new GstReturnPeriod { PeriodKey = req.PeriodKey, ReturnType = req.ReturnType };
            period.TotalInvoiceCount = invoices.Count;
            period.TotalTaxableValue = taxableValue;
            period.TotalGstCollected = gstCollected;
            period.GeneratedAt = DateTime.UtcNow;
            if (existing == null) _db.GstReturnPeriods.Add(period);
            await _db.SaveChangesAsync();

            return (true, "Return summary generated.", await ToPeriodDtoAsync(period));
        }

        public async Task<(bool Success, string Message)> MarkFiledAsync(int adminUserId, MarkGstReturnFiledRequestDto req)
        {
            var period = await _db.GstReturnPeriods.FindAsync(req.ReturnPeriodId);
            if (period == null) return (false, "Return period not found.");
            period.Status = "Filed";
            period.FilingReference = req.FilingReference;
            period.FiledByAdminId = adminUserId;
            period.FiledAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, $"{period.ReturnType} for {period.PeriodKey} marked Filed.");
        }

        public async Task<List<GstReturnPeriodDto>> GetReturnsAsync()
        {
            var periods = await _db.GstReturnPeriods.Include(p => p.FiledByAdmin).OrderByDescending(p => p.PeriodKey).ToListAsync();
            var result = new List<GstReturnPeriodDto>();
            foreach (var p in periods) result.Add(await ToPeriodDtoAsync(p));
            return result;
        }

        private async Task<ChefGstProfileDto> ToDtoAsync(ChefGstProfile p)
        {
            var chef = p.Chef ?? await _db.Users.FindAsync(p.ChefId);
            var verifier = p.VerifiedByAdmin ?? (p.VerifiedByAdminId.HasValue ? await _db.Users.FindAsync(p.VerifiedByAdminId.Value) : null);
            return new ChefGstProfileDto
            {
                Id = p.Id, ChefId = p.ChefId, ChefName = chef?.FullName, Gstin = p.Gstin, LegalBusinessName = p.LegalBusinessName,
                PlaceOfSupplyState = p.PlaceOfSupplyState, IsGstRegistered = p.IsGstRegistered, IsVerified = p.IsVerified,
                VerifiedByName = verifier?.FullName, VerifiedAt = p.VerifiedAt,
            };
        }

        private async Task<GstReturnPeriodDto> ToPeriodDtoAsync(GstReturnPeriod p)
        {
            var filer = p.FiledByAdmin ?? (p.FiledByAdminId.HasValue ? await _db.Users.FindAsync(p.FiledByAdminId.Value) : null);
            return new GstReturnPeriodDto
            {
                Id = p.Id, PeriodKey = p.PeriodKey, ReturnType = p.ReturnType, TotalInvoiceCount = p.TotalInvoiceCount,
                TotalTaxableValue = p.TotalTaxableValue, TotalGstCollected = p.TotalGstCollected, Status = p.Status,
                FilingReference = p.FilingReference, FiledByName = filer?.FullName, FiledAt = p.FiledAt, GeneratedAt = p.GeneratedAt,
            };
        }
    }

    // ── M94: TDS Management ────────────────────────────────────────
    public class TdsManagementService
    {
        private readonly AppDbContext _db;
        public TdsManagementService(AppDbContext db) => _db = db;

        public static string GetFinancialYear(DateTime d) => d.Month >= 4 ? $"{d.Year}-{(d.Year + 1) % 100:D2}" : $"{d.Year - 1}-{d.Year % 100:D2}";
        public static string GetQuarter(DateTime d) => d.Month switch { >= 4 and <= 6 => "Q1", >= 7 and <= 9 => "Q2", >= 10 and <= 12 => "Q3", _ => "Q4" };

        public async Task<TdsRateConfigDto> UpsertRateConfigAsync(UpsertTdsRateConfigRequestDto req)
        {
            var config = await _db.TdsRateConfigs.FirstOrDefaultAsync(c => c.Section == req.Section);
            if (config == null) { config = new TdsRateConfig { Section = req.Section }; _db.TdsRateConfigs.Add(config); }
            config.Description = req.Description;
            config.RatePercent = req.RatePercent;
            config.AnnualThresholdAmount = req.AnnualThresholdAmount;
            config.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return new TdsRateConfigDto { Id = config.Id, Section = config.Section, Description = config.Description, RatePercent = config.RatePercent, AnnualThresholdAmount = config.AnnualThresholdAmount, IsActive = config.IsActive };
        }

        public async Task<List<TdsRateConfigDto>> GetRateConfigsAsync()
            => (await _db.TdsRateConfigs.OrderBy(c => c.Section).ToListAsync())
                .Select(c => new TdsRateConfigDto { Id = c.Id, Section = c.Section, Description = c.Description, RatePercent = c.RatePercent, AnnualThresholdAmount = c.AnnualThresholdAmount, IsActive = c.IsActive }).ToList();

        // Computes and records TDS for a chef payout. Only deducts once the
        // chef's cumulative gross for the financial year crosses the
        // section's threshold — before that, TDS is 0 (still logged, for
        // an accurate running total).
        public async Task<(bool Success, string Message, TdsDeductionDto? Deduction)> RecordDeductionAsync(RecordTdsDeductionRequestDto req)
        {
            var config = await _db.TdsRateConfigs.FirstOrDefaultAsync(c => c.Section == req.Section && c.IsActive);
            if (config == null) return (false, $"No active TDS rate config for section {req.Section}.", null);

            var now = DateTime.UtcNow;
            var fy = GetFinancialYear(now);
            var quarter = GetQuarter(now);

            var yearStart = now.Month >= 4 ? new DateTime(now.Year, 4, 1, 0, 0, 0, DateTimeKind.Utc) : new DateTime(now.Year - 1, 4, 1, 0, 0, 0, DateTimeKind.Utc);
            var priorGross = await _db.TdsDeductions.Where(d => d.ChefId == req.ChefId && d.FinancialYear == fy).SumAsync(d => (decimal?)d.GrossAmount) ?? 0;
            var cumulativeGross = priorGross + req.GrossAmount;

            var tdsAmount = cumulativeGross > config.AnnualThresholdAmount ? Math.Round(req.GrossAmount * config.RatePercent / 100, 2) : 0;

            var deduction = new TdsDeduction
            {
                ChefId = req.ChefId, SettlementRequestId = req.SettlementRequestId, Section = req.Section,
                GrossAmount = req.GrossAmount, TdsRatePercent = tdsAmount > 0 ? config.RatePercent : 0, TdsAmount = tdsAmount,
                NetPayableAmount = req.GrossAmount - tdsAmount, FinancialYear = fy, Quarter = quarter,
            };
            _db.TdsDeductions.Add(deduction);
            await _db.SaveChangesAsync();

            var chef = await _db.Users.FindAsync(req.ChefId);
            return (true, tdsAmount > 0 ? $"TDS of ₹{tdsAmount} deducted." : "Below annual threshold — no TDS deducted this time.", new TdsDeductionDto
            {
                Id = deduction.Id, ChefId = deduction.ChefId, ChefName = chef?.FullName, Section = deduction.Section,
                GrossAmount = deduction.GrossAmount, TdsRatePercent = deduction.TdsRatePercent, TdsAmount = deduction.TdsAmount,
                NetPayableAmount = deduction.NetPayableAmount, FinancialYear = deduction.FinancialYear, Quarter = deduction.Quarter, DeductedAt = deduction.DeductedAt,
            });
        }

        public async Task<ChefTdsSummaryDto> GetChefSummaryAsync(int chefId, string financialYear)
        {
            var deductions = await _db.TdsDeductions.Where(d => d.ChefId == chefId && d.FinancialYear == financialYear).ToListAsync();
            var config = await _db.TdsRateConfigs.FirstOrDefaultAsync(c => c.Section == "194O" && c.IsActive);
            var totalGross = deductions.Sum(d => d.GrossAmount);
            return new ChefTdsSummaryDto
            {
                ChefId = chefId, FinancialYear = financialYear, TotalGrossAmount = totalGross,
                TotalTdsDeducted = deductions.Sum(d => d.TdsAmount), DeductionCount = deductions.Count,
                ThresholdCrossed = config != null && totalGross > config.AnnualThresholdAmount,
            };
        }

        public async Task<(bool Success, string Message, TdsReturnDto? Return)> GenerateReturnAsync(GenerateTdsReturnRequestDto req)
        {
            var existing = await _db.TdsReturns.FirstOrDefaultAsync(r => r.FinancialYear == req.FinancialYear && r.Quarter == req.Quarter);
            if (existing != null && existing.Status == "Filed") return (false, "This quarter's return is already Filed.", null);

            var deductions = await _db.TdsDeductions.Where(d => d.FinancialYear == req.FinancialYear && d.Quarter == req.Quarter).ToListAsync();
            var ret = existing ?? new TdsReturn { FinancialYear = req.FinancialYear, Quarter = req.Quarter };
            ret.TotalDeducteeCount = deductions.Select(d => d.ChefId).Distinct().Count();
            ret.TotalGrossAmount = deductions.Sum(d => d.GrossAmount);
            ret.TotalTdsDeducted = deductions.Sum(d => d.TdsAmount);
            ret.GeneratedAt = DateTime.UtcNow;
            if (existing == null) _db.TdsReturns.Add(ret);
            await _db.SaveChangesAsync();

            return (true, "Quarterly TDS return generated.", ToReturnDto(ret, null));
        }

        public async Task<(bool Success, string Message)> MarkReturnFiledAsync(int adminUserId, MarkTdsReturnFiledRequestDto req)
        {
            var ret = await _db.TdsReturns.FindAsync(req.ReturnId);
            if (ret == null) return (false, "TDS return not found.");
            ret.Status = "Filed";
            ret.FilingReference = req.FilingReference;
            ret.FiledByAdminId = adminUserId;
            ret.FiledAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, $"TDS return {ret.Quarter} {ret.FinancialYear} marked Filed.");
        }

        public async Task<List<TdsReturnDto>> GetReturnsAsync()
        {
            var returns = await _db.TdsReturns.Include(r => r.FiledByAdmin).OrderByDescending(r => r.FinancialYear).ThenByDescending(r => r.Quarter).ToListAsync();
            return returns.Select(r => ToReturnDto(r, r.FiledByAdmin?.FullName)).ToList();
        }

        public async Task<(bool Success, string Message, TdsCertificateDto? Certificate)> IssueCertificateAsync(IssueTdsCertificateRequestDto req)
        {
            var deductions = await _db.TdsDeductions.Where(d => d.ChefId == req.ChefId && d.FinancialYear == req.FinancialYear && d.Quarter == req.Quarter).ToListAsync();
            if (deductions.Count == 0) return (false, "No TDS deductions found for this chef in the given period.", null);

            var cert = new TdsCertificate
            {
                ChefId = req.ChefId, FinancialYear = req.FinancialYear, Quarter = req.Quarter,
                CertificateNumber = $"LOVEAT-16A-{req.FinancialYear}-{req.Quarter}-{req.ChefId:D6}",
                TotalGrossAmount = deductions.Sum(d => d.GrossAmount), TotalTdsAmount = deductions.Sum(d => d.TdsAmount),
            };
            _db.TdsCertificates.Add(cert);
            await _db.SaveChangesAsync();
            var chef = await _db.Users.FindAsync(req.ChefId);
            return (true, "Certificate issued.", new TdsCertificateDto
            {
                Id = cert.Id, ChefId = cert.ChefId, ChefName = chef?.FullName, FinancialYear = cert.FinancialYear, Quarter = cert.Quarter,
                CertificateNumber = cert.CertificateNumber, TotalGrossAmount = cert.TotalGrossAmount, TotalTdsAmount = cert.TotalTdsAmount, IssuedAt = cert.IssuedAt,
            });
        }

        private static TdsReturnDto ToReturnDto(TdsReturn r, string? filedByName) => new()
        {
            Id = r.Id, FinancialYear = r.FinancialYear, Quarter = r.Quarter, TotalDeducteeCount = r.TotalDeducteeCount,
            TotalGrossAmount = r.TotalGrossAmount, TotalTdsDeducted = r.TotalTdsDeducted, Status = r.Status,
            FilingReference = r.FilingReference, FiledByName = filedByName, FiledAt = r.FiledAt,
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/gst-management"), Authorize(Roles = "Admin")]
    public class GstManagementController : ControllerBase
    {
        private readonly Services.GstManagementService _svc;
        public GstManagementController(Services.GstManagementService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("chef-profile")]
        public async Task<IActionResult> UpsertProfile([FromBody] UpsertChefGstProfileRequestDto req) => Ok(new { success = true, data = await _svc.UpsertChefProfileAsync(req) });

        [HttpGet("chef-profiles")]
        public async Task<IActionResult> GetProfiles() => Ok(new { success = true, data = await _svc.GetAllProfilesAsync() });

        [HttpPost("chef-profile/verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyChefGstRequestDto req)
        {
            var (success, message) = await _svc.VerifyAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("hsn-codes")]
        public async Task<IActionResult> UpsertHsn([FromBody] UpsertHsnSacCodeRequestDto req) => Ok(new { success = true, data = await _svc.UpsertHsnCodeAsync(req) });

        [HttpGet("hsn-codes")]
        public async Task<IActionResult> GetHsn() => Ok(new { success = true, data = await _svc.GetHsnCodesAsync() });

        [HttpPost("returns/generate")]
        public async Task<IActionResult> GenerateReturn([FromBody] GenerateGstReturnRequestDto req)
        {
            var (success, message, period) = await _svc.GenerateReturnAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = period });
        }

        [HttpPost("returns/mark-filed")]
        public async Task<IActionResult> MarkFiled([FromBody] MarkGstReturnFiledRequestDto req)
        {
            var (success, message) = await _svc.MarkFiledAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("returns")]
        public async Task<IActionResult> GetReturns() => Ok(new { success = true, data = await _svc.GetReturnsAsync() });
    }

    [ApiController, Route("api/tds-management"), Authorize(Roles = "Admin")]
    public class TdsManagementController : ControllerBase
    {
        private readonly Services.TdsManagementService _svc;
        public TdsManagementController(Services.TdsManagementService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("rate-config")]
        public async Task<IActionResult> UpsertConfig([FromBody] UpsertTdsRateConfigRequestDto req) => Ok(new { success = true, data = await _svc.UpsertRateConfigAsync(req) });

        [HttpGet("rate-config")]
        public async Task<IActionResult> GetConfigs() => Ok(new { success = true, data = await _svc.GetRateConfigsAsync() });

        [HttpPost("deductions")]
        public async Task<IActionResult> RecordDeduction([FromBody] RecordTdsDeductionRequestDto req)
        {
            var (success, message, deduction) = await _svc.RecordDeductionAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = deduction });
        }

        [HttpGet("chefs/{chefId}/summary")]
        public async Task<IActionResult> GetChefSummary(int chefId, [FromQuery] string financialYear) => Ok(new { success = true, data = await _svc.GetChefSummaryAsync(chefId, financialYear) });

        [HttpPost("returns/generate")]
        public async Task<IActionResult> GenerateReturn([FromBody] GenerateTdsReturnRequestDto req)
        {
            var (success, message, ret) = await _svc.GenerateReturnAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = ret });
        }

        [HttpPost("returns/mark-filed")]
        public async Task<IActionResult> MarkFiled([FromBody] MarkTdsReturnFiledRequestDto req)
        {
            var (success, message) = await _svc.MarkReturnFiledAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("returns")]
        public async Task<IActionResult> GetReturns() => Ok(new { success = true, data = await _svc.GetReturnsAsync() });

        [HttpPost("certificates")]
        public async Task<IActionResult> IssueCertificate([FromBody] IssueTdsCertificateRequestDto req)
        {
            var (success, message, cert) = await _svc.IssueCertificateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = cert });
        }
    }
}
