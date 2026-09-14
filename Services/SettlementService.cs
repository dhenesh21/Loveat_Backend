using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class SettlementService
    {
        private readonly AppDbContext _db;
        private const decimal MIN_WITHDRAWAL = 200m;

        public SettlementService(AppDbContext db) => _db = db;

        // ── Get chef wallet overview ───────────────────────────────
        public async Task<ChefWalletOverviewDto> GetWalletOverviewAsync(int chefId)
        {
            var ledger   = await _db.CommissionLedgers.Where(l => l.ChefId == chefId).ToListAsync();
            var requests = await _db.SettlementRequests
                .Include(r => r.BankAccount)
                .Where(r => r.ChefId == chefId)
                .OrderByDescending(r => r.RequestedAt).ToListAsync();
            var banks    = await _db.ChefBankAccounts.Where(b => b.ChefId == chefId).ToListAsync();

            decimal totalEarned    = ledger.Sum(l => l.ChefAmount);
            decimal totalWithdrawn = requests.Where(r => r.Status == "Completed").Sum(r => r.Amount);
            decimal pending        = requests.Where(r => r.Status is "Pending" or "Processing").Sum(r => r.Amount);
            decimal available      = totalEarned - totalWithdrawn - pending;

            return new ChefWalletOverviewDto
            {
                AvailableBalance  = Math.Max(available, 0),
                TotalEarned       = totalEarned,
                TotalWithdrawn    = totalWithdrawn,
                PendingSettlement = pending,
                MinWithdrawal     = MIN_WITHDRAWAL,
                BankAccounts      = banks.Select(MapBank).ToList(),
                History           = requests.Take(20).Select(MapRequest).ToList(),
            };
        }

        // ── Add bank account ───────────────────────────────────────
        public async Task<ChefBankAccountDto> AddBankAccountAsync(int chefId, AddBankAccountDto dto)
        {
            // Set existing as non-primary
            var existing = await _db.ChefBankAccounts.Where(b => b.ChefId == chefId).ToListAsync();
            existing.ForEach(b => b.IsPrimary = false);

            var account = new ChefBankAccount
            {
                ChefId              = chefId,
                AccountHolderName   = dto.AccountHolderName,
                AccountNumber       = dto.AccountNumber,
                IFSC                = dto.IFSC.ToUpper(),
                BankName            = dto.BankName,
                AccountType         = dto.AccountType,
                UpiId               = dto.UpiId,
                IsPrimary           = true,
                IsVerified          = false, // requires penny drop in production
            };
            _db.ChefBankAccounts.Add(account);
            await _db.SaveChangesAsync();
            return MapBank(account);
        }

        // ── Request withdrawal ─────────────────────────────────────
        public async Task<(bool Success, string Message, SettlementRequestDto? Data)> RequestWithdrawalAsync(int chefId, WithdrawalRequestDto dto)
        {
            var overview = await GetWalletOverviewAsync(chefId);

            if (dto.Amount < MIN_WITHDRAWAL)
                return (false, $"Minimum withdrawal is ₹{MIN_WITHDRAWAL}", null);

            if (dto.Amount > overview.AvailableBalance)
                return (false, "Insufficient balance", null);

            var request = new SettlementRequest
            {
                ChefId        = chefId,
                Amount        = dto.Amount,
                PaymentMethod = dto.PaymentMethod,
                BankAccountId = dto.BankAccountId,
                Status        = "Pending",
            };
            _db.SettlementRequests.Add(request);
            await _db.SaveChangesAsync();

            return (true, "Withdrawal request submitted. Will be processed within 2 business days.", MapRequest(request));
        }

        // ── Admin: Process settlement ──────────────────────────────
        public async Task<bool> ProcessSettlementAsync(int requestId, string status, string? txnRef = null, string? rejectionReason = null)
        {
            var req = await _db.SettlementRequests.FindAsync(requestId);
            if (req == null) return false;
            req.Status          = status;
            req.TransactionRef  = txnRef;
            req.RejectionReason = rejectionReason;
            req.ProcessedAt     = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Admin: Get requests for the settlement screen ───────────
        // Previously only ever returned Pending — the admin page has
        // status tabs (Processing/Completed/Failed/Rejected) that had
        // nothing to query, since there was no way to see anything but
        // pending requests. null/"All" now returns everything.
        public async Task<List<SettlementRequestDto>> GetForAdminAsync(string? status = "Pending")
        {
            var q = _db.SettlementRequests
                .Include(r => r.Chef)
                .Include(r => r.BankAccount)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                q = q.Where(r => r.Status == status);
            var list = await q.OrderByDescending(r => r.RequestedAt).ToListAsync();
            return list.Select(MapRequest).ToList();
        }

        private static ChefBankAccountDto MapBank(ChefBankAccount b) => new()
        {
            Id=b.Id, AccountHolderName=b.AccountHolderName, IFSC=b.IFSC, BankName=b.BankName,
            AccountType=b.AccountType, IsVerified=b.IsVerified, IsPrimary=b.IsPrimary, UpiId=b.UpiId,
            AccountNumberMasked = b.AccountNumber.Length >= 4 ? "****" + b.AccountNumber[^4..] : "****",
        };

        private static SettlementRequestDto MapRequest(SettlementRequest r) => new()
        {
            Id=r.Id, Amount=r.Amount, Status=r.Status, PaymentMethod=r.PaymentMethod,
            TransactionRef=r.TransactionRef, RejectionReason=r.RejectionReason,
            RequestedAt=r.RequestedAt.ToString("MMM dd, yyyy"),
            ProcessedAt=r.ProcessedAt?.ToString("MMM dd, yyyy"),
            BankMasked=r.BankAccount != null
                ? $"{r.BankAccount.BankName} ****{r.BankAccount.AccountNumber[^4..]}"
                : r.PaymentMethod == "UPI" ? "UPI Transfer" : "Bank Transfer",
            ChefName=r.Chef?.FullName,
            Phone=r.Chef?.PhoneNumber,
        };
    }
}
