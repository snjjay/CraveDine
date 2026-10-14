using EatKath.API.Enums;

namespace EatKath.API.DTOs.SurpriseBag
{
    // Public view of a Surprise Bag. Amounts are in CurrencyCode (the
    // restaurant's currency); pickup date and times are Kathmandu time.
    public class SurpriseBagDto
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }

        public string RestaurantName { get; set; } = string.Empty;

        public string CurrencyCode { get; set; } = "NPR";

        public string Title { get; set; } = string.Empty;

        public SurpriseBagCategory Category { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Conditions { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public decimal? EstimatedValue { get; set; }

        public int MaxPerCustomer { get; set; }

        public DateOnly PickupDate { get; set; }

        public TimeOnly PickupStartTime { get; set; }

        public TimeOnly PickupEndTime { get; set; }

        // Stored image path (e.g. "/uploads/surprisebags/12/image-<guid>.jpg"),
        // or null when the bag has no image.
        public string? ImageUrl { get; set; }

        // Calculated by SurpriseBagService (Reserved and Collected
        // reservations hold stock), not mapped.
        public int RemainingQuantity { get; set; }

        public bool IsSoldOut { get; set; }
    }
}
