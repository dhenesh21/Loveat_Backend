namespace LovEat.API.DTOs
{
    // ── M89: CMS Management ────────────────────────────────────────
    public class SaveContentBlockDraftRequestDto
    {
        public string Key { get; set; } = "";
        public string Locale { get; set; } = "en";
        public string ContentType { get; set; } = "PlainText";
        public string DraftContent { get; set; } = "";
        public string? Note { get; set; }
    }

    public class ContentBlockDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public string Locale { get; set; } = "";
        public string ContentType { get; set; } = "";
        public string Status { get; set; } = "";
        public string DraftContent { get; set; } = "";
        public string? PublishedContent { get; set; }
        public int Version { get; set; }
        public string? PublishedByName { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ContentBlockVersionDto
    {
        public int Version { get; set; }
        public string Content { get; set; } = "";
        public string? SavedByName { get; set; }
        public string? Note { get; set; }
        public DateTime SavedAt { get; set; }
    }

    public class PublishContentBlockRequestDto { public int ContentBlockId { get; set; } }

    public class SaveCmsPageRequestDto
    {
        public string Slug { get; set; } = "";
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    public class CmsPageRevisionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string? EditedByName { get; set; }
        public DateTime EditedAt { get; set; }
    }

    // ── M90: Banner/Promotion Management ───────────────────────────
    public class CreatePromoCampaignRequestDto
    {
        public string Name { get; set; } = "";
        public string CampaignType { get; set; } = "Banner";
        public string TargetAudience { get; set; } = "All";
        public string? TargetCity { get; set; }
        public string? LinkedCouponCode { get; set; }
        public int Priority { get; set; } = 100;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }

    public class AddPromoVariantRequestDto
    {
        public int PromoCampaignId { get; set; }
        public string VariantName { get; set; } = "A";
        public string? ImageUrl { get; set; }
        public string? HeadlineText { get; set; }
        public string? SubText { get; set; }
        public string? DeepLink { get; set; }
        public int Weight { get; set; } = 100;
    }

    public class PromoVariantDto
    {
        public int Id { get; set; }
        public string VariantName { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string? HeadlineText { get; set; }
        public string? SubText { get; set; }
        public string? DeepLink { get; set; }
        public int Weight { get; set; }
        public int ImpressionCount { get; set; }
        public int ClickCount { get; set; }
        public decimal ClickThroughRate { get; set; }
        public bool IsActive { get; set; }
    }

    public class PromoCampaignDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string CampaignType { get; set; } = "";
        public string Status { get; set; } = "";
        public string TargetAudience { get; set; } = "";
        public string? TargetCity { get; set; }
        public string? LinkedCouponCode { get; set; }
        public int Priority { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string? CreatedByName { get; set; }
        public List<PromoVariantDto> Variants { get; set; } = new();
        public int TotalImpressions { get; set; }
        public int TotalClicks { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdatePromoCampaignStatusRequestDto
    {
        public int CampaignId { get; set; }
        public string Status { get; set; } = ""; // Scheduled / Live / Paused / Ended
    }

    public class ActivePromoRequestDto
    {
        public string Audience { get; set; } = "All"; // Customer / Chef
        public string? City { get; set; }
        public string? Placement { get; set; } // maps to CampaignType, optional filter
    }

    public class ServedPromoDto
    {
        public int CampaignId { get; set; }
        public int VariantId { get; set; }
        public string CampaignType { get; set; } = "";
        public string? ImageUrl { get; set; }
        public string? HeadlineText { get; set; }
        public string? SubText { get; set; }
        public string? DeepLink { get; set; }
    }

    public class RecordPromoEventRequestDto
    {
        public int VariantId { get; set; }
        public string EventType { get; set; } = "Impression"; // Impression / Click
    }

    public class PromoStatsDto
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = "";
        public int TotalImpressions { get; set; }
        public int TotalClicks { get; set; }
        public decimal ClickThroughRate { get; set; }
        public List<PromoDailyStatDto> DailyBreakdown { get; set; } = new();
    }

    public class PromoDailyStatDto
    {
        public string DateKey { get; set; } = "";
        public int Impressions { get; set; }
        public int Clicks { get; set; }
    }
}
