namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M75: Localization
    // ══════════════════════════════════════════════════════════════
    public class SupportedLanguageDto
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string NativeName { get; set; } = "";
    }

    public class UpsertTranslationRequestDto
    {
        public string Key { get; set; } = "";
        public string LanguageCode { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class SetPreferredLanguageRequestDto
    {
        public string Code { get; set; } = "en";
    }

    // ══════════════════════════════════════════════════════════════
    // M76: Meal box delivery
    // ══════════════════════════════════════════════════════════════
    public class MealBoxPlanDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int MealsPerBox { get; set; }
        public decimal PricePerBox { get; set; }
        public string Frequency { get; set; } = "";
        public string? CuisineType { get; set; }
        public string? Description { get; set; }
    }

    public class SubscribeMealBoxRequestDto
    {
        public int PlanId { get; set; }
        public string DeliveryAddress { get; set; } = "";
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }

    public class MealBoxSubscriptionDto
    {
        public int Id { get; set; }
        public string PlanName { get; set; } = "";
        public int MealsPerBox { get; set; }
        public decimal PricePerBox { get; set; }
        public string Frequency { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime NextDeliveryDate { get; set; }
        public string DeliveryAddress { get; set; } = "";
    }

    public class MealBoxDeliveryDto
    {
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string Status { get; set; } = "";
        public DateTime? DeliveredAt { get; set; }
    }
}
