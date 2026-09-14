using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>
    /// User-initiated fraud reporting (the "Fraud report" screen in
    /// CustomerApp). Writes into the same `FraudDetectionLog` table batch
    /// 23's automatic `FraudDetectionService.AnalyzeBookingAsync` uses, just
    /// tagged with DetectionType "UserReported" so admins can tell manual
    /// reports apart from automatic flags in the same review queue.
    /// </summary>
    public class FraudService
    {
        private readonly AppDbContext _db;
        public FraudService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message)> SubmitReportAsync(int userId, SubmitFraudReportRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Description))
                return (false, "Please describe what happened.");

            _db.FraudDetectionLogs.Add(new FraudDetectionLog
            {
                UserId = userId,
                BookingId = req.BookingId,
                DetectionType = req.DetectionType,
                Severity = "Medium",
                RiskScore = 50, // manual reports start at a neutral score pending admin review
                Evidence = System.Text.Json.JsonSerializer.Serialize(new { description = req.Description }),
                Action = "Flagged",
            });

            await _db.SaveChangesAsync();
            return (true, "Report submitted. Our team will review it shortly.");
        }

        public async Task<List<FraudDetectionLog>> GetMyReportsAsync(int userId)
            => await _db.FraudDetectionLogs.Where(l => l.UserId == userId).OrderByDescending(l => l.DetectedAt).ToListAsync();
    }
}
