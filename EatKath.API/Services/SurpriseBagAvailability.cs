using EatKath.API.Data;
using EatKath.API.Entities;
using EatKath.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Services
{
    // How much of a bag is held by reservations. Reserved and Collected
    // reservations hold stock; Cancelled and NoShow free it again.
    public readonly record struct SurpriseBagUsage(int ReservedQuantity, int CollectedQuantity)
    {
        public static readonly SurpriseBagUsage None = new(0, 0);

        public int UsedQuantity => ReservedQuantity + CollectedQuantity;

        public int RemainingOf(int quantityTotal) => Math.Max(0, quantityTotal - UsedQuantity);
    }

    // Shared Surprise Bag stock and visibility rules ("today" and "now" are
    // Kathmandu business time from TimeProvider.GetLocalNow()).
    public static class SurpriseBagAvailability
    {
        // Shown to customers: active bag at an active restaurant whose
        // pickup window hasn't ended (today's open bags and upcoming ones).
        // Sold-out bags stay visible.
        public static IQueryable<SurpriseBag> PubliclyVisible(
            this IQueryable<SurpriseBag> bags,
            DateOnly today,
            TimeOnly now)
        {
            return bags.Where(b =>
                b.IsActive &&
                b.Restaurant.IsActive &&
                (b.PickupDate > today ||
                    (b.PickupDate == today && b.PickupEndTime > now)));
        }

        public static bool HasPickupEnded(DateOnly pickupDate, TimeOnly pickupEndTime, DateOnly today, TimeOnly now)
        {
            return pickupDate < today || (pickupDate == today && pickupEndTime <= now);
        }

        // Usage for many bags in one query (bags without stock-holding
        // reservations are absent: use SurpriseBagUsage.None).
        public static async Task<Dictionary<int, SurpriseBagUsage>> GetUsageAsync(
            ApplicationDbContext context,
            IReadOnlyCollection<int> bagIds)
        {
            if (bagIds.Count == 0)
                return new Dictionary<int, SurpriseBagUsage>();

            var rows = await context.SurpriseBagReservations
                .Where(r =>
                    bagIds.Contains(r.SurpriseBagId) &&
                    (r.Status == SurpriseBagReservationStatus.Reserved ||
                     r.Status == SurpriseBagReservationStatus.Collected))
                .GroupBy(r => new { r.SurpriseBagId, r.Status })
                .Select(g => new { g.Key.SurpriseBagId, g.Key.Status, Quantity = g.Sum(r => r.Quantity) })
                .ToListAsync();

            return rows
                .GroupBy(r => r.SurpriseBagId)
                .ToDictionary(
                    g => g.Key,
                    g => new SurpriseBagUsage(
                        g.Where(r => r.Status == SurpriseBagReservationStatus.Reserved).Sum(r => r.Quantity),
                        g.Where(r => r.Status == SurpriseBagReservationStatus.Collected).Sum(r => r.Quantity)));
        }

        public static async Task<SurpriseBagUsage> GetUsageAsync(ApplicationDbContext context, int bagId)
        {
            var usage = await GetUsageAsync(context, new[] { bagId });

            return usage.GetValueOrDefault(bagId, SurpriseBagUsage.None);
        }

        // The most stock any one customer holds on this bag (Reserved +
        // Collected), or 0 - MaxPerCustomer can't go below it.
        public static async Task<int> GetLargestCustomerQuantityAsync(ApplicationDbContext context, int bagId)
        {
            var perCustomer = await context.SurpriseBagReservations
                .Where(r =>
                    r.SurpriseBagId == bagId &&
                    (r.Status == SurpriseBagReservationStatus.Reserved ||
                     r.Status == SurpriseBagReservationStatus.Collected))
                .GroupBy(r => r.UserId)
                .Select(g => g.Sum(r => r.Quantity))
                .ToListAsync();

            return perCustomer.DefaultIfEmpty(0).Max();
        }
    }
}
