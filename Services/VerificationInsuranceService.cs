using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LovEat.API.Services
{
    // ── M61: Background Verification Service ──────────────────────
    public class BackgroundVerificationService
    {
        private readonly AppDbContext _db;
        public BackgroundVerificationService(AppDbContext db) => _db = db;

        public async Task<ChefVerificationSummaryDto> GetChefSummaryAsync(int chefId)
        {
            var chef    = await _db.Users.FindAsync(chefId);
            var verifs  = await _db.BackgroundVerifications
                .Where(v => v.ChefId == chefId)
                .OrderByDescending(v => v.UpdatedAt)
                .ToListAsync();

            var required = new[] { "Police", "Aadhaar", "PAN", "Address" };
            var verified = verifs.Where(v => v.Status == "Verified").Select(v => v.VerificationType).ToHashSet();
            bool fullyVerified = required.All(r => verified.Contains(r));

            string overall = fullyVerified ? "Fully Verified"
                : verifs.Any(v => v.Status == "InProgress") ? "In Progress"
                : verifs.Any(v => v.Status == "Failed")     ? "Action Required"
                : "Pending";

            return new ChefVerificationSummaryDto
            {
                ChefId          = chefId,
                ChefName        = chef?.FullName ?? "",
                IsFullyVerified = fullyVerified,
                OverallStatus   = overall,
                Verifications   = verifs.Select(Map).ToList(),
            };
        }

        public async Task<BackgroundVerificationDto> SubmitAsync(int chefId, SubmitVerificationDto dto)
        {
            // Check if already exists and update, else create
            var existing = await _db.BackgroundVerifications
                .FirstOrDefaultAsync(v => v.ChefId == chefId && v.VerificationType == dto.VerificationType);

            if (existing != null)
            {
                existing.DocumentNumber = dto.DocumentNumber;
                existing.DocumentUrl    = dto.DocumentUrl;
                existing.Status         = "Pending";
                existing.UpdatedAt      = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return Map(existing);
            }

            var verif = new BackgroundVerification
            {
                ChefId           = chefId,
                VerificationType = dto.VerificationType,
                DocumentNumber   = dto.DocumentNumber,
                DocumentUrl      = dto.DocumentUrl,
                Status           = "Pending",
                ExpiresAt        = DateTime.UtcNow.AddYears(2),
            };
            _db.BackgroundVerifications.Add(verif);
            await _db.SaveChangesAsync();
            return Map(verif);
        }

        // Admin: Update verification status
        public async Task<bool> UpdateStatusAsync(int id, string status, string? agency = null, string? ref_ = null, string? reason = null)
        {
            var v = await _db.BackgroundVerifications.FindAsync(id);
            if (v == null) return false;
            v.Status             = status;
            v.VerifiedByAgency   = agency ?? v.VerifiedByAgency;
            v.VerificationRef    = ref_ ?? v.VerificationRef;
            v.RejectionReason    = reason;
            v.VerifiedAt         = status == "Verified" ? DateTime.UtcNow : v.VerifiedAt;
            v.UpdatedAt          = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<ChefVerificationSummaryDto>> GetAllPendingAsync()
        {
            var pending = await _db.BackgroundVerifications
                .Include(v => v.Chef)
                .Where(v => v.Status == "Pending" || v.Status == "InProgress")
                .GroupBy(v => v.ChefId)
                .ToListAsync();

            var result = new List<ChefVerificationSummaryDto>();
            foreach (var g in pending)
            {
                var chef = g.First().Chef;
                result.Add(new ChefVerificationSummaryDto
                {
                    ChefId          = g.Key,
                    ChefName        = chef?.FullName ?? "",
                    IsFullyVerified = false,
                    OverallStatus   = "Pending",
                    Verifications   = g.Select(Map).ToList(),
                });
            }
            return result;
        }

        private static BackgroundVerificationDto Map(BackgroundVerification v) => new()
        {
            Id=v.Id, VerificationType=v.VerificationType, Status=v.Status,
            DocumentNumber=v.DocumentNumber, DocumentUrl=v.DocumentUrl,
            VerifiedByAgency=v.VerifiedByAgency, RejectionReason=v.RejectionReason,
            VerificationRef=v.VerificationRef,
            VerifiedAt=v.VerifiedAt?.ToString("MMM dd, yyyy"),
            ExpiresAt=v.ExpiresAt?.ToString("MMM dd, yyyy"),
            SubmittedAt=v.SubmittedAt.ToString("MMM dd, yyyy"),
        };
    }

    // ── M62: Insurance Service ─────────────────────────────────────
    public class InsuranceService
    {
        private readonly AppDbContext _db;
        public InsuranceService(AppDbContext db) => _db = db;

        public async Task<List<InsurancePolicyDto>> GetUserPoliciesAsync(int userId)
        {
            var policies = await _db.InsurancePolicies
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return policies.Select(MapPolicy).ToList();
        }

        public async Task<InsurancePolicyDto> CreatePolicyAsync(int userId, string policyType, string provider, decimal premium, string period, decimal coverage)
        {
            var count = await _db.InsurancePolicies.CountAsync() + 1;
            var policy = new InsurancePolicy
            {
                UserId          = userId,
                PolicyType      = policyType,
                PolicyNumber    = $"LOVEAT-INS-{count:D6}",
                Provider        = provider,
                PremiumAmount   = premium,
                PremiumPeriod   = period,
                CoverageAmount  = coverage,
                StartDate       = DateTime.UtcNow,
                EndDate         = period == "Annual" ? DateTime.UtcNow.AddYears(1) : DateTime.UtcNow.AddMonths(1),
            };
            _db.InsurancePolicies.Add(policy);
            await _db.SaveChangesAsync();
            return MapPolicy(policy);
        }

        public async Task<InsuranceClaimDto> FileClaimAsync(FileClaimDto dto)
        {
            var count = await _db.InsuranceClaims.CountAsync() + 1;
            var claim = new InsuranceClaim
            {
                PolicyId    = dto.PolicyId,
                BookingId   = dto.BookingId,
                ClaimType   = dto.ClaimType,
                Description = dto.Description,
                ClaimAmount = dto.ClaimAmount,
                EvidenceUrl = dto.EvidenceUrl,
                ClaimRef    = $"CLM-{DateTime.UtcNow:yyyyMMdd}-{count:D4}",
            };
            _db.InsuranceClaims.Add(claim);
            await _db.SaveChangesAsync();
            return MapClaim(claim);
        }

        public async Task<List<InsuranceClaimDto>> GetClaimsAsync(int userId)
        {
            var claims = await _db.InsuranceClaims
                .Include(c => c.Policy)
                .Where(c => c.Policy!.UserId == userId)
                .OrderByDescending(c => c.FiledAt)
                .ToListAsync();
            return claims.Select(MapClaim).ToList();
        }

        public async Task<bool> UpdateClaimAsync(int claimId, string status, string? resolution)
        {
            var claim = await _db.InsuranceClaims.FindAsync(claimId);
            if (claim == null) return false;
            claim.Status     = status;
            claim.Resolution = resolution;
            claim.ResolvedAt = status is "Approved" or "Rejected" or "Paid" ? DateTime.UtcNow : null;
            await _db.SaveChangesAsync();
            return true;
        }

        private static InsurancePolicyDto MapPolicy(InsurancePolicy p) => new()
        {
            Id=p.Id, PolicyType=p.PolicyType, PolicyNumber=p.PolicyNumber, Provider=p.Provider,
            PremiumAmount=p.PremiumAmount, PremiumPeriod=p.PremiumPeriod, CoverageAmount=p.CoverageAmount,
            Status=p.Status, PolicyDocUrl=p.PolicyDocUrl,
            StartDate=p.StartDate.ToString("MMM dd, yyyy"), EndDate=p.EndDate.ToString("MMM dd, yyyy"),
            DaysRemaining=Math.Max(0,(int)(p.EndDate - DateTime.UtcNow).TotalDays),
        };

        private static InsuranceClaimDto MapClaim(InsuranceClaim c) => new()
        {
            Id=c.Id, ClaimType=c.ClaimType, Description=c.Description, ClaimAmount=c.ClaimAmount,
            Status=c.Status, EvidenceUrl=c.EvidenceUrl, ClaimRef=c.ClaimRef, Resolution=c.Resolution,
            FiledAt=c.FiledAt.ToString("MMM dd, yyyy"),
            ResolvedAt=c.ResolvedAt?.ToString("MMM dd, yyyy"),
        };
    }
}

namespace LovEat.API.Controllers
{
    // ── M61 Controller ─────────────────────────────────────────────
    // Route renamed from "api/verification" to "api/background-verification" —
    // that base path collided with Controllers/VerificationController.cs's
    // POST submit and GET admin/pending (different feature, same route),
    // which would throw AmbiguousMatchException at request time.
    [ApiController, Route("api/background-verification"), Authorize]
    public class BackgroundVerificationController : ControllerBase
    {
        private readonly Services.BackgroundVerificationService _svc;
        public BackgroundVerificationController(Services.BackgroundVerificationService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("my")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> GetMy()
            => Ok(new { success=true, data=await _svc.GetChefSummaryAsync(UserId) });

        [HttpPost("submit")]
        [Authorize(Roles = "Chef")]
        public async Task<IActionResult> Submit([FromBody] SubmitVerificationDto dto)
        {
            var data = await _svc.SubmitAsync(UserId, dto);
            return Ok(new { success=true, data, message="Verification submitted. Our team will review within 48 hours." });
        }

        [HttpGet("admin/pending"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminPending()
            => Ok(new { success=true, data=await _svc.GetAllPendingAsync() });

        [HttpGet("admin/chef/{chefId}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminChef(int chefId)
            => Ok(new { success=true, data=await _svc.GetChefSummaryAsync(chefId) });

        [HttpPost("admin/{id}/update"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminUpdate(int id, [FromBody] dynamic req)
        {
            string status = (string)(req.status ?? "");
            string? agency = (string?)(req.agency);
            string? refNo  = (string?)(req.verificationRef);
            string? reason = (string?)(req.rejectionReason);
            var ok = await _svc.UpdateStatusAsync(id, status, agency, refNo, reason);
            return Ok(new { success=ok });
        }
    }

    // ── M62 Controller ─────────────────────────────────────────────
    [ApiController, Route("api/insurance"), Authorize]
    public class InsuranceController : ControllerBase
    {
        private readonly Services.InsuranceService _svc;
        public InsuranceController(Services.InsuranceService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("my")]
        public async Task<IActionResult> GetMyPolicies()
            => Ok(new { success=true, data=await _svc.GetUserPoliciesAsync(UserId) });

        [HttpPost("enroll")]
        public async Task<IActionResult> Enroll([FromBody] dynamic req)
        {
            string type     = (string)(req.policyType ?? "ChefLiability");
            string provider = (string)(req.provider ?? "ICICI Lombard");
            decimal premium = (decimal)(req.premiumAmount ?? 299);
            string period   = (string)(req.premiumPeriod ?? "Monthly");
            decimal coverage= (decimal)(req.coverageAmount ?? 500000);
            var data = await _svc.CreatePolicyAsync(UserId, type, provider, premium, period, coverage);
            return Ok(new { success=true, data, message="Insurance policy enrolled successfully." });
        }

        [HttpGet("claims")]
        public async Task<IActionResult> GetClaims()
            => Ok(new { success=true, data=await _svc.GetClaimsAsync(UserId) });

        [HttpPost("claims/file")]
        public async Task<IActionResult> FileClaim([FromBody] FileClaimDto dto)
        {
            var data = await _svc.FileClaimAsync(dto);
            return Ok(new { success=true, data, message=$"Claim {data.ClaimRef} filed. We'll review within 3 working days." });
        }

        [HttpPost("admin/claims/{id}/update"), Authorize(Roles="Admin")]
        public async Task<IActionResult> AdminUpdate(int id, [FromBody] dynamic req)
        {
            string status   = (string)(req.status ?? "");
            string? res     = (string?)(req.resolution);
            var ok = await _svc.UpdateClaimAsync(id, status, res);
            return Ok(new { success=ok });
        }
    }
}
