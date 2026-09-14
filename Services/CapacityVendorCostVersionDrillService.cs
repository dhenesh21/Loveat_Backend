using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class CapacityPlanningService
    {
        private readonly AppDbContext _db;
        public CapacityPlanningService(AppDbContext db) => _db = db;

        public async Task<CapacityForecastDto> GenerateAsync(GenerateCapacityForecastRequestDto req)
        {
            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
            var recentBookings = await _db.Bookings.CountAsync(b => b.CreatedAt >= threeMonthsAgo);
            var monthlyAvg = recentBookings / 3;
            var growthRate = 1.15m;
            var projectedBookings = (int)(monthlyAvg * growthRate);
            var projectedPeak = (int)(projectedBookings * 0.08m);

            var recommendation = projectedBookings > monthlyAvg * 1.3m
                ? "Booking volume is projected to grow significantly - consider scaling DB read replicas and API instance count ahead of this period."
                : "Projected growth is within normal range - current capacity should suffice.";

            var forecast = new CapacityForecast { PeriodKey = req.PeriodKey, ProjectedBookingCount = projectedBookings, ProjectedPeakConcurrentUsers = projectedPeak, ScalingRecommendation = recommendation };
            _db.CapacityForecasts.Add(forecast);
            await _db.SaveChangesAsync();

            return new CapacityForecastDto { PeriodKey = forecast.PeriodKey, ProjectedBookingCount = forecast.ProjectedBookingCount, ProjectedPeakConcurrentUsers = forecast.ProjectedPeakConcurrentUsers, ScalingRecommendation = forecast.ScalingRecommendation, GeneratedAt = forecast.GeneratedAt };
        }

        public async Task<List<CapacityForecastDto>> GetHistoryAsync()
            => (await _db.CapacityForecasts.OrderByDescending(f => f.PeriodKey).ToListAsync())
                .Select(f => new CapacityForecastDto { PeriodKey = f.PeriodKey, ProjectedBookingCount = f.ProjectedBookingCount, ProjectedPeakConcurrentUsers = f.ProjectedPeakConcurrentUsers, ScalingRecommendation = f.ScalingRecommendation, GeneratedAt = f.GeneratedAt }).ToList();
    }

    public class DependencyHealthService
    {
        private readonly AppDbContext _db;
        public DependencyHealthService(AppDbContext db) => _db = db;

        public async Task<DependencyHealthDto> LogAsync(LogDependencyHealthRequestDto req)
        {
            var check = new DependencyHealthCheck { ServiceName = req.ServiceName, Status = req.Status, ResponseTimeMs = req.ResponseTimeMs };
            _db.DependencyHealthChecks.Add(check);
            await _db.SaveChangesAsync();
            return ToDto(check);
        }

        public async Task<DependencyHealthOverviewDto> GetOverviewAsync()
        {
            var latest = await _db.DependencyHealthChecks
                .GroupBy(c => c.ServiceName)
                .Select(g => g.OrderByDescending(c => c.CheckedAt).First())
                .ToListAsync();

            return new DependencyHealthOverviewDto { Services = latest.Select(ToDto).ToList(), AllOperational = latest.All(c => c.Status == "Operational") };
        }

        private static DependencyHealthDto ToDto(DependencyHealthCheck c) => new() { ServiceName = c.ServiceName, Status = c.Status, ResponseTimeMs = c.ResponseTimeMs, CheckedAt = c.CheckedAt };
    }

    public class InfraCostService
    {
        private readonly AppDbContext _db;
        public InfraCostService(AppDbContext db) => _db = db;

        public async Task<InfraCostEntryDto> LogAsync(LogInfraCostRequestDto req)
        {
            var entry = new InfraCostEntry { PeriodKey = req.PeriodKey, ServiceCategory = req.ServiceCategory, Amount = req.Amount, BudgetLimit = req.BudgetLimit };
            _db.InfraCostEntries.Add(entry);
            await _db.SaveChangesAsync();
            return ToDto(entry);
        }

        public async Task<InfraCostSummaryDto> GetSummaryAsync(string periodKey)
        {
            var entries = await _db.InfraCostEntries.Where(e => e.PeriodKey == periodKey).ToListAsync();
            return new InfraCostSummaryDto { PeriodKey = periodKey, TotalCost = entries.Sum(e => e.Amount), ByCategory = entries.Select(ToDto).ToList() };
        }

        private static InfraCostEntryDto ToDto(InfraCostEntry e) => new() { ServiceCategory = e.ServiceCategory, Amount = e.Amount, BudgetLimit = e.BudgetLimit, OverBudget = e.BudgetLimit.HasValue && e.Amount > e.BudgetLimit.Value };
    }

    public class ApiVersionService
    {
        private readonly AppDbContext _db;
        public ApiVersionService(AppDbContext db) => _db = db;

        public async Task<ApiVersionDto> CreateAsync(CreateApiVersionRequestDto req)
        {
            var version = new ApiVersionRecord { Version = req.Version };
            _db.ApiVersionRecords.Add(version);
            await _db.SaveChangesAsync();
            return ToDto(version);
        }

        public async Task<List<ApiVersionDto>> GetAllAsync() => (await _db.ApiVersionRecords.ToListAsync()).Select(ToDto).ToList();

        public async Task<(bool Success, string Message)> DeprecateAsync(DeprecateApiVersionRequestDto req)
        {
            var version = await _db.ApiVersionRecords.FindAsync(req.VersionId);
            if (version == null) return (false, "Version not found.");
            version.Status = "Deprecated";
            version.DeprecationAnnouncedAt = DateTime.UtcNow;
            version.SunsetDate = req.SunsetDate;
            version.Notes = req.Notes;
            await _db.SaveChangesAsync();
            return (true, $"Version {version.Version} marked deprecated, sunset date {req.SunsetDate:yyyy-MM-dd}.");
        }

        private static ApiVersionDto ToDto(ApiVersionRecord v) => new() { Id = v.Id, Version = v.Version, Status = v.Status, DeprecationAnnouncedAt = v.DeprecationAnnouncedAt, SunsetDate = v.SunsetDate, Notes = v.Notes };
    }

    public class DrDrillService
    {
        private readonly AppDbContext _db;
        public DrDrillService(AppDbContext db) => _db = db;

        public async Task<DrDrillLogDto> LogAsync(LogDrDrillRequestDto req)
        {
            var drill = new DrDrillLog { ScenarioName = req.ScenarioName, Outcome = req.Outcome, RecoveryTimeMinutes = req.RecoveryTimeMinutes, Findings = req.Findings, ConductedByAdminId = req.ConductedByAdminId };
            _db.DrDrillLogs.Add(drill);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(drill);
        }

        public async Task<List<DrDrillLogDto>> GetAllAsync()
        {
            var drills = await _db.DrDrillLogs.OrderByDescending(d => d.ConductedAt).ToListAsync();
            var result = new List<DrDrillLogDto>();
            foreach (var d in drills) result.Add(await ToDtoAsync(d));
            return result;
        }

        private async Task<DrDrillLogDto> ToDtoAsync(DrDrillLog d)
        {
            var admin = await _db.Users.FindAsync(d.ConductedByAdminId);
            return new DrDrillLogDto { ScenarioName = d.ScenarioName, Outcome = d.Outcome, RecoveryTimeMinutes = d.RecoveryTimeMinutes, ConductedByName = admin?.FullName, ConductedAt = d.ConductedAt };
        }
    }

    public class OpsDashboardService
    {
        private readonly AppDbContext _db;
        public OpsDashboardService(AppDbContext db) => _db = db;

        public async Task<OpsDashboardSummaryDto> GetSummaryAsync()
        {
            var activeIncidents = await _db.SystemIncidents.CountAsync(i => i.Status != "Resolved");

            var lastBackup = await _db.BackupRecords.Where(b => b.Status == "Success").OrderByDescending(b => b.CompletedAt).Select(b => b.CompletedAt).FirstOrDefaultAsync();
            var backupsHealthy = lastBackup.HasValue && lastBackup.Value >= DateTime.UtcNow.AddDays(-2);

            var latestDeps = await _db.DependencyHealthChecks.GroupBy(c => c.ServiceName).Select(g => g.OrderByDescending(c => c.CheckedAt).First()).ToListAsync();
            var allDepsOperational = latestDeps.Count == 0 || latestDeps.All(c => c.Status == "Operational");

            var totalCompliance = await _db.ComplianceChecklistItems.CountAsync();
            var compliantCount = await _db.ComplianceChecklistItems.CountAsync(i => i.Status == "Compliant");
            var compliancePercent = totalCompliance == 0 ? 100 : Math.Round((decimal)compliantCount / totalCompliance * 100, 1);

            var pendingChanges = await _db.ChangeRequests.CountAsync(c => c.Status == "Pending");

            var overallStatus = activeIncidents > 0 && !backupsHealthy ? "Critical" : activeIncidents > 0 || !allDepsOperational || !backupsHealthy ? "Degraded" : "Healthy";

            return new OpsDashboardSummaryDto
            {
                ActiveIncidents = activeIncidents, BackupsHealthy = backupsHealthy, AllDependenciesOperational = allDepsOperational,
                CompliancePercent = compliancePercent, PendingChangeRequests = pendingChanges, OverallHealthStatus = overallStatus,
            };
        }
    }
}
