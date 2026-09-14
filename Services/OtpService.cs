using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>
    /// Generates and validates OTPs (M2). Real SMS delivery via MSG91 —
    /// P1 hardening, replacing the previous Console.WriteLine stub.
    /// </summary>
    public class OtpService
    {
        private readonly AppDbContext _db;
        private readonly ISmsService _sms;
        private const int OtpLength = 6;
        private const int ExpiryMinutes = 10;
        private const int MaxRequestsPerWindow = 5;
        private static readonly TimeSpan RequestWindow = TimeSpan.FromMinutes(10);
        private const int MaxVerifyAttempts = 5;

        public OtpService(AppDbContext db, ISmsService sms) { _db = db; _sms = sms; }

        /// <summary>Generates a new OTP for the given phone/purpose, persists it, and sends it via SMS. Returns (success, message).</summary>
        public async Task<(bool Success, string Message)> SendOtpAsync(string phone, string purpose)
        {
            var windowStart = DateTime.UtcNow.Subtract(RequestWindow);
            var recentCount = await _db.OtpVerifications
                .CountAsync(o => o.Phone == phone && o.Purpose == purpose && o.CreatedAt >= windowStart);

            if (recentCount >= MaxRequestsPerWindow)
                return (false, "Too many OTP requests. Please try again in a few minutes.");

            var code = GenerateNumericCode(OtpLength);

            var otp = new OtpVerification
            {
                Phone = phone,
                OtpCode = code,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(ExpiryMinutes),
            };
            _db.OtpVerifications.Add(otp);
            await _db.SaveChangesAsync();

            var (sent, sendError) = await _sms.SendAsync(phone, $"Your LovEat verification code is {code}. It expires in {ExpiryMinutes} minutes. Do not share this code with anyone.");
            if (!sent)
            {

                if (!_sms.IsConfigured)
                {
                    // DEV-ONLY: no real SMS provider configured locally.
                    // The OTP row above is still valid — let the app proceed
                    // to the OTP screen and print the code to the console
                    // instead of failing the request.
                    Console.WriteLine($"[DEV OTP] {phone} -> {code}");
                    return (true, "OTP sent successfully (dev mode — check backend console for the code).");
                }
                // OTP row stays valid — a delivery failure shouldn't lock the
                // user out if they still somehow received it (e.g. retried
                // provider), but surface the failure so the caller can decide
                // whether to tell the user to try again.
                return (false, sendError ?? "Could not send OTP. Please try again.");
            }

            return (true, "OTP sent successfully.");
        }

        /// <summary>Validates a submitted code. Marks it used on success so it can't be replayed.</summary>
        public async Task<(bool Success, string Message)> VerifyOtpAsync(string phone, string code, string purpose)
        {
            var otp = await _db.OtpVerifications
                .Where(o => o.Phone == phone && o.Purpose == purpose && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otp == null)
                return (false, "No OTP request found. Please request a new one.");

            if (otp.ExpiresAt < DateTime.UtcNow)
                return (false, "OTP has expired. Please request a new one.");

            if (otp.Attempts >= MaxVerifyAttempts)
                return (false, "Too many incorrect attempts. Please request a new OTP.");

            if (otp.OtpCode != code)
            {
                otp.Attempts++;
                await _db.SaveChangesAsync();
                return (false, "Incorrect OTP.");
            }

            otp.IsUsed = true;
            await _db.SaveChangesAsync();
            return (true, "OTP verified.");
        }

        private static string GenerateNumericCode(int length)
        {
            var bytes = new byte[length];
            System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
            var chars = new char[length];
            for (int i = 0; i < length; i++)
                chars[i] = (char)('0' + (bytes[i] % 10));
            return new string(chars);
        }
    }
}
