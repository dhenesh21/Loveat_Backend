using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ── M125: Analytics — Custom Report Builder ────────────────────
    public class ReportBuilderService
    {
        private readonly AppDbContext _db;
        public ReportBuilderService(AppDbContext db) => _db = db;

        public async Task<ReportDefinitionDto> CreateAsync(int adminUserId, CreateReportDefinitionRequestDto req)
        {
            var def = new ReportDefinition
            {
                Name = req.Name, Metric = req.Metric, GroupBy = req.GroupBy, DateRangeType = req.DateRangeType,
                CustomFrom = req.CustomFrom, CustomTo = req.CustomTo, CreatedByAdminId = adminUserId,
            };
            _db.ReportDefinitions.Add(def);
            await _db.SaveChangesAsync();
            var admin = await _db.Users.FindAsync(adminUserId);
            return ToDto(def, admin?.FullName);
        }

        public async Task<List<ReportDefinitionDto>> GetAllAsync()
        {
            var defs = await _db.ReportDefinitions.Include(d => d.CreatedByAdmin).OrderByDescending(d => d.CreatedAt).ToListAsync();
            return defs.Select(d => ToDto(d, d.CreatedByAdmin?.FullName)).ToList();
        }

        // Runs the report definition against Booking (the shared source of
        // truth every other Analytics module in this phase also reads from)
        // — resolves the date range, groups by the chosen dimension, and
        // aggregates the chosen metric.
        public async Task<(bool Success, string Message, ReportResultDto? Result)> RunAsync(int reportDefinitionId)
        {
            var def = await _db.ReportDefinitions.FindAsync(reportDefinitionId);
            if (def == null) return (false, "Report definition not found.", null);

            var (from, to) = ResolveDateRange(def);
            var bookings = await _db.Bookings.Where(b => b.CreatedAt >= from && b.CreatedAt < to).ToListAsync();
            var chefCities = await _db.ChefProfiles.ToDictionaryAsync(c => c.UserId, c => c.City);

            Func<Booking, string> keySelector = def.GroupBy switch
            {
                "Day" => b => b.CreatedAt.ToString("yyyy-MM-dd"),
                "Week" => b => $"{System.Globalization.ISOWeek.GetYear(b.CreatedAt)}-W{System.Globalization.ISOWeek.GetWeekOfYear(b.CreatedAt):D2}",
                "Month" => b => b.CreatedAt.ToString("yyyy-MM"),
                "City" => b => chefCities.GetValueOrDefault(b.ChefId) ?? "Unknown",
                "Category" => b => b.Cuisine ?? "Unspecified",
                _ => b => b.CreatedAt.ToString("yyyy-MM-dd"),
            };

            Func<IGrouping<string, Booking>, decimal> valueSelector = def.Metric switch
            {
                "Revenue" => g => g.Where(b => b.Status == "Completed").Sum(b => b.TotalAmount),
                "BookingCount" => g => g.Count(),
                "CancellationCount" => g => g.Count(b => b.Status == "Cancelled"),
                "NewCustomers" => g => g.Select(b => b.CustomerId).Distinct().Count(),
                _ => g => g.Count(),
            };

            var rows = bookings.GroupBy(keySelector)
                .Select(g => new ReportResultRowDto { Label = g.Key, Value = valueSelector(g) })
                .OrderBy(r => r.Label).ToList();

            return (true, "OK", new ReportResultDto { ReportDefinitionId = def.Id, Name = def.Name, Metric = def.Metric, Rows = rows, Total = rows.Sum(r => r.Value) });
        }

        internal static (DateTime From, DateTime To) ResolveDateRange(ReportDefinition def)
        {
            var now = DateTime.UtcNow;
            return def.DateRangeType switch
            {
                "Last7Days" => (now.AddDays(-7), now),
                "Last30Days" => (now.AddDays(-30), now),
                "Last90Days" => (now.AddDays(-90), now),
                "Custom" => (def.CustomFrom ?? now.AddDays(-30), def.CustomTo ?? now),
                _ => (now.AddDays(-30), now),
            };
        }

        private static ReportDefinitionDto ToDto(ReportDefinition d, string? createdByName) => new()
        {
            Id = d.Id, Name = d.Name, Metric = d.Metric, GroupBy = d.GroupBy, DateRangeType = d.DateRangeType, CreatedByName = createdByName, CreatedAt = d.CreatedAt,
        };
    }

    // ── M126: Analytics — Scheduled Report Delivery ────────────────
    public class ScheduledReportService
    {
        private readonly AppDbContext _db;
        private readonly ReportBuilderService _reportSvc;
        public ScheduledReportService(AppDbContext db, ReportBuilderService reportSvc) { _db = db; _reportSvc = reportSvc; }

        public async Task<(bool Success, string Message, ScheduledReportDto? Scheduled)> CreateAsync(CreateScheduledReportRequestDto req)
        {
            if (!await _db.ReportDefinitions.AnyAsync(d => d.Id == req.ReportDefinitionId)) return (false, "Report definition not found.", null);
            var nextDue = req.Frequency switch { "Daily" => DateTime.UtcNow.AddDays(1), "Monthly" => DateTime.UtcNow.AddMonths(1), _ => DateTime.UtcNow.AddDays(7) };
            var scheduled = new ScheduledReport { ReportDefinitionId = req.ReportDefinitionId, Frequency = req.Frequency, RecipientEmails = req.RecipientEmails, NextRunDue = nextDue };
            _db.ScheduledReports.Add(scheduled);
            await _db.SaveChangesAsync();
            return (true, "Scheduled.", await ToDtoAsync(scheduled));
        }

        public async Task<List<ScheduledReportDto>> GetAllAsync()
        {
            var list = await _db.ScheduledReports.Include(s => s.ReportDefinition).OrderBy(s => s.NextRunDue).ToListAsync();
            var result = new List<ScheduledReportDto>();
            foreach (var s in list) result.Add(await ToDtoAsync(s));
            return result;
        }

        // Simulates what a cron/background worker would call — runs the
        // underlying report now, logs the outcome, and advances NextRunDue.
        // No real background scheduler exists on this platform yet, so this
        // is triggered manually/on-demand for now.
        public async Task<(bool Success, string Message, ScheduledReportRunDto? Run)> RunNowAsync(int scheduledReportId)
        {
            var scheduled = await _db.ScheduledReports.FindAsync(scheduledReportId);
            if (scheduled == null) return (false, "Scheduled report not found.", null);

            var (ok, message, result) = await _reportSvc.RunAsync(scheduled.ReportDefinitionId);
            var run = new ScheduledReportRun
            {
                ScheduledReportId = scheduledReportId, Status = ok ? "Success" : "Failed",
                ResultSummary = ok ? $"{result!.Rows.Count} rows, total {result.Total}" : message,
            };
            _db.ScheduledReportRuns.Add(run);

            scheduled.LastRunAt = DateTime.UtcNow;
            scheduled.NextRunDue = scheduled.Frequency switch { "Daily" => DateTime.UtcNow.AddDays(1), "Monthly" => DateTime.UtcNow.AddMonths(1), _ => DateTime.UtcNow.AddDays(7) };
            await _db.SaveChangesAsync();

            return (true, "Run complete.", new ScheduledReportRunDto { Status = run.Status, ResultSummary = run.ResultSummary, RunAt = run.RunAt });
        }

        private async Task<ScheduledReportDto> ToDtoAsync(ScheduledReport s)
        {
            var def = s.ReportDefinition ?? await _db.ReportDefinitions.FindAsync(s.ReportDefinitionId);
            return new ScheduledReportDto { Id = s.Id, ReportDefinitionId = s.ReportDefinitionId, ReportName = def?.Name, Frequency = s.Frequency, RecipientEmails = s.RecipientEmails, IsActive = s.IsActive, LastRunAt = s.LastRunAt, NextRunDue = s.NextRunDue };
        }
    }

    // ── M127: Analytics — A/B Test Analytics ───────────────────────
    public class ExperimentAnalyticsService
    {
        private readonly AppDbContext _db;
        public ExperimentAnalyticsService(AppDbContext db) => _db = db;

        public async Task<ExperimentDto> CreateAsync(CreateExperimentRequestDto req)
        {
            var exp = new ExperimentDefinition { Name = req.Name, Description = req.Description, VariantAName = req.VariantAName, VariantBName = req.VariantBName };
            _db.ExperimentDefinitions.Add(exp);
            await _db.SaveChangesAsync();
            return ToDto(exp);
        }

        public async Task<List<ExperimentDto>> GetAllAsync() => (await _db.ExperimentDefinitions.OrderByDescending(e => e.CreatedAt).ToListAsync()).Select(ToDto).ToList();

        public async Task<(bool Success, string Message)> UpdateStatusAsync(UpdateExperimentStatusRequestDto req)
        {
            var exp = await _db.ExperimentDefinitions.FindAsync(req.ExperimentId);
            if (exp == null) return (false, "Experiment not found.");
            exp.Status = req.Status;
            if (req.Status == "Running" && exp.StartedAt == null) exp.StartedAt = DateTime.UtcNow;
            if (req.Status == "Completed") exp.EndedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, $"Experiment set to {req.Status}.");
        }

        // Deterministic hash-bucketed 50/50 assignment — same pattern as the
        // M91 FeatureFlag rollout bucketing, so a given user always lands on
        // the same variant for a given experiment.
        public async Task<(bool Success, string Message, string? Variant)> GetVariantAsync(GetVariantRequestDto req)
        {
            var exp = await _db.ExperimentDefinitions.FindAsync(req.ExperimentId);
            if (exp == null) return (false, "Experiment not found.", null);
            if (exp.Status != "Running") return (false, "Experiment is not currently running.", null);

            var bucket = Math.Abs((req.ExperimentId + ":" + req.UserId).GetHashCode()) % 2;
            var variant = bucket == 0 ? exp.VariantAName : exp.VariantBName;

            if (!await _db.ExperimentEvents.AnyAsync(e => e.ExperimentId == req.ExperimentId && e.UserId == req.UserId && e.EventType == "Exposure"))
            {
                _db.ExperimentEvents.Add(new ExperimentEvent { ExperimentId = req.ExperimentId, UserId = req.UserId, Variant = variant, EventType = "Exposure" });
                await _db.SaveChangesAsync();
            }
            return (true, "OK", variant);
        }

        public async Task<(bool Success, string Message)> RecordEventAsync(RecordExperimentEventRequestDto req)
        {
            var exposure = await _db.ExperimentEvents.FirstOrDefaultAsync(e => e.ExperimentId == req.ExperimentId && e.UserId == req.UserId && e.EventType == "Exposure");
            var variant = exposure?.Variant ?? "Unknown";
            _db.ExperimentEvents.Add(new ExperimentEvent { ExperimentId = req.ExperimentId, UserId = req.UserId, Variant = variant, EventType = req.EventType, Value = req.Value });
            await _db.SaveChangesAsync();
            return (true, "Recorded.");
        }

        public async Task<ExperimentResultsDto?> GetResultsAsync(int experimentId)
        {
            var exp = await _db.ExperimentDefinitions.FindAsync(experimentId);
            if (exp == null) return null;
            var events = await _db.ExperimentEvents.Where(e => e.ExperimentId == experimentId).ToListAsync();

            ExperimentVariantResultDto BuildVariant(string name)
            {
                var exposures = events.Count(e => e.Variant == name && e.EventType == "Exposure");
                var conversions = events.Count(e => e.Variant == name && e.EventType == "Conversion");
                return new ExperimentVariantResultDto
                {
                    Variant = name, Exposures = exposures, Conversions = conversions,
                    ConversionRate = exposures == 0 ? 0 : Math.Round((decimal)conversions / exposures * 100, 2),
                    TotalValue = events.Where(e => e.Variant == name && e.EventType == "Conversion").Sum(e => e.Value ?? 0),
                };
            }

            var variantA = BuildVariant(exp.VariantAName);
            var variantB = BuildVariant(exp.VariantBName);
            decimal? lift = variantA.ConversionRate == 0 ? null : Math.Round((variantB.ConversionRate - variantA.ConversionRate) / variantA.ConversionRate * 100, 1);

            return new ExperimentResultsDto { ExperimentId = experimentId, Name = exp.Name, Variants = new List<ExperimentVariantResultDto> { variantA, variantB }, LiftPercent = lift };
        }

        private static ExperimentDto ToDto(ExperimentDefinition e) => new()
        {
            Id = e.Id, Name = e.Name, Description = e.Description, VariantAName = e.VariantAName, VariantBName = e.VariantBName,
            Status = e.Status, StartedAt = e.StartedAt, EndedAt = e.EndedAt,
        };
    }

    // ── M128: Analytics — Data Export ──────────────────────────────
    public class DataExportService
    {
        private readonly AppDbContext _db;
        private const int PREVIEW_ROW_CAP = 50;
        public DataExportService(AppDbContext db) => _db = db;

        // Synchronous export (no background worker infra exists yet) —
        // suitable for moderate result sizes. Stores the true record count
        // plus a capped JSON preview so even large exports are summarized
        // honestly rather than silently truncated without saying so.
        public async Task<(bool Success, string Message, DataExportJobDto? Job)> CreateAsync(int adminUserId, CreateExportJobRequestDto req)
        {
            object? rawData = req.ExportType switch
            {
                "Bookings" => await _db.Bookings.Where(b => b.CreatedAt >= req.DateRangeFrom && b.CreatedAt <= req.DateRangeTo)
                    .Select(b => new { b.Id, b.CustomerId, b.ChefId, b.Status, b.TotalAmount, b.CreatedAt }).ToListAsync(),
                "Users" => await _db.Users.Where(u => u.CreatedAt >= req.DateRangeFrom && u.CreatedAt <= req.DateRangeTo)
                    .Select(u => new { u.Id, u.FullName, u.Role, u.CreatedAt }).ToListAsync(),
                "Invoices" => await _db.Invoices.Where(i => i.CreatedAt >= req.DateRangeFrom && i.CreatedAt <= req.DateRangeTo)
                    .Select(i => new { i.Id, i.InvoiceNumber, i.TotalAmount, i.CreatedAt }).ToListAsync(),
                "Reviews" => await _db.Reviews.Where(r => r.CreatedAt >= req.DateRangeFrom && r.CreatedAt <= req.DateRangeTo)
                    .Select(r => new { r.Id, r.ChefId, r.Rating, r.CreatedAt }).ToListAsync(),
                _ => null,
            };
            if (rawData == null) return (false, $"Unsupported export type '{req.ExportType}'.", null);

            var list = ((System.Collections.IEnumerable)rawData).Cast<object>().ToList();
            var preview = list.Take(PREVIEW_ROW_CAP).ToList();

            var job = new DataExportJob
            {
                ExportType = req.ExportType, Format = req.Format, DateRangeFrom = req.DateRangeFrom, DateRangeTo = req.DateRangeTo,
                RecordCount = list.Count, ResultPreview = System.Text.Json.JsonSerializer.Serialize(preview),
                RequestedByAdminId = adminUserId, CompletedAt = DateTime.UtcNow,
            };
            _db.DataExportJobs.Add(job);
            await _db.SaveChangesAsync();

            var admin = await _db.Users.FindAsync(adminUserId);
            return (true, $"Export complete — {list.Count} record(s) ({Math.Min(list.Count, PREVIEW_ROW_CAP)} shown in preview).", ToDto(job, admin?.FullName));
        }

        public async Task<List<DataExportJobDto>> GetAllAsync()
        {
            var jobs = await _db.DataExportJobs.Include(j => j.RequestedByAdmin).OrderByDescending(j => j.RequestedAt).ToListAsync();
            return jobs.Select(j => ToDto(j, j.RequestedByAdmin?.FullName)).ToList();
        }

        private static DataExportJobDto ToDto(DataExportJob j, string? requestedByName) => new()
        {
            Id = j.Id, ExportType = j.ExportType, Format = j.Format, DateRangeFrom = j.DateRangeFrom, DateRangeTo = j.DateRangeTo,
            Status = j.Status, RecordCount = j.RecordCount, ResultPreview = j.ResultPreview, RequestedByName = requestedByName,
            RequestedAt = j.RequestedAt, CompletedAt = j.CompletedAt,
        };
    }
}

namespace LovEat.API.Controllers
{
    [ApiController, Route("api/report-builder"), Authorize(Roles = "Admin")]
    public class ReportBuilderController : ControllerBase
    {
        private readonly Services.ReportBuilderService _svc;
        public ReportBuilderController(Services.ReportBuilderService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReportDefinitionRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(UserId, req) });

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("{id}/run")]
        public async Task<IActionResult> Run(int id)
        {
            var (success, message, result) = await _svc.RunAsync(id);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, data = result });
        }
    }

    [ApiController, Route("api/scheduled-reports"), Authorize(Roles = "Admin")]
    public class ScheduledReportController : ControllerBase
    {
        private readonly Services.ScheduledReportService _svc;
        public ScheduledReportController(Services.ScheduledReportService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateScheduledReportRequestDto req)
        {
            var (success, message, scheduled) = await _svc.CreateAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = scheduled });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("run")]
        public async Task<IActionResult> RunNow([FromBody] RunScheduledReportRequestDto req)
        {
            var (success, message, run) = await _svc.RunNowAsync(req.ScheduledReportId);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = run });
        }
    }

    [ApiController, Route("api/experiments"), Authorize(Roles = "Admin")]
    public class ExperimentAnalyticsController : ControllerBase
    {
        private readonly Services.ExperimentAnalyticsService _svc;
        public ExperimentAnalyticsController(Services.ExperimentAnalyticsService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExperimentRequestDto req) => Ok(new { success = true, data = await _svc.CreateAsync(req) });

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });

        [HttpPost("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateExperimentStatusRequestDto req)
        {
            var (success, message) = await _svc.UpdateStatusAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }

        [HttpPost("{id}/results")]
        public async Task<IActionResult> GetResults(int id)
        {
            var results = await _svc.GetResultsAsync(id);
            if (results == null) return NotFound(new { success = false, message = "Experiment not found." });
            return Ok(new { success = true, data = results });
        }
    }

    // Public/authenticated-user surface — apps call this to get a user's
    // assigned variant and to report conversions.
    [ApiController, Route("api/experiments/assign")]
    public class ExperimentAssignmentController : ControllerBase
    {
        private readonly Services.ExperimentAnalyticsService _svc;
        public ExperimentAssignmentController(Services.ExperimentAnalyticsService svc) => _svc = svc;

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> GetVariant([FromBody] GetVariantRequestDto req)
        {
            var (success, message, variant) = await _svc.GetVariantAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, data = new { variant } });
        }

        [HttpPost("event"), AllowAnonymous]
        public async Task<IActionResult> RecordEvent([FromBody] RecordExperimentEventRequestDto req)
        {
            var (success, message) = await _svc.RecordEventAsync(req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message });
        }
    }

    [ApiController, Route("api/data-export"), Authorize(Roles = "Admin")]
    public class DataExportController : ControllerBase
    {
        private readonly Services.DataExportService _svc;
        public DataExportController(Services.DataExportService svc) => _svc = svc;
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExportJobRequestDto req)
        {
            var (success, message, job) = await _svc.CreateAsync(UserId, req);
            if (!success) return BadRequest(new { success, message });
            return Ok(new { success, message, data = job });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(new { success = true, data = await _svc.GetAllAsync() });
    }
}
