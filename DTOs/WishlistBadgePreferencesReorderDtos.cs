namespace LovEat.API.DTOs
{
    // ── M149: Customer Experience — Wishlist/Favorites ──────────────
    public class AddWishlistItemRequestDto
    {
        public int CustomerId { get; set; }
        public string ItemType { get; set; } = "Chef";
        public int ItemId { get; set; }
    }

    public class WishlistItemDto
    {
        public int Id { get; set; }
        public string ItemType { get; set; } = "";
        public int ItemId { get; set; }
        public string? DisplayName { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RemoveWishlistItemRequestDto { public int WishlistItemId { get; set; } }

    // ── M150: Customer Experience — Gamification / Badges ───────────
    public class CustomerBadgeDto
    {
        public string Key { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string IconName { get; set; } = "";
        public string Tier { get; set; } = "";
        public DateTime EarnedAt { get; set; }
    }

    public class CheckAndAwardBadgesRequestDto { public int CustomerId { get; set; } }

    public class BadgeAwardResultDto
    {
        public List<CustomerBadgeDto> NewlyAwarded { get; set; } = new();
    }

    // ── M151: Customer Experience — Preferences Center ──────────────
    public class UpdatePreferencesRequestDto
    {
        public int CustomerId { get; set; }
        public bool PushNotificationsEnabled { get; set; } = true;
        public bool EmailNotificationsEnabled { get; set; } = true;
        public bool SmsNotificationsEnabled { get; set; } = true;
        public bool PromoNotificationsEnabled { get; set; } = true;
        public string Theme { get; set; } = "Light";
        public string Language { get; set; } = "en";
        public string? DefaultPaymentMethod { get; set; }
        public bool ShowSpicyWarnings { get; set; } = true;
    }

    public class CustomerPreferencesDto
    {
        public bool PushNotificationsEnabled { get; set; }
        public bool EmailNotificationsEnabled { get; set; }
        public bool SmsNotificationsEnabled { get; set; }
        public bool PromoNotificationsEnabled { get; set; }
        public string Theme { get; set; } = "";
        public string Language { get; set; } = "";
        public string? DefaultPaymentMethod { get; set; }
        public bool ShowSpicyWarnings { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // ── M152: Customer Experience — Reorder & Quick Actions ─────────
    public class SaveComboItemRequestDto
    {
        public int MenuItemId { get; set; }
        public string DishName { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
    }

    public class SaveQuickReorderComboRequestDto
    {
        public int CustomerId { get; set; }
        public int ChefId { get; set; }
        public int? SourceBookingId { get; set; }
        public string ComboName { get; set; } = "";
        public List<SaveComboItemRequestDto> Items { get; set; } = new();
    }

    public class QuickReorderComboDto
    {
        public int Id { get; set; }
        public int ChefId { get; set; }
        public string? ChefName { get; set; }
        public string ComboName { get; set; } = "";
        public List<SaveComboItemRequestDto> Items { get; set; } = new();
        public decimal EstimatedTotal { get; set; }
        public int UseCount { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    public class UseComboRequestDto { public int ComboId { get; set; } }
    public class DeleteComboRequestDto { public int ComboId { get; set; } }
}
