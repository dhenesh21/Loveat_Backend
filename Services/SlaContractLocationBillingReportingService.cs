using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M133: Enterprise — SLA Contracts ────────────────────────────
    public class SlaContractService
    {
        private readonly AppDbContext _db;
        public SlaContractService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, SlaContractDto? Contract)> CreateAsync(CreateSlaContractRequestDto req)
        {
            if (!await _db.CorporateAccounts.AnyAsync(a => a.Id == req.CorporateAccountId)) return (false, "Corporate account not found.", null);
            var contract = new SlaContract
            {
                CorporateAccountId = req.CorporateAccountId, ContractNumber = $"SLA-{DateTime.UtcNow:yyyy}-{(await _db.SlaContracts.CountAsync() + 1):D4}",
                UptimeGuaranteePercent = req.UptimeGuaranteePercent, SupportResponseTimeMinutes = req.SupportResponseTimeMinutes,
                PenaltyPercentPerBreachHour = req.PenaltyPercentPerBreachHour, MonthlyContractValue = req.MonthlyContractValue,
                StartDate = req.StartDate, EndDate = req.EndDate,
            };
            _db.SlaContracts.Add(contract);
            await _db.SaveChangesAsync();
            return (true, "SLA contract created.", await ToDtoAsync(contract));
        }

        public async Task<List<SlaContractDto>> GetAllAsync()
        {
            var contracts = await _db.SlaContracts.Include(c => c.CorporateAccount).OrderByDescending(c => c.CreatedAt).ToListAsync();
            var result = new List<SlaContractDto>();
            foreach (var c in contracts) result.Add(await ToDtoAsync(c));
            return result;
        }

        // Records a breach and computes the penalty credit as
        // (PenaltyPercentPerBreachHour% of MonthlyContractValue) × BreachHours
        // — a transparent formula stated directly in the contract terms.
        public async Task<(bool Success, string Message, SlaBreachDto? Breach)> RecordBreachAsync(RecordSlaBreachRequestDto req)
        {
            var contract = await _db.SlaContracts.FindAsync(req.SlaContractId);
            if (contract == null) return (false, "SLA contract not found.", null);
            var credit = Math.Round(contract.MonthlyContractValue * contract.PenaltyPercentPerBreachHour / 100 * req.BreachHours, 2);
            var breach = new SlaContractBreach { SlaContractId = req.SlaContractId, Description = req.Description, BreachHours = req.BreachHours, CreditAmount = credit };
            _db.SlaContractBreaches.Add(breach);
            await _db.SaveChangesAsync();
            return (true, $"Breach recorded — ₹{credit} credit issued.", new SlaBreachDto { Description = breach.Description, BreachHours = breach.BreachHours, CreditAmount = breach.CreditAmount, RecordedAt = breach.RecordedAt });
        }

        public async Task<List<SlaBreachDto>> GetBreachesAsync(int slaContractId)
            => (await _db.SlaContractBreaches.Where(b => b.SlaContractId == slaContractId).OrderByDescending(b => b.RecordedAt).ToListAsync())
                .Select(b => new SlaBreachDto { Description = b.Description, BreachHours = b.BreachHours, CreditAmount = b.CreditAmount, RecordedAt = b.RecordedAt }).ToList();

        private async Task<SlaContractDto> ToDtoAsync(SlaContract c)
        {
            var account = c.CorporateAccount ?? await _db.CorporateAccounts.FindAsync(c.CorporateAccountId);
            var totalCredits = await _db.SlaContractBreaches.Where(b => b.SlaContractId == c.Id).SumAsync(b => (decimal?)b.CreditAmount) ?? 0;
            return new SlaContractDto
            {
                Id = c.Id, CorporateAccountId = c.CorporateAccountId, CompanyName = account?.CompanyName, ContractNumber = c.ContractNumber,
                UptimeGuaranteePercent = c.UptimeGuaranteePercent, SupportResponseTimeMinutes = c.SupportResponseTimeMinutes,
                PenaltyPercentPerBreachHour = c.PenaltyPercentPerBreachHour, MonthlyContractValue = c.MonthlyContractValue,
                StartDate = c.StartDate, EndDate = c.EndDate, Status = c.Status, TotalCreditsIssued = totalCredits,
            };
        }
    }

    // ── M134: Enterprise — Multi-Location Management ────────────────
    public class CorporateLocationService
    {
        private readonly AppDbContext _db;
        public CorporateLocationService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, CorporateLocationDto? Location)> CreateAsync(CreateCorporateLocationRequestDto req)
        {
            if (!await _db.CorporateAccounts.AnyAsync(a => a.Id == req.CorporateAccountId)) return (false, "Corporate account not found.", null);
            var loc = new CorporateLocation
            {
                CorporateAccountId = req.CorporateAccountId, LocationName = req.LocationName, Address = req.Address,
                City = req.City, Latitude = req.Latitude, Longitude = req.Longitude, DefaultHeadcount = req.DefaultHeadcount,
            };
            _db.CorporateLocations.Add(loc);
            await _db.SaveChangesAsync();
            return (true, "Location added.", ToDto(loc));
        }

        public async Task<List<CorporateLocationDto>> GetForAccountAsync(int corporateAccountId)
            => (await _db.CorporateLocations.Where(l => l.CorporateAccountId == corporateAccountId).ToListAsync()).Select(ToDto).ToList();

        public async Task<(bool Success, string Message)> DeactivateAsync(int locationId)
        {
            var loc = await _db.CorporateLocations.FindAsync(locationId);
            if (loc == null) return (false, "Location not found.");
            loc.IsActive = false;
            await _db.SaveChangesAsync();
            return (true, "Location deactivated.");
        }

        private static CorporateLocationDto ToDto(CorporateLocation l) => new()
        {
            Id = l.Id, CorporateAccountId = l.CorporateAccountId, LocationName = l.LocationName, Address = l.Address,
            City = l.City, DefaultHeadcount = l.DefaultHeadcount, IsActive = l.IsActive,
        };
    }

    // ── M135: Enterprise — Custom Billing ────────────────────────────
    public class CorporateBillingService
    {
        private readonly AppDbContext _db;
        public CorporateBillingService(AppDbContext db) => _db = db;
        private const decimal GST_PERCENT = 18;

        // Consolidates a month's Fulfilled BulkOrderRequests for the
        // account into one invoice — the enterprise equivalent of the
        // M43 per-booking Invoice flow, which stays untouched.
        public async Task<(bool Success, string Message, CorporateInvoiceDto? Invoice)> GenerateAsync(GenerateCorporateInvoiceRequestDto req)
        {
            if (!DateTime.TryParseExact(req.PeriodKey + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                return (false, "PeriodKey must be in yyyy-MM format.", null);
            var monthEnd = monthStart.AddMonths(1);

            var account = await _db.CorporateAccounts.FindAsync(req.CorporateAccountId);
            if (account == null) return (false, "Corporate account not found.", null);

            var orders = await _db.BulkOrderRequests.Where(o => o.CorporateAccountId == req.CorporateAccountId && o.Status == "Fulfilled" && o.CreatedAt >= monthStart && o.CreatedAt < monthEnd).ToListAsync();
            var subtotal = orders.Sum(o => o.EstimatedTotal);
            var gst = Math.Round(subtotal * GST_PERCENT / 100, 2);

            var invoice = new CorporateInvoice
            {
                CorporateAccountId = req.CorporateAccountId, InvoiceNumber = $"CINV-{req.PeriodKey}-{req.CorporateAccountId:D4}",
                PeriodKey = req.PeriodKey, Subtotal = subtotal, GstAmount = gst, TotalAmount = subtotal + gst,
                OrderCount = orders.Count, DueDate = monthEnd.AddDays(15),
            };
            _db.CorporateInvoices.Add(invoice);

            account.CurrentOutstanding += invoice.TotalAmount;
            await _db.SaveChangesAsync();

            return (true, "Invoice generated.", ToDto(invoice, account.CompanyName));
        }

        public async Task<(bool Success, string Message)> MarkPaidAsync(int invoiceId)
        {
            var invoice = await _db.CorporateInvoices.FindAsync(invoiceId);
            if (invoice == null) return (false, "Invoice not found.");
            if (invoice.Status == "Paid") return (false, "Already paid.");
            invoice.Status = "Paid";
            invoice.PaidAt = DateTime.UtcNow;
            var account = await _db.CorporateAccounts.FindAsync(invoice.CorporateAccountId);
            if (account != null) account.CurrentOutstanding = Math.Max(0, account.CurrentOutstanding - invoice.TotalAmount);
            await _db.SaveChangesAsync();
            return (true, "Invoice marked Paid.");
        }

        public async Task<List<CorporateInvoiceDto>> GetForAccountAsync(int corporateAccountId)
        {
            var account = await _db.CorporateAccounts.FindAsync(corporateAccountId);
            var invoices = await _db.CorporateInvoices.Where(i => i.CorporateAccountId == corporateAccountId).OrderByDescending(i => i.PeriodKey).ToListAsync();
            return invoices.Select(i => ToDto(i, account?.CompanyName)).ToList();
        }

        private static CorporateInvoiceDto ToDto(CorporateInvoice i, string? companyName) => new()
        {
            Id = i.Id, CorporateAccountId = i.CorporateAccountId, CompanyName = companyName, InvoiceNumber = i.InvoiceNumber,
            PeriodKey = i.PeriodKey, Subtotal = i.Subtotal, GstAmount = i.GstAmount, TotalAmount = i.TotalAmount,
            OrderCount = i.OrderCount, Status = i.Status, DueDate = i.DueDate, PaidAt = i.PaidAt,
        };
    }

    // ── M136: Enterprise — Enterprise Reporting ──────────────────────
    public class EnterpriseReportingService
    {
        private readonly AppDbContext _db;
        public EnterpriseReportingService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, EnterpriseUsageDto? Usage)> GenerateAsync(GenerateEnterpriseUsageRequestDto req)
        {
            if (!DateTime.TryParseExact(req.PeriodKey + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                return (false, "PeriodKey must be in yyyy-MM format.", null);
            var monthEnd = monthStart.AddMonths(1);

            var account = await _db.CorporateAccounts.FindAsync(req.CorporateAccountId);
            if (account == null) return (false, "Corporate account not found.", null);

            var orders = await _db.BulkOrderRequests.Where(o => o.CorporateAccountId == req.CorporateAccountId && o.Status == "Fulfilled" && o.CreatedAt >= monthStart && o.CreatedAt < monthEnd).ToListAsync();
            var activeMembers = await _db.CorporateAccountMembers.CountAsync(m => m.CorporateAccountId == req.CorporateAccountId && m.IsActive);
            var activeLocations = await _db.CorporateLocations.CountAsync(l => l.CorporateAccountId == req.CorporateAccountId && l.IsActive);

            var snapshot = await _db.EnterpriseUsageSnapshots.FirstOrDefaultAsync(s => s.CorporateAccountId == req.CorporateAccountId && s.PeriodKey == req.PeriodKey) ?? new EnterpriseUsageSnapshot { CorporateAccountId = req.CorporateAccountId, PeriodKey = req.PeriodKey };
            var isNew = snapshot.Id == 0;
            snapshot.TotalOrders = orders.Count;
            snapshot.TotalMeals = orders.Sum(o => o.MealCount);
            snapshot.TotalSpend = orders.Sum(o => o.EstimatedTotal);
            snapshot.ActiveMembers = activeMembers;
            snapshot.ActiveLocations = activeLocations;
            snapshot.GeneratedAt = DateTime.UtcNow;
            if (isNew) _db.EnterpriseUsageSnapshots.Add(snapshot);
            await _db.SaveChangesAsync();

            return (true, "Generated.", new EnterpriseUsageDto
            {
                CorporateAccountId = req.CorporateAccountId, CompanyName = account.CompanyName, PeriodKey = req.PeriodKey,
                TotalOrders = snapshot.TotalOrders, TotalMeals = snapshot.TotalMeals, TotalSpend = snapshot.TotalSpend,
                ActiveMembers = snapshot.ActiveMembers, ActiveLocations = snapshot.ActiveLocations,
            });
        }

        public async Task<List<EnterpriseUsageDto>> GetHistoryAsync(int corporateAccountId)
        {
            var account = await _db.CorporateAccounts.FindAsync(corporateAccountId);
            var snapshots = await _db.EnterpriseUsageSnapshots.Where(s => s.CorporateAccountId == corporateAccountId).OrderByDescending(s => s.PeriodKey).ToListAsync();
            return snapshots.Select(s => new EnterpriseUsageDto
            {
                CorporateAccountId = corporateAccountId, CompanyName = account?.CompanyName, PeriodKey = s.PeriodKey,
                TotalOrders = s.TotalOrders, TotalMeals = s.TotalMeals, TotalSpend = s.TotalSpend, ActiveMembers = s.ActiveMembers, ActiveLocations = s.ActiveLocations,
            }).ToList();
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/sla-contracts"), Authorize(Roles = "Admin")]
    public class SlaContractController : ControllerBase
    {
        private readonly Services.SlaContractService _svc;
        public SlaContractController(Services.SlaContractService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSlaContractRequestDto req)
        {
            var (success, message, contract) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = contract });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("breaches")]
        public async Task<IActionResult> RecordBreach([FromBody] RecordSlaBreachRequestDto req)
        {
            var (success, message, breach) = await _svc.RecordBreachAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = breach });
        }

        [HttpGet("{id}/breaches")]
        public async Task<IActionResult> GetBreaches(int id) => Ok(new { success = true, data = await _svc.GetBreachesAsync(id) });
    }

    [ApiController, Route("api/corporate-locations"), Authorize(Roles = "Admin")]
    public class CorporateLocationController : ControllerBase
    {
        private readonly Services.CorporateLocationService _svc;
        public CorporateLocationController(Services.CorporateLocationService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCorporateLocationRequestDto req)
        {
            var (success, message, location) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = location });
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetForAccount(int accountId) => Ok(new { success = true, data = await _svc.GetForAccountAsync(accountId) });

        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var (success, message) = await _svc.DeactivateAsync(id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/corporate-billing"), Authorize(Roles = "Admin")]
    public class CorporateBillingController : ControllerBase
    {
        private readonly Services.CorporateBillingService _svc;
        public CorporateBillingController(Services.CorporateBillingService svc) => _svc = svc;

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateCorporateInvoiceRequestDto req)
        {
            var (success, message, invoice) = await _svc.GenerateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = invoice });
        }

        [HttpPost("mark-paid")]
        public async Task<IActionResult> MarkPaid([FromBody] MarkCorporateInvoicePaidRequestDto req)
        {
            var (success, message) = await _svc.MarkPaidAsync(req.InvoiceId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetForAccount(int accountId) => Ok(new { success = true, data = await _svc.GetForAccountAsync(accountId) });
    }

    [ApiController, Route("api/enterprise-reporting"), Authorize(Roles = "Admin")]
    public class EnterpriseReportingController : ControllerBase
    {
        private readonly Services.EnterpriseReportingService _svc;
        public EnterpriseReportingController(Services.EnterpriseReportingService svc) => _svc = svc;

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateEnterpriseUsageRequestDto req)
        {
            var (success, message, usage) = await _svc.GenerateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = usage });
        }

        [HttpGet("account/{accountId}")]
        public async Task<IActionResult> GetHistory(int accountId) => Ok(new { success = true, data = await _svc.GetHistoryAsync(accountId) });
    }
}
