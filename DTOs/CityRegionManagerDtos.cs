namespace LovEat.API.DTOs
{
    public class AssignCityManagerRequestDto { public int ServiceCityId { get; set; } public int UserId { get; set; } }
    public class CityManagerDto { public int ServiceCityId { get; set; } public string CityName { get; set; } = ""; public int UserId { get; set; } public string ManagerName { get; set; } = ""; public DateTime AssignedAt { get; set; } }
    public class CityPerformanceDto { public int ServiceCityId { get; set; } public string CityName { get; set; } = ""; public int TotalBookings { get; set; } public decimal TotalRevenue { get; set; } public int ActiveChefs { get; set; } public decimal AverageRating { get; set; } public string PeriodKey { get; set; } = ""; }
    public class CreateRegionRequestDto { public string Name { get; set; } = ""; public int? ManagerUserId { get; set; } public List<int> ServiceCityIds { get; set; } = new(); }
    public class RegionDto { public int Id { get; set; } public string Name { get; set; } = ""; public string? ManagerName { get; set; } public List<string> Cities { get; set; } = new(); public int TotalChefs { get; set; } public int TotalBookings { get; set; } }
}
