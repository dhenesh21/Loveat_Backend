using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>
    /// M13: chef document verification during onboarding. Required document
    /// types a chef must submit before they can go live — kept as a simple
    /// constant list rather than config, since adding a new required doc
    /// type is a product decision that should go through a code review, not
    /// a silent config change.
    /// </summary>
    public class VerificationService
    {
        private readonly AppDbContext _db;
        private static readonly string[] RequiredDocumentTypes = { "IdProof", "AddressProof", "CookingCertificate", "ProfilePhoto" };

        public VerificationService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message)> SubmitDocumentAsync(int chefId, SubmitVerificationRequestDto req)
        {
            if (!RequiredDocumentTypes.Contains(req.DocumentType))
                return (false, $"Unknown document type. Expected one of: {string.Join(", ", RequiredDocumentTypes)}");

            // Replace any previous pending/rejected submission of the same type rather than piling up duplicates.
            var existing = await _db.ChefVerifications
                .Where(v => v.ChefId == chefId && v.DocumentType == req.DocumentType && v.Status != "Approved")
                .ToListAsync();
            _db.ChefVerifications.RemoveRange(existing);

            _db.ChefVerifications.Add(new ChefVerification
            {
                ChefId = chefId,
                DocumentType = req.DocumentType,
                DocumentUrl = req.DocumentUrl,
                Status = "Pending",
            });

            await _db.SaveChangesAsync();
            return (true, "Document submitted for review.");
        }

        public async Task<ChefVerificationStatusDto> GetStatusAsync(int chefId)
        {
            var docs = await _db.ChefVerifications
                .Include(v => v.Chef)
                .Where(v => v.ChefId == chefId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var approvedTypes = docs.Where(d => d.Status == "Approved").Select(d => d.DocumentType).ToHashSet();
            var missing = RequiredDocumentTypes.Where(t => !approvedTypes.Contains(t)).ToList();

            return new ChefVerificationStatusDto
            {
                IsFullyVerified = missing.Count == 0,
                MissingDocumentTypes = missing,
                Documents = docs.Select(ToDto).ToList(),
            };
        }

        public async Task<List<VerificationDto>> GetPendingForAdminAsync()
        {
            var docs = await _db.ChefVerifications
                .Include(v => v.Chef)
                .Where(v => v.Status == "Pending")
                .OrderBy(v => v.CreatedAt)
                .ToListAsync();

            return docs.Select(ToDto).ToList();
        }

        public async Task<(bool Success, string Message)> ReviewAsync(int adminUserId, ReviewVerificationRequestDto req)
        {
            var doc = await _db.ChefVerifications.FirstOrDefaultAsync(v => v.Id == req.VerificationId);
            if (doc == null) return (false, "Verification record not found.");

            doc.Status = req.Approve ? "Approved" : "Rejected";
            doc.RejectionReason = req.Approve ? null : req.RejectionReason;
            doc.ReviewedByAdminId = adminUserId;
            doc.ReviewedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // If every required doc type is now approved for this chef, flip ChefProfile.IsVerified.
            if (req.Approve)
            {
                var approvedTypes = await _db.ChefVerifications
                    .Where(v => v.ChefId == doc.ChefId && v.Status == "Approved")
                    .Select(v => v.DocumentType)
                    .ToListAsync();

                if (RequiredDocumentTypes.All(t => approvedTypes.Contains(t)))
                {
                    var profile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == doc.ChefId);
                    if (profile != null)
                    {
                        profile.IsVerified = true;
                        await _db.SaveChangesAsync();
                    }
                }
            }

            return (true, req.Approve ? "Document approved." : "Document rejected.");
        }

        private static VerificationDto ToDto(ChefVerification v) => new()
        {
            Id = v.Id,
            ChefId = v.ChefId,
            ChefName = v.Chef?.FullName ?? "",
            DocumentType = v.DocumentType,
            DocumentUrl = v.DocumentUrl,
            Status = v.Status,
            RejectionReason = v.RejectionReason,
            ReviewedAt = v.ReviewedAt,
            CreatedAt = v.CreatedAt,
        };
    }
}
