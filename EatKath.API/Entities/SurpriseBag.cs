using EatKath.API.Enums;

namespace EatKath.API.Entities
{
    // A restaurant's surplus-food bag for one pickup window. Customers
    // reserve without paying online and pay the restaurant at pickup.
    // Amounts are in the restaurant's currency (Restaurant.CurrencyCode);
    // the pickup date and times are Kathmandu business time.
    public class SurpriseBag : BaseEntity
    {
        public int RestaurantId { get; set; }

        public string Title { get; set; } = string.Empty;

        public SurpriseBagCategory Category { get; set; }

        public string Description { get; set; } = string.Empty;

        // Allergens, what to bring, pickup instructions.
        public string Conditions { get; set; } = string.Empty;

        // Paid at pickup.
        public decimal Price { get; set; }

        // Typical value of the contents ("worth about"), if given.
        public decimal? EstimatedValue { get; set; }

        // Bags offered; Reserved and Collected reservations count against it.
        public int QuantityTotal { get; set; }

        public int MaxPerCustomer { get; set; } = 1;

        public DateOnly PickupDate { get; set; }

        // Same-day window (cannot cross midnight).
        public TimeOnly PickupStartTime { get; set; }

        public TimeOnly PickupEndTime { get; set; }

        public bool IsActive { get; set; } = true;

        // Optional photo, e.g. "/uploads/surprisebags/12/image-<guid>.jpg"
        // (set only through the image upload endpoint). Null: the category
        // icon is shown instead.
        public string? ImageUrl { get; set; }

        // Navigation Properties
        public Restaurant Restaurant { get; set; } = null!;

        public ICollection<SurpriseBagReservation> Reservations { get; set; } = new List<SurpriseBagReservation>();
    }
}
