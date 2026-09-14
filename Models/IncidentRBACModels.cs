using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M47: Safety Incident ───────────────────────────────────────
    public class SafetyIncident
    {
        [Key] public int Id { get; set; }
        public string  IncidentNumber   { get; set; } = ""; // INC-2024-00001
        public int     ReportedByUserId { get; set; }
        public User?   ReportedBy       { get; set; }

        /// <summary>M182: true when this incident was created automatically by
        /// BookingTimeoutAlertService rather than filed by a person. When true,
        /// ReportedByUserId is the booking's chef purely for query/association
        /// convenience (SafetyIncident.ReportedByUserId is a required FK and no
        /// dedicated "system" user row exists in this codebase) — admins should not
        /// read it as "this chef filed a complaint."</summary>
        public bool IsSystemGenerated { get; set; } = false;

        public int?    BookingId        { get; set; }
        public string  IncidentType     { get; set; } = ""; // SOS / Harassment / Theft / Injury / Other
        public string  Severity         { get; set; } = "Medium"; // Low / Medium / High / Critical
        public string  Description      { get; set; } = "";
        public string  Status           { get; set; } = "Open"; // Open / InProgress / Resolved / Closed
        public decimal? Latitude        { get; set; }
        public decimal? Longitude       { get; set; }
        public string? AdminNotes       { get; set; }
        public string? Resolution       { get; set; }
        public int?    AssignedAdminId  { get; set; }
        public DateTime? ResolvedAt     { get; set; }
        public DateTime  CreatedAt      { get; set; } = DateTime.UtcNow;
        public DateTime  UpdatedAt      { get; set; } = DateTime.UtcNow;
    }

    // ── M48: RBAC - Admin Roles ────────────────────────────────────
    public class AdminRole
    {
        [Key] public int Id { get; set; }
        public string RoleName    { get; set; } = ""; // SuperAdmin / FinanceAdmin / SupportAdmin / ContentAdmin
        public string Description { get; set; } = "";
        public string Permissions { get; set; } = "[]"; // JSON array of permission keys
        public bool   IsActive    { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<AdminUser> AdminUsers { get; set; } = new List<AdminUser>();
    }

    public class AdminUser
    {
        [Key] public int Id { get; set; }
        public int    UserId      { get; set; }
        public User?  User        { get; set; }
        public int    AdminRoleId { get; set; }
        public AdminRole? AdminRole { get; set; }
        public bool   IsActive    { get; set; } = true;
        public string? CreatedBy  { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
    }
}

namespace LovEat.API.DTOs
{
    // M47 DTOs
    public class CreateIncidentDto
    {
        public string  IncidentType { get; set; } = "";
        public string  Severity     { get; set; } = "Medium";
        public string  Description  { get; set; } = "";
        public int?    BookingId    { get; set; }
        public decimal? Latitude    { get; set; }
        public decimal? Longitude   { get; set; }
    }

    public class IncidentDto
    {
        public int     Id             { get; set; }
        public string  IncidentNumber { get; set; } = "";
        public string  ReportedBy     { get; set; } = "";
        public int?    BookingId      { get; set; }
        public string  IncidentType   { get; set; } = "";
        public string  Severity       { get; set; } = "";
        public string  Description    { get; set; } = "";
        public string  Status         { get; set; } = "";
        public string? AdminNotes     { get; set; }
        public string? Resolution     { get; set; }
        public string  CreatedAt      { get; set; } = "";
        public string  UpdatedAt      { get; set; } = "";
    }

    // M48 DTOs
    public class AdminRoleDto
    {
        public int      Id          { get; set; }
        public string   RoleName    { get; set; } = "";
        public string   Description { get; set; } = "";
        public List<string> Permissions { get; set; } = new();
        public bool     IsActive    { get; set; }
        public int      AdminCount  { get; set; }
    }

    public class AdminUserDto
    {
        public int    Id         { get; set; }
        public string FullName   { get; set; } = "";
        public string Email      { get; set; } = "";
        public string Phone      { get; set; } = "";
        public string RoleName   { get; set; } = "";
        public bool   IsActive   { get; set; }
        public string? LastLogin { get; set; }
        public string CreatedAt  { get; set; } = "";
    }

    public class CreateAdminUserDto
    {
        public string FullName    { get; set; } = "";
        public string Phone       { get; set; } = "";
        public string Email       { get; set; } = "";
        public int    AdminRoleId { get; set; }
    }
}
