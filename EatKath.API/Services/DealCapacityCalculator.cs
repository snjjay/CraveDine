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
            var totalUsed = deal.ReservationLimit > 0
                ? await ActiveRedemptions(context, deal.Id).CountAsync()
                : 0;

            var dailyUsed = deal.DailyRedemptionLimit > 0
                ? await ActiveRedemptions(context, deal.Id).CountAsync(r => r.ArrivalDate == arrivalDate)
                : 0;

            return BuildCapacity(deal, totalUsed, dailyUsed);
        }

        // The arrival date availability refers to: today, or the deal's
        // start date if it has not started yet.
        public static DateOnly AvailabilityDate(Deal deal, DateOnly today)
        {
            return deal.StartDate > today ? deal.StartDate : today;
        }

        // Capacity for many deals at once (for list pages), each for its
        // AvailabilityDate. Uses two grouped queries instead of one or
        // two queries per deal; the limit rules are the same as
        // GetCapacityAsync (shared BuildCapacity).
        public static async Task<Dictionary<int, DealCapacity>> GetCapacitiesAsync(
            ApplicationDbContext context,
            IReadOnlyCollection<Deal> deals,
            DateOnly today)
        {
            var result = new Dictionary<int, DealCapacity>();

            if (deals.Count == 0)
                return result;

            var dealIds = deals.Select(d => d.Id).ToList();

            var active = context.Redemptions.Where(r =>
                dealIds.Contains(r.DealId) &&
                (r.Status == RedemptionStatus.Redeemed ||
                 r.Status == RedemptionStatus.Completed));

            var totals = await active
                .GroupBy(r => r.DealId)
                .Select(g => new { DealId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DealId, x => x.Count);

            // Availability dates are always today or later.
            var dailyCounts = await active
                .Where(r => r.ArrivalDate >= today)
                .GroupBy(r => new { r.DealId, r.ArrivalDate })
                .Select(g => new { g.Key.DealId, g.Key.ArrivalDate, Count = g.Count() })
                .ToListAsync();

            foreach (var deal in deals)
            {
                var date = AvailabilityDate(deal, today);

                var totalUsed = totals.GetValueOrDefault(deal.Id);

                var dailyUsed = dailyCounts
                    .Where(x => x.DealId == deal.Id && x.ArrivalDate == date)
                    .Sum(x => x.Count);

                result[deal.Id] = BuildCapacity(deal, totalUsed, dailyUsed);
            }

            return result;
        }

        private static DealCapacity BuildCapacity(Deal deal, int totalUsed, int dailyUsed)
        {
            int? totalRemaining = deal.ReservationLimit > 0
                ? Math.Max(0, deal.ReservationLimit - totalUsed)
                : null;

            int? dailyRemaining = deal.DailyRedemptionLimit > 0
                ? Math.Max(0, deal.DailyRedemptionLimit - dailyUsed)
                : null;

            return new DealCapacity(totalRemaining, dailyRemaining);
        }
    }
}
