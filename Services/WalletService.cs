using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>
    /// M15: in-app wallet. TopUpAsync is a dev/test-only credit path (no real
    /// money moves) until the payment gateway (G1, Phase 5) is wired — at
    /// that point top-up should go through PaymentService and only call
    /// CreditAsync after the gateway confirms the charge succeeded.
    /// AdjustAsync is the one place that should ever touch User.WalletBalance,
    /// so every other module (refunds, settlements, payouts) should call
    /// through here rather than editing the balance directly.
    /// </summary>
    public class WalletService
    {
        private readonly AppDbContext _db;
        public WalletService(AppDbContext db) => _db = db;

        public async Task<WalletDto?> GetWalletAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;

            var transactions = await _db.WalletTransactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    Type = t.Type,
                    Amount = t.Amount,
                    BalanceAfter = t.BalanceAfter,
                    Reason = t.Reason,
                    CreatedAt = t.CreatedAt,
                })
                .ToListAsync();

            return new WalletDto { Balance = user.WalletBalance, RecentTransactions = transactions };
        }

        /// <summary>TODO (G1): this should only be called after a real payment gateway confirms the charge. For now it credits immediately so the rest of the app (booking payment, refunds) can be built against a working wallet.</summary>
        public async Task<(bool Success, string Message)> TopUpAsync(int userId, decimal amount)
        {
            if (amount <= 0) return (false, "Amount must be positive.");
            await AdjustAsync(userId, amount, "Credit", "TopUp", null);
            return (true, $"₹{amount} added to your wallet.");
        }

        /// <summary>Central place to move money in/out of a user's wallet — always writes a WalletTransaction row alongside updating the balance.</summary>
        public async Task<(bool Success, string Message)> AdjustAsync(int userId, decimal amount, string type, string reason, int? relatedBookingId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return (false, "User not found.");

            if (type == "Debit" && user.WalletBalance < amount)
                return (false, "Insufficient wallet balance.");

            user.WalletBalance += type == "Credit" ? amount : -amount;
            user.UpdatedAt = DateTime.UtcNow;

            _db.WalletTransactions.Add(new WalletTransaction
            {
                UserId = userId,
                Type = type,
                Amount = amount,
                BalanceAfter = user.WalletBalance,
                Reason = reason,
                RelatedBookingId = relatedBookingId,
            });

            await _db.SaveChangesAsync();
            return (true, "Wallet updated.");
        }
    }
}
