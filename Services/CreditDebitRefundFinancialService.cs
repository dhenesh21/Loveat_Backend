using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M97: Credit Notes ──────────────────────────────────────────
    public class CreditNoteService
    {
        private readonly AppDbContext _db;
        public CreditNoteService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, CreditNoteDto? Note)> IssueAsync(int adminUserId, IssueCreditNoteRequestDto req)
        {
            var invoice = await _db.Invoices.FindAsync(req.InvoiceId);
            if (invoice == null) return (false, "Invoice not found.", null);
            if (req.Amount <= 0 || req.Amount > invoice.TotalAmount) return (false, "Amount must be positive and not exceed the invoice total.", null);

            var gstAmount = Math.Round(req.Amount * invoice.GSTPercent / (100 + invoice.GSTPercent), 2);
            var note = new CreditNote
            {
                CreditNoteNumber = $"CN-{DateTime.UtcNow:yyyy}-{(await _db.CreditNotes.CountAsync() + 1):D5}",
                InvoiceId = req.InvoiceId, CustomerId = invoice.CustomerId, Reason = req.Reason,
                Amount = Math.Round(req.Amount - gstAmount, 2), GstAmount = gstAmount, TotalAmount = req.Amount,
                LinkedRefundRequestId = req.LinkedRefundRequestId, IssuedByAdminId = adminUserId,
            };
            _db.CreditNotes.Add(note);
            await _db.SaveChangesAsync();
            return (true, $"Credit note {note.CreditNoteNumber} issued.", await ToDtoAsync(note));
        }

        public async Task<(bool Success, string Message)> CancelAsync(int creditNoteId)
        {
            var note = await _db.CreditNotes.FindAsync(creditNoteId);
            if (note == null) return (false, "Credit note not found.");
            if (note.Status == "Cancelled") return (false, "Already cancelled.");
            note.Status = "Cancelled";
            await _db.SaveChangesAsync();
            return (true, $"Credit note {note.CreditNoteNumber} cancelled.");
        }

        public async Task<List<CreditNoteDto>> GetAllAsync()
        {
            var notes = await _db.CreditNotes.Include(n => n.Invoice).Include(n => n.Customer).Include(n => n.IssuedByAdmin).OrderByDescending(n => n.IssuedAt).ToListAsync();
            var result = new List<CreditNoteDto>();
            foreach (var n in notes) result.Add(await ToDtoAsync(n));
            return result;
        }

        private Task<CreditNoteDto> ToDtoAsync(CreditNote n) => Task.FromResult(new CreditNoteDto
        {
            Id = n.Id, CreditNoteNumber = n.CreditNoteNumber, InvoiceId = n.InvoiceId, InvoiceNumber = n.Invoice?.InvoiceNumber,
            CustomerId = n.CustomerId, CustomerName = n.Customer?.FullName, Reason = n.Reason, Amount = n.Amount,
            GstAmount = n.GstAmount, TotalAmount = n.TotalAmount, Status = n.Status, IssuedByName = n.IssuedByAdmin?.FullName, IssuedAt = n.IssuedAt,
        });
    }

    // ── M98: Debit Notes ───────────────────────────────────────────
    public class DebitNoteService
    {
        private readonly AppDbContext _db;
        private const decimal DEFAULT_GST_PERCENT = 18;
        public DebitNoteService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, DebitNoteDto? Note)> IssueAsync(int adminUserId, IssueDebitNoteRequestDto req)
        {
            if (req.Amount <= 0) return (false, "Amount must be positive.", null);
            if (!await _db.Users.AnyAsync(u => u.Id == req.PartyId)) return (false, "Party (user) not found.", null);

            var gstAmount = Math.Round(req.Amount * DEFAULT_GST_PERCENT / 100, 2);
            var note = new DebitNote
            {
                DebitNoteNumber = $"DN-{DateTime.UtcNow:yyyy}-{(await _db.DebitNotes.CountAsync() + 1):D5}",
                InvoiceId = req.InvoiceId, PartyType = req.PartyType, PartyId = req.PartyId, Reason = req.Reason,
                Amount = req.Amount, GstAmount = gstAmount, TotalAmount = req.Amount + gstAmount, IssuedByAdminId = adminUserId,
            };
            _db.DebitNotes.Add(note);
            await _db.SaveChangesAsync();
            return (true, $"Debit note {note.DebitNoteNumber} issued.", await ToDtoAsync(note));
        }

        public async Task<(bool Success, string Message)> CancelAsync(int debitNoteId)
        {
            var note = await _db.DebitNotes.FindAsync(debitNoteId);
            if (note == null) return (false, "Debit note not found.");
            if (note.Status == "Cancelled") return (false, "Already cancelled.");
            note.Status = "Cancelled";
            await _db.SaveChangesAsync();
            return (true, $"Debit note {note.DebitNoteNumber} cancelled.");
        }

        public async Task<List<DebitNoteDto>> GetAllAsync()
        {
            var notes = await _db.DebitNotes.Include(n => n.Party).Include(n => n.IssuedByAdmin).OrderByDescending(n => n.IssuedAt).ToListAsync();
            var result = new List<DebitNoteDto>();
            foreach (var n in notes) result.Add(await ToDtoAsync(n));
            return result;
        }

        private Task<DebitNoteDto> ToDtoAsync(DebitNote n) => Task.FromResult(new DebitNoteDto
        {
            Id = n.Id, DebitNoteNumber = n.DebitNoteNumber, InvoiceId = n.InvoiceId, PartyType = n.PartyType, PartyId = n.PartyId,
            PartyName = n.Party?.FullName, Reason = n.Reason, Amount = n.Amount, GstAmount = n.GstAmount, TotalAmount = n.TotalAmount,
            Status = n.Status, IssuedByName = n.IssuedByAdmin?.FullName, IssuedAt = n.IssuedAt,
        });
    }

    // ── M99: Refund Dashboard ──────────────────────────────────────
    public class RefundDashboardService
    {
        private readonly AppDbContext _db;
        private readonly CreditNoteService _creditNoteSvc;
        public RefundDashboardService(AppDbContext db, CreditNoteService creditNoteSvc) { _db = db; _creditNoteSvc = creditNoteSvc; }

        public async Task<RefundRequestDto> CreateAsync(CreateRefundRequestDto req)
        {
            var request = new RefundRequest { BookingId = req.BookingId, CustomerId = req.CustomerId, Amount = req.Amount, Reason = req.Reason, RefundMethod = req.RefundMethod };
            _db.RefundRequests.Add(request);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(request);
        }

        public async Task<(bool Success, string Message)> DecideAsync(int adminUserId, DecideRefundRequestDto req)
        {
            var request = await _db.RefundRequests.FindAsync(req.RefundRequestId);
            if (request == null) return (false, "Refund request not found.");
            if (request.Status != "Requested") return (false, $"Only Requested refunds can be decided (current: {request.Status}).");
            request.Status = req.Approve ? "Approved" : "Rejected";
            request.RejectionReason = req.Approve ? null : req.RejectionReason;
            request.ProcessedByAdminId = adminUserId;
            if (!req.Approve) request.ProcessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, req.Approve ? "Refund approved — ready to process." : "Refund rejected.");
        }

        // Marks the refund Processed and, unless opted out, issues a
        // CreditNote against the booking's invoice to formalize it.
        public async Task<(bool Success, string Message)> ProcessAsync(int adminUserId, ProcessRefundRequestDto req)
        {
            var request = await _db.RefundRequests.FindAsync(req.RefundRequestId);
            if (request == null) return (false, "Refund request not found.");
            if (request.Status != "Approved") return (false, $"Only Approved refunds can be processed (current: {request.Status}).");

            request.Status = "Processed";
            request.TransactionRef = req.TransactionRef;
            request.ProcessedAt = DateTime.UtcNow;

            if (req.IssueCreditNote)
            {
                var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.BookingId == request.BookingId);
                if (invoice != null)
                {
                    var (ok, _, note) = await _creditNoteSvc.IssueAsync(adminUserId, new IssueCreditNoteRequestDto
                    {
                        InvoiceId = invoice.Id, Reason = $"Refund: {request.Reason}", Amount = request.Amount, LinkedRefundRequestId = request.Id,
                    });
                    if (ok && note != null) request.LinkedCreditNoteId = note.Id;
                }
            }

            await _db.SaveChangesAsync();
            return (true, "Refund processed.");
        }

        public async Task<List<RefundRequestDto>> GetAllAsync(string? status = null)
        {
            var q = _db.RefundRequests.Include(r => r.Customer).Include(r => r.ProcessedByAdmin).AsQueryable();
            if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);
            var list = await q.OrderByDescending(r => r.RequestedAt).ToListAsync();
            var result = new List<RefundRequestDto>();
            foreach (var r in list) result.Add(await ToDtoAsync(r));
            return result;
        }

        public async Task<RefundDashboardStatsDto> GetStatsAsync()
        {
            var all = await _db.RefundRequests.ToListAsync();
            var processed = all.Where(r => r.Status == "Processed" && r.ProcessedAt.HasValue).ToList();
            var avgHours = processed.Count == 0 ? 0 : processed.Average(r => (r.ProcessedAt!.Value - r.RequestedAt).TotalHours);
            return new RefundDashboardStatsDto
            {
                TotalRequested = all.Count(r => r.Status == "Requested"), TotalApproved = all.Count(r => r.Status == "Approved"),
                TotalRejected = all.Count(r => r.Status == "Rejected"), TotalProcessed = processed.Count,
                TotalAmountProcessed = processed.Sum(r => r.Amount), TotalAmountPending = all.Where(r => r.Status is "Requested" or "Approved").Sum(r => r.Amount),
                AvgProcessingHours = Math.Round(avgHours, 1),
            };
        }

        private async Task<RefundRequestDto> ToDtoAsync(RefundRequest r)
        {
            var customer = r.Customer ?? await _db.Users.FindAsync(r.CustomerId);
            var processedBy = r.ProcessedByAdmin ?? (r.ProcessedByAdminId.HasValue ? await _db.Users.FindAsync(r.ProcessedByAdminId.Value) : null);
            return new RefundRequestDto
            {
                Id = r.Id, BookingId = r.BookingId, CustomerId = r.CustomerId, CustomerName = customer?.FullName, Amount = r.Amount,
                Reason = r.Reason, Status = r.Status, RefundMethod = r.RefundMethod, TransactionRef = r.TransactionRef,
                RejectionReason = r.RejectionReason, LinkedCreditNoteId = r.LinkedCreditNoteId, ProcessedByName = processedBy?.FullName,
                RequestedAt = r.RequestedAt, ProcessedAt = r.ProcessedAt,
            };
        }
    }

    // ── M100: Financial Reports ────────────────────────────────────
    public class FinancialReportService
    {
        private readonly AppDbContext _db;
        public FinancialReportService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, FinancialReportDto? Report)> GenerateAsync(int adminUserId, GenerateFinancialReportRequestDto req)
        {
            if (!DateTime.TryParseExact(req.PeriodKey + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var monthStart))
                return (false, "PeriodKey must be in yyyy-MM format.", null);
            var monthEnd = monthStart.AddMonths(1);

            var invoices = await _db.Invoices.Where(i => i.CreatedAt >= monthStart && i.CreatedAt < monthEnd).ToListAsync();
            var commissions = await _db.CommissionLedgers.Where(l => l.CreatedAt >= monthStart && l.CreatedAt < monthEnd).ToListAsync();
            var tds = await _db.TdsDeductions.Where(d => d.DeductedAt >= monthStart && d.DeductedAt < monthEnd).ToListAsync();
            var refunds = await _db.RefundRequests.Where(r => r.Status == "Processed" && r.ProcessedAt >= monthStart && r.ProcessedAt < monthEnd).ToListAsync();
            var creditNotes = await _db.CreditNotes.Where(c => c.Status == "Issued" && c.IssuedAt >= monthStart && c.IssuedAt < monthEnd).ToListAsync();
            var debitNotes = await _db.DebitNotes.Where(d => d.Status == "Issued" && d.IssuedAt >= monthStart && d.IssuedAt < monthEnd).ToListAsync();

            var commissionEarned = commissions.Sum(c => c.PlatformAmount);
            var refundTotal = refunds.Sum(r => r.Amount);
            var creditTotal = creditNotes.Sum(c => c.TotalAmount);
            var debitTotal = debitNotes.Sum(d => d.TotalAmount);

            var snapshot = await _db.FinancialReportSnapshots.FirstOrDefaultAsync(s => s.PeriodKey == req.PeriodKey) ?? new FinancialReportSnapshot { PeriodKey = req.PeriodKey };
            var isNew = snapshot.Id == 0;
            snapshot.GrossRevenue = invoices.Sum(i => i.TotalAmount);
            snapshot.GstCollected = invoices.Sum(i => i.GSTAmount);
            snapshot.PlatformCommissionEarned = commissionEarned;
            snapshot.TdsDeducted = tds.Sum(d => d.TdsAmount);
            snapshot.TotalRefundsIssued = refundTotal;
            snapshot.TotalCreditNotesValue = creditTotal;
            snapshot.TotalDebitNotesValue = debitTotal;
            snapshot.NetPlatformRevenue = commissionEarned - refundTotal + debitTotal - creditTotal;
            snapshot.InvoiceCount = invoices.Count;
            snapshot.RefundCount = refunds.Count;
            snapshot.GeneratedByAdminId = adminUserId;
            snapshot.GeneratedAt = DateTime.UtcNow;
            if (isNew) _db.FinancialReportSnapshots.Add(snapshot);
            await _db.SaveChangesAsync();

            return (true, "Report generated.", await ToDtoAsync(snapshot));
        }

        public async Task<List<FinancialReportDto>> GetAllAsync()
        {
            var snapshots = await _db.FinancialReportSnapshots.Include(s => s.GeneratedByAdmin).OrderByDescending(s => s.PeriodKey).ToListAsync();
            var result = new List<FinancialReportDto>();
            foreach (var s in snapshots) result.Add(await ToDtoAsync(s));
            return result;
        }

        private Task<FinancialReportDto> ToDtoAsync(FinancialReportSnapshot s) => Task.FromResult(new FinancialReportDto
        {
            PeriodKey = s.PeriodKey, GrossRevenue = s.GrossRevenue, GstCollected = s.GstCollected, PlatformCommissionEarned = s.PlatformCommissionEarned,
            TdsDeducted = s.TdsDeducted, TotalRefundsIssued = s.TotalRefundsIssued, TotalCreditNotesValue = s.TotalCreditNotesValue,
            TotalDebitNotesValue = s.TotalDebitNotesValue, NetPlatformRevenue = s.NetPlatformRevenue, InvoiceCount = s.InvoiceCount,
            RefundCount = s.RefundCount, GeneratedByName = s.GeneratedByAdmin?.FullName, GeneratedAt = s.GeneratedAt,
        });
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/credit-notes"), Authorize(Roles = "Admin")]
    public class CreditNoteController : ControllerBase
    {
        private readonly Services.CreditNoteService _svc;
        public CreditNoteController(Services.CreditNoteService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Issue([FromBody] IssueCreditNoteRequestDto req)
        {
            var (success, message, note) = await _svc.IssueAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = note });
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] CancelCreditNoteRequestDto req)
        {
            var (success, message) = await _svc.CancelAsync(req.CreditNoteId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });
    }

    [ApiController, Route("api/debit-notes"), Authorize(Roles = "Admin")]
    public class DebitNoteController : ControllerBase
    {
        private readonly Services.DebitNoteService _svc;
        public DebitNoteController(Services.DebitNoteService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Issue([FromBody] IssueDebitNoteRequestDto req)
        {
            var (success, message, note) = await _svc.IssueAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = note });
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> Cancel([FromBody] CancelDebitNoteRequestDto req)
        {
            var (success, message) = await _svc.CancelAsync(req.DebitNoteId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });
    }

    [ApiController, Route("api/refund-dashboard")]
    public class RefundDashboardController : ControllerBase
    {
        private readonly Services.RefundDashboardService _svc;
        public RefundDashboardController(Services.RefundDashboardService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost, Authorize]
        public async Task<IActionResult> Create([FromBody] CreateRefundRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpPost("decide"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Decide([FromBody] DecideRefundRequestDto req)
        {
            var (success, message) = await _svc.DecideAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("process"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Process([FromBody] ProcessRefundRequestDto req)
        {
            var (success, message) = await _svc.ProcessAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet, Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? status) => Ok(new { success = true, data = await _svc.GetAllAsync(status) });

        [HttpGet("stats"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStats() => Ok(new { success = true, data = await _svc.GetStatsAsync() });
    }

    [ApiController, Route("api/financial-reports"), Authorize(Roles = "Admin")]
    public class FinancialReportController : ControllerBase
    {
        private readonly Services.FinancialReportService _svc;
        public FinancialReportController(Services.FinancialReportService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GenerateFinancialReportRequestDto req)
        {
            var (success, message, report) = await _svc.GenerateAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = report });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });
    }
}
