namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M67: Equipment Marketplace
    // ══════════════════════════════════════════════════════════════
    public class EquipmentListingDto
    {
        public int     Id            { get; set; }
        public int     ChefId        { get; set; }
        public string  ChefName      { get; set; } = "";
        public string  ChefCity      { get; set; } = "";
        public string  Title         { get; set; } = "";
        public string  Description   { get; set; } = "";
        public string  Category      { get; set; } = "";
        public string  Condition     { get; set; } = "";
        public string  ListingType   { get; set; } = "";
        public decimal Price         { get; set; }
        public string  PriceUnit     { get; set; } = "";
        public List<string> ImageUrls{ get; set; } = new();
        public string? Brand         { get; set; }
        public bool    IsAvailable   { get; set; }
        public string? City          { get; set; }
        public string  CreatedAt     { get; set; } = "";
    }

    public class CreateListingDto
    {
        public string  Title         { get; set; } = "";
        public string  Description   { get; set; } = "";
        public string  Category      { get; set; } = "";
        public string  Condition     { get; set; } = "Good";
        public string  ListingType   { get; set; } = "Rent";
        public decimal Price         { get; set; }
        public string  PriceUnit     { get; set; } = "per day";
        public string? Brand         { get; set; }
        public List<string> ImageUrls{ get; set; } = new();
    }

    public class EquipmentRequestDto
    {
        public int     Id              { get; set; }
        public int     ListingId       { get; set; }
        public string  ListingTitle    { get; set; } = "";
        public string  RequestedByName { get; set; } = "";
        public string  Status          { get; set; } = "";
        public string? FromDate        { get; set; }
        public string? ToDate          { get; set; }
        public decimal? TotalCost      { get; set; }
        public string? Message         { get; set; }
        public string  CreatedAt       { get; set; } = "";
    }

    /// <summary>
    /// Body for POST /api/equipment/{id}/request. Replaces an earlier
    /// `dynamic` parameter, which ASP.NET Core cannot bind a JSON request
    /// body to directly — [FromBody] needs a concrete type to deserialize
    /// into. FromDate/ToDate arrive as ISO date strings from the client and
    /// are parsed in the controller.
    /// </summary>
    public class RequestEquipmentActionDto
    {
        public string? Message  { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate   { get; set; }
    }

    /// <summary>Body for POST /api/equipment/requests/{id}/update. See RequestEquipmentActionDto for why this replaces a `dynamic` parameter.</summary>
    public class UpdateEquipmentRequestStatusDto
    {
        public string Status { get; set; } = "";
    }

    // ══════════════════════════════════════════════════════════════
    // M68: Chef Certifications
    // ══════════════════════════════════════════════════════════════
    public class CertificationCourseDto
    {
        public int     Id             { get; set; }
        public string  Title          { get; set; } = "";
        public string  Description    { get; set; } = "";
        public string  Category       { get; set; } = "";
        public string  Level          { get; set; } = "";
        public string  Provider       { get; set; } = "";
        public int     DurationHours  { get; set; }
        public decimal Fee            { get; set; }
        public string  BadgeIconUrl   { get; set; } = "";
        public bool    IsEnrolled     { get; set; }
        public bool    IsCompleted    { get; set; }
        public decimal ProgressPercent{ get; set; }
    }

    public class ChefCertificationDto
    {
        public int     Id              { get; set; }
        public string  CourseTitle     { get; set; } = "";
        public string  Category        { get; set; } = "";
        public string  Level           { get; set; } = "";
        public string  Provider        { get; set; } = "";
        public string  Status          { get; set; } = "";
        public decimal ProgressPercent { get; set; }
        public string? CertificateNo   { get; set; }
        public string? CertificateUrl  { get; set; }
        public string  EnrolledAt      { get; set; } = "";
        public string? CompletedAt     { get; set; }
        public string? ExpiresAt       { get; set; }
        public string  BadgeIconUrl    { get; set; } = "";
    }

    /// <summary>Body for POST /api/certifications/progress/{courseId}. See RequestEquipmentActionDto for why this replaces a `dynamic` parameter.</summary>
    public class UpdateCertificationProgressDto
    {
        public decimal ProgressPercent { get; set; }
    }
}
