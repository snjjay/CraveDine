using EatKath.API.Entities;

namespace EatKath.API.Services
{
    // ==========================================================
    // Deal/restaurant availability checks used by
    // ReservationService.CreateAsync before a reservation (and its
    // linked redemption) is created.
    //
    // Pure validation only - no persistence, no side effects.
    // ==========================================================
    public static class DealAvailabilityValidator
    {
        public static void EnsureAvailable(
            Deal deal,
            Restaurant restaurant,
            DateOnly arrivalDate,
            TimeOnly arrivalTime,
            int guestCount)
        {
            if (!deal.IsActive)
                throw new BusinessRuleException("Offer is inactive.");

            if (arrivalDate < deal.StartDate ||
                arrivalDate > deal.EndDate)
            {
                throw new BusinessRuleException("Offer is not available on the selected arrival date.");
            }

            if (arrivalTime < deal.StartTime ||
                arrivalTime > deal.EndTime)
            {
                throw new BusinessRuleException("Arrival time must be within the offer time.");
            }

            if (!restaurant.IsActive)
                throw new BusinessRuleException("Restaurant is inactive.");

            if (guestCount > deal.MaximumGuests)
                throw new BusinessRuleException($"Maximum {deal.MaximumGuests} guests allowed.");
        }
    }
}
