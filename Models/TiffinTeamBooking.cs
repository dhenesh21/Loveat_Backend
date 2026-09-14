using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    /// <summary>M11: team booking — multiple chefs assigned to one large event/booking (e.g. a wedding). Each row is one chef's assignment within the team.</summary>
    public class TeamBookingAssignment
    {
        [Key] public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int ChefId { get; set; }
        public User? Chef { get; set; }

        [MaxLength(50)]
        public string RoleInTeam { get; set; } = "Cook"; // Lead / Cook / Assistant

        [MaxLength(20)]
        public string Status { get; set; } = "Assigned"; // Assigned / Confirmed / Declined / Completed

        public decimal AssignedAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>M12: recurring corporate tiffin subscription for a company — daily meals for N employees rather than a one-off booking.</summary>
    public class CorporateTiffinBooking
    {
        [Key] public int Id { get; set; }

        /// <summary>The corporate account owner (a User with Role=Customer acting on behalf of a company).</summary>
        public int CompanyUserId { get; set; }
        public User? CompanyUser { get; set; }

        public int? PrimaryChefId { get; set; }
        public User? PrimaryChef { get; set; }

        [MaxLength(100)]
        public string CompanyName { get; set; } = "";

        public int MealsPerDay { get; set; }

        [MaxLength(30)]
        public string MealType { get; set; } = "Lunch"; // Breakfast / Lunch / Dinner / Snack

        /// <summary>Comma-separated days, e.g. "Mon,Tue,Wed,Thu,Fri"</summary>
        [MaxLength(50)]
        public string RecurringDays { get; set; } = "Mon,Tue,Wed,Thu,Fri";

        public decimal PricePerMeal { get; set; }
        public decimal MonthlyAmount { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active"; // Active / Paused / Cancelled

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
