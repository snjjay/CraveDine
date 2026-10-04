using EatKath.API.Enums;

namespace EatKath.API.DTOs.Redemption
{
    public class RedemptionDto
    {
        public int Id { get; set; }

        public int DealId { get; set; }

        public string DealTitle { get; set; } = string.Empty;

        public int RestaurantId { get; set; }

        public string RestaurantName { get; set; } = string.Empty;

        public int UserId { get; set; }

        // Customer contact details always come from the User account.
        public string CustomerName { get; set; } = string.Empty;

        public string CustomerPhone { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        // Set only for redemptions created by the legacy reservation
        // flow; null for walk-in offer claims.
        public int? ReservationId { get; set; }

        public DateOnly ArrivalDate { get; set; }

        public TimeOnly ArrivalTime { get; set; }

        public int GuestCount { get; set; }

        public decimal? BillAmount { get; set; }

        public decimal? DiscountAmount { get; set; }

        public decimal? FinalAmount { get; set; }

        // Resolved from Deal.Restaurant.CurrencyCode at read time -
        // Redemption itself carries no currency field of its own.
        public string CurrencyCode { get; set; } = "NPR";

        public RedemptionStatus Status { get; set; }

        public DateTime RedeemedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}