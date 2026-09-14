using System.ComponentModel.DataAnnotations;

namespace LovEat.API.Models
{
    // ══════════════════════════════════════════════════════════════
    // Chef Ops Suite — Business Suite Overview page.
    // (Equipment/Certifications and Outlets/Tax/Goals reuse the
    // pre-existing EquipmentCertificationModels.cs and
    // TaxLoyaltyGoalOutletModels.cs models — those already had DbSets
    // migrated but no service/controller, so we wire them here instead
    // of creating duplicates.)
    // ══════════════════════════════════════════════════════════════

    public class ChefInventoryItem
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }

        [MaxLength(150)] public string ItemName { get; set; } = "";
        public int QuantityOnHand { get; set; }
        public int LowStockThreshold { get; set; } = 5;
        [MaxLength(30)] public string Unit { get; set; } = "units";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ChefStaffMember
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }

        [MaxLength(100)] public string StaffName { get; set; } = "";
        [MaxLength(50)] public string Role { get; set; } = "Assistant"; // Assistant / SousChef / Server / Cleaner
        public bool Active { get; set; } = true;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }

    public class ChefPackage
    {
        [Key] public int Id { get; set; }
        public int ChefId { get; set; }
        public User? Chef { get; set; }

        [MaxLength(150)] public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int TimesBooked { get; set; } = 0;
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
