namespace LovEat.API.DTOs
{
    public class AddTrackingEventRequestDto { public int BookingId { get; set; } public string Stage { get; set; } = ""; public DateTime? EstimatedAt { get; set; } public string? Notes { get; set; } }
    public class OrderTrackingEventDto { public string Stage { get; set; } = ""; public DateTime? EstimatedAt { get; set; } public DateTime OccurredAt { get; set; } public string? Notes { get; set; } }
    public class OrderTrackingTimelineDto { public int BookingId { get; set; } public string CurrentStage { get; set; } = ""; public List<OrderTrackingEventDto> Events { get; set; } = new(); }

    public class CreateWeatherRuleRequestDto { public string WeatherCondition { get; set; } = ""; public string SuggestedCuisines { get; set; } = ""; public string Message { get; set; } = ""; }
    public class WeatherRuleDto { public int Id { get; set; } public string WeatherCondition { get; set; } = ""; public List<string> SuggestedCuisines { get; set; } = new(); public string Message { get; set; } = ""; }
    public class GetWeatherRecommendationRequestDto { public string WeatherCondition { get; set; } = ""; public string? City { get; set; } }

    public class CreateDigitalContractRequestDto { public int BookingId { get; set; } public string TermsText { get; set; } = ""; }
    public class AcceptContractRequestDto { public int ContractId { get; set; } public string Role { get; set; } = "Customer"; public string? IpAddress { get; set; } }
    public class DigitalContractDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string TermsText { get; set; } = "";
        public bool CustomerAccepted { get; set; }
        public bool ChefAccepted { get; set; }
        public string Status { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    public class BuildItemRequestDto { public int MenuItemId { get; set; } public string DishName { get; set; } = ""; public int Quantity { get; set; } = 1; public decimal UnitPrice { get; set; } }
    public class SaveCustomMenuBuildRequestDto { public int CustomerId { get; set; } public int ChefId { get; set; } public string BuildName { get; set; } = ""; public List<BuildItemRequestDto> Items { get; set; } = new(); }
    public class CustomMenuBuildDto
    {
        public int Id { get; set; }
        public string BuildName { get; set; } = "";
        public string? ChefName { get; set; }
        public List<BuildItemRequestDto> Items { get; set; } = new();
        public decimal EstimatedTotal { get; set; }
        public string Status { get; set; } = "";
    }
    public class SendBuildToChefRequestDto { public int BuildId { get; set; } }
}
