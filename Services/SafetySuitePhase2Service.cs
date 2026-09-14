using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    // ══════════════════════════════════════════════════════════════
    // M182: Check-in Timeout Auto-Alert
    // ══════════════════════════════════════════════════════════════
    public class BookingTimeoutAlertService
    {
        private readonly AppDbContext _db;
        private readonly IPushNotificationService _push;
        private readonly TrustedContactAlertService _trustedContact;
        private readonly ILogger<BookingTimeoutAlertService> _logger;

        public BookingTimeoutAlertService(AppDbContext db, IPushNotificationService push,
            TrustedContactAlertService trustedContact, ILogger<BookingTimeoutAlertService> logger)
        {
            _db = db;
            _push = push;
            _trustedContact = trustedContact;
            _logger = logger;
        }

        /// <summary>Called by BookingService when a booking moves to "Accepted".</summary>
        public async Task CreateAsync(int bookingId)
        {
            var booking = await _db.Bookings.FindAsync(bookingId);
            if (booking == null) return;

            var existing = await _db.BookingTimeoutAlerts.FirstOrDefaultAsync(a => a.BookingId == bookingId);
            if (existing != null) return;

            _db.BookingTimeoutAlerts.Add(new BookingTimeoutAlert
            {
                BookingId = bookingId,
                ExpectedEndTime = booking.ScheduledAt.AddMinutes(booking.DurationMinutes),
                GraceMinutes = 30,
                AlertLevel = "None"
            });
            await _db.SaveChangesAsync();
        }

        /// <summary>Booking reached a terminal state normally — stop watching it.</summary>
        public async Task ResolveAsync(int bookingId)
        {
            var alert = await _db.BookingTimeoutAlerts.FirstOrDefaultAsync(a => a.BookingId == bookingId);
            if (alert != null && alert.ResolvedAt == null)
            {
                alert.ResolvedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>Called by BookingTimeoutMonitorService on its polling cycle. Two-step escalation:
        /// Nudge (push to both parties) first, then Escalated (SafetyIncident + trusted-contact) if
        /// still unresolved after another grace window.</summary>
        public async Task<int> ScanAndEscalateAsync()
        {
            var now = DateTime.UtcNow;
            var candidates = await _db.BookingTimeoutAlerts
                .Include(a => a.Booking)
                .Where(a => a.ResolvedAt == null && a.Booking != null &&
                            a.Booking.Status != "Completed" && a.Booking.Status != "Cancelled")
                .ToListAsync();

            int actioned = 0;
            foreach (var alert in candidates)
            {
                var deadline = alert.ExpectedEndTime.AddMinutes(alert.GraceMinutes);
                if (now < deadline) continue;

                if (alert.AlertLevel == "None")
                {
                    await FireNudgeAsync(alert);
                    actioned++;
                }
                else if (alert.AlertLevel == "Nudge" && now >= deadline.AddMinutes(alert.GraceMinutes))
                {
                    await EscalateAsync(alert);
                    actioned++;
                }
            }
            return actioned;
        }

        private async Task FireNudgeAsync(BookingTimeoutAlert alert)
        {
            alert.AlertLevel = "Nudge";
            alert.NudgeFiredAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var booking = alert.Booking!;
            await PushToUserAsync(booking.ChefId, "Booking running long", "This booking is past its expected end time — please update your status.");
            await PushToUserAsync(booking.CustomerId, "Checking in", "Your booking is running past its expected time. Everything okay?");
        }

        private async Task EscalateAsync(BookingTimeoutAlert alert)
        {
            var booking = alert.Booking!;

            var count = await _db.SafetyIncidents.CountAsync() + 1;
            var incident = new SafetyIncident
            {
                IncidentNumber = $"INC-{DateTime.UtcNow:yyyy}-{count:D5}",
                ReportedByUserId = booking.ChefId, // system-generated, attributed to the booking's chef record for query convenience — see IsSystemGenerated
                IsSystemGenerated = true,
                BookingId = booking.Id,
                IncidentType = "Other",
                Severity = "High",
                Description = $"Automated: booking #{booking.Id} exceeded expected end time by more than {alert.GraceMinutes * 2} minutes with no status update.",
                Status = "Open"
            };
            _db.SafetyIncidents.Add(incident);
            await _db.SaveChangesAsync();

            alert.AlertLevel = "Escalated";
            alert.EscalatedAt = DateTime.UtcNow;
            alert.RelatedSafetyIncidentId = incident.Id;
            await _db.SaveChangesAsync();

            _logger.LogWarning("Booking {BookingId} timeout escalated to SafetyIncident {IncidentId}", booking.Id, incident.Id);

            await _trustedContact.NotifyBothPartiesAsync(booking.Id);
        }

        private async Task PushToUserAsync(int userId, string title, string body)
        {
            var tokens = await _db.DeviceTokens.Where(t => t.UserId == userId).ToListAsync();
            foreach (var t in tokens)
            {
                try { await _push.SendAsync(t.Token, title, body); }
                catch (Exception ex) { _logger.LogWarning(ex, "Push failed for user {UserId}", userId); }
            }
        }
    }

    /// <summary>Polls every 5 minutes for bookings past their expected end time. Simple
    /// polling IHostedService — this project has no job-queue infra yet (M172/M173 not
    /// built), so this is the lightest correct implementation rather than a dependency
    /// on infrastructure that doesn't exist.</summary>
    public class BookingTimeoutMonitorService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<BookingTimeoutMonitorService> _logger;

        public BookingTimeoutMonitorService(IServiceProvider services, ILogger<BookingTimeoutMonitorService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<BookingTimeoutAlertService>();
                    var actioned = await svc.ScanAndEscalateAsync();
                    if (actioned > 0) _logger.LogInformation("BookingTimeoutMonitor actioned {Count} alerts", actioned);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "BookingTimeoutMonitor cycle failed");
                }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════
    // M183: Dedicated/Same-Chef Monthly Assignment
    // ══════════════════════════════════════════════════════════════
    public class DedicatedChefService
    {
        private readonly AppDbContext _db;
        public DedicatedChefService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message)> SetPreferredChefAsync(int userId, SetPreferredChefRequestDto req)
        {
            var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "Active");
            if (sub == null) return (false, "You need an active subscription to set a preferred chef.");

            if (req.PreferredChefId.HasValue)
            {
                var chef = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.PreferredChefId && u.Role == "Chef" && u.IsActive);
                if (chef == null) return (false, "Chef not found or not active.");
            }

            sub.PreferredChefId = req.PreferredChefId;
            sub.AutoAssignSameChef = req.PreferredChefId.HasValue && req.AutoAssignSameChef;
            sub.FallbackPolicy = req.FallbackPolicy;
            await _db.SaveChangesAsync();

            return (true, req.PreferredChefId.HasValue ? "Preferred chef set." : "Preferred chef cleared.");
        }

        /// <summary>Called by BookingService.CreateBookingAsync when the customer has an
        /// active subscription with a preferred chef and didn't explicitly pick a different
        /// chef. Returns null if there's no applicable preference (caller proceeds with
        /// req.ChefId as normal).</summary>
        public async Task<int?> ResolvePreferredChefAsync(int customerUserId)
        {
            var sub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == customerUserId && s.Status == "Active");
            if (sub?.PreferredChefId == null || !sub.AutoAssignSameChef) return null;

            var chefProfile = await _db.ChefProfiles.FirstOrDefaultAsync(p => p.UserId == sub.PreferredChefId);
            if (chefProfile != null && chefProfile.IsAvailable) return sub.PreferredChefId;

            // Preferred chef unavailable — fallback policy decides what happens next.
            // AutoAssignAlternate: caller (BookingService) proceeds with whatever chef was
            // requested/searched normally. NotifyCustomer: same, but a null-ish signal here
            // means "don't force it" — the actual notification is a client-side prompt,
            // not something this backend method fires on its own.
            return null;
        }
    }

    // ══════════════════════════════════════════════════════════════
    // M184: Party Staff Add-on
    // ══════════════════════════════════════════════════════════════
    public class EventStaffService
    {
        private readonly AppDbContext _db;
        public EventStaffService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, EventStaffRequestDto? Data)> AddRequestAsync(int customerUserId, AddEventStaffRequestDto req)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == req.BookingId && b.CustomerId == customerUserId);
            if (booking == null) return (false, "Booking not found.", null);
            if (booking.BookingType != "Event") return (false, "Party staff can only be added to Event bookings.", null);
            if (!new[] { "Bartender", "Waiter", "Server" }.Contains(req.Role))
                return (false, "Role must be Bartender, Waiter, or Server.", null);
            if (req.Count <= 0) return (false, "Count must be positive.", null);

            var entity = new EventStaffRequest
            {
                BookingId = req.BookingId,
                Role = req.Role,
                Count = req.Count,
                RatePerPerson = req.RatePerPerson,
                Status = "Requested"
            };
            _db.EventStaffRequests.Add(entity);

            var addOnCost = req.Count * req.RatePerPerson;
            booking.BaseAmount += addOnCost;
            booking.TotalAmount += addOnCost;

            await _db.SaveChangesAsync();
            return (true, "Staff request added.", await ToDtoAsync(entity.Id));
        }

        /// <summary>Chef assigns from their own StaffMember pool (M141).</summary>
        public async Task<(bool Success, string Message)> AssignStaffAsync(int chefUserId, AssignSupportStaffRequestDto req)
        {
            var request = await _db.EventStaffRequests.Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.Id == req.EventStaffRequestId);
            if (request?.Booking == null || request.Booking.ChefId != chefUserId)
                return (false, "Request not found or not your booking.");

            var staff = await _db.StaffMembers.FirstOrDefaultAsync(s => s.Id == req.StaffMemberId && s.ChefId == chefUserId && s.IsActive);
            if (staff == null) return (false, "Staff member not found.");

            var alreadyAssigned = await _db.SupportStaffAssignments.CountAsync(a => a.EventStaffRequestId == req.EventStaffRequestId && a.Status != "NoShow");
            if (alreadyAssigned >= request.Count) return (false, "This role is already fully staffed for this event.");

            _db.SupportStaffAssignments.Add(new SupportStaffAssignment
            {
                EventStaffRequestId = req.EventStaffRequestId,
                StaffMemberId = req.StaffMemberId,
                Status = "Assigned"
            });

            if (alreadyAssigned + 1 >= request.Count) request.Status = "Assigned";
            await _db.SaveChangesAsync();
            return (true, "Staff assigned.");
        }

        public async Task<List<EventStaffRequestDto>> GetForBookingAsync(int bookingId)
        {
            var requests = await _db.EventStaffRequests.Where(r => r.BookingId == bookingId).ToListAsync();
            var result = new List<EventStaffRequestDto>();
            foreach (var r in requests) result.Add(await ToDtoAsync(r.Id) ?? new EventStaffRequestDto());
            return result;
        }

        private async Task<EventStaffRequestDto?> ToDtoAsync(int id)
        {
            var r = await _db.EventStaffRequests.FirstOrDefaultAsync(x => x.Id == id);
            if (r == null) return null;

            var names = await _db.SupportStaffAssignments
                .Where(a => a.EventStaffRequestId == id)
                .Join(_db.StaffMembers, a => a.StaffMemberId, s => s.Id, (a, s) => s.Name)
                .ToListAsync();

            return new EventStaffRequestDto
            {
                Id = r.Id, BookingId = r.BookingId, Role = r.Role, Count = r.Count,
                RatePerPerson = r.RatePerPerson, Status = r.Status, AssignedStaffNames = names
            };
        }
    }
}
