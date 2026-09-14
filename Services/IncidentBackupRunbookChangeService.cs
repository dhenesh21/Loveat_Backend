using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    public class SystemIncidentService
    {
        private readonly AppDbContext _db;
        public SystemIncidentService(AppDbContext db) => _db = db;

        public async Task<SystemIncidentDto> CreateAsync(CreateSystemIncidentRequestDto req)
        {
            var incident = new SystemIncident { Title = req.Title, Description = req.Description, Severity = req.Severity, OnCallAdminId = req.OnCallAdminId };
            _db.SystemIncidents.Add(incident);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(incident);
        }

        public async Task<List<SystemIncidentDto>> GetAllAsync(bool activeOnly = false)
        {
            var q = _db.SystemIncidents.AsQueryable();
            if (activeOnly) q = q.Where(i => i.Status != "Resolved");
            var incidents = await q.OrderByDescending(i => i.StartedAt).ToListAsync();
            var result = new List<SystemIncidentDto>();
            foreach (var i in incidents) result.Add(await ToDtoAsync(i));
            return result;
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(UpdateIncidentStatusRequestDto req)
        {
            var incident = await _db.SystemIncidents.FindAsync(req.IncidentId);
            if (incident == null) return (false, "Incident not found.");
            incident.Status = req.Status;
            incident.PostmortemNotes = req.PostmortemNotes ?? incident.PostmortemNotes;
            if (req.Status == "Resolved") incident.ResolvedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Updated.");
        }

        public async Task<OnCallScheduleDto> CreateOnCallAsync(CreateOnCallScheduleRequestDto req)
        {
            var schedule = new OnCallSchedule { AdminId = req.AdminId, ShiftStart = req.ShiftStart, ShiftEnd = req.ShiftEnd };
            _db.OnCallSchedules.Add(schedule);
            await _db.SaveChangesAsync();
            return await ToOnCallDtoAsync(schedule);
        }

        public async Task<List<OnCallScheduleDto>> GetOnCallScheduleAsync()
        {
            var schedules = await _db.OnCallSchedules.Where(s => s.IsActive).OrderBy(s => s.ShiftStart).ToListAsync();
            var result = new List<OnCallScheduleDto>();
            foreach (var s in schedules) result.Add(await ToOnCallDtoAsync(s));
            return result;
        }

        private async Task<SystemIncidentDto> ToDtoAsync(SystemIncident i)
        {
            var admin = i.OnCallAdminId.HasValue ? await _db.Users.FindAsync(i.OnCallAdminId.Value) : null;
            return new SystemIncidentDto { Id = i.Id, Title = i.Title, Description = i.Description, Severity = i.Severity, Status = i.Status, OnCallAdminName = admin?.FullName, StartedAt = i.StartedAt, ResolvedAt = i.ResolvedAt };
        }

        private async Task<OnCallScheduleDto> ToOnCallDtoAsync(OnCallSchedule s)
        {
            var admin = await _db.Users.FindAsync(s.AdminId);
            return new OnCallScheduleDto { Id = s.Id, AdminName = admin?.FullName, ShiftStart = s.ShiftStart, ShiftEnd = s.ShiftEnd, IsActive = s.IsActive };
        }
    }

    public class BackupTrackingService
    {
        private readonly AppDbContext _db;
        public BackupTrackingService(AppDbContext db) => _db = db;

        public async Task<BackupRecordDto> LogAsync(LogBackupRequestDto req)
        {
            var record = new BackupRecord { BackupType = req.BackupType, Status = req.Status, SizeBytes = req.SizeBytes, StorageLocation = req.StorageLocation, ErrorMessage = req.ErrorMessage, CompletedAt = req.Status == "Success" ? DateTime.UtcNow : null };
            _db.BackupRecords.Add(record);
            await _db.SaveChangesAsync();
            return ToDto(record);
        }

        public async Task<List<BackupRecordDto>> GetRecentAsync(int take = 30) => (await _db.BackupRecords.OrderByDescending(b => b.StartedAt).Take(take).ToListAsync()).Select(ToDto).ToList();

        public async Task<BackupHealthSummaryDto> GetHealthSummaryAsync()
        {
            var lastSuccess = await _db.BackupRecords.Where(b => b.Status == "Success").OrderByDescending(b => b.CompletedAt).Select(b => b.CompletedAt).FirstOrDefaultAsync();
            var failures = await _db.BackupRecords.CountAsync(b => b.Status == "Failed" && b.StartedAt >= DateTime.UtcNow.AddDays(-7));
            var isHealthy = lastSuccess.HasValue && lastSuccess.Value >= DateTime.UtcNow.AddDays(-2);
            return new BackupHealthSummaryDto { LastSuccessfulBackupAt = lastSuccess, FailuresLast7Days = failures, IsHealthy = isHealthy };
        }

        private static BackupRecordDto ToDto(BackupRecord b) => new() { Id = b.Id, BackupType = b.BackupType, Status = b.Status, SizeBytes = b.SizeBytes, StartedAt = b.StartedAt, CompletedAt = b.CompletedAt };
    }

    public class RunbookService
    {
        private readonly AppDbContext _db;
        public RunbookService(AppDbContext db) => _db = db;

        public async Task<RunbookDto> CreateAsync(CreateRunbookRequestDto req)
        {
            var runbook = new Runbook { Title = req.Title, Category = req.Category, StepsMarkdown = req.StepsMarkdown, CreatedByAdminId = req.CreatedByAdminId };
            _db.Runbooks.Add(runbook);
            await _db.SaveChangesAsync();
            return ToDto(runbook);
        }

        public async Task<List<RunbookDto>> GetAllAsync(string? category = null)
        {
            var q = _db.Runbooks.AsQueryable();
            if (!string.IsNullOrEmpty(category)) q = q.Where(r => r.Category == category);
            return (await q.OrderBy(r => r.Category).ToListAsync()).Select(ToDto).ToList();
        }

        public async Task<(bool Success, string Message)> MarkUsedAsync(int runbookId)
        {
            var runbook = await _db.Runbooks.FindAsync(runbookId);
            if (runbook == null) return (false, "Runbook not found.");
            runbook.UsageCount++;
            await _db.SaveChangesAsync();
            return (true, "Marked used.");
        }

        private static RunbookDto ToDto(Runbook r) => new() { Id = r.Id, Title = r.Title, Category = r.Category, StepsMarkdown = r.StepsMarkdown, UsageCount = r.UsageCount, UpdatedAt = r.UpdatedAt };
    }

    public class ChangeManagementService
    {
        private readonly AppDbContext _db;
        public ChangeManagementService(AppDbContext db) => _db = db;

        public async Task<ChangeRequestDto> CreateAsync(CreateChangeRequestDto req)
        {
            var change = new ChangeRequest { Title = req.Title, Description = req.Description, RiskLevel = req.RiskLevel, RequestedByAdminId = req.RequestedByAdminId, ScheduledAt = req.ScheduledAt };
            _db.ChangeRequests.Add(change);
            await _db.SaveChangesAsync();
            return await ToDtoAsync(change);
        }

        public async Task<List<ChangeRequestDto>> GetAllAsync()
        {
            var changes = await _db.ChangeRequests.OrderByDescending(c => c.CreatedAt).ToListAsync();
            var result = new List<ChangeRequestDto>();
            foreach (var c in changes) result.Add(await ToDtoAsync(c));
            return result;
        }

        public async Task<(bool Success, string Message)> DecideAsync(DecideChangeRequestDto req)
        {
            var change = await _db.ChangeRequests.FindAsync(req.ChangeRequestId);
            if (change == null) return (false, "Change request not found.");
            change.Status = req.Approve ? "Approved" : "Rejected";
            change.ApprovedByAdminId = req.ApprovedByAdminId;
            await _db.SaveChangesAsync();
            return (true, req.Approve ? "Approved." : "Rejected.");
        }

        public async Task<(bool Success, string Message)> MarkDeployedAsync(int changeRequestId)
        {
            var change = await _db.ChangeRequests.FindAsync(changeRequestId);
            if (change == null) return (false, "Change request not found.");
            if (change.Status != "Approved") return (false, "Only approved changes can be marked deployed.");
            change.Status = "Deployed";
            change.DeployedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Marked deployed.");
        }

        private async Task<ChangeRequestDto> ToDtoAsync(ChangeRequest c)
        {
            var requester = await _db.Users.FindAsync(c.RequestedByAdminId);
            var approver = c.ApprovedByAdminId.HasValue ? await _db.Users.FindAsync(c.ApprovedByAdminId.Value) : null;
            return new ChangeRequestDto { Id = c.Id, Title = c.Title, Description = c.Description, RiskLevel = c.RiskLevel, Status = c.Status, RequestedByName = requester?.FullName, ApprovedByName = approver?.FullName, ScheduledAt = c.ScheduledAt, DeployedAt = c.DeployedAt };
        }
    }
}
