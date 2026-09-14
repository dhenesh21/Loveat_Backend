using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class TwoFactorService
    {
        private readonly AppDbContext _db;
        private readonly SecurityAuditService _audit;
        public TwoFactorService(AppDbContext db, SecurityAuditService audit) { _db = db; _audit = audit; }

        private const int Step = 30;
        private const int Digits = 6;

        public async Task<TwoFactorSetupResultDto> SetupAsync(int userId)
        {
            var secretBytes = RandomNumberGenerator.GetBytes(20);
            var secret = Base32Encode(secretBytes);
            var backupCodes = Enumerable.Range(0, 8).Select(_ => RandomNumberGenerator.GetInt32(100000, 999999).ToString()).ToList();
            var hashedBackupCodes = backupCodes.Select(HashBackupCode).ToList();

            var existing = await _db.TwoFactorSettings.FirstOrDefaultAsync(t => t.UserId == userId);
            if (existing == null) { existing = new TwoFactorSetting { UserId = userId }; _db.TwoFactorSettings.Add(existing); }
            existing.TotpSecretEncrypted = secret;
            existing.BackupCodesJson = JsonSerializer.Serialize(hashedBackupCodes);
            existing.IsEnabled = false;
            await _db.SaveChangesAsync();

            return new TwoFactorSetupResultDto { SecretForAuthenticatorApp = secret, BackupCodes = backupCodes };
        }

        public async Task<(bool Success, string Message)> VerifyAndEnableAsync(VerifyTwoFactorRequestDto req)
        {
            var setting = await _db.TwoFactorSettings.FirstOrDefaultAsync(t => t.UserId == req.UserId);
            if (setting?.TotpSecretEncrypted == null) return (false, "2FA setup not started. Call setup first.");
            if (!ValidateCode(setting.TotpSecretEncrypted, req.Code)) return (false, "Incorrect code.");

            setting.IsEnabled = true;
            setting.EnabledAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await _audit.LogAsync(req.UserId, "TwoFactorEnabled", null, null);
            return (true, "Two-factor authentication enabled.");
        }

        public async Task<(bool Success, string Message)> DisableAsync(DisableTwoFactorRequestDto req)
        {
            var setting = await _db.TwoFactorSettings.FirstOrDefaultAsync(t => t.UserId == req.UserId);
            if (setting == null || !setting.IsEnabled) return (false, "2FA is not currently enabled.");
            if (!ValidateCode(setting.TotpSecretEncrypted!, req.Code)) return (false, "Incorrect code.");

            setting.IsEnabled = false;
            await _db.SaveChangesAsync();
            await _audit.LogAsync(req.UserId, "TwoFactorDisabled", null, null);
            return (true, "Two-factor authentication disabled.");
        }

        public async Task<TwoFactorStatusDto> GetStatusAsync(int userId)
        {
            var setting = await _db.TwoFactorSettings.FirstOrDefaultAsync(t => t.UserId == userId);
            return new TwoFactorStatusDto { IsEnabled = setting?.IsEnabled ?? false, EnabledAt = setting?.EnabledAt };
        }

        private static bool ValidateCode(string base32Secret, string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            var secretBytes = Base32Decode(base32Secret);
            var counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / Step;
            for (var offset = -1; offset <= 1; offset++)
                if (ComputeTotp(secretBytes, counter + offset) == code) return true;
            return false;
        }

        private static string ComputeTotp(byte[] secret, long counter)
        {
            var counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);

            var hash = new HMACSHA1(secret).ComputeHash(counterBytes);
            var offset = hash[^1] & 0x0F;
            var binaryCode = ((hash[offset] & 0x7F) << 24) | ((hash[offset + 1] & 0xFF) << 16) | ((hash[offset + 2] & 0xFF) << 8) | (hash[offset + 3] & 0xFF);
            var otp = binaryCode % (int)Math.Pow(10, Digits);
            return otp.ToString().PadLeft(Digits, '0');
        }

        private static string HashBackupCode(string code) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

        private static string Base32Encode(byte[] data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var output = new StringBuilder();
            int bits = 0, value = 0;
            foreach (var b in data)
            {
                value = (value << 8) | b;
                bits += 8;
                while (bits >= 5) { output.Append(alphabet[(value >> (bits - 5)) & 31]); bits -= 5; }
            }
            if (bits > 0) output.Append(alphabet[(value << (5 - bits)) & 31]);
            return output.ToString();
        }

        private static byte[] Base32Decode(string input)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var bytes = new List<byte>();
            int bits = 0, value = 0;
            foreach (var c in input.TrimEnd('='))
            {
                var idx = alphabet.IndexOf(char.ToUpperInvariant(c));
                if (idx < 0) continue;
                value = (value << 5) | idx;
                bits += 5;
                if (bits >= 8) { bytes.Add((byte)((value >> (bits - 8)) & 0xFF)); bits -= 8; }
            }
            return bytes.ToArray();
        }
    }

    // Note: M162 Session Management already exists — see DeviceSecurityCorporateService (M58).

    public class DataPrivacyService
    {
        private readonly AppDbContext _db;
        public DataPrivacyService(AppDbContext db) => _db = db;

        public async Task<DataPrivacyRequestDto> CreateAsync(CreatePrivacyRequestDto req)
        {
            var request = new DataPrivacyRequest { UserId = req.UserId, RequestType = req.RequestType };
            _db.DataPrivacyRequests.Add(request);
            await _db.SaveChangesAsync();
            return ToDto(request);
        }

        public async Task<List<DataPrivacyRequestDto>> GetForUserAsync(int userId)
            => (await _db.DataPrivacyRequests.Where(r => r.UserId == userId).OrderByDescending(r => r.RequestedAt).ToListAsync()).Select(ToDto).ToList();

        public async Task<List<DataPrivacyRequestDto>> GetAllPendingAsync()
            => (await _db.DataPrivacyRequests.Where(r => r.Status == "Pending").OrderBy(r => r.RequestedAt).ToListAsync()).Select(ToDto).ToList();

        public async Task<(bool Success, string Message)> ProcessAsync(ProcessPrivacyRequestDto req)
        {
            var request = await _db.DataPrivacyRequests.FindAsync(req.RequestId);
            if (request == null) return (false, "Request not found.");
            if (request.Status != "Pending") return (false, $"Request already {request.Status}.");

            if (!req.Approve)
            {
                request.Status = "Rejected";
                request.RejectionReason = req.RejectionReason;
                await _db.SaveChangesAsync();
                return (true, "Request rejected.");
            }

            if (request.RequestType == "Export")
            {
                var user = await _db.Users.FindAsync(request.UserId);
                var bookings = await _db.Bookings.Where(b => b.CustomerId == request.UserId).Select(b => new { b.Id, b.Status, b.TotalAmount, b.CreatedAt }).ToListAsync();
                var reviews = await _db.Reviews.Where(r => r.CustomerId == request.UserId).Select(r => new { r.Id, r.Rating, r.CreatedAt }).ToListAsync();
                request.ResultDataJson = JsonSerializer.Serialize(new { user = new { user?.FullName, user?.Email, user?.PhoneNumber }, bookings, reviews });
            }
            else if (request.RequestType == "Deletion")
            {
                var user = await _db.Users.FindAsync(request.UserId);
                if (user != null)
                {
                    user.FullName = "Deleted User";
                    user.Email = null;
                    user.PhoneNumber = $"DELETED_{user.Id}";
                    user.IsActive = false;
                }
            }

            request.Status = "Completed";
            request.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Request processed.");
        }

        private static DataPrivacyRequestDto ToDto(DataPrivacyRequest r) => new()
        {
            Id = r.Id, RequestType = r.RequestType, Status = r.Status, RequestedAt = r.RequestedAt, CompletedAt = r.CompletedAt,
        };
    }

    public class SecurityAuditService
    {
        private readonly AppDbContext _db;
        public SecurityAuditService(AppDbContext db) => _db = db;

        public async Task LogAsync(int userId, string eventType, string? ipAddress, string? details)
        {
            _db.SecurityAuditEntries.Add(new SecurityAuditEntry { UserId = userId, EventType = eventType, IpAddress = ipAddress, Details = details });
            await _db.SaveChangesAsync();
        }

        public async Task<List<SecurityAuditEntryDto>> GetForUserAsync(int userId, int take = 50)
            => (await _db.SecurityAuditEntries.Where(e => e.UserId == userId).OrderByDescending(e => e.OccurredAt).Take(take).ToListAsync())
                .Select(e => new SecurityAuditEntryDto { EventType = e.EventType, IpAddress = e.IpAddress, Details = e.Details, OccurredAt = e.OccurredAt }).ToList();
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/two-factor"), Authorize]
    public class TwoFactorController : ControllerBase
    {
        private readonly Services.TwoFactorService _svc;
        public TwoFactorController(Services.TwoFactorService svc) => _svc = svc;

        [HttpPost("setup")]
        public async Task<IActionResult> Setup([FromBody] SetupTwoFactorRequestDto req) => Ok(new { success = true, data = await _svc.SetupAsync(req.UserId) });

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyTwoFactorRequestDto req)
        {
            var (success, message) = await _svc.VerifyAndEnableAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("disable")]
        public async Task<IActionResult> Disable([FromBody] DisableTwoFactorRequestDto req)
        {
            var (success, message) = await _svc.DisableAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpGet("status/{userId}")]
        public async Task<IActionResult> GetStatus(int userId) => Ok(new { success = true, data = await _svc.GetStatusAsync(userId) });
    }

    [ApiController, Route("api/data-privacy"), Authorize]
    public class DataPrivacyController : ControllerBase
    {
        private readonly Services.DataPrivacyService _svc;
        public DataPrivacyController(Services.DataPrivacyService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePrivacyRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(int userId) => Ok(new { success = true, data = await _svc.GetForUserAsync(userId) });

        [HttpGet("pending"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPending() => Ok(new { success = true, data = await _svc.GetAllPendingAsync() });

        [HttpPost("process"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Process([FromBody] ProcessPrivacyRequestDto req)
        {
            var (success, message) = await _svc.ProcessAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/security-audit"), Authorize]
    public class SecurityAuditController : ControllerBase
    {
        private readonly Services.SecurityAuditService _svc;
        public SecurityAuditController(Services.SecurityAuditService svc) => _svc = svc;

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetForUser(int userId, [FromQuery] int take = 50) => Ok(new { success = true, data = await _svc.GetForUserAsync(userId, take) });
    }
}
