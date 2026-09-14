using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M49: Audit Logs ────────────────────────────────────────────
    public class AuditLog
    {
        [Key] public int Id { get; set; }
        public int    AdminUserId  { get; set; }
        public User?  AdminUser    { get; set; }
        public string Action       { get; set; } = ""; // Create/Update/Delete/View/Login/Export
        public string Module       { get; set; } = ""; // Bookings/Users/Commission/etc.
        public string EntityType   { get; set; } = "";
        public int?   EntityId     { get; set; }
        public string? OldValues   { get; set; } // JSON
        public string? NewValues   { get; set; } // JSON
        public string? IpAddress   { get; set; }
        public string? UserAgent   { get; set; }
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    }

    // ── M50: CMS ───────────────────────────────────────────────────
    public class CmsBanner
    {
        [Key] public int Id { get; set; }
        public string Title       { get; set; } = "";
        public string? Subtitle   { get; set; }
        public string? ImageUrl   { get; set; }
        public string? DeepLink   { get; set; }
        public string  Target     { get; set; } = "All"; // All/Customer/Chef
        public int     SortOrder  { get; set; } = 0;
        public bool    IsActive   { get; set; } = true;
        public string? ValidFrom  { get; set; }
        public string? ValidTo    { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CmsPage
    {
        [Key] public int Id { get; set; }
        public string Slug      { get; set; } = ""; // terms / privacy / about / faq
        public string Title     { get; set; } = "";
        public string Content   { get; set; } = ""; // HTML/Markdown
        public bool   IsActive  { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

namespace LovEat.API.DTOs
{
    public class AuditLogDto
    {
        public int     Id           { get; set; }
        public string  AdminName    { get; set; } = "";
        public string  Action       { get; set; } = "";
        public string  Module       { get; set; } = "";
        public string  EntityType   { get; set; } = "";
        public int?    EntityId     { get; set; }
        public string? OldValues    { get; set; }
        public string? NewValues    { get; set; }
        public string? IpAddress    { get; set; }
        public string  CreatedAt    { get; set; } = "";
    }

    public class CmsBannerDto
    {
        public int     Id        { get; set; }
        public string  Title     { get; set; } = "";
        public string? Subtitle  { get; set; }
        public string? ImageUrl  { get; set; }
        public string? DeepLink  { get; set; }
        public string  Target    { get; set; } = "";
        public int     SortOrder { get; set; }
        public bool    IsActive  { get; set; }
        public string? ValidFrom { get; set; }
        public string? ValidTo   { get; set; }
    }

    public class CmsPageDto
    {
        public int    Id       { get; set; }
        public string Slug     { get; set; } = "";
        public string Title    { get; set; } = "";
        public string Content  { get; set; } = "";
        public bool   IsActive { get; set; }
        public string UpdatedAt{ get; set; } = "";
    }
}
