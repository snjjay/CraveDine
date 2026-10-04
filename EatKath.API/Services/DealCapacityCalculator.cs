using EatKath.API.Data;
using EatKath.API.Entities;
using EatKath.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    // Remaining offers for a deal. Null means unlimited.
    public sealed record DealCapacity(int? TotalRemaining, int? DailyRemaining)
    {
        // The lower of the total and daily remaining offers.
        public int? Remaining =>
            TotalRemaining is null ? DailyRemaining :
            DailyRemaining is null ? TotalRemaining :
            Math.Min(TotalRemaining.Value, DailyRemaining.Value);
    }

    // ==========================================================
    // Offer limits (walk-in redemption model)
    // ==========================================================
    //
    // Deal.ReservationLimit     = total offer cap      (0 = unlimited)
    // Deal.DailyRedemptionLimit = claims per arrival date (0 = unlimited)
    //
    // Only "active" redemptions use up an offer: Redeemed (claimed,
    // not yet visited) and Completed (visited, bill recorded).
    // Cancelled and Expired claims give the offer back.
    //
    // Redemptions auto-created by legacy reservations are counted
    // too, so both flows share the same limits.
    public static class DealCapacityCalculator
    {
        public static IQueryable<Redemption> ActiveRedemptions(
            ApplicationDbContext context,
            int dealId)
        {
            return context.Redemptions.Where(r =>
                r.DealId == dealId &&
                (r.Status == RedemptionStatus.Redeemed ||
                 r.Status == RedemptionStatus.Completed));
        }

        public static async Task<DealCapacity> GetCapacityAsync(
            ApplicationDbContext context,
            Deal deal,
            DateOnly arrivalDate)
        {
            int? totalRemaining = null;
            int? dailyRemaining = null;

            if (deal.ReservationLimit > 0)
            {
                var totalUsed = await ActiveRedemptions(context, deal.Id)
                    .CountAsync();

                totalRemaining = Math.Max(0, deal.ReservationLimit - totalUsed);
            }

            if (deal.DailyRedemptionLimit > 0)
            {
                var dailyUsed = await ActiveRedemptions(context, deal.Id)
                    .CountAsync(r => r.ArrivalDate == arrivalDate);

                dailyRemaining = Math.Max(0, deal.DailyRedemptionLimit - dailyUsed);
            }

            return new DealCapacity(totalRemaining, dailyRemaining);
        }
    }
}
