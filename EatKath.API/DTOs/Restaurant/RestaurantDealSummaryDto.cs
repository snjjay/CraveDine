using EatKath.API.Enums;

namespace EatKath.API.DTOs.Restaurant
{
    // Compact summary of one active, not-yet-ended deal, included in the
    // restaurant list (GET api/Restaurants) so restaurant cards can show
    // offer overlays without a request per restaurant. Availability uses
    // the same rules as the restaurant deal list (DealCapacityCalculator).
    public class RestaurantDealSummaryDto
    {
        public int Id { get; set; }

        public decimal DiscountPercentage { get; set; }

        public OfferType OfferType { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        // Arrival window
        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        // Lower of the remaining total and daily offers for
        // AvailabilityDate. Null = unlimited.
        public int? RemainingOffers { get; set; }

        // Today, or the start date if the deal has not started yet.
        public DateOnly AvailabilityDate { get; set; }

        // Total offer cap used up.
        public bool IsSoldOut { get; set; }
    }
}
