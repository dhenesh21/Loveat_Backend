using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LovEat.API.Models
{
    /// <summary>
    /// Core account for every person on the platform — customers, chefs, and
    /// (optionally) admins all live in this one table, distinguished by Role.
    /// Referenced everywhere else in the codebase (batches 15-33) via a plain
    /// `int` FK + `User?` navigation, usually named for context: ChefId/Chef,
    /// CustomerId/Customer, RaisedByUserId/RaisedBy, ReportedBy, Follower, etc.
    /// EF Core resolves these automatically by the "{Name}Id" + "{Name}"
    /// convention — no extra Fluent config needed for the FK itself.
    ///
    /// NOTE for Phase 7 (migrations): since many entities each hold their own
    /// FK to this same Users table (Chef, Customer, RaisedBy, ReportedBy...),
    /// EF Core's default cascade-delete behavior on required FKs can trigger a
    /// "multiple cascade paths" error when the migration is generated. If that
    /// happens, set `.OnDelete(DeleteBehavior.Restrict)` on the relevant
    /// `HasOne(...).WithMany()` calls in AppDbContext.OnModelCreating — most of
    /// them don't need cascading deletes anyway (you don't want deleting a user
    /// to silently delete disputes/invoices/audit history that reference them).
    /// </summary>
    public class User
    {
        [Key] public int Id { get; set; }

        [Required, MaxLength(15)]
        public string PhoneNumber { get; set; } = ""; // Note: named PhoneNumber (not Phone) to match the convention already used by batches 15-33 (CrmService, LoyaltyService, ChefEarningsService, etc.)

        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>BCrypt/Argon2 hash — never store plaintext. Set by AuthService (Phase 3).</summary>
        public string? PasswordHash { get; set; }

        [Required, MaxLength(20)]
        public string Role { get; set; } = "Customer"; // Customer / Chef / Admin

        [Required, MaxLength(100)]
        public string FullName { get; set; } = "";

        public string? ProfileImageUrl { get; set; }

        public bool IsPhoneVerified { get; set; } = false;
        public bool IsEmailVerified { get; set; } = false;
        public bool IsActive { get; set; } = true;

        /// <summary>In-app wallet balance in rupees (M15). Mutated only via WalletService + WalletTransaction ledger — never edit directly.</summary>
        public decimal WalletBalance { get; set; } = 0;

        [MaxLength(10)]
        public string PreferredLanguage { get; set; } = "en"; // M-extra: multi-language support

        [MaxLength(5)]
        public string PreferredCurrency { get; set; } = "INR"; // M73: multi-currency — display currency only; all amounts are still stored/calculated in INR

        public DateTime? LastLoginAt { get; set; }

        // P0 auth hardening: brute-force lockout for password-based logins
        // (admin accounts only — customer/chef OTP login isn't affected by
        // this since it has no password to guess). Reset to 0/null on any
        // successful login.
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockedUntil { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation (one-to-one side profiles) ──
        public UserProfile? Profile { get; set; }
        public ChefProfile? ChefProfileData { get; set; }
    }
}
