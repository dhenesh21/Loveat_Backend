using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    public class CityManagerAssignment
    {
        [Key] public int Id { get; set; }
        public int ServiceCityId { get; set; }
        public ServiceCity? ServiceCity { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }

    public class CityPerformanceSnapshot
    {
        [Key] public int Id { get; set; }
        public int ServiceCityId { get; set; }
        public ServiceCity? ServiceCity { get; set; }
        [MaxLength(20)] public string Period { get; set; } = "Monthly";
        [MaxLength(20)] public string PeriodKey { get; set; } = "";
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ActiveChefs { get; set; }
        public decimal AverageRating { get; set; }
        public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
    }

    public class Region
    {
        [Key] public int Id { get; set; }
        [Required, MaxLength(60)] public string Name { get; set; } = "";
        public int? ManagerUserId { get; set; }
        public User? Manager { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class RegionCity
    {
        [Key] public int Id { get; set; }
        public int RegionId { get; set; }
        public Region? Region { get; set; }
        public int ServiceCityId { get; set; }
        public ServiceCity? ServiceCity { get; set; }
    }
}
