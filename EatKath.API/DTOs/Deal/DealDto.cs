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

        public int ReservationLimit { get; set; }

        public int DailyRedemptionLimit { get; set; }

        public bool IsActive { get; set; }

        // Walk-in availability - only filled in for the customer deal
        // list (GET api/Deal/restaurant/{id}).
        //
        // RemainingOffers: lower of the remaining total and daily
        // offers for AvailabilityDate. Null = unlimited.
        public int? RemainingOffers { get; set; }

        // The arrival date RemainingOffers refers to: today, or the
        // deal's start date if it has not started yet. Null when the
        // deal has already ended.
        public DateOnly? AvailabilityDate { get; set; }

        // True when the total offer cap is used up (no date can be
        // claimed). A used-up daily limit alone does not sell out the
        // offer - other dates may still be available.
        public bool IsSoldOut { get; set; }
    }
}