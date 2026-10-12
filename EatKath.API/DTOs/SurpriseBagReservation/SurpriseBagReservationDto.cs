using EatKath.API.Enums;

namespace EatKath.API.DTOs.SurpriseBagReservation
{
    // A customer's reservation. Bag and restaurant details come from the
    // reservation's own bag (also when it is now inactive or past); prices
    // are what was reserved (UnitPrice), in CurrencyCode, paid at pickup.
    // Pickup date and times are Kathmandu time.
    public class SurpriseBagReservationDto
    {
        public int Id { get; set; }

        public int SurpriseBagId { get; set; }

        public string BagTitle { get; set; } = string.Empty;

        public SurpriseBagCategory Category { get; set; }

        public int RestaurantId { get; set; }

        public string RestaurantName { get; set; } = string.Empty;

        public string RestaurantAddress { get; set; } = string.Empty;

        public string RestaurantPhone { get; set; } = string.Empty;

        public string CurrencyCode { get; set; } = "NPR";

        public DateOnly PickupDate { get; set; }

        public TimeOnly PickupStartTime { get; set; }

        public TimeOnly PickupEndTime { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        // Quantity x UnitPrice: the amount to pay at pickup.
        public decimal TotalPrice { get; set; }

        public string ConfirmationCode { get; set; } = string.Empty;

        public SurpriseBagReservationStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? CollectedAt { get; set; }

        public DateTime? CancelledAt { get; set; }

        // Calculated by SurpriseBagReservationService: Reserved and the
        // pickup window hasn't started yet.
        public bool CanCancel { get; set; }
    }
}
