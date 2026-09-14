using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M43: Invoice Service ───────────────────────────────────────
    public class InvoiceService
    {
        private readonly AppDbContext _db;
        private const decimal GST_PERCENT = 18m;

        public InvoiceService(AppDbContext db) => _db = db;

        public async Task<InvoiceDto> GenerateInvoiceAsync(int bookingId)
        {
            var booking  = await _db.Bookings.Include(b => b.Customer).Include(b => b.Chef).FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null) throw new Exception("Booking not found");
            var payment  = await _db.Payments.Where(p => p.BookingId == bookingId).OrderByDescending(p => p.Id).FirstOrDefaultAsync();

            var count    = await _db.Invoices.CountAsync() + 1;
            var subTotal = booking.TotalAmount / (1 + GST_PERCENT / 100);
            var gstAmt   = booking.TotalAmount - subTotal;
            var platFee  = subTotal * 0.15m;

            var invoice  = new Invoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyy}-{count:D5}",
                BookingId     = bookingId,
                CustomerId    = booking.CustomerId,
                ChefId        = booking.ChefId,
                SubTotal      = Math.Round(subTotal, 2),
                GSTPercent    = GST_PERCENT,
                GSTAmount     = Math.Round(gstAmt, 2),
                TotalAmount   = booking.TotalAmount,
                PlatformFee   = Math.Round(platFee, 2),
                PaymentMethod = payment?.Method ?? "Online",
                InvoiceDate   = DateTime.UtcNow.ToString("dd MMM yyyy"),
            };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();
            return Map(invoice, booking.Customer?.FullName ?? "", booking.Chef?.FullName ?? "");
        }

        public async Task<List<InvoiceDto>> GetUserInvoicesAsync(int userId)
        {
            var list = await _db.Invoices.Include(i => i.Customer).Include(i => i.Chef)
                .Where(i => i.CustomerId == userId)
                .OrderByDescending(i => i.CreatedAt).ToListAsync();
            return list.Select(i => Map(i, i.Customer?.FullName ?? "", i.Chef?.FullName ?? "")).ToList();
        }

        public async Task<TaxReportDto> GetTaxReportAsync(string period)
        {
            var now  = DateTime.UtcNow;
            var from = period switch
            {
                "thisMonth"  => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                "lastMonth"  => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1),
                "thisYear"   => new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                _            => now.AddDays(-30),
            };
            var invoices = await _db.Invoices.Include(i => i.Customer).Include(i => i.Chef)
                .Where(i => i.CreatedAt >= from)
                .OrderByDescending(i => i.CreatedAt).ToListAsync();

            return new TaxReportDto
            {
                Period             = period,
                TotalRevenue       = invoices.Sum(i => i.TotalAmount),
                TotalGSTCollected  = invoices.Sum(i => i.GSTAmount),
                TotalSubTotal      = invoices.Sum(i => i.SubTotal),
                TotalPlatformFee   = invoices.Sum(i => i.PlatformFee),
                TotalInvoices      = invoices.Count,
                Invoices           = invoices.Select(i => Map(i, i.Customer?.FullName ?? "", i.Chef?.FullName ?? "")).ToList(),
            };
        }

        private static InvoiceDto Map(Invoice i, string custName, string chefName) => new()
        {
            Id=i.Id, InvoiceNumber=i.InvoiceNumber, BookingId=i.BookingId,
            CustomerName=custName, ChefName=chefName, SubTotal=i.SubTotal,
            GSTPercent=i.GSTPercent, GSTAmount=i.GSTAmount, TotalAmount=i.TotalAmount,
            PlatformFee=i.PlatformFee, Status=i.Status, PaymentMethod=i.PaymentMethod,
            GSTIN=i.GSTIN, InvoiceDate=i.InvoiceDate, CreatedAt=i.CreatedAt.ToString("MMM dd, yyyy"),
        };
    }

    // ── M44: Transaction Ledger Service ───────────────────────────
    public class LedgerService
    {
        private readonly AppDbContext _db;
        public LedgerService(AppDbContext db) => _db = db;

        public async Task RecordAsync(int? userId, string type, string direction, decimal amount,
            decimal balanceAfter, string description, string paymentMethod = "", int? bookingId = null, string? reference = null)
        {
            var count = await _db.TransactionRecords.CountAsync() + 1;
            _db.TransactionRecords.Add(new TransactionRecord
            {
                TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{count:D4}",
                UserId        = userId,
                BookingId     = bookingId,
                Type          = type,
                Direction     = direction,
                Amount        = amount,
                BalanceAfter  = balanceAfter,
                Description   = description,
                PaymentMethod = paymentMethod,
                Reference     = reference,
            });
            await _db.SaveChangesAsync();
        }

        public async Task<LedgerSummaryDto> GetSummaryAsync(int? userId = null, string? type = null, int days = 30)
        {
            var from = DateTime.UtcNow.AddDays(-days);
            var q    = _db.TransactionRecords.Include(t => t.User).Where(t => t.CreatedAt >= from);
            if (userId.HasValue) q = q.Where(t => t.UserId == userId);
            if (type != null)    q = q.Where(t => t.Type == type);

            var list = await q.OrderByDescending(t => t.CreatedAt).ToListAsync();
            var credits = list.Where(t => t.Direction == "Credit").Sum(t => t.Amount);
            var debits  = list.Where(t => t.Direction == "Debit").Sum(t => t.Amount);

            return new LedgerSummaryDto
            {
                TotalCredits      = credits,
                TotalDebits       = debits,
                NetFlow           = credits - debits,
                TotalTransactions = list.Count,
                ByType = list.GroupBy(t => t.Type).ToDictionary(g => g.Key, g => g.Sum(t => t.Amount)),
                Recent = list.Take(50).Select(t => new TransactionDto
                {
                    Id=t.Id, TransactionId=t.TransactionId, UserName=t.User?.FullName ?? "System",
                    BookingId=t.BookingId, Type=t.Type, Direction=t.Direction, Amount=t.Amount,
                    BalanceAfter=t.BalanceAfter, Description=t.Description, PaymentMethod=t.PaymentMethod,
                    Status=t.Status, Reference=t.Reference, CreatedAt=t.CreatedAt.ToString("MMM dd, yyyy hh:mm tt"),
                }).ToList(),
            };
        }
    }
}
