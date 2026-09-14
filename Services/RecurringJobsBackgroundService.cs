using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;

namespace LovEat.API.Services
{
    /// <summary>
    /// P1 hardening: real recurring background job execution, replacing the
    /// "manually trigger via an admin endpoint" pattern used by several
    /// sweep/scheduled methods across the codebase (scheduled report
    /// delivery, OTP cleanup, etc.) — those admin endpoints still work for
    /// on-demand runs, but this is what actually makes them run on a
    /// schedule without a human clicking a button.
    ///
    /// Uses .NET's built-in BackgroundService rather than pulling in
    /// Hangfire/Quartz — those need their own persistent job-storage setup
    /// (a table or separate store) which is unnecessary complexity for a
    /// single-instance-friendly polling loop like this. If LovEat scales to
    /// multiple API instances, this should move to a proper distributed
    /// scheduler (e.g. Hangfire with a shared DB job store) so jobs don't
    /// run once per instance — noted here rather than silently left as a
    /// footgun.
    /// </summary>
    public class RecurringJobsBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RecurringJobsBackgroundService> _logger;
        private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(15);

        public RecurringJobsBackgroundService(IServiceScopeFactory scopeFactory, ILogger<RecurringJobsBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RecurringJobsBackgroundService started — polling every {Interval}.", PollInterval);

            // Small initial delay so the DB/migrations are fully ready before the first sweep.
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunScheduledReportsAsync(stoppingToken);
                    await CleanupExpiredOtpsAsync(stoppingToken);
                    await CleanupExpiredRefreshTokensAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // A failed sweep must never crash the whole background
                    // loop — log and try again next interval.
                    _logger.LogError(ex, "RecurringJobsBackgroundService sweep failed.");
                }

                await Task.Delay(PollInterval, stoppingToken);
            }
        }

        private async Task RunScheduledReportsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var reportSvc = scope.ServiceProvider.GetRequiredService<ScheduledReportService>();

            var due = await db.ScheduledReports
                .Where(s => s.IsActive && s.NextRunDue <= DateTime.UtcNow)
                .Select(s => s.Id)
                .ToListAsync(ct);

            foreach (var id in due)
            {
                var (success, message, _) = await reportSvc.RunNowAsync(id);
                if (!success) _logger.LogWarning("Scheduled report {Id} failed to run: {Message}", id, message);
            }

            if (due.Count > 0) _logger.LogInformation("Ran {Count} due scheduled report(s).", due.Count);
        }

        // Hygiene: OTP rows past their expiry are useless (already unusable
        // via VerifyOtpAsync's expiry check) but accumulate forever without
        // this — keeps the table small on a high-traffic OTP purpose like login.
        private async Task CleanupExpiredOtpsAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoff = DateTime.UtcNow.AddDays(-7); // keep a week of history for support/fraud investigation, purge older
            var deleted = await db.OtpVerifications.Where(o => o.ExpiresAt < cutoff).ExecuteDeleteAsync(ct);
            if (deleted > 0) _logger.LogInformation("Purged {Count} expired OTP row(s).", deleted);
        }

        private async Task CleanupExpiredRefreshTokensAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var deleted = await db.RefreshTokens.Where(t => t.ExpiresAt < DateTime.UtcNow).ExecuteDeleteAsync(ct);
            if (deleted > 0) _logger.LogInformation("Purged {Count} expired refresh token(s).", deleted);
        }
    }
}
