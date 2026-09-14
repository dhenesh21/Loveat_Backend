using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M89: CMS Management ────────────────────────────────────────
    // This extends the simple M50 CmsPage/CmsBanner CRUD (Batch 25) with a
    // proper Super Admin content layer: draft/publish workflow + full
    // version history for reusable content blocks, and a revision trail for
    // CmsPage edits. Kept in new tables (no changes to CmsPage/CmsBanner
    // themselves) so nothing already reading those tables breaks.
    public class CmsContentBlock
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(80)] public string Key { get; set; } = ""; // e.g. "home_hero_text", "onboarding_tip_1"
        [MaxLength(10)] public string Locale { get; set; } = "en";
        [MaxLength(20)] public string ContentType { get; set; } = "PlainText"; // PlainText / Html / Markdown / Json
        [MaxLength(20)] public string Status { get; set; } = "Draft"; // Draft / Published
        public string DraftContent { get; set; } = "";
        public string? PublishedContent { get; set; }
        public int Version { get; set; } = 0; // increments only on Publish
        public int? PublishedByUserId { get; set; }
        public User? PublishedBy { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CmsContentBlockVersion
    {
        [Key] public int Id { get; set; }
        public int CmsContentBlockId { get; set; }
        public CmsContentBlock? CmsContentBlock { get; set; }
        public int Version { get; set; }
        public string Content { get; set; } = "";
        public int SavedByUserId { get; set; }
        public User? SavedBy { get; set; }
        public string? Note { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }

    // Revision snapshot taken every time an existing CmsPage (from M50) is
    // saved via the new CMS Management endpoints, so admins can diff/restore.
    public class CmsPageRevision
    {
        [Key] public int Id { get; set; }
        public int CmsPageId { get; set; }
        public CmsPage? CmsPage { get; set; }
        [MaxLength(150)] public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public int EditedByUserId { get; set; }
        public User? EditedBy { get; set; }
        public DateTime EditedAt { get; set; } = DateTime.UtcNow;
    }

    // ── M90: Banner/Promotion Management ───────────────────────────
    // A campaign engine that sits above the simple M50 CmsBanner: campaigns
    // can carry multiple A/B variants, audience/city targeting, scheduling,
    // and impression/click analytics — the M50 banner rail stays as the
    // lightweight "always-on" carousel; this is for time-boxed pushes
    // (sales, festival promos, city launches) that need to be measured.
    public class PromoCampaign
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        [MaxLength(20)] public string CampaignType { get; set; } = "Banner"; // Banner / Popup / PushNotification / InAppMessage
        [MaxLength(20)] public string Status { get; set; } = "Draft"; // Draft / Scheduled / Live / Paused / Ended
        [MaxLength(20)] public string TargetAudience { get; set; } = "All"; // All / Customer / Chef / NewUsers
        [MaxLength(60)] public string? TargetCity { get; set; } // null = all cities
        [MaxLength(30)] public string? LinkedCouponCode { get; set; }
        public int Priority { get; set; } = 100; // lower shows first when multiple campaigns are live
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int CreatedByUserId { get; set; }
        public User? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<PromoCampaignVariant> Variants { get; set; } = new List<PromoCampaignVariant>();
    }

    public class PromoCampaignVariant
    {
        [Key] public int Id { get; set; }
        public int PromoCampaignId { get; set; }
        public PromoCampaign? PromoCampaign { get; set; }
        [MaxLength(10)] public string VariantName { get; set; } = "A"; // A / B / C...
        public string? ImageUrl { get; set; }
        [MaxLength(120)] public string? HeadlineText { get; set; }
        [MaxLength(300)] public string? SubText { get; set; }
        public string? DeepLink { get; set; }
        public int Weight { get; set; } = 100; // relative traffic split among a campaign's active variants
        public int ImpressionCount { get; set; } = 0;
        public int ClickCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    // Per-day rollup so the admin dashboard can chart trends without scanning
    // a raw event log; updated (upserted) on every impression/click.
    public class PromoCampaignDailyStat
    {
        [Key] public int Id { get; set; }
        public int PromoCampaignId { get; set; }
        public PromoCampaign? PromoCampaign { get; set; }
        public int PromoCampaignVariantId { get; set; }
        public PromoCampaignVariant? Variant { get; set; }
        [MaxLength(10)] public string DateKey { get; set; } = ""; // yyyy-MM-dd (UTC)
        public int Impressions { get; set; } = 0;
        public int Clicks { get; set; } = 0;
    }
}
