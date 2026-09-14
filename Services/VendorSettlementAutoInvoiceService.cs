using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M95: Vendor Settlement ──────────────────────────────────────
    public class VendorSettlementService
    {
        private readonly AppDbContext _db;
        private readonly SettlementService _settlementSvc;
        public VendorSettlementService(AppDbContext db, SettlementService settlementSvc) { _db = db; _settlementSvc = settlementSvc; }

        // Pulls every Pending SettlementRequest raised within the period,
        // skips any chef currently on hold, attaches the latest TDS
        // deduction on file (if any) so the batch shows net payable.
        public async Task<SettlementBatchDto> CreateBatchAsync(int adminUserId, CreateSettlementBatchRequestDto req)
        {
            var heldChefIds = await _db.VendorSettlementHolds.Where(h => h.IsActive).Select(h => h.ChefId).ToListAsync();
            var pending = await _db.SettlementRequests.Include(r => r.Chef)
                .Where(r => r.Status == "Pending" && r.RequestedAt >= req.PeriodStart && r.RequestedAt <= req.PeriodEnd)
                .ToListAsync();

            var batch = new SettlementBatch
            {
                BatchNumber = $"SB-{DateTime.UtcNow:yyyyMMdd}-{(await _db.SettlementBatches.CountAsync() + 1):D4}",
                PeriodStart = req.PeriodStart, PeriodEnd = req.PeriodEnd, CreatedByAdminId = adminUserId,
            };
            _db.SettlementBatches.Add(batch);
            await _db.SaveChangesAsync();

            foreach (var sr in pending)
            {
                var onHold = heldChefIds.Contains(sr.ChefId);
                var tds = await _db.TdsDeductions.Where(d => d.SettlementRequestId == sr.Id).OrderByDescending(d => d.DeductedAt).FirstOrDefaultAsync();
                var tdsAmount = tds?.TdsAmount ?? 0;
                _db.SettlementBatchItems.Add(new SettlementBatchItem
                {
                    SettlementBatchId = batch.Id, SettlementRequestId = sr.Id, ChefId = sr.ChefId,
                    GrossAmount = sr.Amount, TdsAmount = tdsAmount, NetAmount = sr.Amount - tdsAmount,
                    Status = onHold ? "Excluded" : "Included",
                    FailureReason = onHold ? "Vendor is on hold" : null,
                });
            }
            await _db.SaveChangesAsync();

            var included = await _db.SettlementBatchItems.Where(i => i.SettlementBatchId == batch.Id && i.Status == "Included").ToListAsync();
            batch.VendorCount = included.Select(i => i.ChefId).Distinct().Count();
            batch.TotalGrossAmount = included.Sum(i => i.GrossAmount);
            batch.TotalTdsAmount = included.Sum(i => i.TdsAmount);
            batch.TotalNetAmount = included.Sum(i => i.NetAmount);
            await _db.SaveChangesAsync();

            return await ToDtoAsync(batch);
        }

        public async Task<(bool Success, string Message)> ApproveAsync(int adminUserId, int batchId)
        {
            var batch = await _db.SettlementBatches.FindAsync(batchId);
            if (batch == null) return (false, "Batch not found.");
            if (batch.Status != "Draft") return (false, $"Only Draft batches can be approved (current: {batch.Status}).");
            batch.Status = "Approved";
            batch.ApprovedByAdminId = adminUserId;
            batch.ApprovedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, $"Batch {batch.BatchNumber} approved.");
        }

        // Marks every Included item's underlying SettlementRequest Completed
        // via the existing SettlementService (single source of truth for
        // withdrawal status), then closes the batch.
        public async Task<(bool Success, string Message)> ProcessAsync(int batchId)
        {
            var batch = await _db.SettlementBatches.Include(b => b.Items).FirstOrDefaultAsync(b => b.Id == batchId);
            if (batch == null) return (false, "Batch not found.");
            if (batch.Status != "Approved") return (false, $"Only Approved batches can be processed (current: {batch.Status}).");

            var failures = 0;
            foreach (var item in batch.Items.Where(i => i.Status == "Included"))
            {
                var ok = await _settlementSvc.ProcessSettlementAsync(item.SettlementRequestId, "Completed", txnRef: $"{batch.BatchNumber}-{item.Id}");
                if (!ok) { item.Status = "Failed"; item.FailureReason = "Underlying settlement request not found."; failures++; }
            }

            batch.Status = failures > 0 ? "Failed" : "Processed";
            batch.ProcessedAt = DateTime.UtcNow;
            batch.BankFileReference = $"BF-{batch.BatchNumber}";
            await _db.SaveChangesAsync();
            return (true, failures > 0 ? $"Processed with {failures} failure(s) — review batch items." : $"Batch {batch.BatchNumber} processed — {batch.VendorCount} vendor(s) paid.");
        }

        public async Task<List<SettlementBatchDto>> GetAllAsync()
        {
            var batches = await _db.SettlementBatches.Include(b => b.CreatedByAdmin).Include(b => b.ApprovedByAdmin).OrderByDescending(b => b.CreatedAt).ToListAsync();
            var result = new List<SettlementBatchDto>();
            foreach (var b in batches) result.Add(await ToDtoAsync(b));
            return result;
        }

        public async Task<(bool Success, string Message, VendorHoldDto? Hold)> PlaceHoldAsync(int adminUserId, PlaceVendorHoldRequestDto req)
        {
            if (await _db.VendorSettlementHolds.AnyAsync(h => h.ChefId == req.ChefId && h.IsActive))
                return (false, "This vendor already has an active hold.", null);
            var hold = new VendorSettlementHold { ChefId = req.ChefId, Reason = req.Reason, HeldByAdminId = adminUserId };
            _db.VendorSettlementHolds.Add(hold);
            await _db.SaveChangesAsync();
            var chef = await _db.Users.FindAsync(req.ChefId);
            var admin = await _db.Users.FindAsync(adminUserId);
            return (true, "Hold placed.", new VendorHoldDto { Id = hold.Id, ChefId = hold.ChefId, ChefName = chef?.FullName, Reason = hold.Reason, HeldByName = admin?.FullName, HeldAt = hold.HeldAt, IsActive = true });
        }

        public async Task<(bool Success, string Message)> ReleaseHoldAsync(int adminUserId, int holdId)
        {
            var hold = await _db.VendorSettlementHolds.FindAsync(holdId);
            if (hold == null) return (false, "Hold not found.");
            if (!hold.IsActive) return (false, "Hold is already released.");
            hold.IsActive = false;
            hold.ReleasedByAdminId = adminUserId;
            hold.ReleasedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Hold released.");
        }

        public async Task<List<VendorHoldDto>> GetHoldsAsync()
        {
            var holds = await _db.VendorSettlementHolds.Include(h => h.Chef).Include(h => h.HeldByAdmin).OrderByDescending(h => h.HeldAt).ToListAsync();
            return holds.Select(h => new VendorHoldDto { Id = h.Id, ChefId = h.ChefId, ChefName = h.Chef?.FullName, Reason = h.Reason, HeldByName = h.HeldByAdmin?.FullName, HeldAt = h.HeldAt, IsActive = h.IsActive, ReleasedAt = h.ReleasedAt }).ToList();
        }

        private async Task<SettlementBatchDto> ToDtoAsync(SettlementBatch b)
        {
            var items = await _db.SettlementBatchItems.Include(i => i.Chef).Where(i => i.SettlementBatchId == b.Id).ToListAsync();
            var createdBy = b.CreatedByAdmin ?? await _db.Users.FindAsync(b.CreatedByAdminId);
            var approvedBy = b.ApprovedByAdmin ?? (b.ApprovedByAdminId.HasValue ? await _db.Users.FindAsync(b.ApprovedByAdminId.Value) : null);
            return new SettlementBatchDto
            {
                Id = b.Id, BatchNumber = b.BatchNumber, PeriodStart = b.PeriodStart, PeriodEnd = b.PeriodEnd, Status = b.Status,
                VendorCount = b.VendorCount, TotalGrossAmount = b.TotalGrossAmount, TotalTdsAmount = b.TotalTdsAmount, TotalNetAmount = b.TotalNetAmount,
                CreatedByName = createdBy?.FullName, ApprovedByName = approvedBy?.FullName, ApprovedAt = b.ApprovedAt, ProcessedAt = b.ProcessedAt,
                BankFileReference = b.BankFileReference, CreatedAt = b.CreatedAt,
                Items = items.Select(i => new SettlementBatchItemDto
                {
                    Id = i.Id, SettlementRequestId = i.SettlementRequestId, ChefId = i.ChefId, ChefName = i.Chef?.FullName,
                    GrossAmount = i.GrossAmount, TdsAmount = i.TdsAmount, NetAmount = i.NetAmount, Status = i.Status, FailureReason = i.FailureReason,
                }).ToList(),
            };
        }
    }

    // ── M96: Automatic Invoice ──────────────────────────────────────
    public class AutoInvoiceService
    {
        private readonly AppDbContext _db;
        private readonly InvoiceService _invoiceSvc;
        public AutoInvoiceService(AppDbContext db, InvoiceService invoiceSvc) { _db = db; _invoiceSvc = invoiceSvc; }

        public async Task<AutoInvoiceConfigDto> GetConfigAsync()
        {
            var config = await _db.AutoInvoiceConfigs.FirstOrDefaultAsync();
            if (config == null) { config = new AutoInvoiceConfig(); _db.AutoInvoiceConfigs.Add(config); await _db.SaveChangesAsync(); }
            return ToConfigDto(config);
        }

        public async Task<AutoInvoiceConfigDto> UpdateConfigAsync(UpdateAutoInvoiceConfigRequestDto req)
        {
            var config = await _db.AutoInvoiceConfigs.FirstOrDefaultAsync() ?? new AutoInvoiceConfig();
            if (config.Id == 0) _db.AutoInvoiceConfigs.Add(config);
            config.IsEnabled = req.IsEnabled;
            config.TriggerBookingStatus = req.TriggerBookingStatus;
            config.AutoSendToCustomer = req.AutoSendToCustomer;
            config.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return ToConfigDto(config);
        }

        // Idempotent: safe to call repeatedly for the same booking (e.g. from
        // a webhook that might retry) — returns AlreadyExists rather than
        // duplicating an invoice.
        public async Task<AutoInvoiceRunResultDto> TriggerForBookingAsync(int bookingId)
        {
            var config = await _db.AutoInvoiceConfigs.FirstOrDefaultAsync() ?? new AutoInvoiceConfig();
            if (!config.IsEnabled)
                return await LogAndReturn(bookingId, "Skipped", null, "Auto-invoice is disabled.");

            var booking = await _db.Bookings.FindAsync(bookingId);
            if (booking == null) return await LogAndReturn(bookingId, "Failed", null, "Booking not found.");
            if (booking.Status != config.TriggerBookingStatus)
                return await LogAndReturn(bookingId, "Skipped", null, $"Booking status is '{booking.Status}', trigger requires '{config.TriggerBookingStatus}'.");

            var existing = await _db.Invoices.FirstOrDefaultAsync(i => i.BookingId == bookingId);
            if (existing != null) return await LogAndReturn(bookingId, "AlreadyExists", existing.Id, "Invoice already generated for this booking.");

            try
            {
                var dto = await _invoiceSvc.GenerateInvoiceAsync(bookingId);
                if (config.AutoSendToCustomer)
                    _db.InvoiceDeliveryLogs.Add(new InvoiceDeliveryLog { InvoiceId = dto.Id, Channel = "Email", Status = "Sent" });
                await _db.SaveChangesAsync();
                return await LogAndReturn(bookingId, "Generated", dto.Id, null);
            }
            catch (Exception ex)
            {
                return await LogAndReturn(bookingId, "Failed", null, ex.Message);
            }
        }

        // Sweeps for bookings matching the trigger status with no invoice yet
        // — a backfill for anything that missed the live trigger (deploy
        // gaps, disabled window, manual data fixes).
        public async Task<BackfillAutoInvoiceResultDto> BackfillAsync(int maxBookings = 200)
        {
            var config = await _db.AutoInvoiceConfigs.FirstOrDefaultAsync() ?? new AutoInvoiceConfig();
            var invoicedBookingIds = await _db.Invoices.Select(i => i.BookingId).ToListAsync();
            var candidates = await _db.Bookings.Where(b => b.Status == config.TriggerBookingStatus && !invoicedBookingIds.Contains(b.Id))
                .OrderBy(b => b.CreatedAt).Take(maxBookings).ToListAsync();

            var result = new BackfillAutoInvoiceResultDto { ScannedCount = candidates.Count };
            foreach (var b in candidates)
            {
                var r = await TriggerForBookingAsync(b.Id);
                result.Results.Add(r);
                if (r.Result == "Generated") result.GeneratedCount++;
                else if (r.Result == "Failed") result.FailedCount++;
                else result.SkippedCount++;
            }
            return result;
        }

        public async Task<List<AutoInvoiceRunLogDto>> GetRecentRunsAsync(int take = 50)
        {
            var logs = await _db.AutoInvoiceRunLogs.OrderByDescending(l => l.RunAt).Take(take).ToListAsync();
            return logs.Select(l => new AutoInvoiceRunLogDto { BookingId = l.BookingId, Result = l.Result, InvoiceId = l.InvoiceId, Detail = l.Detail, RunAt = l.RunAt }).ToList();
        }

        private async Task<AutoInvoiceRunResultDto> LogAndReturn(int bookingId, string result, int? invoiceId, string? detail)
        {
            _db.AutoInvoiceRunLogs.Add(new AutoInvoiceRunLog { BookingId = bookingId, Result = result, InvoiceId = invoiceId, Detail = detail });
            await _db.SaveChangesAsync();
            return new AutoInvoiceRunResultDto { BookingId = bookingId, Result = result, InvoiceId = invoiceId, Detail = detail };
        }

        private static AutoInvoiceConfigDto ToConfigDto(AutoInvoiceConfig c) => new()
        {
            IsEnabled = c.IsEnabled, TriggerBookingStatus = c.TriggerBookingStatus, AutoSendToCustomer = c.AutoSendToCustomer, UpdatedAt = c.UpdatedAt,
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/vendor-settlement"), Authorize(Roles = "Admin")]
    public class VendorSettlementController : ControllerBase
    {
        private readonly Services.VendorSettlementService _svc;
        public VendorSettlementController(Services.VendorSettlementService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("batches")]
        public async Task<IActionResult> CreateBatch([FromBody] CreateSettlementBatchRequestDto req) => Ok(new { success = true, data = await _svc.CreateBatchAsync(UserId, req) });

        [HttpGet("batches")]
        public async Task<IActionResult> GetBatches() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("batches/approve")]
        public async Task<IActionResult> Approve([FromBody] ApproveSettlementBatchRequestDto req)
        {
            var (success, message) = await _svc.ApproveAsync(UserId, req.BatchId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("batches/process")]
        public async Task<IActionResult> Process([FromBody] ProcessSettlementBatchRequestDto req)
        {
            var (success, message) = await _svc.ProcessAsync(req.BatchId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("holds")]
        public async Task<IActionResult> PlaceHold([FromBody] PlaceVendorHoldRequestDto req)
        {
            var (success, message, hold) = await _svc.PlaceHoldAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = hold });
        }

        [HttpPost("holds/{id}/release")]
        public async Task<IActionResult> ReleaseHold(int id)
        {
            var (success, message) = await _svc.ReleaseHoldAsync(UserId, id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("holds")]
        public async Task<IActionResult> GetHolds() => Ok(new { success = true, data = await _svc.GetHoldsAsync() });
    }

    [ApiController, Route("api/auto-invoice"), Authorize(Roles = "Admin")]
    public class AutoInvoiceController : ControllerBase
    {
        private readonly Services.AutoInvoiceService _svc;
        public AutoInvoiceController(Services.AutoInvoiceService svc) => _svc = svc;

        [HttpGet("config")]
        public async Task<IActionResult> GetConfig() => Ok(new { success = true, data = await _svc.GetConfigAsync() });

        [HttpPost("config")]
        public async Task<IActionResult> UpdateConfig([FromBody] UpdateAutoInvoiceConfigRequestDto req) => Ok(new { success = true, data = await _svc.UpdateConfigAsync(req) });

        [HttpPost("trigger")]
        public async Task<IActionResult> Trigger([FromBody] TriggerAutoInvoiceRequestDto req) => Ok(new { success = true, data = await _svc.TriggerForBookingAsync(req.BookingId) });

        [HttpPost("backfill")]
        public async Task<IActionResult> Backfill([FromQuery] int maxBookings = 200) => Ok(new { success = true, data = await _svc.BackfillAsync(maxBookings) });

        [HttpGet("runs")]
        public async Task<IActionResult> GetRuns([FromQuery] int take = 50) => Ok(new { success = true, data = await _svc.GetRecentRunsAsync(take) });
    }
}
