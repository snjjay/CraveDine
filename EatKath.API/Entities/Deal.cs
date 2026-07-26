using EatKath.API.Enums;

namespace EatKath.API.Entities
{
    public class Deal : BaseEntity
    {
        public int RestaurantId { get; set; }

        // Example: 25% OFF Dine-In
        public string Title { get; set; } = string.Empty;

        // Example: Enjoy 25% off your total dine-in bill.
        public string Description { get; set; } = string.Empty;

        // Percentage Discount
        public decimal DiscountPercentage { get; set; }

        // DineIn / Takeaway / Delivery
        public OfferType OfferType { get; set; }

        // Optional promotional image
        public string PromoImageUrl { get; set; } = string.Empty;

        // Terms displayed to customers
        public string TermsAndConditions { get; set; } = string.Empty;

        // Offer validity dates
        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        // Daily offer time window
        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        // Maximum guests allowed for this offer
        public int MaximumGuests { get; set; }

        // Customer can redeem this many minutes before arrival
        public int AdvanceRedeemMinutes { get; set; } = 30;

        // Maximum completed redemptions allowed per day
        // 0 = Unlimited
        public int DailyRedemptionLimit { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Restaurant Restaurant { get; set; } = null!;

        public ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
    }
}