namespace LovEat.API.DTOs
{
    // ══════════════════════════════════════════════════════════════
    // M11: Team booking (multiple chefs on one booking)
    // ══════════════════════════════════════════════════════════════
    public class AssignTeamChefRequestDto
    {
        public int BookingId { get; set; }
        public int ChefId { get; set; }
        public string RoleInTeam { get; set; } = "Cook"; // Lead / Cook / Assistant
        public decimal AssignedAmount { get; set; }
    }

    public class TeamAssignmentDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int ChefId { get; set; }
        public string ChefName { get; set; } = "";
        public string RoleInTeam { get; set; } = "";
        public string Status { get; set; } = "";
        public decimal AssignedAmount { get; set; }
    }

    // Admin: one row per booking that has a chef team assigned to it,
    // rather than one row per chef (that's TeamAssignmentDto, used by the
    // booking-detail view).
    public class AdminTeamBookingDto
    {
        public int BookingId { get; set; }
        public string Event { get; set; } = "";
        public int Chefs { get; set; }
        public string Status { get; set; } = "";
        public decimal TotalAmount { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    // M12: Corporate tiffin (recurring company meal subscription)
    // ══════════════════════════════════════════════════════════════
    public class CreateCorporateTiffinRequestDto
    {
        public string CompanyName { get; set; } = "";
        public int MealsPerDay { get; set; }
        public string MealType { get; set; } = "Lunch";
        public string RecurringDays { get; set; } = "Mon,Tue,Wed,Thu,Fri";
        public decimal PricePerMeal { get; set; }
        public DateTime StartDate { get; set; }
        public int? PrimaryChefId { get; set; }
    }

    public class CorporateTiffinDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public int MealsPerDay { get; set; }
        public string MealType { get; set; } = "";
        public string RecurringDays { get; set; } = "";
        public decimal PricePerMeal { get; set; }
        public decimal MonthlyAmount { get; set; }
        public string Status { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? PrimaryChefName { get; set; }
    }
}
