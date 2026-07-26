using EatKath.API.Enums;

namespace EatKath.API.DTOs.Deal
{
    public class DealDto
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }

        public string RestaurantName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal DiscountPercentage { get; set; }

        public OfferType OfferType { get; set; }

        public string PromoImageUrl { get; set; } = string.Empty;

        public string TermsAndConditions { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public int MaximumGuests { get; set; }

        public int AdvanceRedeemMinutes { get; set; }

        public int DailyRedemptionLimit { get; set; }

        public bool IsActive { get; set; }
    }
}