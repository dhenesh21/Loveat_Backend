using Microsoft.EntityFrameworkCore;
using LovEat.API.Data;
using LovEat.API.DTOs;
using LovEat.API.Models;

namespace LovEat.API.Services
{
    /// <summary>M11: assigning multiple chefs to one large booking (e.g. a wedding). The parent Booking row (Phase 2) stays the single source of truth for status/payment; this just tracks which chefs are on the team and their individual roles/cuts.</summary>
    public class TeamBookingService
    {
        private readonly AppDbContext _db;
        public TeamBookingService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message, TeamAssignmentDto? Data)> AssignChefAsync(int customerId, AssignTeamChefRequestDto req)
        {
            var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == req.BookingId && b.CustomerId == customerId);
            if (booking == null) return (false, "Booking not found.", null);

            var chef = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.ChefId && u.Role == "Chef");
            if (chef == null) return (false, "Chef not found.", null);

            var existing = await _db.TeamBookingAssignments.AnyAsync(a => a.BookingId == req.BookingId && a.ChefId == req.ChefId);
            if (existing) return (false, "This chef is already assigned to this booking.", null);

            var assignment = new TeamBookingAssignment
            {
                BookingId = req.BookingId,
                ChefId = req.ChefId,
                RoleInTeam = req.RoleInTeam,
                AssignedAmount = req.AssignedAmount,
                Status = "Assigned",
            };
            _db.TeamBookingAssignments.Add(assignment);
            await _db.SaveChangesAsync();

            return (true, "Chef assigned to team.", new TeamAssignmentDto
            {
                Id = assignment.Id,
                BookingId = assignment.BookingId,
                ChefId = assignment.ChefId,
                ChefName = chef.FullName,
                RoleInTeam = assignment.RoleInTeam,
                Status = assignment.Status,
                AssignedAmount = assignment.AssignedAmount,
            });
        }

        public async Task<(bool Success, string Message)> RespondAsync(int chefId, int assignmentId, bool accept)
        {
            var assignment = await _db.TeamBookingAssignments.FirstOrDefaultAsync(a => a.Id == assignmentId && a.ChefId == chefId);
            if (assignment == null) return (false, "Assignment not found.");

            assignment.Status = accept ? "Confirmed" : "Declined";
            await _db.SaveChangesAsync();
            return (true, accept ? "Assignment confirmed." : "Assignment declined.");
        }

        public async Task<List<TeamAssignmentDto>> GetTeamAsync(int bookingId)
        {
            var assignments = await _db.TeamBookingAssignments.Where(a => a.BookingId == bookingId).ToListAsync();
            var result = new List<TeamAssignmentDto>();

            foreach (var a in assignments)
            {
                var chef = await _db.Users.FindAsync(a.ChefId);
                result.Add(new TeamAssignmentDto
                {
                    Id = a.Id,
                    BookingId = a.BookingId,
                    ChefId = a.ChefId,
                    ChefName = chef?.FullName ?? "",
                    RoleInTeam = a.RoleInTeam,
                    Status = a.Status,
                    AssignedAmount = a.AssignedAmount,
                });
            }

            return result;
        }

        /// <summary>Admin: every booking that has a chef team assigned, one row per booking</summary>
        public async Task<List<AdminTeamBookingDto>> GetAllTeamBookingsAsync()
        {
            var bookingIds = await _db.TeamBookingAssignments.Select(a => a.BookingId).Distinct().ToListAsync();
            var bookings = await _db.Bookings.Where(b => bookingIds.Contains(b.Id)).ToListAsync();
            var counts = await _db.TeamBookingAssignments
                .GroupBy(a => a.BookingId)
                .Select(g => new { BookingId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BookingId, x => x.Count);

            return bookings
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new AdminTeamBookingDto
                {
                    BookingId = b.Id,
                    Event = !string.IsNullOrWhiteSpace(b.Notes) ? b.Notes! : $"{b.BookingType} · {b.GuestCount} guests",
                    Chefs = counts.TryGetValue(b.Id, out var c) ? c : 0,
                    Status = b.Status,
                    TotalAmount = b.TotalAmount,
                }).ToList();
        }
    }
}
