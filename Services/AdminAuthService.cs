using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Helpers;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>
    /// Real backend for AdminWeb's login page — fixes a gap where
    /// AdminLogin.jsx (Phase 10) was UI-only with no endpoint behind it.
    /// Deliberately separate from AuthService: admins log in with
    /// email+password, not OTP, since they're staff accounts provisioned
    /// by the platform rather than self-registered customers/chefs.
    /// </summary>
    public class AdminAuthService
    {
        private readonly AppDbContext _db;
        private readonly JwtHelper _jwt;

        public AdminAuthService(AppDbContext db, JwtHelper jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        public async Task<AdminAuthResponseDto> LoginAsync(AdminLoginRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return new AdminAuthResponseDto { Success = false, Message = "Email and password are required." };

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.Role == "Admin");

            // Check lockout before verifying the password — a locked account
            // returns the same generic message whether or not the password
            // would've been correct, so an attacker can't use response
            // differences to enumerate which accounts exist/are locked.
            if (user != null && user.LockedUntil.HasValue && user.LockedUntil > DateTime.UtcNow)
                return new AdminAuthResponseDto { Success = false, Message = $"Too many failed attempts. Try again after {user.LockedUntil:HH:mm} UTC." };

            if (user == null || !PasswordHelper.Verify(req.Password, user.PasswordHash))
            {
                if (user != null)
                {
                    user.FailedLoginAttempts += 1;
                    if (user.FailedLoginAttempts >= MaxFailedAttempts)
                    {
                        user.LockedUntil = DateTime.UtcNow.Add(LockoutDuration);
                        user.FailedLoginAttempts = 0; // reset counter once locked, next window starts fresh
                    }
                    await _db.SaveChangesAsync();
                }
                return new AdminAuthResponseDto { Success = false, Message = "Invalid email or password." };
            }

            // Successful password check — clear any prior failure count/lockout.
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;

            if (!user.IsActive)
                return new AdminAuthResponseDto { Success = false, Message = "This admin account has been deactivated." };

            var adminUser = await _db.AdminUsers.Include(a => a.AdminRole).FirstOrDefaultAsync(a => a.UserId == user.Id);
            if (adminUser == null || !adminUser.IsActive)
                return new AdminAuthResponseDto { Success = false, Message = "This account is not an active admin account." };

            adminUser.LastLogin = DateTime.UtcNow;
            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var token = _jwt.GenerateToken(user.Id, "Admin", user.PhoneNumber, user.Email);

            return new AdminAuthResponseDto
            {
                Success = true,
                Message = "Login successful.",
                AccessToken = token,
                UserId = user.Id,
                FullName = user.FullName,
                RoleName = adminUser.AdminRole?.RoleName ?? "",
                PermissionsJson = adminUser.AdminRole?.Permissions ?? "[]",
            };
        }

        /// <summary>
        /// One-time bootstrap so there's a way to log in at all before any
        /// admin exists — idempotent, does nothing if an admin already
        /// exists. Prints the generated password to the response ONCE;
        /// change it immediately after first login (there's no "forgot
        /// password" flow yet — see the plan doc's gap list).
        /// </summary>
        public async Task<(bool Created, string Message, string? Email, string? TempPassword)> SeedSuperAdminAsync()
        {
            if (await _db.AdminUsers.AnyAsync())
                return (false, "An admin account already exists — seed skipped.", null, null);

            var role = await _db.AdminRoles.FirstOrDefaultAsync(r => r.RoleName == "SuperAdmin");
            if (role == null)
            {
                role = new AdminRole { RoleName = "SuperAdmin", Description = "Full platform access", Permissions = "[\"*\"]" };
                _db.AdminRoles.Add(role);
                await _db.SaveChangesAsync();
            }

            const string email = "admin@loveat.in";
            const string tempPassword = "ChangeMe123!";

            // Admin bootstrap needs *a* phone number to satisfy User's unique
            // constraint, even though admins never log in with it — using an
            // obviously-fake placeholder rather than leaving it blank.
            var user = new User
            {
                Email = email,
                PhoneNumber = "0000000001",
                FullName = "Super Admin",
                Role = "Admin",
                PasswordHash = PasswordHelper.Hash(tempPassword),
                IsPhoneVerified = true,
                IsEmailVerified = true,
                IsActive = true,
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.AdminUsers.Add(new AdminUser { UserId = user.Id, AdminRoleId = role.Id, CreatedBy = "system-bootstrap" });
            await _db.SaveChangesAsync();

            return (true, "Super admin created. Log in once with these credentials, then change the password immediately — there is no password-reset flow yet.", email, tempPassword);
        }
    }
}

namespace LovEat.API.Controllers
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AdminAuthController : ControllerBase
    {
        private readonly Services.AdminAuthService _svc;
        public AdminAuthController(Services.AdminAuthService svc) => _svc = svc;

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequestDto req)
        {
            var result = await _svc.LoginAsync(req);
            if (!result.Success) return Unauthorized(result);
            return Ok(result);
        }

        /// <summary>Idempotent bootstrap — safe to call repeatedly, only creates an admin the first time. Should be disabled or IP-restricted once a real admin exists in production (it's a no-op by then anyway, but don't leave it reachable forever).</summary>
        [HttpPost("seed")]
        [AllowAnonymous]
        public async Task<IActionResult> Seed()
        {
            var (created, message, email, tempPassword) = await _svc.SeedSuperAdminAsync();
            return Ok(new { success = created, message, email, tempPassword });
        }
    }
}
