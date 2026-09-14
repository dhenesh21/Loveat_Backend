using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Helpers;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>
    /// M1: authentication. LovEat is OTP-first (no password required for
    /// Customer/Chef login) — a phone number that doesn't exist yet is
    /// created automatically on first successful OTP verify. Admin accounts
    /// go through a separate password-based flow — see AdminAuthService.
    /// </summary>
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly OtpService _otp;
        private readonly JwtHelper _jwt;
        private const int RefreshTokenExpiryDays = 30;

        public AuthService(AppDbContext db, OtpService otp, JwtHelper jwt)
        {
            _db = db;
            _otp = otp;
            _jwt = jwt;
        }

        public async Task<AuthResponseDto> VerifyOtpAndAuthenticateAsync(VerifyOtpRequestDto req)
        {
            var (otpOk, otpMessage) = await _otp.VerifyOtpAsync(req.Phone, req.Code, req.Purpose);
            if (!otpOk)
                return new AuthResponseDto { Success = false, Message = otpMessage };

            var user = await _db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == req.Phone);
            bool isNewUser = user == null;

            if (user == null)
            {
                var role = req.Role == "Chef" ? "Chef" : "Customer";
                user = new User
                {
                    PhoneNumber = req.Phone,
                    FullName = string.IsNullOrWhiteSpace(req.FullName) ? "New User" : req.FullName!,
                    Role = role,
                    IsPhoneVerified = true,
                    LastLoginAt = DateTime.UtcNow,
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                _db.UserProfiles.Add(new UserProfile { UserId = user.Id });

                if (role == "Chef")
                    _db.ChefProfiles.Add(new ChefProfile { UserId = user.Id });

                await _db.SaveChangesAsync();
            }
            else
            {
                if (!user.IsActive)
                    return new AuthResponseDto { Success = false, Message = "This account has been deactivated. Contact support." };

                user.IsPhoneVerified = true;
                user.LastLoginAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            var accessToken = _jwt.GenerateToken(user.Id, user.Role, user.PhoneNumber, user.Email);
            var refreshToken = await IssueRefreshTokenAsync(user.Id);

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            return new AuthResponseDto
            {
                Success = true,
                Message = isNewUser ? "Account created." : "Login successful.",
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                IsNewUser = isNewUser,
                User = ToUserDto(user, profile?.ProfileCompleted ?? false),
            };
        }

        public async Task<UserDto?> GetCurrentUserAsync(int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return null;

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            return ToUserDto(user, profile?.ProfileCompleted ?? false);
        }

        /// <summary>
        /// Exchanges a valid, unexpired, unrevoked refresh token for a new
        /// access token. Rotates the refresh token on every use (the old one
        /// is revoked, a new one issued) so a leaked-but-unused old token
        /// stops working the moment the legitimate client refreshes.
        /// </summary>
        public async Task<RefreshTokenResponseDto> RefreshAsync(string refreshToken)
        {
            var hash = HashToken(refreshToken);
            var record = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (record == null || record.IsRevoked || record.ExpiresAt < DateTime.UtcNow)
                return new RefreshTokenResponseDto { Success = false, Message = "Refresh token is invalid or expired. Please log in again." };

            var user = await _db.Users.FindAsync(record.UserId);
            if (user == null || !user.IsActive)
                return new RefreshTokenResponseDto { Success = false, Message = "Account not found or deactivated." };

            record.IsRevoked = true; // rotate: this token is now single-use
            var newAccessToken = _jwt.GenerateToken(user.Id, user.Role, user.PhoneNumber, user.Email);
            var newRefreshToken = await IssueRefreshTokenAsync(user.Id);

            return new RefreshTokenResponseDto { Success = true, Message = "Token refreshed.", AccessToken = newAccessToken, RefreshToken = newRefreshToken };
        }

        /// <summary>Revokes every refresh token for this user — real "logout everywhere", now that tokens are actually persisted.</summary>
        public async Task LogoutAsync(int userId)
        {
            var tokens = await _db.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked).ToListAsync();
            foreach (var t in tokens) t.IsRevoked = true;
            await _db.SaveChangesAsync();
        }

        private async Task<string> IssueRefreshTokenAsync(int userId)
        {
            var token = _jwt.GenerateRefreshToken();
            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                TokenHash = HashToken(token),
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
            });
            await _db.SaveChangesAsync();
            return token;
        }

        /// <summary>Only the hash is ever persisted — same principle as password storage, so a leaked RefreshTokens table doesn't hand out usable tokens.</summary>
        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }

        private static UserDto ToUserDto(User user, bool profileCompleted) => new()
        {
            Id = user.Id,
            Phone = user.PhoneNumber,
            Email = user.Email,
            Role = user.Role,
            FullName = user.FullName,
            ProfileImageUrl = user.ProfileImageUrl,
            IsPhoneVerified = user.IsPhoneVerified,
            WalletBalance = user.WalletBalance,
            ProfileCompleted = profileCompleted,
        };
    }
}
