using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ── M54: Service Area Management ──────────────────────────────
    public class ServiceCity
    {
        [Key] public int Id { get; set; }
        public string  CityName     { get; set; } = "";
        public string  State        { get; set; } = "";
        public decimal Latitude     { get; set; }
        public decimal Longitude    { get; set; }
        public decimal RadiusKm     { get; set; } = 25;
        public bool    IsActive     { get; set; } = true;
        public bool    IsLaunching  { get; set; } = false; // "Coming Soon"
        public int     ChefCount    { get; set; } = 0;
        public int     BookingCount { get; set; } = 0;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
        public ICollection<ServiceZone> Zones { get; set; } = new List<ServiceZone>();
    }

    public class ServiceZone
    {
        [Key] public int Id { get; set; }
        public int    ServiceCityId { get; set; }
        public ServiceCity? ServiceCity { get; set; }
        public string ZoneName     { get; set; } = "";
        public decimal Latitude    { get; set; }
        public decimal Longitude   { get; set; }
        public decimal RadiusKm    { get; set; } = 8;
        public bool    IsActive    { get; set; } = true;
        public bool    IsPeakZone  { get; set; } = false;
        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
    }

    // ── M55: AI Analytics ─────────────────────────────────────────
    public class DemandPrediction
    {
        [Key] public int Id { get; set; }
        public string  City         { get; set; } = "";
        public string  Date         { get; set; } = "";    // YYYY-MM-DD
        public int     Hour         { get; set; }          // 0–23
        public int     DayOfWeek    { get; set; }          // 0=Sun…6=Sat
        public string  BookingType  { get; set; } = "";
        public decimal PredictedDemand { get; set; }       // normalized 0–100
        public decimal ActualDemand    { get; set; } = 0;
        public decimal Confidence      { get; set; }       // 0–1
        public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;
    }
}

namespace LovEat.API.DTOs
{
    public class ServiceCityDto
    {
        public int     Id           { get; set; }
        public string  CityName     { get; set; } = "";
        public string  State        { get; set; } = "";
        public decimal Latitude     { get; set; }
        public decimal Longitude    { get; set; }
        public decimal RadiusKm     { get; set; }
        public bool    IsActive     { get; set; }
        public bool    IsLaunching  { get; set; }
        public int     ChefCount    { get; set; }
        public int     BookingCount { get; set; }
        public List<ServiceZoneDto> Zones { get; set; } = new();
    }

    public class ServiceZoneDto
    {
        public int     Id           { get; set; }
        public string  ZoneName     { get; set; } = "";
        public decimal Latitude     { get; set; }
        public decimal Longitude    { get; set; }
        public decimal RadiusKm     { get; set; }
        public bool    IsActive     { get; set; }
        public bool    IsPeakZone   { get; set; }
    }

    public class CreateCityDto
    {
        public string  CityName    { get; set; } = "";
        public string  State       { get; set; } = "";
        public decimal Latitude    { get; set; }
        public decimal Longitude   { get; set; }
        public decimal RadiusKm    { get; set; } = 25;
        public bool    IsLaunching { get; set; } = false;
    }

    public class AIAnalyticsSummaryDto
    {
        public List<DemandForecastDto> HourlyForecast  { get; set; } = new();
        public List<DemandForecastDto> WeeklyForecast  { get; set; } = new();
        public List<CityDemandDto>     CityDemand      { get; set; } = new();
        public List<string>            Recommendations  { get; set; } = new();
        public decimal                 PredictionAccuracy { get; set; }
    }

    public class DemandForecastDto
    {
        public string  Label    { get; set; } = "";
        public decimal Predicted{ get; set; }
        public decimal Actual   { get; set; }
        public decimal Confidence{ get; set; }
    }

    public class CityDemandDto
    {
        public string  City         { get; set; } = "";
        public decimal DemandScore  { get; set; }
        public int     ChefShortage { get; set; }
        public string  Trend        { get; set; } = "";
    }
}
